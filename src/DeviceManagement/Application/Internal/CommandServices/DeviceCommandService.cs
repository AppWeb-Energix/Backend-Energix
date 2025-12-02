using System;
using System.Linq;
using System.Threading.Tasks;
using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Domain.Model.Commands;
using Energix.API.DeviceManagement.Domain.Model.Commands.Devices;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Domain.Repositories;
using Energix.API.DeviceManagement.Domain.Services;
using Energix.API.Notifications.Domain.Aggregates;
using Energix.API.Notifications.Domain.Repositories;

namespace Energix.API.DeviceManagement.Application.Internal.CommandServices
{
    /// <summary>
    /// Application service for handling device-related commands
    /// </summary>
    public class DeviceCommandService : IDeviceCommandService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IZoneRepository _zoneRepository;
        private readonly IPlanValidationService _planValidationService;
        private readonly IDeviceNamingService _deviceNamingService;
        private readonly IAlertRepository _alertRepository;

        public DeviceCommandService(
            IDeviceRepository deviceRepository,
            IZoneRepository zoneRepository,
            IPlanValidationService planValidationService,
            IDeviceNamingService deviceNamingService,
            IAlertRepository alertRepository)
        {
            _deviceRepository = deviceRepository;
            _zoneRepository = zoneRepository;
            _planValidationService = planValidationService;
            _deviceNamingService = deviceNamingService;
            _alertRepository = alertRepository;
        }

        private async Task<string> ResolveDeviceNameAsync(CreateDeviceCommand command)
        {
            var trimmed = command.Name?.Trim();

            // If the client provided a valid name, use it
            if (!string.IsNullOrWhiteSpace(trimmed) && _deviceNamingService.IsValidDeviceName(trimmed))
                return trimmed;

            // Otherwise generate an automatic name
            var nextNumber = await _deviceNamingService.GetNextDeviceNumberAsync(command.UserId, command.Type);
            return await _deviceNamingService.GenerateDeviceNameAsync(command.UserId, command.Type, nextNumber, command.DeviceKind);
        }

        /// <summary>
        /// Manage the creation of a device
        /// </summary>
        public async Task<CommandResult<Device>> Handle(CreateDeviceCommand command, PlanType userPlan)
        {
            try
            {
                Console.WriteLine($"[1] Iniciando creación - UserId: {command.UserId}, Type: {command.Type}, Plan: {userPlan}");

                // 1. Verify that the user does not have any other type of device/plan
                var existingDevices = (await _deviceRepository.FindByUserIdAsync(command.UserId)).ToList();
                Console.WriteLine($"[2] Dispositivos existentes: {existingDevices.Count}");

                if (existingDevices.Any())
                {
                    var existingType = existingDevices.First().Type;
                    var expectedType = _planValidationService.GetDeviceTypeForPlan(userPlan);
                    Console.WriteLine($"[3] Tipo existente: {existingType}, Tipo esperado: {expectedType}");

                    if (existingType != expectedType)
                    {
                        Console.WriteLine($"[ERROR] Tipos no coinciden");
                        return CommandResult<Device>.Fail(
                            $"No puedes crear dispositivos de tipo '{expectedType.ToLowerString()}'. " +
                            $"Tu plan actual solo permite dispositivos de tipo '{existingType.ToLowerString()}'");
                    }
                }

                // 2. Validate device limit according to the plan
                var currentCount = await _deviceRepository.CountByUserIdAsync(command.UserId);
                Console.WriteLine($"[4] Cantidad actual: {currentCount}");

                var canAdd = await _planValidationService.CanAddDeviceAsync(command.UserId, userPlan, currentCount);
                Console.WriteLine($"[5] CanAdd: {canAdd}");

                if (!canAdd)
                {
                    var limit = _planValidationService.GetDeviceLimit(userPlan);
                    Console.WriteLine($"[ERROR] Límite alcanzado: {limit}");
                    return CommandResult<Device>.Fail($"Has alcanzado el límite de {limit} dispositivos para tu plan");
                }

                // 3. Verify that the device type corresponds to the plan
                var expectedDeviceType = _planValidationService.GetDeviceTypeForPlan(userPlan);
                Console.WriteLine($"[6] Tipo esperado: {expectedDeviceType}, Tipo recibido: {command.Type}");

                if (command.Type != expectedDeviceType)
                {
                    Console.WriteLine($"[ERROR] Tipo no válido para el plan");
                    return CommandResult<Device>.Fail($"Tu plan solo permite dispositivos de tipo '{expectedDeviceType.ToLowerString()}'");
                }

                // 4. Resolve device name
                Console.WriteLine($"[7] Resolviendo nombre del dispositivo");
                string deviceName;
                var nextNumber = await _deviceNamingService.GetNextDeviceNumberAsync(command.UserId, command.Type);
                Console.WriteLine($"[8] Siguiente número: {nextNumber}");

                deviceName = userPlan == PlanType.Basic
                    ? await _deviceNamingService.GenerateDeviceNameAsync(command.UserId, command.Type, nextNumber, command.DeviceKind)
                    : await ResolveDeviceNameAsync(command);

                Console.WriteLine($"[9] Nombre resuelto: {deviceName}");

                // 5. Create the device according to the type
                Device device;

                if (command.Type == DeviceType.Manual)
                {
                    Console.WriteLine($"[10] Creando dispositivo manual");
                    Console.WriteLine($"[10.1] DeviceKind: {command.DeviceKind?.ToString() ?? "NULL"}");
                    Console.WriteLine($"[10.2] Monthly: {command.Monthly?.ToString() ?? "NULL"}");
                    Console.WriteLine($"[10.3] EstimatedCost: {command.EstimatedCost?.ToString() ?? "NULL"}");

                    // Verify that the required fields for manual devices are present
                    if (!command.DeviceKind.HasValue || !command.Monthly.HasValue || !command.EstimatedCost.HasValue)
                    {
                        Console.WriteLine($"[ERROR] Faltan campos requeridos para dispositivo manual");
                        return CommandResult<Device>.Fail("Para dispositivos manuales se requiere: DeviceKind, Monthly y EstimatedCost");
                    }

                    var metrics = new DeviceMetrics(
                        command.Monthly.Value,
                        command.EstimatedCost.Value,
                        command.Tariff // keep nullable
                    );

                    Console.WriteLine($"[11] Métricas creadas");

                    device = new Device(
                        command.UserId,
                        deviceName,
                        command.DeviceKind.Value,
                        metrics
                    );

                    Console.WriteLine($"[12] Dispositivo creado en memoria");
                }
                else
                {
                    Console.WriteLine($"[13] Creando dispositivo automático");
                    // Automatic device (plug or sensor)
                    // Validate that manual-only fields are NOT included
                    if (command.DeviceKind.HasValue || command.Monthly.HasValue || command.EstimatedCost.HasValue || command.Tariff.HasValue)
                    {
                        return CommandResult<Device>.Fail("Los dispositivos automáticos (plug/sensor) no requieren DeviceKind, Monthly, EstimatedCost ni Tariff");
                    }

                    device = new Device(
                        command.UserId,
                        deviceName,
                        command.Type
                    );
                }

                Console.WriteLine($"[14] Intentando guardar en BD");
                var created = await _deviceRepository.AddAsync(device);
                Console.WriteLine($"[15] Dispositivo guardado exitosamente con ID: {created.Id}");

                // 6. Create alert for device creation
                try
                {
                    var alertMessage = $"Dispositivo {created.Name} agregado ({created.Type.ToLowerString()})";
                    var alert = new Alert(created.UserId, created.Id, "created", alertMessage);
                    await _alertRepository.AddAsync(alert);
                    Console.WriteLine($"[16] Alerta creada: {alertMessage}");
                }
                catch (Exception alertEx)
                {
                    Console.WriteLine($"⚠️ [ALERT ERROR] No se pudo crear alerta: {alertEx.Message}");
                    // Don't fail the entire operation if alert creation fails
                }

                return CommandResult<Device>.Ok(created);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"[STACK] {ex.StackTrace}");
                return CommandResult<Device>.Fail($"Error al crear dispositivo: {ex.Message}");
            }
        }

        /// <summary>
        /// Manage the updating of a device
        /// </summary>
        public async Task<CommandResult<Device>> Handle(UpdateDeviceCommand command, PlanType userPlan)
        {
            try
            {
                // 1. Get the device
                var device = await _deviceRepository.FindByIdAsync(command.DeviceId);
                if (device == null)
                {
                    return CommandResult<Device>.Fail("Dispositivo no encontrado");
                }

                // Store old name for alert if renaming
                var oldName = device.Name;
                var wasRenamed = false;

                // 2. Update name (if applicable)
                if (!string.IsNullOrWhiteSpace(command.Name))
                {
                    if (!_planValidationService.CanRenameDevice(userPlan))
                    {
                        return CommandResult<Device>.Fail("Tu plan no permite renombrar dispositivos");
                    }
                    device.Rename(command.Name);
                    wasRenamed = true;
                }

                // 3. Toggle power (if applicable)
                if (command.Status.HasValue || command.Online.HasValue)
                {
                    if (!_planValidationService.CanTogglePower(userPlan))
                    {
                        return CommandResult<Device>.Fail("Tu plan no permite controlar el encendido/apagado de dispositivos");
                    }

                    if (device.IsManual())
                    {
                        return CommandResult<Device>.Fail("No se puede controlar el estado de dispositivos manuales");
                    }

                    // If status provided, set it
                    if (command.Status.HasValue)
                    {
                        if (command.Status.Value == DeviceStatus.On)
                        {
                            device.TurnOn();
                        }
                        else
                        {
                            device.TurnOff();
                        }
                    }

                    // Online flag update
                    if (command.Online.HasValue)
                    {
                        if (command.Online.Value)
                            device.TurnOn();
                        else
                            device.TurnOff();
                    }
                }

                // 4. Assign/remove zone (if applicable)
                if (command.ZoneId.HasValue || command.RemoveFromZone == true)
                {
                    if (!_planValidationService.CanManageZones(userPlan))
                    {
                        return CommandResult<Device>.Fail("Tu plan no permite gestionar zonas");
                    }

                    if (command.RemoveFromZone == true)
                    {
                        device.AssignToZone(null);
                    }
                    else if (command.ZoneId.HasValue)
                    {
                        // Verify that the zone exists and belongs to the user
                        var zone = await _zoneRepository.FindByIdAsync(command.ZoneId.Value);
                        if (zone == null)
                        {
                            return CommandResult<Device>.Fail("Zona no encontrada");
                        }
                        if (zone.UserId != device.UserId)
                        {
                            return CommandResult<Device>.Fail("La zona no pertenece al mismo usuario");
                        }

                        device.AssignToZone(command.ZoneId.Value);
                    }
                }

                // 5. Update metrics (if applicable)
                if (command.Monthly.HasValue || command.EstimatedCost.HasValue || command.Tariff.HasValue)
                {
                    if (!device.IsManual())
                    {
                        return CommandResult<Device>.Fail("Solo los dispositivos manuales tienen métricas editables");
                    }

                    var currentMetrics = device.Metrics ?? new DeviceMetrics(0, 0);
                    var newMetrics = new DeviceMetrics(
                        command.Monthly ?? currentMetrics.Monthly,
                        command.EstimatedCost ?? currentMetrics.EstimatedCost,
                        command.Tariff ?? currentMetrics.Tariff
                    );

                    device.UpdateMetrics(newMetrics);
                }

                var updated = await _deviceRepository.UpdateAsync(device);

                // 6. Create alert if device was renamed
                if (wasRenamed)
                {
                    try
                    {
                        var alertMessage = $"Dispositivo {oldName} renombrado a {updated.Name}";
                        var alert = new Alert(updated.UserId, updated.Id, "renamed", alertMessage);
                        await _alertRepository.AddAsync(alert);
                        Console.WriteLine($"✅ Alerta creada: {alertMessage}");
                    }
                    catch (Exception alertEx)
                    {
                        Console.WriteLine($"⚠️ [ALERT ERROR] No se pudo crear alerta: {alertEx.Message}");
                        // Don't fail the entire operation if alert creation fails
                    }
                }

                return CommandResult<Device>.Ok(updated);
            }
            catch (Exception ex)
            {
                return CommandResult<Device>.Fail($"Error al actualizar dispositivo: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the disposal of a device
        /// </summary>
        public async Task<CommandResult> Handle(DeleteDeviceCommand command)
        {
            try
            {
                // 1. Validate that it exists and belongs to the user
                var exists = await _deviceRepository.ExistsByIdAndUserIdAsync(command.DeviceId, command.UserId);
                if (!exists)
                {
                    return CommandResult.Fail("Dispositivo no encontrado o no pertenece al usuario");
                }

                // 2. Get and delete
                var device = await _deviceRepository.FindByIdAsync(command.DeviceId);
                if (device == null)
                {
                    return CommandResult.Fail("Dispositivo no encontrado");
                }

                // Store device info for alert before deletion
                var deviceName = device.Name;
                var deviceUserId = device.UserId;
                var deviceId = device.Id;

                await _deviceRepository.DeleteAsync(device);

                // 3. Create alert for device deletion
                try
                {
                    var alertMessage = $"Dispositivo {deviceName} eliminado";
                    var alert = new Alert(deviceUserId, deviceId, "deleted", alertMessage);
                    await _alertRepository.AddAsync(alert);
                    Console.WriteLine($"✅ Alerta creada: {alertMessage}");
                }
                catch (Exception alertEx)
                {
                    Console.WriteLine($"⚠️ [ALERT ERROR] No se pudo crear alerta: {alertEx.Message}");
                    // Don't fail the entire operation if alert creation fails
                }

                return CommandResult.Ok();
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Error al eliminar dispositivo: {ex.Message}");
            }
        }

        // Interface implementation methods (simple wrappers)


        /// <summary>
        /// Handle updating a device (without plan parameter for interface)
        /// </summary>
        async Task<Device?> IDeviceCommandService.Handle(UpdateDeviceCommand command)
        {
            // Default to Basic plan if not specified - should be overridden by controller
            var result = await Handle(command, PlanType.Basic);
            if (!result.Success)
            {
                throw new InvalidOperationException(result.ErrorMessage ?? "Error desconocido al actualizar dispositivo");
            }
            return result.Data;
        }

        /// <summary>
        /// Handle deleting a device (bool return for interface)
        /// </summary>
        async Task<bool> IDeviceCommandService.Handle(DeleteDeviceCommand command)
        {
            var result = await Handle(command);
            return result.Success;
        }
    }
}

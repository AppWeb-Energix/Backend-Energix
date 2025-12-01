using System;
using System.Linq;
using System.Threading.Tasks;
using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Domain.Model.Commands;
using Energix.API.DeviceManagement.Domain.Model.Commands.Devices;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Domain.Repositories;
using Energix.API.DeviceManagement.Domain.Services;

namespace Energix.API.DeviceManagement.Application.Internal.CommandServices
{
    /// <summary>
    /// Application service for handling device-related commands
    /// </summary>
    public class DeviceCommandService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IZoneRepository _zoneRepository;
        private readonly IPlanValidationService _planValidationService;
        private readonly IDeviceNamingService _deviceNamingService;

        public DeviceCommandService(
            IDeviceRepository deviceRepository,
            IZoneRepository zoneRepository,
            IPlanValidationService planValidationService,
            IDeviceNamingService deviceNamingService)
        {
            _deviceRepository = deviceRepository;
            _zoneRepository = zoneRepository;
            _planValidationService = planValidationService;
            _deviceNamingService = deviceNamingService;
        }

        private async Task<string> ResolveDeviceNameAsync(CreateDeviceCommand command)
        {
            var trimmed = command.Name?.Trim();

            // If the client provided a valid name, use it
            if (!string.IsNullOrWhiteSpace(trimmed) && _deviceNamingService.IsValidDeviceName(trimmed))
                return trimmed;

            // Otherwise generate an automatic name
            var nextNumber = await _deviceNamingService.GetNextDeviceNumberAsync(command.UserId, command.Type);
            return await _deviceNamingService.GenerateDeviceNameAsync(command.UserId, command.Type, nextNumber);
        }

        /// <summary>
        /// Manage the creation of a device
        /// </summary>
        public async Task<CommandResult<Device>> Handle(CreateDeviceCommand command, PlanType userPlan)
        {
            try
            {
                // 1. Verify that the user does not have any other type of device/plan
                var existingDevices = (await _deviceRepository.FindByUserIdAsync(command.UserId)).ToList();
                if (existingDevices.Any())
                {
                    var existingType = existingDevices.First().Type;
                    var expectedType = _planValidationService.GetDeviceTypeForPlan(userPlan);

                    if (existingType != expectedType)
                    {
                        return CommandResult<Device>.Fail(
                            $"No puedes crear dispositivos de tipo '{expectedType.ToLowerString()}'. " +
                            $"Tu plan actual solo permite dispositivos de tipo '{existingType.ToLowerString()}'");
                    }
                }

                // 2. Validate device limit according to the plan
                var currentCount = await _deviceRepository.CountByUserIdAsync(command.UserId);
                var canAdd = await _planValidationService.CanAddDeviceAsync(command.UserId, userPlan, currentCount);

                if (!canAdd)
                {
                    var limit = _planValidationService.GetDeviceLimit(userPlan);
                    return CommandResult<Device>.Fail($"Has alcanzado el límite de {limit} dispositivos para tu plan");
                }

                // 3. Verify that the device type corresponds to the plan
                var expectedDeviceType = _planValidationService.GetDeviceTypeForPlan(userPlan);
                if (command.Type != expectedDeviceType)
                {
                    return CommandResult<Device>.Fail($"Tu plan solo permite dispositivos de tipo '{expectedDeviceType.ToLowerString()}'");
                }

                // 4. Resolve device name
                string deviceName;
                if (userPlan == PlanType.Basic)
                {
                    // Basic plan forces automatic naming
                    var nextNumber = await _deviceNamingService.GetNextDeviceNumberAsync(command.UserId, command.Type);
                    deviceName = await _deviceNamingService.GenerateDeviceNameAsync(command.UserId, command.Type, nextNumber);
                }
                else
                {
                    deviceName = await ResolveDeviceNameAsync(command);
                }

                // 5. Create the device according to the type
                Device device;

                if (command.Type == DeviceType.Manual)
                {
                    // Verify that the required fields for manual devices are present
                    if (!command.DeviceKind.HasValue || !command.Monthly.HasValue || !command.EstimatedCost.HasValue)
                    {
                        return CommandResult<Device>.Fail("Para dispositivos manuales se requiere: DeviceKind, Monthly y EstimatedCost");
                    }

                    var metrics = new DeviceMetrics(
                        command.Monthly.Value,
                        command.EstimatedCost.Value,
                        command.Tariff // keep nullable
                    );

                    device = new Device(
                        command.UserId,
                        deviceName,
                        command.DeviceKind.Value,
                        metrics
                    );
                }
                else
                {
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

                var created = await _deviceRepository.AddAsync(device);

                return CommandResult<Device>.Ok(created);
            }
            catch (Exception ex)
            {
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

                // 2. Update name (if applicable)
                if (!string.IsNullOrWhiteSpace(command.Name))
                {
                    if (!_planValidationService.CanRenameDevice(userPlan))
                    {
                        return CommandResult<Device>.Fail("Tu plan no permite renombrar dispositivos");
                    }
                    device.Rename(command.Name);
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
                            device.TurnOn();   // usa la lógica de dominio
                        else
                            device.TurnOff();  // usa la lógica de dominio
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

                await _deviceRepository.DeleteAsync(device);

                return CommandResult.Ok();
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Error al eliminar dispositivo: {ex.Message}");
            }
        }
    }
}

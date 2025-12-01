    using Energix.API.DeviceManagement.Domain.Model.Aggregates;
    using Energix.API.DeviceManagement.Domain.Model.Commands;
    using Energix.API.DeviceManagement.Domain.Model.Commands.Zones;
    using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
    using Energix.API.DeviceManagement.Domain.Repositories;
    using Energix.API.DeviceManagement.Domain.Services;

    namespace Energix.API.DeviceManagement.Application.Internal.CommandServices;

    /// <summary>
    /// Application service for handling zone-related commands
    /// </summary>
    public class ZoneCommandService
    {
        private readonly IZoneRepository _zoneRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IPlanValidationService _planValidationService;

        public ZoneCommandService(
            IZoneRepository zoneRepository,
            IDeviceRepository deviceRepository,
            IPlanValidationService planValidationService)
        {
            _zoneRepository = zoneRepository;
            _deviceRepository = deviceRepository;
            _planValidationService = planValidationService;
        }

        /// <summary>
        /// Manage the creation of a zone
        /// </summary>
        public async Task<CommandResult<Zone>> Handle(CreateZoneCommand command, PlanType userPlan)
        {
            try
            {
                // 1. Verify that the plan allows for the management of zones
                if (!_planValidationService.CanManageZones(userPlan))
                {
                    return CommandResult<Zone>.Fail("Tu plan no permite gestionar zonas. Solo disponible en Plan Familiar");
                }

                // 2. Verify that there is no zone with that name for the user
                var exists = await _zoneRepository.ExistsByUserIdAndNameAsync(command.UserId, command.Name);
                if (exists)
                {
                    return CommandResult<Zone>.Fail($"Ya existe una zona con el nombre '{command.Name}'");
                }

                // 3. Create the zone
                var zone = new Zone(command.UserId, command.Name);

                var created = await _zoneRepository.AddAsync(zone);
                
                return CommandResult<Zone>.Ok(created);
            }
            catch (Exception ex)
            {
                return CommandResult<Zone>.Fail($"Error al crear zona: {ex.Message}");
            }
        }

        /// <summary>
        /// Manage the renaming of an area
        /// </summary>
        public async Task<CommandResult<Zone>> Handle(RenameZoneCommand command)
        {
            try
            {
                // 1. Verify that the area exists and belongs to the user
                var zone = await _zoneRepository.FindByIdAsync(command.ZoneId);
                if (zone == null)
                {
                    return CommandResult<Zone>.Fail("Zona no encontrada");
                }

                if (zone.UserId != command.UserId)
                {
                    return CommandResult<Zone>.Fail("La zona no pertenece a este usuario");
                }

                // 2. Verify that the new name is not in use.
                if (zone.Name != command.NewName)
                {
                    var exists = await _zoneRepository.ExistsByUserIdAndNameAsync(command.UserId, command.NewName);
                    if (exists)
                    {
                        return CommandResult<Zone>.Fail($"Ya existe una zona con el nombre '{command.NewName}'");
                    }
                }

                // 3. Rename
                zone.Rename(command.NewName);

                var updated = await _zoneRepository.UpdateAsync(zone);
                
                return CommandResult<Zone>.Ok(updated);
            }
            catch (Exception ex)
            {
                return CommandResult<Zone>.Fail($"Error al renombrar zona: {ex.Message}");
            }
        }

        /// <summary>
        /// Manage the elimination of an area
        /// </summary>
        public async Task<CommandResult> Handle(DeleteZoneCommand command, PlanType userPlan)
        {
            try
            {
                // 1. Verify that the plan allows for the management of zones
                if (!_planValidationService.CanManageZones(userPlan))
                {
                    return CommandResult.Fail("Tu plan no permite gestionar zonas");
                }

                // 2. Verify that the area exists and belongs to the user
                var zone = await _zoneRepository.FindByIdAsync(command.ZoneId);
                if (zone == null)
                {
                    return CommandResult.Fail("Zona no encontrada");
                }

                if (zone.UserId != command.UserId)
                {
                    return CommandResult.Fail("La zona no pertenece a este usuario");
                }

                // 3. Unassign all devices in this zone
                var devicesInZone = await _deviceRepository.FindByZoneIdAsync(command.ZoneId);
                foreach (var device in devicesInZone)
                {
                    device.AssignToZone(null);
                    await _deviceRepository.UpdateAsync(device);
                }

                // 4. Delete zone
                await _zoneRepository.DeleteAsync(zone);
                
                return CommandResult.Ok();
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Error al eliminar zona: {ex.Message}");
            }
        }
    }
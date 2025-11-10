using Energix.API.DeviceManagement.Domain.Model.Events.Devices;

namespace Energix.API.DeviceManagement.Application.Internal.EventHandlers.Devices;

/// <summary>
/// DeviceAssignedToZoneEvent event handler
/// It runs when a device is assigned to a zone
/// </summary>
public class DeviceAssignedToZoneEventHandler
{
    private readonly ILogger<DeviceAssignedToZoneEventHandler> _logger;

    public DeviceAssignedToZoneEventHandler(ILogger<DeviceAssignedToZoneEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the device to zone assignment event
    /// It maintains consistency and can trigger notifications
    /// </summary>
    public async Task Handle(DeviceAssignedToZoneEvent @event)
    {
        _logger.LogInformation(
            "Dispositivo asignado a zona: DeviceId={DeviceId}, ZoneId={ZoneId}, UserId={UserId}",
            @event.DeviceId, @event.ZoneId, @event.UserId);

        await Task.CompletedTask;
    }
}


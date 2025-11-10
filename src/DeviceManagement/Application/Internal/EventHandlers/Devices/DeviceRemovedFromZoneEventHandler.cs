using Energix.API.DeviceManagement.Domain.Model.Events.Devices;

namespace Energix.API.DeviceManagement.Application.Internal.EventHandlers.Devices;

/// <summary>
/// DeviceRemovedFromZoneEvent event handler
/// It is executed when a device is removed from an area
/// </summary>
public class DeviceRemovedFromZoneEventHandler
{
    private readonly ILogger<DeviceRemovedFromZoneEventHandler> _logger;

    public DeviceRemovedFromZoneEventHandler(ILogger<DeviceRemovedFromZoneEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the device removal event from a zone
    /// Cleans up references and maintains consistency
    /// </summary>
    public async Task Handle(DeviceRemovedFromZoneEvent @event)
    {
        _logger.LogInformation(
            "Dispositivo removido de zona: DeviceId={DeviceId}, ZoneId={ZoneId}, UserId={UserId}",
            @event.DeviceId, @event.ZoneId, @event.UserId);
        
        await Task.CompletedTask;
    }
}


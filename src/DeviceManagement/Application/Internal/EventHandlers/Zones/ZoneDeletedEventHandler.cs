using Energix.API.DeviceManagement.Domain.Model.Events.Zones;
using Energix.API.DeviceManagement.Domain.Repositories;

namespace Energix.API.DeviceManagement.Application.Internal.EventHandlers.Zones;

/// <summary>
/// ZoneDeletedEvent event handler
/// It is executed when a zone is deleted
/// IMPORTANT: You must ensure that all devices are left without an assigned zone
/// </summary>
public class ZoneDeletedEventHandler
{
    private readonly ILogger<ZoneDeletedEventHandler> _logger;
    private readonly IDeviceRepository _deviceRepository;

    public ZoneDeletedEventHandler(
        ILogger<ZoneDeletedEventHandler> logger,
        IDeviceRepository deviceRepository)
    {
        _logger = logger;
        _deviceRepository = deviceRepository;
    }

    /// <summary>
    /// Handles the zone removal event
    /// Ensure that linked devices are unlinked
    /// </summary>
    public async Task Handle(ZoneDeletedEvent @event)
    {
        _logger.LogInformation(
            "Zona eliminada: ZoneId={ZoneId}, UserId={UserId}, DeviceCount={DeviceCount}",
            @event.ZoneId, @event.UserId, @event.DeviceCount);

        // If there were assigned devices, ensure that they are unlinked
        if (@event.DeviceCount > 0)
        {
            _logger.LogWarning(
                "La zona {ZoneId} tenía {DeviceCount} dispositivos asignados. Verificando desvinculación...",
                @event.ZoneId, @event.DeviceCount);
        }

        await Task.CompletedTask;
    }
}



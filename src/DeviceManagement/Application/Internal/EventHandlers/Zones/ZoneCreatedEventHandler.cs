using Energix.API.DeviceManagement.Domain.Model.Events.Zones;

namespace Energix.API.DeviceManagement.Application.Internal.EventHandlers.Zones;

/// <summary>
/// ZoneCreatedEvent event handler
/// It runs when a new zone is created
/// </summary>
public class ZoneCreatedEventHandler
{
    private readonly ILogger<ZoneCreatedEventHandler> _logger;

    public ZoneCreatedEventHandler(ILogger<ZoneCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Manage the zone creation event
    /// Here you could: send notifications, record audits, etc.
    /// </summary>
    public async Task Handle(ZoneCreatedEvent @event)
    {
        _logger.LogInformation(
            "Zona creada: ZoneId={ZoneId}, UserId={UserId}, Name={ZoneName}",
            @event.ZoneId, @event.UserId, @event.ZoneName);

        await Task.CompletedTask;
    }
}


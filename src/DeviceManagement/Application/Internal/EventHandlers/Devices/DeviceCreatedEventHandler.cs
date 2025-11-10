using Energix.API.DeviceManagement.Domain.Model.Events.Devices;

namespace Energix.API.DeviceManagement.Application.Internal.EventHandlers.Devices;

/// <summary>
/// DeviceCreatedEvent handler
/// It runs when a new device is created
/// </summary>
public class DeviceCreatedEventHandler
{
    private readonly ILogger<DeviceCreatedEventHandler> _logger;

    public DeviceCreatedEventHandler(ILogger<DeviceCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the device creation event
    /// Here you could: send notifications, update statistics, record audits, etc.
    /// </summary>
    public async Task Handle(DeviceCreatedEvent @event)
    {
        _logger.LogInformation(
            "Dispositivo creado: DeviceId={DeviceId}, UserId={UserId}, Name={DeviceName}, Type={DeviceType}",
            @event.DeviceId, @event.UserId, @event.DeviceName, @event.DeviceType);

        await Task.CompletedTask;
    }
}
namespace Energix.API.DeviceManagement.Domain.Model.Events.Devices;

/// <summary>
/// Domain event that is triggered when a device is removed from a zone
/// </summary>
public record DeviceRemovedFromZoneEvent(
    int DeviceId,
    int ZoneId,
    int UserId,
    DateTime RemovedAt
);


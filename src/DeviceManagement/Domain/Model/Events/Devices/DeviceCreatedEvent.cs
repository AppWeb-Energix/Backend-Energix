namespace Energix.API.DeviceManagement.Domain.Model.Events.Devices;

/// <summary>
/// Domain event that is triggered when a new device is created
/// </summary>
public record DeviceCreatedEvent(
    int DeviceId,
    int UserId,
    string DeviceName,
    string DeviceType,
    DateTime LinkedAt
);
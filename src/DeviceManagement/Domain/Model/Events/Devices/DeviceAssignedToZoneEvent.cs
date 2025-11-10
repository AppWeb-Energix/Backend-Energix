namespace Energix.API.DeviceManagement.Domain.Model.Events.Devices;

/// <summary>
/// Domain event that is triggered when a device is assigned to a zone
/// </summary>
public record DeviceAssignedToZoneEvent(
    int DeviceId,
    int ZoneId,
    int UserId,
    DateTime AssignedAt
);


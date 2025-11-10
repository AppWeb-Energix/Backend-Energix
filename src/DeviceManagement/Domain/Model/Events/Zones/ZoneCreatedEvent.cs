namespace Energix.API.DeviceManagement.Domain.Model.Events.Zones;

/// <summary>
/// Domain event that is triggered when a new zone is created
/// </summary>
public record ZoneCreatedEvent(
    int ZoneId,
    int UserId,
    string ZoneName,
    DateTime CreatedAt
);


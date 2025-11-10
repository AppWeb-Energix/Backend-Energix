namespace Energix.API.DeviceManagement.Domain.Model.Events.Zones;

/// <summary>
/// Domain event that is triggered when a zone is deleted
/// </summary>
public record ZoneDeletedEvent(
    int ZoneId,
    int UserId,
    int DeviceCount,
    DateTime DeletedAt
);



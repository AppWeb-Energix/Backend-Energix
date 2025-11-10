namespace Energix.API.DeviceManagement.Domain.Model.Queries.Zones;

/// <summary>
/// Query to get all the zones of a user
/// </summary>
public record GetZonesByUserIdQuery(
    int UserId
);
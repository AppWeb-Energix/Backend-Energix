namespace Energix.API.DeviceManagement.Domain.Model.Queries.Zones;

/// <summary>
/// Query to retrieve a zone by its ID
/// </summary>
public record GetZoneByIdQuery(
    int ZoneId
);
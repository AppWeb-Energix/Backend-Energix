namespace Energix.API.DeviceManagement.Domain.Model.Queries.Zones;

/// <summary>
/// Query to obtain zones with your devices
/// </summary>
public record GetZonesWithDevicesQuery(
    int UserId
);

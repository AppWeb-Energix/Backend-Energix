namespace Energix.API.DeviceManagement.Domain.Model.Queries.Devices;

/// <summary>
/// Query to obtain devices by zone
/// </summary>
public record GetDevicesByZoneIdQuery(
    int ZoneId
);
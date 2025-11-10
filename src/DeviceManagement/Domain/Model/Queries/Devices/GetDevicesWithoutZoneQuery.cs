namespace Energix.API.DeviceManagement.Domain.Model.Queries.Devices;

/// <summary>
/// Query to obtain devices without assigned zone
/// </summary>
public record GetDevicesWithoutZoneQuery(
    int UserId
);
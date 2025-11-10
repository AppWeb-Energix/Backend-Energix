namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Resource for area with its included devices
/// </summary>
public record ZoneWithDevicesResource(
    int Id,
    int UserId,
    string Name,
    DateTime CreatedAt,
    List<DeviceResource> Devices
);
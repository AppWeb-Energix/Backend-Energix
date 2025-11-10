namespace Energix.API.DeviceManagement.Domain.Model.Queries.Devices;

/// <summary>
/// Query to retrieve a device by its ID
/// </summary>
public record GetDeviceByIdQuery(
    int DeviceId
);
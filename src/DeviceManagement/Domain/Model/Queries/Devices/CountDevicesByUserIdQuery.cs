namespace Energix.API.DeviceManagement.Domain.Model.Queries.Devices;

/// <summary>
/// Query to count a user's devices
/// </summary>
public record CountDevicesByUserIdQuery(
    int UserId
);
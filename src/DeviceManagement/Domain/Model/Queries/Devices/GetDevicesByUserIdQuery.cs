using Energix.API.DeviceManagement.Domain.Model.ValueObjects;

namespace Energix.API.DeviceManagement.Domain.Model.Queries.Devices;

/// <summary>
/// Query to get all devices of a user
/// </summary>
public record GetDevicesByUserIdQuery(
    int UserId,
    DeviceType? Type = null
);
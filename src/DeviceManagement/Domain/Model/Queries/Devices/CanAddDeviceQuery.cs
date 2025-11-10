using Energix.API.DeviceManagement.Domain.Model.ValueObjects;

namespace Energix.API.DeviceManagement.Domain.Model.Queries.Devices;

/// <summary>
/// Query to validate if a user can add more devices according to their plan
/// </summary>
public record CanAddDeviceQuery(
    int UserId,
    PlanType Plan
);
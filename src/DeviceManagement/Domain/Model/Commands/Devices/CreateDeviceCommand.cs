using Energix.API.DeviceManagement.Domain.Model.ValueObjects;

namespace Energix.API.DeviceManagement.Domain.Model.Commands.Devices;

/// <summary>
/// Command to create a device (automatic or manual)
/// </summary>
public record CreateDeviceCommand(
    int UserId,
    string Name,
    DeviceType Type,
    // Fields for manual devices
    DeviceKind? DeviceKind = null,
    decimal? Monthly = null,
    decimal? EstimatedCost = null,
    decimal? Tariff = null
);
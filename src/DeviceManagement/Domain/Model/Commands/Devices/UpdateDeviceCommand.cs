using Energix.API.DeviceManagement.Domain.Model.ValueObjects;

namespace Energix.API.DeviceManagement.Domain.Model.Commands.Devices;

/// <summary>
/// Command to update a device
/// It handles: rename, toggle power, assign zone, update metrics
/// </summary>
public record UpdateDeviceCommand(
    int DeviceId,
    // Basic fields
    string? Name = null,
    DeviceStatus? Status = null,
    bool? Online = null,
    // Zone
    int? ZoneId = null,
    bool? RemoveFromZone = null, // true to remove from zone (ZoneId = null)
    // Metrics (for manuals only)
    decimal? Monthly = null,
    decimal? EstimatedCost = null,
    decimal? Tariff = null,
    decimal? DailyAvg = null
);
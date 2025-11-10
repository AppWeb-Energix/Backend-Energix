namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Resource to update a device
/// </summary>
public record UpdateDeviceResource(
    string? Name = null,
    string? Status = null,
    bool? Online = null,
    int? ZoneId = null,
    bool? RemoveFromZone = null,
    decimal? Monthly = null,
    decimal? EstimatedCost = null,
    decimal? Tariff = null
);
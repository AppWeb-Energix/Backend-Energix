namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

public record CreateDeviceResource(
    string Name,
    string Type,
    string? DeviceKind,
    decimal? Monthly,      // ✅ Cambiado de double? a decimal?
    decimal? EstimatedCost, // ✅ Cambiado de double? a decimal?
    decimal? Tariff         // ✅ Cambiado de double? a decimal?
);
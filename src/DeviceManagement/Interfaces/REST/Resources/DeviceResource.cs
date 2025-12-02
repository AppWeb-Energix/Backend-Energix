namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Resource to represent a device in the responses
/// </summary>
public record DeviceResource(
    int Id,
    int UserId,
    string Name,
    string Type,
    string Status,
    bool Online,
    DateTime LinkedAt,
    int? ZoneId,
    string? DeviceKind,
    DeviceMetricsSummaryResource? Metrics
);
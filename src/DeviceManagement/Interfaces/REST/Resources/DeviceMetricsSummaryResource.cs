namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Resource for manual device metrics
/// </summary>
public record DeviceMetricsSummaryResource(
    decimal Monthly,
    decimal EstimatedCost,
    decimal? Tariff,
    decimal DailyAvg,
    List<ManualDeviceMetricsResource> Breakdown
);
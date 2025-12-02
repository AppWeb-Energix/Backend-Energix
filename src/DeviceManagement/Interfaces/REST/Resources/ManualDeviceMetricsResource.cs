/// <summary>
/// Resource representing the metrics of a manual device.
/// </summary>
public record ManualDeviceMetricsResource(
    int DeviceId,
    string Name,
    string? DeviceKind,
    decimal Monthly,
    decimal EstimatedCost,
    decimal DailyAverage,
    decimal? Tariff
);
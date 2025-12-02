namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Point for charting manual device metrics.
/// </summary>
public record ManualDeviceChartPointResource(
    string Name,
    decimal MonthlyConsumption,
    decimal EstimatedCost,
    decimal DailyAverage,
    decimal? Tariff
);
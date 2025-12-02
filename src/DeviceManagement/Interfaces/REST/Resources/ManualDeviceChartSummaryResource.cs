namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Chart-friendly response for manual device metrics.
/// </summary>
/// <param name="Labels">Device names used as chart labels</param>
/// <param name="Datasets">Datasets aligned with the labels</param>
/// <param name="TotalMonthlyConsumption">Sum of monthly kWh for all manual devices</param>
/// <param name="TotalEstimatedCost">Sum of monthly estimated cost for all manual devices</param>
/// <param name="AverageDailyConsumption">Average daily kWh across manual devices</param>
public record ManualDeviceChartSummaryResource(
    IEnumerable<string> Labels,
    IEnumerable<ManualDeviceChartDatasetResource> Datasets,
    decimal TotalMonthlyConsumption,
    decimal TotalEstimatedCost,
    decimal AverageDailyConsumption);
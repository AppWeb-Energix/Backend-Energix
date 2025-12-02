namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Aggregated totals used to render highlights beside the chart.
/// </summary>
/// <param name="TotalMonthlyConsumption">Sum of monthly kWh for all manual devices</param>
/// <param name="TotalEstimatedCost">Sum of monthly estimated cost for all manual devices</param>
/// <param name="AverageDailyConsumption">Average daily kWh across manual devices</param>
public record ManualDeviceChartTotalsResource(
    decimal TotalMonthlyConsumption,
    decimal TotalEstimatedCost,
    decimal AverageDailyConsumption);
namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Additional metadata to help the frontend render context-aware labels.
/// </summary>
/// <param name="DeviceCount">Number of manual devices included</param>
/// <param name="ConsumptionLabel">Label used for consumption-related datasets</param>
/// <param name="CostLabel">Label used for cost datasets (can include currency)</param>
/// <param name="DailyAverageLabel">Label used for daily averages</param>
public record ManualDeviceChartMetadataResource(
    int DeviceCount,
    string ConsumptionLabel,
    string CostLabel,
    string DailyAverageLabel);
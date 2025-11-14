namespace Energix.API.Personalization.Interfaces.REST.Resources;

/// <summary>
/// Resource to update personalization settings
/// </summary>
public record UpdatePersonalizationResource(
    bool? KpiCurrent = null,
    bool? KpiCost = null,
    bool? KpiMonthly = null,
    bool? ChartHourly = null,
    bool? ChartMonthly = null,
    bool? ChartDevice = null
);



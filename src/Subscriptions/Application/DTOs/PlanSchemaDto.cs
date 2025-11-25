namespace Energix.Subscriptions.Application.DTOs;

/// <summary>
/// DTO que representa un plan de suscripción disponible
/// </summary>
public class PlanSchemaDto
{
    public string PlanType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public decimal MonthlyPrice { get; set; }
    public string MonthlyPriceCurrency { get; set; } = "USD";
    
    public decimal YearlyPrice { get; set; }
    public string YearlyPriceCurrency { get; set; } = "USD";
    
    public decimal YearlySavings { get; set; }
    public decimal YearlyDiscountPercentage { get; set; }
    
    public int MaxDevices { get; set; }
    public int HistoryDays { get; set; }
    public bool HasUnlimitedHistory { get; set; }
    public bool HasConsumptionForecast { get; set; }
    public bool HasSmartAlerts { get; set; }
    public bool HasPersonalizedRecommendations { get; set; }
    public bool HasReportExport { get; set; }
    public bool AllowsManualDeviceAddition { get; set; }
    
    public List<PlanFeatureDto> Features { get; set; } = new();
    public List<string> IncludedFeatures { get; set; } = new();
}


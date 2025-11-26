namespace Energix.Subscriptions.Application.DTOs;

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public string PlanType { get; set; } = string.Empty;
    public string PlanDisplayName { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    
    public decimal PriceAmount { get; set; }
    public string PriceCurrency { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public bool IsActive { get; set; }
    public bool AutoRenew { get; set; }
    
    public int MaxDevices { get; set; }
    public int HistoryDays { get; set; }
    public bool HasUnlimitedHistory { get; set; }
    public bool HasSmartAlerts { get; set; }
    public bool HasPersonalizedRecommendations { get; set; }
    public bool HasConsumptionForecast { get; set; }
    public bool HasReportExport { get; set; }
    
    public List<PaymentMethodDto> PaymentMethods { get; set; } = new();
    public List<string> IncludedFeatures { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}


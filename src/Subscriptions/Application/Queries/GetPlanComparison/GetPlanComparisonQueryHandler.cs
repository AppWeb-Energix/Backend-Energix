using Energix.Subscriptions.Application.DTOs;
using Energix.Subscriptions.Domain.Schemas;

namespace Energix.Subscriptions.Application.Queries.GetPlanComparison;

/// <summary>
/// Resultado de comparación entre planes
/// </summary>
public class PlanComparisonResult
{
    public PlanSchemaDto? CurrentPlan { get; set; }
    public PlanSchemaDto? TargetPlan { get; set; }
    public bool IsUpgrade { get; set; }
    public bool IsDowngrade { get; set; }
    public decimal PriceDifferenceMonthly { get; set; }
    public decimal PriceDifferenceYearly { get; set; }
    public List<string> NewFeatures { get; set; } = new();
    public List<string> RemovedFeatures { get; set; } = new();
}

/// <summary>
/// Handler para comparar planes
/// </summary>
public class GetPlanComparisonQueryHandler
{
    public Task<PlanComparisonResult> HandleAsync(
        GetPlanComparisonQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = new PlanComparisonResult();

        if (!query.CurrentPlan.HasValue || !query.TargetPlan.HasValue)
        {
            return Task.FromResult(result);
        }

        var currentPlanSchema = PlanSchema.GetPlanSchema(query.CurrentPlan.Value);
        var targetPlanSchema = PlanSchema.GetPlanSchema(query.TargetPlan.Value);

        result.CurrentPlan = MapToPlanSchemaDto(currentPlanSchema);
        result.TargetPlan = MapToPlanSchemaDto(targetPlanSchema);
        result.IsUpgrade = PlanSchema.IsUpgrade(query.CurrentPlan.Value, query.TargetPlan.Value);
        result.IsDowngrade = PlanSchema.IsDowngrade(query.CurrentPlan.Value, query.TargetPlan.Value);
        result.PriceDifferenceMonthly = targetPlanSchema.MonthlyPrice.Amount - currentPlanSchema.MonthlyPrice.Amount;
        result.PriceDifferenceYearly = targetPlanSchema.YearlyPrice.Amount - currentPlanSchema.YearlyPrice.Amount;

        // Identificar características nuevas y removidas
        var currentFeatures = currentPlanSchema.IncludedFeatures.ToHashSet();
        var targetFeatures = targetPlanSchema.IncludedFeatures.ToHashSet();

        result.NewFeatures = targetFeatures.Except(currentFeatures).ToList();
        result.RemovedFeatures = currentFeatures.Except(targetFeatures).ToList();

        return Task.FromResult(result);
    }

    private PlanSchemaDto MapToPlanSchemaDto(PlanSchema plan)
    {
        return new PlanSchemaDto
        {
            PlanType = plan.PlanType.ToString(),
            Name = plan.Name,
            DisplayName = plan.DisplayName,
            Description = plan.Description,
            MonthlyPrice = plan.MonthlyPrice.Amount,
            MonthlyPriceCurrency = plan.MonthlyPrice.Currency,
            YearlyPrice = plan.YearlyPrice.Amount,
            YearlyPriceCurrency = plan.YearlyPrice.Currency,
            YearlySavings = plan.CalculateYearlySavings(),
            YearlyDiscountPercentage = plan.YearlyDiscountPercentage(),
            MaxDevices = plan.MaxDevices == int.MaxValue ? -1 : plan.MaxDevices,
            HistoryDays = plan.HistoryDays == int.MaxValue ? -1 : plan.HistoryDays,
            HasUnlimitedHistory = plan.HasUnlimitedHistory,
            HasConsumptionForecast = plan.HasConsumptionForecast,
            HasSmartAlerts = plan.HasSmartAlerts,
            HasPersonalizedRecommendations = plan.HasPersonalizedRecommendations,
            HasReportExport = plan.HasReportExport,
            AllowsManualDeviceAddition = plan.AllowsManualDeviceAddition,
            Features = plan.Features.Select(f => new PlanFeatureDto
            {
                Name = f.Name,
                Description = f.Description,
                IsAvailable = f.IsAvailable
            }).ToList(),
            IncludedFeatures = plan.IncludedFeatures
        };
    }
}


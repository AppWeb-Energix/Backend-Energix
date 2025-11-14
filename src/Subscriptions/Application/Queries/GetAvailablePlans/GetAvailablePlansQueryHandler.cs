using Energix.Subscriptions.Application.DTOs;
using Energix.Subscriptions.Domain.Schemas;

namespace Energix.Subscriptions.Application.Queries.GetAvailablePlans;

/// <summary>
/// Handler para obtener todos los planes disponibles
/// </summary>
public class GetAvailablePlansQueryHandler
{
    public Task<List<PlanSchemaDto>> HandleAsync(
        GetAvailablePlansQuery query,
        CancellationToken cancellationToken = default)
    {
        var plans = PlanSchema.GetAllPlans();
        
        var dtos = plans.Select(plan => new PlanSchemaDto
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
            MaxDevices = plan.MaxDevices == int.MaxValue ? -1 : plan.MaxDevices, // -1 indica ilimitado
            HistoryDays = plan.HistoryDays == int.MaxValue ? -1 : plan.HistoryDays,
            HasUnlimitedHistory = plan.HasUnlimitedHistory,
            HasConsumptionForecast = plan.HasConsumptionForecast,
            HasSmartAlerts = plan.HasSmartAlerts,
            HasPersonalizedRecommendations = plan.HasPersonalizedRecommendations,
            HasReportExport = plan.HasReportExport,
            AllowsManualDeviceAddition = plan.AllowsManualDeviceAddition,
            Features = query.IncludeFeatures 
                ? plan.Features.Select(f => new PlanFeatureDto
                {
                    Name = f.Name,
                    Description = f.Description,
                    IsAvailable = f.IsAvailable
                }).ToList()
                : new List<PlanFeatureDto>(),
            IncludedFeatures = query.IncludeFeatures 
                ? plan.IncludedFeatures 
                : new List<string>()
        }).ToList();

        return Task.FromResult(dtos);
    }
}


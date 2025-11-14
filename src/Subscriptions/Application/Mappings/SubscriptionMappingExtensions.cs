using Energix.Subscriptions.Application.DTOs;
using Energix.Subscriptions.Domain.Aggregates;
using Energix.Subscriptions.Domain.Schemas;

namespace Energix.Subscriptions.Application.Mappings;

/// <summary>
/// Profile para mapeo entre entidades del dominio y DTOs
/// </summary>
public static class SubscriptionMappingProfile
{
    public static SubscriptionDto ToDto(this Subscription subscription, bool includePaymentMethods = false)
    {
        var planSchema = subscription.GetPlanSchema();

        var dto = new SubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            PlanType = subscription.PlanType.ToString(),
            PlanDisplayName = planSchema.DisplayName,
            BillingPeriod = subscription.BillingPeriod.ToString(),
            PriceAmount = subscription.Price.Amount,
            PriceCurrency = subscription.Price.Currency,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            NextBillingDate = subscription.NextBillingDate,
            IsActive = subscription.IsActive,
            AutoRenew = subscription.AutoRenew,
            MaxDevices = planSchema.MaxDevices == int.MaxValue ? -1 : planSchema.MaxDevices,
            HistoryDays = planSchema.HistoryDays == int.MaxValue ? -1 : planSchema.HistoryDays,
            HasUnlimitedHistory = planSchema.HasUnlimitedHistory,
            HasSmartAlerts = planSchema.HasSmartAlerts,
            HasPersonalizedRecommendations = planSchema.HasPersonalizedRecommendations,
            HasConsumptionForecast = planSchema.HasConsumptionForecast,
            HasReportExport = planSchema.HasReportExport,
            IncludedFeatures = planSchema.IncludedFeatures,
            CreatedAt = subscription.CreatedAt,
            UpdatedAt = subscription.UpdatedAt
        };

        if (includePaymentMethods)
        {
            dto.PaymentMethods = subscription.PaymentMethods
                .Where(pm => pm.IsActive)
                .OrderByDescending(pm => pm.IsDefault)
                .ThenByDescending(pm => pm.CreatedAt)
                .Select(pm => pm.ToDto())
                .ToList();
        }

        return dto;
    }

    public static PaymentMethodDto ToDto(this PaymentMethod paymentMethod)
    {
        return new PaymentMethodDto
        {
            Id = paymentMethod.Id,
            MaskedCardNumber = paymentMethod.MaskedCardNumber.Value,
            CardBrand = paymentMethod.CardBrand.Name,
            CardHolderName = paymentMethod.CardHolderName,
            ExpiryDate = paymentMethod.ExpiryDate,
            IsDefault = paymentMethod.IsDefault,
            IsActive = paymentMethod.IsActive,
            IsExpired = paymentMethod.IsExpired(),
            CreatedAt = paymentMethod.CreatedAt
        };
    }

    public static PlanSchemaDto ToDto(this PlanSchema planSchema)
    {
        return new PlanSchemaDto
        {
            PlanType = planSchema.PlanType.ToString(),
            Name = planSchema.Name,
            DisplayName = planSchema.DisplayName,
            Description = planSchema.Description,
            MonthlyPrice = planSchema.MonthlyPrice.Amount,
            MonthlyPriceCurrency = planSchema.MonthlyPrice.Currency,
            YearlyPrice = planSchema.YearlyPrice.Amount,
            YearlyPriceCurrency = planSchema.YearlyPrice.Currency,
            YearlySavings = planSchema.CalculateYearlySavings(),
            YearlyDiscountPercentage = planSchema.YearlyDiscountPercentage(),
            MaxDevices = planSchema.MaxDevices == int.MaxValue ? -1 : planSchema.MaxDevices,
            HistoryDays = planSchema.HistoryDays == int.MaxValue ? -1 : planSchema.HistoryDays,
            HasUnlimitedHistory = planSchema.HasUnlimitedHistory,
            HasConsumptionForecast = planSchema.HasConsumptionForecast,
            HasSmartAlerts = planSchema.HasSmartAlerts,
            HasPersonalizedRecommendations = planSchema.HasPersonalizedRecommendations,
            HasReportExport = planSchema.HasReportExport,
            AllowsManualDeviceAddition = planSchema.AllowsManualDeviceAddition,
            Features = planSchema.Features.Select(f => new PlanFeatureDto
            {
                Name = f.Name,
                Description = f.Description,
                IsAvailable = f.IsAvailable
            }).ToList(),
            IncludedFeatures = planSchema.IncludedFeatures
        };
    }
}


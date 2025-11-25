using Energix.Subscriptions.Application.DTOs;
using Energix.API;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Queries.GetSubscriptionByUser;

public class GetSubscriptionByUserQueryHandler
{
    private readonly AppDbContext _context;

    public GetSubscriptionByUserQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionDto?> HandleAsync(
        GetSubscriptionByUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var subscriptionQuery = _context.Subscriptions
            .Where(s => s.UserId == query.UserId);

        if (query.IncludePaymentMethods)
        {
            subscriptionQuery = subscriptionQuery.Include(s => s.PaymentMethods);
        }

        var subscription = await subscriptionQuery
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
            return null;

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

        if (query.IncludePaymentMethods)
        {
            dto.PaymentMethods = subscription.PaymentMethods
                .Where(pm => pm.IsActive)
                .OrderByDescending(pm => pm.IsDefault)
                .ThenByDescending(pm => pm.CreatedAt)
                .Select(pm => new PaymentMethodDto
                {
                    Id = pm.Id,
                    MaskedCardNumber = pm.MaskedCardNumber.Value,
                    CardBrand = pm.CardBrand.Name,
                    CardHolderName = pm.CardHolderName,
                    ExpiryDate = pm.ExpiryDate,
                    IsDefault = pm.IsDefault,
                    IsActive = pm.IsActive,
                    IsExpired = pm.IsExpired(),
                    CreatedAt = pm.CreatedAt
                })
                .ToList();
        }

        return dto;
    }
}


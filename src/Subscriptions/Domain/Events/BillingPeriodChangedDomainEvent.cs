using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Domain.Events;

/// <summary>
/// Evento de dominio que se dispara cuando cambia el periodo de facturación
/// </summary>
public class BillingPeriodChangedDomainEvent
{
    public Guid SubscriptionId { get; set; }
    public Guid UserId { get; set; }
    public PlanType PlanType { get; set; }
    public BillingPeriod OldBillingPeriod { get; set; }
    public BillingPeriod NewBillingPeriod { get; set; }
    public decimal OldPriceAmount { get; set; }
    public decimal NewPriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }

    public BillingPeriodChangedDomainEvent(
        Guid subscriptionId,
        Guid userId,
        PlanType planType,
        BillingPeriod oldBillingPeriod,
        BillingPeriod newBillingPeriod,
        decimal oldPriceAmount,
        decimal newPriceAmount,
        string currency)
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        PlanType = planType;
        OldBillingPeriod = oldBillingPeriod;
        NewBillingPeriod = newBillingPeriod;
        OldPriceAmount = oldPriceAmount;
        NewPriceAmount = newPriceAmount;
        Currency = currency;
        ChangedAt = DateTime.UtcNow;
    }
}


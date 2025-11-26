using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Domain.Events;

/// <summary>
/// Evento de dominio que se dispara cuando cambia el plan de una suscripción
/// </summary>
public class SubscriptionPlanChangeDomainEvent
{
    public Guid SubscriptionId { get; set; }
    public int UserId { get; set; }
    public PlanType OldPlanType { get; set; }
    public PlanType NewPlanType { get; set; }
    public BillingPeriod OldBillingPeriod { get; set; }
    public BillingPeriod NewBillingPeriod { get; set; }
    public decimal OldPriceAmount { get; set; }
    public decimal NewPriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsUpgrade { get; set; }
    public bool IsDowngrade { get; set; }
    public DateTime ChangedAt { get; set; }

    public SubscriptionPlanChangeDomainEvent(
        Guid subscriptionId,
        int userId,
        PlanType oldPlanType,
        PlanType newPlanType,
        BillingPeriod oldBillingPeriod,
        BillingPeriod newBillingPeriod,
        decimal oldPriceAmount,
        decimal newPriceAmount,
        string currency,
        bool isUpgrade,
        bool isDowngrade)
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        OldPlanType = oldPlanType;
        NewPlanType = newPlanType;
        OldBillingPeriod = oldBillingPeriod;
        NewBillingPeriod = newBillingPeriod;
        OldPriceAmount = oldPriceAmount;
        NewPriceAmount = newPriceAmount;
        Currency = currency;
        IsUpgrade = isUpgrade;
        IsDowngrade = isDowngrade;
        ChangedAt = DateTime.UtcNow;
    }
}


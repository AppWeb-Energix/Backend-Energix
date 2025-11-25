using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Domain.Events;

/// <summary>
/// Evento de dominio que se dispara cuando se crea una nueva suscripción
/// </summary>
public class SubscriptionCreatedDomainEvent
{
    public Guid SubscriptionId { get; set; }
    public Guid UserId { get; set; }
    public PlanType PlanType { get; set; }
    public BillingPeriod BillingPeriod { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public SubscriptionCreatedDomainEvent(
        Guid subscriptionId,
        Guid userId,
        PlanType planType,
        BillingPeriod billingPeriod,
        decimal priceAmount,
        string currency)
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        PlanType = planType;
        BillingPeriod = billingPeriod;
        PriceAmount = priceAmount;
        Currency = currency;
        CreatedAt = DateTime.UtcNow;
    }
}


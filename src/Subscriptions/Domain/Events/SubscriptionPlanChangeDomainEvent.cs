namespace Energix.Subscriptions.Domain.Events;

public class SubscriptionPlanChangeDomainEvent
{
    public Guid SubscriptionId { get; set; }
    public Guid UserId { get; set; }
    public string OldPlanType { get; set; } = string.Empty;
    public string NewPlanType { get; set; } = string.Empty;
    public decimal OldPriceAmount { get; set; }
    public decimal NewPriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }

    public SubscriptionPlanChangeDomainEvent(
        Guid subscriptionId,
        Guid userId,
        string oldPlanType,
        string newPlanType,
        decimal oldPriceAmount,
        decimal newPriceAmount,
        string currency)
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        OldPlanType = oldPlanType;
        NewPlanType = newPlanType;
        OldPriceAmount = oldPriceAmount;
        NewPriceAmount = newPriceAmount;
        Currency = currency;
        ChangedAt = DateTime.UtcNow;
    }
}


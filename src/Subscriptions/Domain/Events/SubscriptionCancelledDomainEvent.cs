namespace Energix.Subscriptions.Domain.Events;

/// <summary>
/// Evento de dominio que se dispara cuando se cancela una suscripción
/// </summary>
public class SubscriptionCancelledDomainEvent
{
    public Guid SubscriptionId { get; set; }
    public Guid UserId { get; set; }
    public string PlanType { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
    public DateTime? EndDate { get; set; }

    public SubscriptionCancelledDomainEvent(
        Guid subscriptionId,
        Guid userId,
        string planType,
        DateTime? endDate)
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        PlanType = planType;
        EndDate = endDate;
        CancelledAt = DateTime.UtcNow;
    }
}


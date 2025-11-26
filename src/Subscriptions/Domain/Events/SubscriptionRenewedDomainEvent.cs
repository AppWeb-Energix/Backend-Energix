﻿using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Domain.Events;

/// <summary>
/// Evento de dominio que se dispara cuando se renueva una suscripción
/// </summary>
public class SubscriptionRenewedDomainEvent
{
    public Guid SubscriptionId { get; set; }
    public int UserId { get; set; }
    public PlanType PlanType { get; set; }
    public BillingPeriod BillingPeriod { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime RenewedAt { get; set; }
    public DateTime NextBillingDate { get; set; }

    public SubscriptionRenewedDomainEvent(
        Guid subscriptionId,
        int userId,
        PlanType planType,
        BillingPeriod billingPeriod,
        decimal priceAmount,
        string currency,
        DateTime nextBillingDate)
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        PlanType = planType;
        BillingPeriod = billingPeriod;
        PriceAmount = priceAmount;
        Currency = currency;
        NextBillingDate = nextBillingDate;
        RenewedAt = DateTime.UtcNow;
    }
}


using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Application.Commands.ChangeBillingPeriod;

/// <summary>
/// Comando para cambiar el periodo de facturación de una suscripción
/// </summary>
public class ChangeBillingPeriodCommand
{
    public Guid UserId { get; set; }
    public BillingPeriod NewBillingPeriod { get; set; }
}


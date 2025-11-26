using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Application.Commands.ChangeBillingPeriod;

/// <summary>
/// Comando para cambiar el periodo de facturaci�n de una suscripci�n
/// </summary>
public class ChangeBillingPeriodCommand
{
    public int UserId { get; set; }
    public BillingPeriod NewBillingPeriod { get; set; }
}

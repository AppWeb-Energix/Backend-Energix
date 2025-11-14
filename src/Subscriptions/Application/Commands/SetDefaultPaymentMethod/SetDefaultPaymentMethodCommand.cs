namespace Energix.Subscriptions.Application.Commands.SetDefaultPaymentMethod;

public class SetDefaultPaymentMethodCommand
{
    public Guid UserId { get; set; }
    public Guid PaymentMethodId { get; set; }
}



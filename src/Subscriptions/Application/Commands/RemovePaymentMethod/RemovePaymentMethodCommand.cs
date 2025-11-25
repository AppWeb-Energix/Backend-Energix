namespace Energix.Subscriptions.Application.Commands.RemovePaymentMethod;

public class RemovePaymentMethodCommand
{
    public Guid UserId { get; set; }
    public Guid PaymentMethodId { get; set; }
}


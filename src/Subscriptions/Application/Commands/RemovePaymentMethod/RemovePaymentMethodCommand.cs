namespace Energix.Subscriptions.Application.Commands.RemovePaymentMethod;

public class RemovePaymentMethodCommand
{
    public int UserId { get; set; }
    public Guid PaymentMethodId { get; set; }
}


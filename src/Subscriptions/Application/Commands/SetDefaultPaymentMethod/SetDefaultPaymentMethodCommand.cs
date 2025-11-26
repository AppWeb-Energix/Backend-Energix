namespace Energix.Subscriptions.Application.Commands.SetDefaultPaymentMethod;

public class SetDefaultPaymentMethodCommand
{
    public int UserId { get; set; }
    public Guid PaymentMethodId { get; set; }
}



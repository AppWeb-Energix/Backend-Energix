namespace Energix.Subscriptions.Application.Commands.AddPaymentMethods;

public class AddPaymentMethodCommand
{
    public Guid UserId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Cvv { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty;
    public bool SetAsDefault { get; set; }  
    
    
    
    
    
}



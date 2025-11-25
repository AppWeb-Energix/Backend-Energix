namespace Energix.Subscriptions.Application.DTOs;

public class PaymentMethodDto
{
    public Guid Id { get; set; }
    public string MaskedCardNumber { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public DateTime CreatedAt { get; set; }
}



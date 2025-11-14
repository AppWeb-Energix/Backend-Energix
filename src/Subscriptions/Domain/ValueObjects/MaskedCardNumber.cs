namespace Energix.Subscriptions.Domain.ValueObjects;
public class MaskedCardNumber
{
    public string Value { get; private set; }
    private MaskedCardNumber(string value)
    {
        Value = value;
    }
    public static MaskedCardNumber Create(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            throw new ArgumentException("El número de tarjeta no puede estar vacío");
        var last4 = cardNumber.Length >= 4 ? cardNumber.Substring(cardNumber.Length - 4) : cardNumber;
        return new MaskedCardNumber($"****{last4}");
    }
    public override bool Equals(object? obj)
    {
        if (obj is MaskedCardNumber other)
            return Value == other.Value;
        return false;
    }
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}

namespace Energix.Subscriptions.Domain.ValueObjects;

public class Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("El monto no puede ser negativo");
        
        Amount = amount;
        Currency = currency ?? "USD";
    }

    public static Money Create(decimal amount, string currency = "USD")
    {
        return new Money(amount, currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}


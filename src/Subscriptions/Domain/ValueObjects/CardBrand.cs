namespace Energix.Subscriptions.Domain.ValueObjects;

public class CardBrand
{
    public string Name { get; private set; }

    private CardBrand(string name)
    {
        Name = name;
    }

    public static CardBrand Visa => new CardBrand("Visa");
    public static CardBrand Mastercard => new CardBrand("Mastercard");
    public static CardBrand AmericanExpress => new CardBrand("AmericanExpress");
    public static CardBrand Discover => new CardBrand("Discover");

    public static CardBrand FromName(string name)
    {
        return name switch
        {
            "Visa" => Visa,
            "Mastercard" => Mastercard,
            "AmericanExpress" => AmericanExpress,
            "Discover" => Discover,
            _ => throw new ArgumentException($"Marca de tarjeta no válida: {name}")
        };
    }

    public override string ToString() => Name;
}



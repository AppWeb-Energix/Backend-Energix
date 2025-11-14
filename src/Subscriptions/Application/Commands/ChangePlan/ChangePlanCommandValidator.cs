namespace Energix.Subscriptions.Application.Commands.ChangePlan;

public class ChangePlanCommandValidator
{
    private static readonly string[] ValidPlans = { "Free", "Basic", "Premium", "Enterprise" };

    public List<string> Validate(ChangePlanCommand command)
    {
        var errors = new List<string>();

        if (command.UserId == Guid.Empty)
            errors.Add("El ID de usuario es requerido");

        if (string.IsNullOrWhiteSpace(command.NewPlanType))
            errors.Add("El tipo de plan es requerido");
        else if (!ValidPlans.Contains(command.NewPlanType))
            errors.Add($"Plan inválido. Planes válidos: {string.Join(", ", ValidPlans)}");

        if (command.NewPriceAmount < 0)
            errors.Add("El precio no puede ser negativo");

        if (string.IsNullOrWhiteSpace(command.NewPriceCurrency))
            errors.Add("La moneda es requerida");
        else if (command.NewPriceCurrency.Length != 3)
            errors.Add("La moneda debe ser un código ISO de 3 letras (ej: USD, EUR, MXN)");

        return errors;
    }
}


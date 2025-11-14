using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Application.Commands.ChangeBillingPeriod;

public class ChangeBillingPeriodCommandValidator
{
    public List<string> Validate(ChangeBillingPeriodCommand command)
    {
        var errors = new List<string>();

        if (command.UserId == Guid.Empty)
            errors.Add("El ID de usuario es requerido");

        if (!Enum.IsDefined(typeof(BillingPeriod), command.NewBillingPeriod))
            errors.Add($"Periodo de facturación inválido. Valores válidos: {string.Join(", ", Enum.GetNames(typeof(BillingPeriod)))}");

        return errors;
    }
}


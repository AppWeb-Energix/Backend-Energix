using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Application.Commands.ChangeBillingPeriod;

public class ChangeBillingPeriodCommandValidator
{
    public List<string> Validate(ChangeBillingPeriodCommand command)
    {
        var errors = new List<string>();

        if (command.UserId <= 0)
            errors.Add("El ID de usuario es requerido");

        if (!Enum.IsDefined(typeof(BillingPeriod), command.NewBillingPeriod))
            errors.Add($"Periodo de facturaci�n inv�lido. Valores v�lidos: {string.Join(", ", Enum.GetNames(typeof(BillingPeriod)))}");

        return errors;
    }
}


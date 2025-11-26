using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Application.Commands.Subscribe;

public class SubscribeCommandValidator
{
    public List<string> Validate(SubscribeCommand command)
    {
        var errors = new List<string>();

        if (command.UserId <= 0)
            errors.Add("El ID de usuario es requerido");

        if (!Enum.IsDefined(typeof(PlanType), command.PlanType))
            errors.Add($"Plan inv�lido. Planes v�lidos: {string.Join(", ", Enum.GetNames(typeof(PlanType)))}");

        if (!Enum.IsDefined(typeof(BillingPeriod), command.BillingPeriod))
            errors.Add($"Periodo de facturaci�n inv�lido. Valores v�lidos: {string.Join(", ", Enum.GetNames(typeof(BillingPeriod)))}");

        return errors;
    }
}


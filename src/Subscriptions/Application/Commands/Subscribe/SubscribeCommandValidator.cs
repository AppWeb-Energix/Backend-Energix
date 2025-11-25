using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Application.Commands.Subscribe;

public class SubscribeCommandValidator
{
    public List<string> Validate(SubscribeCommand command)
    {
        var errors = new List<string>();

        if (command.UserId == Guid.Empty)
            errors.Add("El ID de usuario es requerido");

        if (!Enum.IsDefined(typeof(PlanType), command.PlanType))
            errors.Add($"Plan inválido. Planes válidos: {string.Join(", ", Enum.GetNames(typeof(PlanType)))}");

        if (!Enum.IsDefined(typeof(BillingPeriod), command.BillingPeriod))
            errors.Add($"Periodo de facturación inválido. Valores válidos: {string.Join(", ", Enum.GetNames(typeof(BillingPeriod)))}");

        return errors;
    }
}


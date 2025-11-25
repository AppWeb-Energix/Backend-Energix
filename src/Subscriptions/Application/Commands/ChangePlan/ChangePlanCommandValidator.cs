using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Application.Commands.ChangePlan;

public class ChangePlanCommandValidator
{
    public List<string> Validate(ChangePlanCommand command)
    {
        var errors = new List<string>();

        if (command.UserId == Guid.Empty)
            errors.Add("El ID de usuario es requerido");

        if (!Enum.IsDefined(typeof(PlanType), command.NewPlanType))
            errors.Add($"Plan inválido. Planes válidos: {string.Join(", ", Enum.GetNames(typeof(PlanType)))}");

        if (command.NewBillingPeriod.HasValue && 
            !Enum.IsDefined(typeof(BillingPeriod), command.NewBillingPeriod.Value))
            errors.Add($"Periodo de facturación inválido. Valores válidos: {string.Join(", ", Enum.GetNames(typeof(BillingPeriod)))}");

        return errors;
    }
}


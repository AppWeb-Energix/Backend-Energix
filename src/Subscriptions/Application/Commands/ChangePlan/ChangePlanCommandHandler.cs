using Energix.Subscriptions.Domain.Schemas;
using Energix.API;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Commands.ChangePlan;

public class ChangePlanCommandHandler
{
    private readonly AppDbContext _context;
    private readonly ChangePlanCommandValidator _validator;

    public ChangePlanCommandHandler(AppDbContext context)
    {
        _context = context;
        _validator = new ChangePlanCommandValidator();
    }

    public async Task<(bool Success, string Message, List<string> Errors)> HandleAsync(
        ChangePlanCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validar el comando
        var errors = _validator.Validate(command);
        if (errors.Any())
        {
            return (false, "Validación fallida", errors);
        }

        try
        {
            // Buscar la suscripción del usuario
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == command.UserId && s.IsActive, cancellationToken);

            if (subscription == null)
            {
                return (false, "No se encontró suscripción activa", new List<string> 
                { 
                    $"No existe una suscripción activa para el usuario {command.UserId}" 
                });
            }

            // Guardar el plan anterior para el mensaje
            var oldPlanType = subscription.PlanType;
            var oldBillingPeriod = subscription.BillingPeriod;
            var isUpgrade = subscription.IsUpgrade(command.NewPlanType);
            var isDowngrade = subscription.IsDowngrade(command.NewPlanType);

            // Cambiar el plan (esto valida internamente y actualiza el precio según el schema)
            subscription.ChangePlan(command.NewPlanType, command.NewBillingPeriod);

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            var changeType = isUpgrade ? "mejorado" : isDowngrade ? "reducido" : "cambiado";
            var billingInfo = command.NewBillingPeriod.HasValue && command.NewBillingPeriod.Value != oldBillingPeriod
                ? $" y periodo de facturación a {command.NewBillingPeriod.Value}"
                : string.Empty;

            return (true, 
                $"Plan {changeType} exitosamente de {oldPlanType} a {command.NewPlanType}{billingInfo}", 
                new List<string>());
        }
        catch (InvalidOperationException ex)
        {
            return (false, "Operación inválida", new List<string> { ex.Message });
        }
        catch (Exception ex)
        {
            return (false, "Error al procesar", new List<string> { $"Error: {ex.Message}" });
        }
    }
}


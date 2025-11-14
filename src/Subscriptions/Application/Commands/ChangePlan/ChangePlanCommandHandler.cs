using Energix.Subscriptions.Domain.ValueObjects;
using Energix.Subscriptions.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Commands.ChangePlan;

public class ChangePlanCommandHandler
{
    private readonly SubscriptionDbContext _context;
    private readonly ChangePlanCommandValidator _validator;

    public ChangePlanCommandHandler(SubscriptionDbContext context)
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

            // Verificar si es el mismo plan
            if (subscription.PlanType == command.NewPlanType)
            {
                return (false, "Plan sin cambios", new List<string> 
                { 
                    $"El usuario ya tiene el plan {command.NewPlanType}" 
                });
            }

            // Guardar el plan anterior para el evento
            var oldPlanType = subscription.PlanType;
            var oldPrice = subscription.Price;

            // Crear el nuevo precio
            var newPrice = Money.Create(command.NewPriceAmount, command.NewPriceCurrency);

            // Cambiar el plan
            subscription.ChangePlan(command.NewPlanType, newPrice);

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            return (true, $"Plan cambiado exitosamente de {oldPlanType} a {command.NewPlanType}", new List<string>());
        }
        catch (Exception ex)
        {
            return (false, "Error al procesar", new List<string> { $"Error: {ex.Message}" });
        }
    }
}


using Energix.Subscriptions.Domain.Aggregates;
using Energix.API;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Commands.Subscribe;

public class SubscribeCommandHandler
{
    private readonly AppDbContext _context;
    private readonly SubscribeCommandValidator _validator;

    public SubscribeCommandHandler(AppDbContext context)
    {
        _context = context;
        _validator = new SubscribeCommandValidator();
    }

    public async Task<(bool Success, string Message, List<string> Errors, Guid? SubscriptionId)> HandleAsync(
        SubscribeCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validar el comando
        var errors = _validator.Validate(command);
        if (errors.Any())
        {
            return (false, "Validación fallida", errors, null);
        }

        try
        {
            // Verificar si ya tiene una suscripción activa
            var existingSubscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == command.UserId && s.IsActive, cancellationToken);

            if (existingSubscription != null)
            {
                return (false, "Ya existe una suscripción activa", 
                    new List<string> { $"El usuario ya tiene una suscripción activa del plan {existingSubscription.PlanType}" },
                    null);
            }

            // Crear la nueva suscripción
            var subscription = Subscription.Create(
                command.UserId,
                command.PlanType,
                command.BillingPeriod
            );

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync(cancellationToken);

            return (true, 
                $"Suscripción al plan {command.PlanType} creada exitosamente", 
                new List<string>(),
                subscription.Id);
        }
        catch (Exception ex)
        {
            return (false, "Error al procesar", new List<string> { $"Error: {ex.Message}" }, null);
        }
    }
}


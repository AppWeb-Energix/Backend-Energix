using Energix.API;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Commands.ChangeBillingPeriod;

public class ChangeBillingPeriodCommandHandler
{
    private readonly AppDbContext _context;
    private readonly ChangeBillingPeriodCommandValidator _validator;

    public ChangeBillingPeriodCommandHandler(AppDbContext context)
    {
        _context = context;
        _validator = new ChangeBillingPeriodCommandValidator();
    }

    public async Task<(bool Success, string Message, List<string> Errors)> HandleAsync(
        ChangeBillingPeriodCommand command,
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

            var oldBillingPeriod = subscription.BillingPeriod;

            // Cambiar el periodo de facturación
            subscription.ChangeBillingPeriod(command.NewBillingPeriod);

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            return (true, 
                $"Periodo de facturación cambiado exitosamente de {oldBillingPeriod} a {command.NewBillingPeriod}", 
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


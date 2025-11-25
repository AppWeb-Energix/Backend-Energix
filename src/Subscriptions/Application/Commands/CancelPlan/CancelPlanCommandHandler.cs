using Energix.API;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Commands.CancelPlan;

public class CancelPlanCommandHandler
{
    private readonly AppDbContext _context;

    public CancelPlanCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, List<string> Errors)> HandleAsync(
        CancelPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        // Validaciones
        if (command.UserId == Guid.Empty)
            errors.Add("El ID de usuario es requerido");

        if (errors.Any())
            return (false, "Validación fallida", errors);

        try
        {
            // Buscar la suscripción activa
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == command.UserId && s.IsActive, cancellationToken);

            if (subscription == null)
            {
                return (false, "Suscripción no encontrada", new List<string>
                {
                    $"No se encontró suscripción activa para el usuario {command.UserId}"
                });
            }

            // Cancelar la suscripción
            subscription.Cancel();

            await _context.SaveChangesAsync(cancellationToken);

            return (true, 
                "Suscripción cancelada exitosamente. El plan permanecerá activo hasta el final del período de facturación.", 
                new List<string>());
        }
        catch (Exception ex)
        {
            return (false, "Error al procesar", new List<string> { $"Error: {ex.Message}" });
        }
    }
}


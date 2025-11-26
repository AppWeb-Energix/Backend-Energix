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
        if (command.UserId <= 0)
            errors.Add("El ID de usuario es requerido");

        if (errors.Any())
            return (false, "Validaci�n fallida", errors);

        try
        {
            // Buscar la suscripci�n activa
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == command.UserId && s.IsActive, cancellationToken);

            if (subscription == null)
            {
                return (false, "Suscripci�n no encontrada", new List<string>
                {
                    $"No se encontr� suscripci�n activa para el usuario {command.UserId}"
                });
            }

            // Cancelar la suscripci�n
            subscription.Cancel();

            await _context.SaveChangesAsync(cancellationToken);

            return (true, 
                "Suscripci�n cancelada exitosamente. El plan permanecer� activo hasta el final del per�odo de facturaci�n.", 
                new List<string>());
        }
        catch (Exception ex)
        {
            return (false, "Error al procesar", new List<string> { $"Error: {ex.Message}" });
        }
    }
}


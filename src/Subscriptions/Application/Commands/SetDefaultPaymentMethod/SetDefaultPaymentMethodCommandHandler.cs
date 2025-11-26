using Energix.API;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Commands.SetDefaultPaymentMethod;

public class SetDefaultPaymentMethodCommandHandler
{
    private readonly AppDbContext _context;

    public SetDefaultPaymentMethodCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, List<string> Errors)> HandleAsync(
        SetDefaultPaymentMethodCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        // Validaciones
        if (command.UserId <= 0)
            errors.Add("El ID de usuario es requerido");

        if (command.PaymentMethodId == Guid.Empty)
            errors.Add("El ID del m�todo de pago es requerido");

        if (errors.Any())
            return (false, "Validaci�n fallida", errors);

        try
        {
            // Buscar la suscripci�n con sus m�todos de pago
            var subscription = await _context.Subscriptions
                .Include(s => s.PaymentMethods)
                .FirstOrDefaultAsync(s => s.UserId == command.UserId && s.IsActive, cancellationToken);

            if (subscription == null)
            {
                return (false, "Suscripci�n no encontrada", new List<string>
                {
                    $"No se encontr� suscripci�n activa para el usuario {command.UserId}"
                });
            }

            // Buscar el método de pago específico
            var paymentMethod = subscription.PaymentMethods
                .FirstOrDefault(pm => pm.Id == command.PaymentMethodId);

            if (paymentMethod == null)
            {
                return (false, "Método de pago no encontrado", new List<string>
                {
                    $"No se encontró el método de pago {command.PaymentMethodId} en la suscripción del usuario {command.UserId}"
                });
            }

            // Verificar si est� inactivo
            if (!paymentMethod.IsActive)
            {
                return (false, "M�todo de pago inactivo", new List<string>
                {
                    "No se puede establecer como predeterminado un m�todo de pago inactivo"
                });
            }

            // Verificar si ya es el predeterminado
            if (paymentMethod.IsDefault)
            {
                return (false, "Ya es el m�todo predeterminado", new List<string>
                {
                    "Este m�todo de pago ya es el predeterminado"
                });
            }

            // Desactivar el m�todo predeterminado anterior
            var currentDefault = subscription.PaymentMethods
                .FirstOrDefault(pm => pm.IsDefault && pm.IsActive);

            if (currentDefault != null)
            {
                currentDefault.SetAsNonDefault();
            }

            // Establecer el nuevo m�todo como predeterminado
            paymentMethod.SetAsDefault();

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            return (true, "M�todo de pago establecido como predeterminado exitosamente", new List<string>());
        }
        catch (Exception ex)
        {
            return (false, "Error al procesar", new List<string> { $"Error: {ex.Message}" });
        }
    }
}


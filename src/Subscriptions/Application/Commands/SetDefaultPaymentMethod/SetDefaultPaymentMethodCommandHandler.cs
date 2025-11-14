using Energix.Subscriptions.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Commands.SetDefaultPaymentMethod;

public class SetDefaultPaymentMethodCommandHandler
{
    private readonly SubscriptionDbContext _context;

    public SetDefaultPaymentMethodCommandHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, List<string> Errors)> HandleAsync(
        SetDefaultPaymentMethodCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        // Validaciones
        if (command.UserId == Guid.Empty)
            errors.Add("El ID de usuario es requerido");

        if (command.PaymentMethodId == Guid.Empty)
            errors.Add("El ID del método de pago es requerido");

        if (errors.Any())
            return (false, "Validación fallida", errors);

        try
        {
            // Buscar la suscripción con sus métodos de pago
            var subscription = await _context.Subscriptions
                .Include(s => s.PaymentMethods)
                .FirstOrDefaultAsync(s => s.UserId == command.UserId && s.IsActive, cancellationToken);

            if (subscription == null)
            {
                return (false, "Suscripción no encontrada", new List<string>
                {
                    $"No se encontró suscripción activa para el usuario {command.UserId}"
                });
            }

            // Buscar el método de pago específico
            var paymentMethod = subscription.PaymentMethods
                .FirstOrDefault(pm => pm.Id == command.PaymentMethodId && pm.UserId == command.UserId);

            if (paymentMethod == null)
            {
                return (false, "Método de pago no encontrado", new List<string>
                {
                    $"No se encontró el método de pago {command.PaymentMethodId} para el usuario {command.UserId}"
                });
            }

            // Verificar si está inactivo
            if (!paymentMethod.IsActive)
            {
                return (false, "Método de pago inactivo", new List<string>
                {
                    "No se puede establecer como predeterminado un método de pago inactivo"
                });
            }

            // Verificar si ya es el predeterminado
            if (paymentMethod.IsDefault)
            {
                return (false, "Ya es el método predeterminado", new List<string>
                {
                    "Este método de pago ya es el predeterminado"
                });
            }

            // Desactivar el método predeterminado anterior
            var currentDefault = subscription.PaymentMethods
                .FirstOrDefault(pm => pm.IsDefault && pm.IsActive);

            if (currentDefault != null)
            {
                currentDefault.SetAsNonDefault();
            }

            // Establecer el nuevo método como predeterminado
            paymentMethod.SetAsDefault();

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            return (true, "Método de pago establecido como predeterminado exitosamente", new List<string>());
        }
        catch (Exception ex)
        {
            return (false, "Error al procesar", new List<string> { $"Error: {ex.Message}" });
        }
    }
}


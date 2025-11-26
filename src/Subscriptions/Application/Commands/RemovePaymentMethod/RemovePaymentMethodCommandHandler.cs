using Energix.API;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Commands.RemovePaymentMethod;

public class RemovePaymentMethodCommandHandler
{
    private readonly AppDbContext _context;

    public RemovePaymentMethodCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, List<string> Errors)> HandleAsync(
        RemovePaymentMethodCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        // Validaciones
        if (command.UserId <= 0)
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

            // Verificar si ya está inactivo
            if (!paymentMethod.IsActive)
            {
                return (false, "Método de pago ya está inactivo", new List<string>
                {
                    "El método de pago ya fue eliminado anteriormente"
                });
            }

            // Verificar si es el último método activo
            var activeMethodsCount = subscription.PaymentMethods.Count(pm => pm.IsActive);
            if (activeMethodsCount == 1)
            {
                return (false, "No se puede eliminar el único método de pago", new List<string>
                {
                    "Debe tener al menos un método de pago activo. Agregue otro antes de eliminar este."
                });
            }

            // Guardar si era el método predeterminado
            var wasDefault = paymentMethod.IsDefault;

            // Desactivar el método de pago
            subscription.RemovePaymentMethod(command.PaymentMethodId);

            // Si era el predeterminado, asignar otro método activo como predeterminado
            if (wasDefault)
            {
                var newDefaultMethod = subscription.PaymentMethods
                    .FirstOrDefault(pm => pm.IsActive && pm.Id != command.PaymentMethodId);

                if (newDefaultMethod != null)
                {
                    newDefaultMethod.SetAsDefault();
                }
            }

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            var message = wasDefault
                ? "Método de pago eliminado. Se asignó un nuevo método predeterminado automáticamente."
                : "Método de pago eliminado exitosamente";

            return (true, message, new List<string>());
        }
        catch (Exception ex)
        {
            return (false, "Error al procesar", new List<string> { $"Error: {ex.Message}" });
        }
    }
}


using Energix.Subscriptions.Infrastructure.Persistance;
using Energix.Subscriptions.Infrastructure.PaymentGateway;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Commands.RenewPlan;

public class RenewPlanCommandHandler
{
    private readonly SubscriptionDbContext _context;
    private readonly IPaymentGateway _paymentGateway;

    public RenewPlanCommandHandler(SubscriptionDbContext context, IPaymentGateway paymentGateway)
    {
        _context = context;
        _paymentGateway = paymentGateway;
    }

    public async Task<(bool Success, string Message, List<string> Errors)> HandleAsync(
        RenewPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        // Validaciones
        if (command.UserId == Guid.Empty)
            errors.Add("El ID de usuario es requerido");

        if (string.IsNullOrWhiteSpace(command.PlanType))
            errors.Add("El tipo de plan es requerido");

        if (command.RenewalMonths <= 0)
            errors.Add("Los meses de renovación deben ser mayor a 0");

        if (errors.Any())
            return (false, "Validación fallida", errors);

        try
        {
            // Buscar la suscripción con métodos de pago
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

            // Verificar método de pago predeterminado
            var defaultPaymentMethod = subscription.PaymentMethods
                .FirstOrDefault(pm => pm.IsDefault && pm.IsActive);

            if (defaultPaymentMethod == null)
            {
                return (false, "No hay método de pago configurado", new List<string>
                {
                    "Debe agregar un método de pago antes de renovar el plan"
                });
            }

            // Verificar que la tarjeta no esté expirada
            if (defaultPaymentMethod.IsExpired())
            {
                return (false, "Tarjeta expirada", new List<string>
                {
                    "El método de pago predeterminado ha expirado. Actualice su información de pago."
                });
            }

            // Calcular el monto total (precio * meses)
            var totalAmount = subscription.Price.Amount * command.RenewalMonths;

            // Procesar el pago
            var paymentRequest = new PaymentRequest
            {
                UserId = command.UserId,
                Amount = totalAmount,
                Currency = subscription.Price.Currency,
                CardNumber = "****" + defaultPaymentMethod.MaskedCardNumber.Value.Substring(4),
                ExpiryMonth = defaultPaymentMethod.ExpiryDate.Month,
                ExpiryYear = defaultPaymentMethod.ExpiryDate.Year,
                Cvv = "***",
                CardHolderName = defaultPaymentMethod.CardHolderName,
                Description = $"Renovación {command.PlanType} - {command.RenewalMonths} mes(es)"
            };

            var paymentResult = await _paymentGateway.ProcessPaymentAsync(paymentRequest);

            if (!paymentResult.IsSuccess)
            {
                return (false, "Error al procesar el pago", new List<string>
                {
                    paymentResult.Message
                });
            }

            // Extender la fecha de fin de la suscripción
            var currentEndDate = subscription.EndDate ?? DateTime.UtcNow;
            var newEndDate = (currentEndDate > DateTime.UtcNow ? currentEndDate : DateTime.UtcNow)
                .AddMonths(command.RenewalMonths);

            // Actualizar la suscripción
            subscription.RenewSubscription(newEndDate);
            
            await _context.SaveChangesAsync(cancellationToken);

            return (true, 
                $"Plan renovado exitosamente por {command.RenewalMonths} mes(es). TransactionId: {paymentResult.TransactionId}", 
                new List<string>());
        }
        catch (Exception ex)
        {
            return (false, "Error al procesar", new List<string> { $"Error: {ex.Message}" });
        }
    }
}


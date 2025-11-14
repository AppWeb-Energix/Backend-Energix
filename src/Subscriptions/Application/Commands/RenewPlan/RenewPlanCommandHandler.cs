﻿using Energix.Subscriptions.Infrastructure.Persistance;
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

        if (errors.Any())
            return (false, "Validación fallida", errors);

        try
        {
            // Buscar la suscripción con métodos de pago
            var subscription = await _context.Subscriptions
                .Include(s => s.PaymentMethods)
                .FirstOrDefaultAsync(s => s.UserId == command.UserId, cancellationToken);

            if (subscription == null)
            {
                return (false, "Suscripción no encontrada", new List<string>
                {
                    $"No se encontró suscripción para el usuario {command.UserId}"
                });
            }

            if (subscription.IsActive && subscription.AutoRenew)
            {
                return (false, "Suscripción ya activa", new List<string>
                {
                    "La suscripción ya está activa y configurada para renovación automática"
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

            // Procesar el pago según el precio actual
            var paymentRequest = new PaymentRequest
            {
                UserId = command.UserId,
                Amount = subscription.Price.Amount,
                Currency = subscription.Price.Currency,
                CardNumber = "****" + defaultPaymentMethod.MaskedCardNumber.Value.Substring(4),
                ExpiryMonth = defaultPaymentMethod.ExpiryDate.Month,
                ExpiryYear = defaultPaymentMethod.ExpiryDate.Year,
                Cvv = "***",
                CardHolderName = defaultPaymentMethod.CardHolderName,
                Description = $"Renovación {subscription.PlanType} - {subscription.BillingPeriod}"
            };

            var paymentResult = await _paymentGateway.ProcessPaymentAsync(paymentRequest);

            if (!paymentResult.IsSuccess)
            {
                return (false, "Error al procesar el pago", new List<string>
                {
                    paymentResult.Message
                });
            }

            // Renovar la suscripción (calcula automáticamente el siguiente periodo)
            subscription.RenewSubscription();
            subscription.SetAutoRenew(true);
            
            await _context.SaveChangesAsync(cancellationToken);

            return (true, 
                $"Plan {subscription.PlanType} renovado exitosamente ({subscription.BillingPeriod}). TransactionId: {paymentResult.TransactionId}", 
                new List<string>());
        }
        catch (Exception ex)
        {
            return (false, "Error al procesar", new List<string> { $"Error: {ex.Message}" });
        }
    }
}


using Energix.Subscriptions.Domain.Aggregates;
using Energix.Subscriptions.Domain.ValueObjects;
using Energix.API;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Commands.AddPaymentMethods;

public class AddPaymentMethodCommandHandler
{
    private readonly AppDbContext _context;
    private readonly AddPaymentMethodCommandValidator _validator;

    public AddPaymentMethodCommandHandler(AppDbContext context)
    {
        _context = context;
        _validator = new AddPaymentMethodCommandValidator();
    }

    public async Task<(bool Success, Guid? PaymentMethodId, List<string> Errors)> HandleAsync(
        AddPaymentMethodCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = _validator.Validate(command);
        if (errors.Any())
        {
            return (false, null, errors);
        }

        try
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.PaymentMethods)
                .FirstOrDefaultAsync(s => s.UserId == command.UserId && s.IsActive, cancellationToken);

            if (subscription == null)
            {
                return (false, null, new List<string> 
                { 
                    $"No se encontró suscripción activa para el usuario {command.UserId}" 
                });
            }

            var maskedNumber = MaskedCardNumber.Create(command.CardNumber);

            var existingMethod = subscription.PaymentMethods
                .FirstOrDefault(pm => pm.MaskedCardNumber.Equals(maskedNumber) && pm.IsActive);

            if (existingMethod != null)
            {
                return (false, null, new List<string> 
                { 
                    "Ya existe un método de pago con esta tarjeta" 
                });
            }

            if (command.SetAsDefault)
            {
                foreach (var pm in subscription.PaymentMethods.Where(p => p.IsDefault && p.IsActive))
                {
                    pm.SetAsNonDefault();
                }
            }

            var cardBrand = CardBrand.FromName(command.CardBrand);
            var expiryDate = new DateTime(
                command.ExpiryYear, 
                command.ExpiryMonth,
                DateTime.DaysInMonth(command.ExpiryYear, command.ExpiryMonth)
            );

            var isDefault = command.SetAsDefault || 
                           !subscription.PaymentMethods.Any(pm => pm.IsActive);

            var paymentMethod = PaymentMethod.Create(
                userId: command.UserId,
                maskedCardNumber: maskedNumber,
                cardBrand: cardBrand,
                cardHolderName: command.CardHolderName,
                expiryDate: expiryDate,
                isDefault: isDefault
            );

            subscription.AddPaymentMethod(paymentMethod);

            await _context.SaveChangesAsync(cancellationToken);

            return (true, paymentMethod.Id, new List<string>());
        }
        catch (Exception ex)
        {
            return (false, null, new List<string> { $"Error al procesar: {ex.Message}" });
        }
    }
}


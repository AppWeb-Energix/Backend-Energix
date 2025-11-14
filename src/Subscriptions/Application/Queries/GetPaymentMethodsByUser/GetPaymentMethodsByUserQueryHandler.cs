using Energix.Subscriptions.Application.DTOs;
using Energix.Subscriptions.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Queries.GetPaymentMethodsByUser;

public class GetPaymentMethodsByUserQueryHandler
{
    private readonly SubscriptionDbContext _context;

    public GetPaymentMethodsByUserQueryHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<List<PaymentMethodDto>> HandleAsync(
        GetPaymentMethodsByUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var paymentMethodsQuery = _context.PaymentMethods
            .Where(pm => pm.UserId == query.UserId);

        if (!query.IncludeInactive)
        {
            paymentMethodsQuery = paymentMethodsQuery.Where(pm => pm.IsActive);
        }

        var paymentMethods = await paymentMethodsQuery
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.CreatedAt)
            .ToListAsync(cancellationToken);

        return paymentMethods.Select(pm => new PaymentMethodDto
        {
            Id = pm.Id,
            MaskedCardNumber = pm.MaskedCardNumber.Value,
            CardBrand = pm.CardBrand.Name,
            CardHolderName = pm.CardHolderName,
            ExpiryDate = pm.ExpiryDate,
            IsDefault = pm.IsDefault,
            IsActive = pm.IsActive,
            IsExpired = pm.IsExpired(),
            CreatedAt = pm.CreatedAt
        }).ToList();
    }
}


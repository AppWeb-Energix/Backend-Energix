namespace Energix.Subscriptions.Application.Queries.GetPaymentMethodsByUser;

public class GetPaymentMethodsByUserQuery
{
    public Guid UserId { get; set; }
    public bool IncludeInactive { get; set; } = false;
}


namespace Energix.Subscriptions.Application.Queries.GetPaymentMethodsByUser;

public class GetPaymentMethodsByUserQuery
{
    public int UserId { get; set; }
    public bool IncludeInactive { get; set; } = false;
}


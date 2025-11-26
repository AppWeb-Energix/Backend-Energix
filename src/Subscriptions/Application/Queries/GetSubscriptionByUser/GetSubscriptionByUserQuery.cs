namespace Energix.Subscriptions.Application.Queries.GetSubscriptionByUser;

public class GetSubscriptionByUserQuery
{
    public int UserId { get; set; }
    public bool IncludePaymentMethods { get; set; } = true;
}


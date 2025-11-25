using Energix.Subscriptions.Domain.Enums;
namespace Energix.Subscriptions.Application.Commands.Subscribe;
public class SubscribeCommand
{
    public Guid UserId { get; set; }
    public PlanType PlanType { get; set; }
    public BillingPeriod BillingPeriod { get; set; } = BillingPeriod.Monthly;
}

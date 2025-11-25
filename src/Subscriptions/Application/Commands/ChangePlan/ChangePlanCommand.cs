using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Application.Commands.ChangePlan;

public class ChangePlanCommand
{
    public Guid UserId { get; set; }
    public PlanType NewPlanType { get; set; }
    public BillingPeriod? NewBillingPeriod { get; set; }
}



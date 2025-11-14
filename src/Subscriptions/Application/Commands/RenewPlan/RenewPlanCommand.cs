namespace Energix.Subscriptions.Application.Commands.RenewPlan;

public class RenewPlanCommand
{
    public Guid UserId { get; set; }
    public string PlanType { get; set; } = string.Empty;
    public int RenewalMonths { get; set; } = 1;
}



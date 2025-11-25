namespace Energix.Subscriptions.Application.Commands.CancelPlan;

public class CancelPlanCommand
{
    public Guid UserId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
}


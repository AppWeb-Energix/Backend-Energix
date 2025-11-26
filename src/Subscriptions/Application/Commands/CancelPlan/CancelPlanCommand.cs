namespace Energix.Subscriptions.Application.Commands.CancelPlan;

public class CancelPlanCommand
{
    public int UserId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
}


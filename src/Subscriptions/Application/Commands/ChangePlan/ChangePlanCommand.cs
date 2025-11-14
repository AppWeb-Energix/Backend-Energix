namespace Energix.Subscriptions.Application.Commands.ChangePlan;

public class ChangePlanCommand
{
    public Guid UserId { get; set; }
    public string NewPlanType { get; set; } = string.Empty;
    public decimal NewPriceAmount { get; set; }
    public string NewPriceCurrency { get; set; } = "USD";
}



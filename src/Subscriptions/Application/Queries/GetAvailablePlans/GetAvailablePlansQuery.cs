namespace Energix.Subscriptions.Application.Queries.GetAvailablePlans;

public class GetAvailablePlansQuery
{
    public bool IncludePricing { get; set; } = true;
    public bool IncludeFeatures { get; set; } = true;
}

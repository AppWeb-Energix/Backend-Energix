using Energix.Subscriptions.Domain.Enums;

namespace Energix.Subscriptions.Application.Queries.GetPlanComparison;

/// <summary>
/// Query para comparar planes de suscripción
/// </summary>
public class GetPlanComparisonQuery
{
    public PlanType? CurrentPlan { get; set; }
    public PlanType? TargetPlan { get; set; }
}


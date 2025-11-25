namespace Energix.Subscriptions.Application.DTOs;

/// <summary>
/// DTO que representa una característica de un plan
/// </summary>
public class PlanFeatureDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}


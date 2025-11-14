namespace Energix.API.Personalization.Domain.Model.Aggregates;

/// <summary>
/// Aggregate root for user personalization settings
/// </summary>
public class PersonalizationAggregate
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    // KPI Settings
    public bool KpiCurrent { get; set; } = true;
    public bool KpiCost { get; set; } = true;
    public bool KpiMonthly { get; set; } = true;
    
    // Chart Settings
    public bool ChartHourly { get; set; } = true;
    public bool ChartMonthly { get; set; } = true;
    public bool ChartDevice { get; set; } = true;
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
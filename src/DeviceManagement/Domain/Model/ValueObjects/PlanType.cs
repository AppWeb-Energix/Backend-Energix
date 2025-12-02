namespace Energix.API.DeviceManagement.Domain.Model.ValueObjects;

/// <summary>
/// User plan type
/// </summary>
public enum PlanType
{
    /// <summary>
    /// Basic Plan - 2 manual devices
    /// </summary>
    Basic = 0,
    
    /// <summary>
    /// Student Plan - 2 devices with plug
    /// </summary>
    Student = 1,
    
    /// <summary>
    /// Family Plan - Unlimited devices with sensor and zones
    /// </summary>
    Family = 2
}

public static class PlanTypeExtensions
{
    /// <summary>
    /// Convert PlanType to string for JSON
    /// </summary>
    public static string ToLowerString(this PlanType plan)
    {
        return plan switch
        {
            PlanType.Basic => "basic",
            PlanType.Student => "student",
            PlanType.Family => "family",
            _ => throw new ArgumentOutOfRangeException(nameof(plan))
        };
    }

    /// <summary>
    /// Convert string to PlanType
    /// </summary>
    public static PlanType ParsePlanType(string value)
    {
        return value?.ToLower() switch
        {
            "basic" => PlanType.Basic,
            "student" => PlanType.Student,
            "family" => PlanType.Family,
            "familypremium" => PlanType.Family,
            "premium" => PlanType.Family,
            _ => throw new ArgumentException($"Tipo de plan inválido: {value}")
        };
    }
}
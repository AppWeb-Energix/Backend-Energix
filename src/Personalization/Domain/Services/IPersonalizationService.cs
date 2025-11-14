namespace Energix.API.Personalization.Domain.Services;

/// <summary>
/// Domain service for personalization business logic
/// </summary>
public interface IPersonalizationService
{
    /// <summary>
    /// Validates if a user can create a new personalization
    /// </summary>
    Task<bool> CanUserCreatePersonalizationAsync(int userId);
    
    /// <summary>
    /// Validates personalization settings
    /// </summary>
    bool ValidatePersonalizationSettings(bool kpiCurrent, bool kpiCost, bool kpiMonthly, 
        bool chartHourly, bool chartMonthly, bool chartDevice);
}


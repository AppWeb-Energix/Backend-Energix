using Energix.API.Personalization.Domain.Repositories;
using Energix.API.Personalization.Domain.Services;

namespace Energix.API.Personalization.Infrastructure.Services;

/// <summary>
/// Implementation of personalization domain service
/// </summary>
public class PersonalizationService : IPersonalizationService
{
    private readonly IPersonalizationRepository _repository;

    public PersonalizationService(IPersonalizationRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> CanUserCreatePersonalizationAsync(int userId)
    {
        var existing = await _repository.GetByUserIdAsync(userId);
        return existing == null;
    }

    public bool ValidatePersonalizationSettings(
        bool kpiCurrent, bool kpiCost, bool kpiMonthly,
        bool chartHourly, bool chartMonthly, bool chartDevice)
    {
        // At least one KPI setting must be enabled
        var hasKpiEnabled = kpiCurrent || kpiCost || kpiMonthly;
        
        // At least one Chart setting must be enabled
        var hasChartEnabled = chartHourly || chartMonthly || chartDevice;
        
        return hasKpiEnabled && hasChartEnabled;
    }
}


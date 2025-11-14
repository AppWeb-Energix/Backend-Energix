using Energix.API.Personalization.Domain.Model.Commands;
using Energix.API.Personalization.Domain.ValueObjects;
using Energix.API.Personalization.Interfaces.REST.Resources;

namespace Energix.API.Personalization.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert UpdatePersonalizationResource to UpdatePersonalizationCommand
/// </summary>
public static class UpdatePersonalizationCommandFromResourceAssembler
{
    public static UpdatePersonalizationCommand ToCommandFromResource(UpdatePersonalizationResource resource, int personalizationId)
    {
        PersonalizationValue? kpiSettings = null;
        PersonalizationValue? chartSettings = null;

        // Create KPI settings if any KPI field is present
        if (resource.KpiCurrent.HasValue || resource.KpiCost.HasValue || resource.KpiMonthly.HasValue)
        {
            kpiSettings = new PersonalizationValue(
                current: resource.KpiCurrent ?? true,
                cost: resource.KpiCost ?? true,
                monthly: resource.KpiMonthly ?? true,
                hourly: false,
                device: false
            );
        }

        // Create Chart settings if any Chart field is present
        if (resource.ChartHourly.HasValue || resource.ChartMonthly.HasValue || resource.ChartDevice.HasValue)
        {
            chartSettings = new PersonalizationValue(
                current: false,
                cost: false,
                monthly: resource.ChartMonthly ?? true,
                hourly: resource.ChartHourly ?? true,
                device: resource.ChartDevice ?? true
            );
        }

        return new UpdatePersonalizationCommand(
            personalizationId: personalizationId,
            kpiSettings: kpiSettings,
            chartSettings: chartSettings
        );
    }
}


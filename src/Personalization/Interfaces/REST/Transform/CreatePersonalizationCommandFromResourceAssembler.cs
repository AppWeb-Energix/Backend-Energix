using Energix.API.Personalization.Domain.Model.Commands;
using Energix.API.Personalization.Domain.ValueObjects;
using Energix.API.Personalization.Interfaces.REST.Resources;

namespace Energix.API.Personalization.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert CreatePersonalizationResource to CreatePersonalizationCommand
/// </summary>
public static class CreatePersonalizationCommandFromResourceAssembler
{
    public static CreatePersonalizationCommand ToCommandFromResource(CreatePersonalizationResource resource)
    {
        var kpiSettings = new PersonalizationValue(
            current: resource.KpiCurrent,
            cost: resource.KpiCost,
            monthly: resource.KpiMonthly,
            hourly: false,
            device: false
        );
        
        var chartSettings = new PersonalizationValue(
            current: false,
            cost: false,
            monthly: resource.ChartMonthly,
            hourly: resource.ChartHourly,
            device: resource.ChartDevice
        );

        return new CreatePersonalizationCommand(
            userId: resource.UserId,
            kpiSettings: kpiSettings,
            chartSettings: chartSettings
        );
    }
}


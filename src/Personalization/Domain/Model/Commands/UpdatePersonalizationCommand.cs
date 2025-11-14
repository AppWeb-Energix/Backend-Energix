using Energix.API.Personalization.Domain.ValueObjects;

namespace Energix.API.Personalization.Domain.Model.Commands;

/// <summary>
/// Command to update an existing personalization configuration
/// </summary>
public record UpdatePersonalizationCommand
{
    public int PersonalizationId { get; init; }
    public PersonalizationValue? KpiSettings { get; init; }
    public PersonalizationValue? ChartSettings { get; init; }

    public UpdatePersonalizationCommand(
        int personalizationId,
        PersonalizationValue? kpiSettings = null,
        PersonalizationValue? chartSettings = null)
    {
        PersonalizationId = personalizationId;
        KpiSettings = kpiSettings;
        ChartSettings = chartSettings;
    }
}


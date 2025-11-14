using Energix.API.Personalization.Domain.ValueObjects;

namespace Energix.API.Personalization.Domain.Model.Commands;

/// <summary>
/// Command to create a new personalization configuration
/// </summary>
public record CreatePersonalizationCommand
{
    public int UserId { get; init; }
    public PersonalizationValue KpiSettings { get; init; }
    public PersonalizationValue ChartSettings { get; init; }

    public CreatePersonalizationCommand(
        int userId,
        PersonalizationValue kpiSettings,
        PersonalizationValue chartSettings)
    {
        UserId = userId;
        KpiSettings = kpiSettings ?? throw new ArgumentNullException(nameof(kpiSettings));
        ChartSettings = chartSettings ?? throw new ArgumentNullException(nameof(chartSettings));
    }
}

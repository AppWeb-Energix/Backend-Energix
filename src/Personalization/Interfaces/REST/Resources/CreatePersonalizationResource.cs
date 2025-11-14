using System.ComponentModel.DataAnnotations;

namespace Energix.API.Personalization.Interfaces.REST.Resources;

/// <summary>
/// Resource for creating a personalization configuration
/// </summary>
public record CreatePersonalizationResource(
    [Required(ErrorMessage = "El ID de usuario es requerido")]
    int UserId,
    
    bool KpiCurrent = true,
    bool KpiCost = true,
    bool KpiMonthly = true,
    bool ChartHourly = true,
    bool ChartMonthly = true,
    bool ChartDevice = true
);


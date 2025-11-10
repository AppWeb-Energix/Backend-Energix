using System.ComponentModel.DataAnnotations;

namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Resource for creating a device
/// </summary>
public record CreateDeviceResource(
    [Required(ErrorMessage = "El nombre es requerido")]
    string Name,
    
    [Required(ErrorMessage = "El tipo es requerido (manual, plug, sensor)")]
    string Type,
    
    string? DeviceKind = null,
    decimal? Monthly = null,
    decimal? EstimatedCost = null,
    decimal? Tariff = null
);
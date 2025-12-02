using System.Collections.Generic;

namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Dataset representation tailored for Chart.js-like consumers.
/// </summary>
/// <param name="Label">Dataset label (e.g., "Consumo mensual (kWh)")</param>
/// <param name="Type">Chart type hint (bar, line, etc.)</param>
/// <param name="Data">Values aligned with the response labels</param>
/// <param name="BackgroundColor">Background color for bars/areas</param>
/// <param name="BorderColor">Stroke color</param>
/// <param name="YAxisId">Optional axis id when using mixed charts</param>
public record ManualDeviceChartDatasetResource(
    string Label,
    string Type,
    IEnumerable<decimal> Data,
    string BackgroundColor,
    string BorderColor,
    string? YAxisId = null);
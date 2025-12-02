using System.Collections.Generic;

namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Chart-friendly response for manual device metrics, ready for Chart.js datasets.
/// </summary>
/// <param name="Labels">Device names used as chart labels</param>
/// <param name="Datasets">Datasets aligned with the labels</param>
/// <param name="Totals">Aggregated totals for KPI cards</param>
/// <param name="Metadata">Auxiliary labels and counts for the UI</param>
public record ManualDeviceChartResource(
    IEnumerable<string> Labels,
    IEnumerable<ManualDeviceChartDatasetResource> Datasets,
    ManualDeviceChartTotalsResource Totals,
    ManualDeviceChartMetadataResource Metadata);
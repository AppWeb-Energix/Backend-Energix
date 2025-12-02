using System.Linq;
using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Interfaces.REST.Resources;

namespace Energix.API.DeviceManagement.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert manual devices into a metrics summary resource.
/// </summary>
public static class DeviceMetricsSummaryResourceAssembler
{
    public static DeviceMetricsSummaryResource ToResourceFromEntities(IEnumerable<Device> manualDevices)
    {
        var devicesWithMetrics = manualDevices
            .Where(d => d.Metrics != null)
            .ToList();

        var breakdown = devicesWithMetrics
            .Select(d => new ManualDeviceMetricsResource(
                d.Id,
                d.Name,
                d.DeviceKind?.ToLowerString(),
                d.Metrics!.Monthly,
                d.Metrics!.EstimatedCost,
                d.Metrics!.DailyAvg,
                d.Metrics!.Tariff
            ))
            .ToList();

        var totalMonthly = breakdown.Sum(d => d.Monthly);
        var totalCost = breakdown.Sum(d => d.EstimatedCost);
        var totalDailyAverage = breakdown.Sum(d => d.DailyAverage);

        return new DeviceMetricsSummaryResource(
            totalMonthly,
            totalCost,
            null, // o calcula el tariff promedio si es necesario
            totalDailyAverage,
            breakdown
        );


    }
}
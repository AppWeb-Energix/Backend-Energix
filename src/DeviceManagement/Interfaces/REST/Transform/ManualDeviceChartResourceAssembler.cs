using System.Collections.Generic;
using System.Linq;
using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Interfaces.REST.Resources;

namespace Energix.API.DeviceManagement.Interfaces.REST.Transform;

public static class ManualDeviceChartResourceAssembler
{
    public static ManualDeviceChartResource ToChartResource(IEnumerable<Device> devices)
    {
        var manualDevices = devices
            .Where(d => d.IsManual() && d.Metrics is not null)
            .ToList();

        var labels = manualDevices
            .Select(d => d.Name)
            .ToList();

        var monthlyConsumption = manualDevices
            .Select(d => d.Metrics!.Monthly)
            .ToList();

        var estimatedCost = manualDevices
            .Select(d => d.Metrics!.EstimatedCost)
            .ToList();

        var dailyAverage = manualDevices
            .Select(d => d.Metrics!.DailyAvg)
            .ToList();

        var datasets = new List<ManualDeviceChartDatasetResource>
        {
            new(
                "Consumo mensual (kWh)",
                "bar",
                monthlyConsumption,
                "rgba(37, 99, 235, 0.25)",
                "#2563eb"),
            new(
                "Costo estimado",
                "bar",
                estimatedCost,
                "rgba(22, 163, 74, 0.25)",
                "#16a34a",
                "cost"),
            new(
                "Consumo diario (kWh)",
                "line",
                dailyAverage,
                "rgba(234, 88, 12, 0.2)",
                "#ea580c",
                "daily")
        };

        var totals = new ManualDeviceChartTotalsResource(
            monthlyConsumption.Sum(),
            estimatedCost.Sum(),
            dailyAverage.Any() ? dailyAverage.Average() : 0m);

        var metadata = new ManualDeviceChartMetadataResource(
            manualDevices.Count,
            "kWh",
            "$",
            "kWh diarios");

        return new ManualDeviceChartResource(labels, datasets, totals, metadata);
    }
}
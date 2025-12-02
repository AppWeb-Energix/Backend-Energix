using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Interfaces.REST.Resources;

namespace Energix.API.DeviceManagement.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert Device entity to DeviceResource
/// </summary>
public static class DeviceResourceFromEntityAssembler
{
    public static DeviceResource ToResourceFromEntity(Device entity)
    {
        return new DeviceResource(
            Id: entity.Id,
            UserId: entity.UserId,
            Name: entity.Name,
            Type: entity.Type.ToLowerString(),
            Status: entity.Status.ToLowerString(),
            Online: entity.Online,
            LinkedAt: entity.LinkedAt,
            ZoneId: entity.ZoneId,
            DeviceKind: entity.DeviceKind?.ToLowerString(),
            Metrics: entity.Metrics != null
                ? new DeviceMetricsSummaryResource(
                    entity.Metrics.Monthly,
                    entity.Metrics.EstimatedCost,
                    entity.Metrics.Tariff,
                    entity.Metrics.DailyAvg,
                    null  // o la propiedad correcta
                )
                : null


        );
    }
}
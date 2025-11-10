using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Interfaces.REST.Resources;

namespace Energix.API.DeviceManagement.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert Zone entity to ZoneResource
/// </summary>
public static class ZoneResourceFromEntityAssembler
{
    public static ZoneResource ToResourceFromEntity(Zone entity)
    {
        return new ZoneResource(
            Id: entity.Id,
            UserId: entity.UserId,
            Name: entity.Name,
            CreatedAt: entity.CreatedAt
        );
    }

    public static ZoneWithDevicesResource ToResourceWithDevicesFromEntity(Zone entity)
    {
        return new ZoneWithDevicesResource(
            Id: entity.Id,
            UserId: entity.UserId,
            Name: entity.Name,
            CreatedAt: entity.CreatedAt,
            Devices: entity.Devices
                .Select(DeviceResourceFromEntityAssembler.ToResourceFromEntity)
                .ToList()
        );
    }
}
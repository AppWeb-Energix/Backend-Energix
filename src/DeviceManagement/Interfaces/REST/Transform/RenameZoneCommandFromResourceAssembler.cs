using Energix.API.DeviceManagement.Domain.Model.Commands.Zones;
using Energix.API.DeviceManagement.Interfaces.REST.Resources;

namespace Energix.API.DeviceManagement.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert UpdateZoneResource to RenameZoneCommand
/// </summary>
public static class RenameZoneCommandFromResourceAssembler
{
    public static RenameZoneCommand ToCommandFromResource(UpdateZoneResource resource, int zoneId, int userId)
    {
        return new RenameZoneCommand(
            ZoneId: zoneId,
            NewName: resource.Name,
            UserId: userId
        );
    }
}
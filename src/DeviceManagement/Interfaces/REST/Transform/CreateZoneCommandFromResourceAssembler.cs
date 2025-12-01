using Energix.API.DeviceManagement.Domain.Model.Commands.Zones;
using Energix.API.DeviceManagement.Interfaces.REST.Resources;

namespace Energix.API.DeviceManagement.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert CreateZoneResource to CreateZoneCommand
/// </summary>
public static class CreateZoneCommandFromResourceAssembler
{
    public static CreateZoneCommand ToCommandFromResource(CreateZoneResource resource, int userId)
    {
        return new CreateZoneCommand(
            UserId: resource.UserId?? userId,
            Name: resource.Name
        );
    }
}
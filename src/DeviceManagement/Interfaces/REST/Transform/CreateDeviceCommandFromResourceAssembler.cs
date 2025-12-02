using Energix.API.DeviceManagement.Domain.Model.Commands.Devices;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Interfaces.REST.Resources;

namespace Energix.API.DeviceManagement.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert CreateDeviceResource to CreateDeviceCommand
/// </summary>
public static class CreateDeviceCommandFromResourceAssembler
{
    public static CreateDeviceCommand ToCommandFromResource(CreateDeviceResource resource, int userId)
    {
        var deviceType = DeviceTypeExtensions.ParseDeviceType(resource.Type);

        DeviceKind? deviceKind = null;
        if (!string.IsNullOrEmpty(resource.DeviceKind))
        {
            deviceKind = DeviceKindExtensions.ParseDeviceKind(resource.DeviceKind);
        }

        // ✅ USAR SOLO EL userId DEL JWT (parámetro), IGNORAR resource.UserId
        return new CreateDeviceCommand(
            UserId: userId, // ← Solo el del JWT
            Name: resource.Name,
            Type: deviceType,
            DeviceKind: deviceKind,
            Monthly: resource.Monthly,
            EstimatedCost: resource.EstimatedCost,
            Tariff: resource.Tariff
        );
    }
}
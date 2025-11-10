using Energix.API.DeviceManagement.Domain.Model.Commands.Devices;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Interfaces.REST.Resources;

namespace Energix.API.DeviceManagement.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert UpdateDeviceResource to UpdateDeviceCommand
/// </summary>
public static class UpdateDeviceCommandFromResourceAssembler
{
    public static UpdateDeviceCommand ToCommandFromResource(UpdateDeviceResource resource, int deviceId)
    {
        DeviceStatus? status = null;
        if (!string.IsNullOrEmpty(resource.Status))
        {
            status = DeviceStatusExtensions.ParseDeviceStatus(resource.Status);
        }

        return new UpdateDeviceCommand(
            DeviceId: deviceId,
            Name: resource.Name,
            Status: status,
            Online: resource.Online,
            ZoneId: resource.ZoneId,
            RemoveFromZone: resource.RemoveFromZone,
            Monthly: resource.Monthly,
            EstimatedCost: resource.EstimatedCost,
            Tariff: resource.Tariff
        );
    }
}
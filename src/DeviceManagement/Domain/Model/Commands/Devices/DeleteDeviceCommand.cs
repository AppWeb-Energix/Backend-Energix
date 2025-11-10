namespace Energix.API.DeviceManagement.Domain.Model.Commands.Devices;

/// <summary>
/// Command to remove (unlink) a device
/// </summary>
public record DeleteDeviceCommand(
    int DeviceId,
    int UserId
);
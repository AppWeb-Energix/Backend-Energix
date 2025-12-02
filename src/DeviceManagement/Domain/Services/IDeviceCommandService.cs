using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Domain.Model.Commands.Devices;

namespace Energix.API.DeviceManagement.Domain.Services;

/// <summary>
/// Service for executing device commands
/// </summary>
public interface IDeviceCommandService
{
    /// <summary>
    /// Handle creating a new device
    /// </summary>
    Task<Device> Handle(CreateDeviceCommand command);
    
    /// <summary>
    /// Handle updating an existing device
    /// </summary>
    Task<Device?> Handle(UpdateDeviceCommand command);
    
    /// <summary>
    /// Handle deleting a device
    /// </summary>
    Task<bool> Handle(DeleteDeviceCommand command);
}

using Energix.API.DeviceManagement.Domain.Model.ValueObjects;

namespace Energix.API.DeviceManagement.Domain.Services;

/// <summary>
/// Domain service for generating automatic device names
/// </summary>
public interface IDeviceNamingService
{
    /// <summary>
    /// Generates an automatic name for a device based on the current type and quantity.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="deviceType">Device type</param>
    /// <param name="currentCount">Current number of devices of that type</param>
    /// <returns>Generated name (e.g., "Device 1", "Device 2")</returns>
    Task<string> GenerateDeviceNameAsync(int userId, DeviceType deviceType, int currentCount);
    
    /// <summary>
    /// Validate whether a device name is valid
    /// </summary>
    /// <param name="name">Name to validate</param>
    /// <returns>True if valid</returns>
    bool IsValidDeviceName(string name);
    
    /// <summary>
    /// Get the next available number for a device
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="deviceType">Device type</param>
    /// <returns>Next available number</returns>
    Task<int> GetNextDeviceNumberAsync(int userId, DeviceType deviceType);
}
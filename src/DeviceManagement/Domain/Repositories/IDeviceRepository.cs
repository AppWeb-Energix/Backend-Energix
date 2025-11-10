using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;

namespace Energix.API.DeviceManagement.Domain.Repositories;

/// <summary>
/// Repository for the aggregate Device
/// </summary>
public interface IDeviceRepository
{
    /// <summary>
    /// Add a new device
    /// </summary>
    Task<Device> AddAsync(Device device);
    
    /// <summary>
    /// Update an existing device
    /// </summary>
    Task<Device> UpdateAsync(Device device);
    
    /// <summary>
    /// Delete a device
    /// </summary>
    Task DeleteAsync(Device device);
    
    /// <summary>
    /// Get a device by your ID
    /// </summary>
    Task<Device?> FindByIdAsync(int deviceId);
    
    /// <summary>
    /// It retrieves all of a user's devices
    /// </summary>
    Task<IEnumerable<Device>> FindByUserIdAsync(int userId);
    
    /// <summary>
    /// Retrieves a user's devices filtered by type
    /// </summary>
    Task<IEnumerable<Device>> FindByUserIdAndTypeAsync(int userId, DeviceType type);
    
    /// <summary>
    /// Retrieves devices assigned to a zone
    /// </summary>
    Task<IEnumerable<Device>> FindByZoneIdAsync(int zoneId);
    
    /// <summary>
    /// Obtains unassigned zone devices from a user
    /// </summary>
    Task<IEnumerable<Device>> FindWithoutZoneByUserIdAsync(int userId);
    
    /// <summary>
    /// Count the number of devices a user has
    /// </summary>
    Task<int> CountByUserIdAsync(int userId);
    
    /// <summary>
    /// Check if a device belongs to a user
    /// </summary>
    Task<bool> ExistsByIdAndUserIdAsync(int deviceId, int userId);
}
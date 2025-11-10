using Energix.API.DeviceManagement.Domain.Model.Aggregates;

namespace Energix.API.DeviceManagement.Domain.Repositories;

/// <summary>
/// Repository for the Aggregate Zone
/// </summary>
public interface IZoneRepository
{
    /// <summary>
    /// Add a new zone
    /// </summary>
    Task<Zone> AddAsync(Zone zone);
    
    /// <summary>
    /// Update an existing zone
    /// </summary>
    Task<Zone> UpdateAsync(Zone zone);
    
    /// <summary>
    /// Delete a zone
    /// </summary>
    Task DeleteAsync(Zone zone);
    
    /// <summary>
    /// You get a zone by your ID
    /// </summary>
    Task<Zone?> FindByIdAsync(int zoneId);
    
    /// <summary>
    /// It retrieves all of a user's zones
    /// </summary>
    Task<IEnumerable<Zone>> FindByUserIdAsync(int userId);
    
    /// <summary>
    /// You get a zone with your devices included
    /// </summary>
    Task<Zone?> FindByIdWithDevicesAsync(int zoneId);
    
    /// <summary>
    /// It accesses all of a user's zones with their devices
    /// </summary>
    Task<IEnumerable<Zone>> FindByUserIdWithDevicesAsync(int userId);
    
    /// <summary>
    /// Check if an area belongs to a user
    /// </summary>
    Task<bool> ExistsByIdAndUserIdAsync(int zoneId, int userId);
    
    /// <summary>
    /// Check if a user already has a zone with that name
    /// </summary>
    Task<bool> ExistsByUserIdAndNameAsync(int userId, string name);
}
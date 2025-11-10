using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Energix.API.DeviceManagement.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Implementing the device repository using Entity Framework Core
/// </summary>
public class DeviceRepository : IDeviceRepository
{
    private readonly AppDbContext _context;

    public DeviceRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Add a new device
    /// </summary>
    public async Task<Device> AddAsync(Device device)
    {
        await _context.Devices.AddAsync(device);
        await _context.SaveChangesAsync();
        return device;
    }

    /// <summary>
    /// Update an existing device
    /// </summary>
    public async Task<Device> UpdateAsync(Device device)
    {
        _context.Devices.Update(device);
        await _context.SaveChangesAsync();
        return device;
    }

    /// <summary>
    /// Delete a device
    /// </summary>
    public async Task DeleteAsync(Device device)
    {
        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Get a device by your ID
    /// </summary>
    public async Task<Device?> FindByIdAsync(int deviceId)
    {
        return await _context.Devices
            .Include(d => d.Zone) // Include zone if it exists
            .FirstOrDefaultAsync(d => d.Id == deviceId);
    }

    /// <summary>
    /// It retrieves all of a user's devices
    /// </summary>
    public async Task<IEnumerable<Device>> FindByUserIdAsync(int userId)
    {
        return await _context.Devices
            .Include(d => d.Zone)
            .Where(d => d.UserId == userId)
            .OrderBy(d => d.LinkedAt) // Sort by date of affiliation
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a user's devices filtered by type
    /// </summary>
    public async Task<IEnumerable<Device>> FindByUserIdAndTypeAsync(int userId, DeviceType type)
    {
        return await _context.Devices
            .Include(d => d.Zone)
            .Where(d => d.UserId == userId && d.Type == type)
            .OrderBy(d => d.LinkedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves devices assigned to a zone
    /// </summary>
    public async Task<IEnumerable<Device>> FindByZoneIdAsync(int zoneId)
    {
        return await _context.Devices
            .Where(d => d.ZoneId == zoneId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Obtains unassigned zone devices from a user
    /// </summary>
    public async Task<IEnumerable<Device>> FindWithoutZoneByUserIdAsync(int userId)
    {
        return await _context.Devices
            .Where(d => d.UserId == userId && d.ZoneId == null)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Count the number of devices a user has
    /// </summary>
    public async Task<int> CountByUserIdAsync(int userId)
    {
        return await _context.Devices
            .CountAsync(d => d.UserId == userId);
    }

    /// <summary>
    /// Check if a device belongs to a user
    /// </summary>
    public async Task<bool> ExistsByIdAndUserIdAsync(int deviceId, int userId)
    {
        return await _context.Devices
            .AnyAsync(d => d.Id == deviceId && d.UserId == userId);
    }
}
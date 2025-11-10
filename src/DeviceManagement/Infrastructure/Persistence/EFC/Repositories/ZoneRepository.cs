using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Energix.API.DeviceManagement.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Implementing the zone repository using Entity Framework Core
/// </summary>
public class ZoneRepository : IZoneRepository
{
    private readonly AppDbContext _context;

    public ZoneRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Add a new zone
    /// </summary>
    public async Task<Zone> AddAsync(Zone zone)
    {
        await _context.Zones.AddAsync(zone);
        await _context.SaveChangesAsync();
        return zone;
    }

    /// <summary>
    /// Update an existing zone
    /// </summary>
    public async Task<Zone> UpdateAsync(Zone zone)
    {
        _context.Zones.Update(zone);
        await _context.SaveChangesAsync();
        return zone;
    }

    /// <summary>
    /// Delete a zone
    /// </summary>
    public async Task DeleteAsync(Zone zone)
    {
        _context.Zones.Remove(zone);
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// You get a zone by your ID
    /// </summary>
    public async Task<Zone?> FindByIdAsync(int zoneId)
    {
        return await _context.Zones
            .FirstOrDefaultAsync(z => z.Id == zoneId);
    }

    /// <summary>
    /// It retrieves all of a user's zones
    /// </summary>
    public async Task<IEnumerable<Zone>> FindByUserIdAsync(int userId)
    {
        return await _context.Zones
            .Where(z => z.UserId == userId)
            .OrderBy(z => z.CreatedAt) // Sort by creation date
            .ToListAsync();
    }

    /// <summary>
    /// You get a zone with your devices included
    /// </summary>
    public async Task<Zone?> FindByIdWithDevicesAsync(int zoneId)
    {
        return await _context.Zones
            .Include(z => z.Devices) // Eager device loading
            .FirstOrDefaultAsync(z => z.Id == zoneId);
    }

    /// <summary>
    /// It accesses all of a user's zones with their devices
    /// </summary>
    public async Task<IEnumerable<Zone>> FindByUserIdWithDevicesAsync(int userId)
    {
        return await _context.Zones
            .Include(z => z.Devices)
            .Where(z => z.UserId == userId)
            .OrderBy(z => z.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Check if an area belongs to a user
    /// </summary>
    public async Task<bool> ExistsByIdAndUserIdAsync(int zoneId, int userId)
    {
        return await _context.Zones
            .AnyAsync(z => z.Id == zoneId && z.UserId == userId);
    }

    /// <summary>
    /// Check if a user already has a zone with that name
    /// </summary>
    public async Task<bool> ExistsByUserIdAndNameAsync(int userId, string name)
    {
        return await _context.Zones
            .AnyAsync(z => z.UserId == userId && z.Name == name);
    }
}
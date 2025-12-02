using Microsoft.EntityFrameworkCore;

namespace Energix.API.AdminManagement.Application.Services;

/// <summary>
/// Service to aggregate dashboard statistics
/// </summary>
public class AdminDashboardService
{
    private readonly AppDbContext _context;

    public AdminDashboardService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the total number of users in the system
    /// </summary>
    public async Task<int> GetTotalUsersAsync()
    {
        return await _context.Users.CountAsync();
    }

    /// <summary>
    /// Gets the number of active (online) devices
    /// </summary>
    public async Task<int> GetActiveDevicesAsync()
    {
        return await _context.Devices.CountAsync(d => d.Online);
    }

    /// <summary>
    /// Gets the distribution of users by plan type
    /// Returns a dictionary with PlanType as key and count as value
    /// </summary>
    public async Task<Dictionary<string, int>> GetPlanDistributionAsync()
    {
        var distribution = await _context.Subscriptions
            .Where(s => s.IsActive)
            .GroupBy(s => s.PlanType)
            .Select(g => new { PlanType = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        return distribution.ToDictionary(x => x.PlanType, x => x.Count);
    }

    /// <summary>
    /// Gets all dashboard data in one call
    /// </summary>
    public async Task<DashboardDataDto> GetDashboardDataAsync()
    {
        var totalUsers = await GetTotalUsersAsync();
        var activeDevices = await GetActiveDevicesAsync();
        var planDistribution = await GetPlanDistributionAsync();

        return new DashboardDataDto
        {
            TotalUsers = totalUsers,
            ActiveDevices = activeDevices,
            PlanDistribution = planDistribution
        };
    }
}

/// <summary>
/// DTO for dashboard data response
/// </summary>
public class DashboardDataDto
{
    public int TotalUsers { get; set; }
    public int ActiveDevices { get; set; }
    public Dictionary<string, int> PlanDistribution { get; set; } = new();
}


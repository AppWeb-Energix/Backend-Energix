using Energix.API.AdminManagement.Application.QueryServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.AdminManagement.Interfaces.REST;

/// <summary>
/// Admin controller for administrative functions
/// Protected with Admin role
/// </summary>
[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly SystemStatsQueryService _systemStatsQueryService;

    public AdminController(SystemStatsQueryService systemStatsQueryService)
    {
        _systemStatsQueryService = systemStatsQueryService;
    }

    /// <summary>
    /// Get system health status
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> GetSystemHealth()
    {
        var health = await _systemStatsQueryService.GetSystemHealthAsync();
        return Ok(health);
    }

    /// <summary>
    /// Get system statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetSystemStats()
    {
        var totalUsers = await _systemStatsQueryService.GetTotalUserCountAsync();
        var activeSubscriptions = await _systemStatsQueryService.GetActiveSubscriptionCountAsync();

        return Ok(new
        {
            totalUsers,
            activeSubscriptions,
            timestamp = DateTime.UtcNow
        });
    }
}


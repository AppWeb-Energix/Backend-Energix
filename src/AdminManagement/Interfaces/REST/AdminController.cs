using Energix.API.AdminManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Energix.API.AdminManagement.Interfaces.REST;

/// <summary>
/// Admin controller for system administration and monitoring
/// </summary>
[ApiController]
[Route("api/v1/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly SystemHealthService _healthService;
    private readonly AdminDashboardService _dashboardService;
    private readonly SimpleAuditService _auditService;
    private readonly AppDbContext _context;

    public AdminController(
        SystemHealthService healthService,
        AdminDashboardService dashboardService,
        SimpleAuditService auditService,
        AppDbContext context)
    {
        _healthService = healthService;
        _dashboardService = dashboardService;
        _auditService = auditService;
        _context = context;
    }

    /// <summary>
    /// Checks system health status (for frontend traffic light indicator)
    /// </summary>
    /// <returns>Health status: "Healthy" or "Unhealthy"</returns>
    [HttpGet("health")]
    public async Task<IActionResult> GetHealth()
    {
        var status = await _healthService.CheckHealthAsync();
        
        return Ok(new 
        { 
            status = status,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets dashboard data with statistics and metrics
    /// </summary>
    /// <returns>Dashboard data including user count, active devices, and plan distribution</returns>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var data = await _dashboardService.GetDashboardDataAsync();
        
        return Ok(new
        {
            totalUsers = data.TotalUsers,
            activeDevices = data.ActiveDevices,
            planDistribution = data.PlanDistribution,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets the latest 50 audit log entries
    /// </summary>
    /// <returns>List of audit log entries ordered by date descending</returns>
    [HttpGet("audit")]
    public async Task<IActionResult> GetAuditLogs()
    {
        var logs = await _context.SimpleAuditLogs
            .OrderByDescending(log => log.CreatedAt)
            .Take(50)
            .Select(log => new
            {
                id = log.Id,
                message = log.Message,
                createdAt = log.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            logs = logs,
            count = logs.Count
        });
    }
}


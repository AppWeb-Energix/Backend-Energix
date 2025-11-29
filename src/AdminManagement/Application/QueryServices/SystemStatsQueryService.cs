using Energix.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Energix.API.AdminManagement.Application.QueryServices;

/// <summary>
/// Query service for system analytics and statistics
/// </summary>
public class SystemStatsQueryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SystemStatsQueryService> _logger;

    public SystemStatsQueryService(AppDbContext context, ILogger<SystemStatsQueryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get total user count
    /// </summary>
    public async Task<int> GetTotalUserCountAsync()
    {
        try
        {
            return await _context.Users.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total user count");
            throw;
        }
    }

    /// <summary>
    /// Get active subscription count
    /// </summary>
    public async Task<int> GetActiveSubscriptionCountAsync()
    {
        try
        {
            return await _context.Subscriptions
                .Where(s => s.IsActive)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active subscription count");
            throw;
        }
    }

    /// <summary>
    /// Get system health status
    /// </summary>
    public async Task<object> GetSystemHealthAsync()
    {
        try
        {
            // Verificar conexión a la base de datos
            var canConnect = await _context.Database.CanConnectAsync();
            
            return new
            {
                status = canConnect ? "Healthy" : "Unhealthy",
                database = canConnect ? "Connected" : "Disconnected",
                timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking system health");
            return new
            {
                status = "Unhealthy",
                database = "Error",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            };
        }
    }
}


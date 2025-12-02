namespace Energix.API.AdminManagement.Application.Services;

/// <summary>
/// Simple service to check system health
/// </summary>
public class SystemHealthService
{
    private readonly AppDbContext _context;

    public SystemHealthService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Checks if the database connection is healthy
    /// </summary>
    public async Task<string> CheckHealthAsync()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            return canConnect ? "Healthy" : "Unhealthy";
        }
        catch
        {
            return "Unhealthy";
        }
    }
}


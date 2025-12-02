using Energix.API;
using Energix.API.Notifications.Domain.Aggregates;
using Energix.API.Notifications.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Energix.API.Notifications.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementation of the alert repository using Entity Framework Core
/// </summary>
public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _context;

    public AlertRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Add a new alert
    /// </summary>
    public async Task<Alert> AddAsync(Alert alert)
    {
        await _context.Alerts.AddAsync(alert);
        await _context.SaveChangesAsync();
        return alert;
    }

    /// <summary>
    /// Get all alerts for a user, ordered by CreatedAt DESC
    /// </summary>
    public async Task<IEnumerable<Alert>> GetByUserIdAsync(int userId)
    {
        return await _context.Alerts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}


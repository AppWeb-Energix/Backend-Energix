using Energix.API.Notifications.Domain.Aggregates;

namespace Energix.API.Notifications.Domain.Repositories;

/// <summary>
/// Repository for the aggregate Alert
/// </summary>
public interface IAlertRepository
{
    /// <summary>
    /// Add a new alert
    /// </summary>
    Task<Alert> AddAsync(Alert alert);
    
    /// <summary>
    /// Get all alerts for a user, ordered by CreatedAt DESC
    /// </summary>
    Task<IEnumerable<Alert>> GetByUserIdAsync(int userId);
}


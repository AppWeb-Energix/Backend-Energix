using Energix.API.AdminManagement.Domain.Entities;

namespace Energix.API.AdminManagement.Application.Services;

/// <summary>
/// Simple service to log admin actions
/// </summary>
public class SimpleAuditService
{
    private readonly AppDbContext _context;

    public SimpleAuditService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Logs an admin action to the audit log
    /// </summary>
    /// <param name="message">The action message (e.g., "Admin X deleted user Y")</param>
    public async Task LogActionAsync(string message)
    {
        var auditLog = new SimpleAuditLog
        {
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        _context.SimpleAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}


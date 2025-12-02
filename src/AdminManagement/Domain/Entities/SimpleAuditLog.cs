namespace Energix.API.AdminManagement.Domain.Entities;

/// <summary>
/// Simple audit log entity to track admin actions
/// </summary>
public class SimpleAuditLog
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


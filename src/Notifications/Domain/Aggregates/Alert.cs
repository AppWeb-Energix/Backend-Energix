namespace Energix.API.Notifications.Domain.Aggregates;

/// <summary>
/// Alert Aggregate Root
/// Represents a notification/alert for user actions on devices
/// </summary>
public class Alert
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int? DeviceId { get; private set; }
    public string Type { get; private set; }
    public string Message { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor
    protected Alert()
    {
        Type = string.Empty;
        Message = string.Empty;
    }

    /// <summary>
    /// Creates a new alert
    /// </summary>
    /// <param name="userId">User ID that owns the alert</param>
    /// <param name="deviceId">Optional device ID related to the alert</param>
    /// <param name="type">Alert type: "created", "renamed", "deleted"</param>
    /// <param name="message">Alert message</param>
    public Alert(int userId, int? deviceId, string type, string message)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId debe ser mayor a 0", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type no puede estar vacío", nameof(type));
        
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message no puede estar vacío", nameof(message));

        UserId = userId;
        DeviceId = deviceId;
        Type = type.Trim().ToLower();
        Message = message.Trim();
        CreatedAt = DateTime.UtcNow;
    }
}


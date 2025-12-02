namespace Energix.API.Profile.Interfaces.REST.Resources;

/// <summary>
/// Resource representing user profile information
/// </summary>
public record UserProfileResource
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Dni { get; init; } = string.Empty;
    public string District { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}


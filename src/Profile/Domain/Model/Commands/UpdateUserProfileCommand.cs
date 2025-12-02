namespace Energix.API.Profile.Domain.Model.Commands;

/// <summary>
/// Command to update user profile information
/// </summary>
public class UpdateUserProfileCommand
{
    public int UserId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Dni { get; init; }
    public string? District { get; init; }
}



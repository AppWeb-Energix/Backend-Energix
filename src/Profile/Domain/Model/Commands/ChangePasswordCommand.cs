namespace Energix.API.Profile.Domain.Model.Commands;

/// <summary>
/// Command to change user password
/// </summary>
public class ChangePasswordCommand
{
    public int UserId { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}


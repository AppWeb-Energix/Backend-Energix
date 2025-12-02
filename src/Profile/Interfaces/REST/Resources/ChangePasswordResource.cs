using System.ComponentModel.DataAnnotations;

namespace Energix.API.Profile.Interfaces.REST.Resources;

/// <summary>
/// Resource for changing user password
/// </summary>
public record ChangePasswordResource
{
    [Required]
    public string CurrentPassword { get; init; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    public string NewPassword { get; init; } = string.Empty;
}


using System.ComponentModel.DataAnnotations;

namespace Energix.API.Profile.Interfaces.REST.Resources;

/// <summary>
/// Resource for updating user profile information
/// </summary>
public record UpdateUserProfileResource
{
    [StringLength(100)]
    public string? FirstName { get; init; }
    
    [StringLength(100)]
    public string? LastName { get; init; }
    
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; init; }
    
    [StringLength(20)]
    public string? Dni { get; init; }
    
    [StringLength(100)]
    public string? District { get; init; }
}


namespace Energix.API.Identity.Interfaces.REST.DTOs;

/// <summary>
/// DTO for user resource
/// </summary>
public record UserResource(
    int Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    DateTime CreatedAt
);


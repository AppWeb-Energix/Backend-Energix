namespace Energix.API.Identity.Interfaces.REST.DTOs;

/// <summary>
/// DTO for user sign-up request
/// </summary>
public record SignUpRequest(
    string Email,
    string Password,
    string Username,
    string FirstName,
    string LastName
);


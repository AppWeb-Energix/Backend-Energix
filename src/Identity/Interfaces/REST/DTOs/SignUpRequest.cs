namespace Energix.API.Identity.Interfaces.REST.DTOs;

/// <summary>
/// DTO for user sign-up request
/// </summary>
public record SignUpRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Dni,
    string District
);


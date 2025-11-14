namespace Energix.API.Identity.Interfaces.REST.DTOs;
/// <summary>
/// DTO for user sign-in request
/// </summary>
public record SignInRequest(
    string Email,
    string Password
);

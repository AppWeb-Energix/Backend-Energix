namespace Energix.API.Identity.Domain.Services;

/// <summary>
/// Domain service contract for User command operations (SignUp, Update, etc.)
/// </summary>
public interface IUserCommandService
{
    Task<(bool Success, string Message, int? UserId)> SignUpAsync(
        string email,
        string password,
        string username,
        string firstName,
        string lastName);

    Task<(bool Success, string Message, string? Token, object? User)> SignInAsync(
        string email,
        string password);
}


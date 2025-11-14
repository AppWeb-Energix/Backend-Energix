namespace Energix.API.Identity.Domain.Services;

/// <summary>
/// Port for password hashing operations
/// </summary>
public interface IHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}


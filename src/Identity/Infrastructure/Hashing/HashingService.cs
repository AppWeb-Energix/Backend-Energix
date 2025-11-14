using Energix.API.Identity.Domain.Services;

namespace Energix.API.Identity.Infrastructure.Hashing;

/// <summary>
/// BCrypt implementation of IHashingService
/// </summary>
public class HashingService : IHashingService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}


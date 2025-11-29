using System.Security.Claims;

namespace Energix.API.Identity.Domain.Services;

/// <summary>
/// Port for JWT token generation
/// </summary>
public interface ITokenService
{
    string GenerateToken(int userId, string email, string username, string role);
    string GenerateToken(IEnumerable<Claim> claims);
}


using Energix.API.Identity.Domain.Entities;

namespace Energix.API.Identity.Domain.Services;

/// <summary>
/// Domain service contract for User query operations
/// </summary>
public interface IUserQueryService
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    // Task<IEnumerable<User>> GetAllAsync();
}


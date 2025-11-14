using Energix.API.Identity.Domain.Entities;

namespace Energix.API.Identity.Domain.Repositories;

/// <summary>
/// Repository contract for User persistence operations
/// </summary>
public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(int id);
    Task<User?> FindByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}


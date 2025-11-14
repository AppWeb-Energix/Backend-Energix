using Energix.API.Identity.Domain.Repositories;
using Energix.API.Identity.Domain.Services;

namespace Energix.API.Identity.Application.Services;

/// <summary>
/// Service for migrating legacy plain-text passwords to BCrypt hashes
/// USE WITH EXTREME CAUTION - Only for one-time migration of legacy data
/// </summary>
public class PasswordMigrationService
{
    private readonly IUserRepository _userRepository;
    private readonly IHashingService _hashingService;
    private readonly IUnitOfWork _unitOfWork;

    public PasswordMigrationService(
        IUserRepository userRepository,
        IHashingService hashingService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _hashingService = hashingService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Migrates a single user's password from plain text to BCrypt hash
    /// WARNING: This assumes the current PasswordHash contains the plain-text password
    /// Only use if you have legacy data that needs migration
    /// </summary>
    /// <param name="userId">User ID to migrate</param>
    /// <returns>Success status and message</returns>
    public async Task<(bool Success, string Message)> MigrateUserPasswordAsync(int userId)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        
        if (user == null)
            return (false, "Usuario no encontrado");

        if (string.IsNullOrEmpty(user.PasswordHash))
            return (false, "Usuario no tiene contraseña configurada");

        // Check if already a BCrypt hash
        if (user.PasswordHash.StartsWith("$2"))
            return (false, "La contraseña ya está hasheada con BCrypt");

        // WARNING: This assumes PasswordHash contains the plain text password
        // This is extremely dangerous and should only be used for one-time migration
        var plainTextPassword = user.PasswordHash;
        
        // Hash the plain text password
        var newHash = _hashingService.HashPassword(plainTextPassword);
        
        // Update user
        user.PasswordHash = newHash;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return (true, $"Contraseña migrada exitosamente para usuario {user.Email}");
    }

    /// <summary>
    /// Alternative migration approach: Hash password on first successful login
    /// This is safer as it doesn't assume the stored value is the actual password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="plainTextPassword">The password provided during login</param>
    /// <returns>Success status</returns>
    public async Task<bool> MigrateOnLoginAsync(int userId, string plainTextPassword)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        
        if (user == null)
            return false;

        // Only migrate if not already a BCrypt hash
        if (user.PasswordHash.StartsWith("$2"))
            return false;

        // Verify the plain text password matches before migrating
        if (!string.Equals(plainTextPassword, user.PasswordHash, StringComparison.Ordinal))
            return false;

        // Hash and update
        user.PasswordHash = _hashingService.HashPassword(plainTextPassword);
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}


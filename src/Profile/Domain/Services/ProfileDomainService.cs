using Energix.API.Identity.Domain.Entities;
using Energix.API.Identity.Domain.Repositories;
using Energix.API.Identity.Domain.Services;

namespace Energix.API.Profile.Domain.Services;

/// <summary>
/// Implementation of profile domain service
/// </summary>
public class ProfileDomainService : IProfileDomainService
{
    private readonly IUserRepository _userRepository;
    private readonly IHashingService _hashingService;

    public ProfileDomainService(
        IUserRepository userRepository,
        IHashingService hashingService)
    {
        _userRepository = userRepository;
        _hashingService = hashingService;
    }

    public async Task<bool> IsEmailAvailableAsync(string email, int userId)
    {
        var existingUser = await _userRepository.FindByEmailAsync(email);
        return existingUser == null || existingUser.Id == userId;
    }

    public bool ValidateCurrentPassword(User user, string currentPassword)
    {
        return _hashingService.VerifyPassword(currentPassword, user.PasswordHash);
    }

    public bool ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        // Password must be at least 8 characters
        if (password.Length < 8)
            return false;

        // Password must contain at least one uppercase letter
        if (!password.Any(char.IsUpper))
            return false;

        // Password must contain at least one lowercase letter
        if (!password.Any(char.IsLower))
            return false;

        // Password must contain at least one digit
        if (!password.Any(char.IsDigit))
            return false;

        return true;
    }
}


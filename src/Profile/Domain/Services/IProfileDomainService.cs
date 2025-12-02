using Energix.API.Identity.Domain.Entities;

namespace Energix.API.Profile.Domain.Services;

/// <summary>
/// Domain service for profile operations validation and business rules
/// </summary>
public interface IProfileDomainService
{
    /// <summary>
    /// Validates if the email is already in use by another user
    /// </summary>
    Task<bool> IsEmailAvailableAsync(string email, int userId);
    
    /// <summary>
    /// Validates if the current password is correct
    /// </summary>
    bool ValidateCurrentPassword(User user, string currentPassword);
    
    /// <summary>
    /// Validates new password strength
    /// </summary>
    bool ValidatePasswordStrength(string password);
}


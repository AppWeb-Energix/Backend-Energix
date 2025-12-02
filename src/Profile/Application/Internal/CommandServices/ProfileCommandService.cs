using Energix.API.Profile.Domain.Model.Commands;
using Energix.API.Profile.Domain.Model.ValueObjects;
using Energix.API.Profile.Domain.Services;
using Energix.API.Identity.Domain.Repositories;
using Energix.API.Identity.Domain.Services;

namespace Energix.API.Profile.Application.Internal.CommandServices;

/// <summary>
/// Application service for handling profile commands
/// </summary>
public class ProfileCommandService
{
    private readonly IUserRepository _userRepository;
    private readonly IProfileDomainService _domainService;
    private readonly IHashingService _hashingService;
    private readonly IUnitOfWork _unitOfWork;

    public ProfileCommandService(
        IUserRepository userRepository,
        IProfileDomainService domainService,
        IHashingService hashingService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _domainService = domainService;
        _hashingService = hashingService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handle update user profile command
    /// </summary>
    public async Task<UserProfileInfo> HandleAsync(UpdateUserProfileCommand command)
    {
        // Find the user
        var user = await _userRepository.FindByIdAsync(command.UserId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {command.UserId} not found");
        }

        // Validate and update email if provided
        if (!string.IsNullOrWhiteSpace(command.Email) && command.Email != user.Email)
        {
            var isEmailAvailable = await _domainService.IsEmailAvailableAsync(command.Email, command.UserId);
            if (!isEmailAvailable)
            {
                throw new InvalidOperationException($"Email {command.Email} is already in use");
            }
            user.Email = command.Email;
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(command.FirstName))
            user.FirstName = command.FirstName;

        if (!string.IsNullOrWhiteSpace(command.LastName))
            user.LastName = command.LastName;

        if (!string.IsNullOrWhiteSpace(command.Dni))
            user.Dni = command.Dni;

        if (!string.IsNullOrWhiteSpace(command.District))
            user.District = command.District;

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new UserProfileInfo
        {
            UserId = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Dni = user.Dni,
            District = user.District,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    /// <summary>
    /// Handle change password command
    /// </summary>
    public async Task<bool> HandleAsync(ChangePasswordCommand command)
    {
        // Find the user
        var user = await _userRepository.FindByIdAsync(command.UserId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {command.UserId} not found");
        }

        // Validate current password
        if (!_domainService.ValidateCurrentPassword(user, command.CurrentPassword))
        {
            throw new InvalidOperationException("Current password is incorrect");
        }

        // Validate new password strength
        if (!_domainService.ValidatePasswordStrength(command.NewPassword))
        {
            throw new InvalidOperationException(
                "New password does not meet security requirements. " +
                "Password must be at least 8 characters and contain uppercase, lowercase, and digit characters.");
        }

        // Hash and update password
        user.PasswordHash = _hashingService.HashPassword(command.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}


using Energix.API.Profile.Domain.Model.Commands;
using Energix.API.Profile.Domain.Model.Queries;
using Energix.API.Profile.Domain.Model.ValueObjects;
using Energix.API.Profile.Interfaces.REST.Resources;

namespace Energix.API.Profile.Interfaces.REST.Transform;

/// <summary>
/// Assembler for transforming between resources and domain models
/// </summary>
public static class ProfileResourceAssembler
{
    /// <summary>
    /// Transform UpdateUserProfileResource to UpdateUserProfileCommand
    /// </summary>
    public static UpdateUserProfileCommand ToCommand(this UpdateUserProfileResource resource, int userId)
    {
        return new UpdateUserProfileCommand
        {
            UserId = userId,
            FirstName = resource.FirstName,
            LastName = resource.LastName,
            Email = resource.Email,
            Dni = resource.Dni,
            District = resource.District
        };
    }

    /// <summary>
    /// Transform ChangePasswordResource to ChangePasswordCommand
    /// </summary>
    public static ChangePasswordCommand ToCommand(this ChangePasswordResource resource, int userId)
    {
        return new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = resource.CurrentPassword,
            NewPassword = resource.NewPassword
        };
    }

    /// <summary>
    /// Transform UserProfileInfo to UserProfileResource
    /// </summary>
    public static UserProfileResource ToResource(this UserProfileInfo profile)
    {
        return new UserProfileResource
        {
            UserId = profile.UserId,
            Username = profile.Username,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Email = profile.Email,
            Dni = profile.Dni,
            District = profile.District,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }

    /// <summary>
    /// Create GetUserProfileQuery
    /// </summary>
    public static GetUserProfileQuery ToQuery(int userId)
    {
        return new GetUserProfileQuery { UserId = userId };
    }
}


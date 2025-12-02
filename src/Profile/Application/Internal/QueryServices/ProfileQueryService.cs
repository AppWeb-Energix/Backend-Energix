using Energix.API.Profile.Domain.Model.Queries;
using Energix.API.Profile.Domain.Model.ValueObjects;
using Energix.API.Identity.Domain.Repositories;

namespace Energix.API.Profile.Application.Internal.QueryServices;

/// <summary>
/// Application service for handling profile queries
/// </summary>
public class ProfileQueryService
{
    private readonly IUserRepository _userRepository;

    public ProfileQueryService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Handle get user profile query
    /// </summary>
    public async Task<UserProfileInfo?> HandleAsync(GetUserProfileQuery query)
    {
        var user = await _userRepository.FindByIdAsync(query.UserId);
        
        if (user == null)
            return null;

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
}


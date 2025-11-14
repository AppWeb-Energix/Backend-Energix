using Energix.API.Identity.Domain.Entities;
using Energix.API.Identity.Interfaces.REST.DTOs;

namespace Energix.API.Identity.Interfaces.REST.Assemblers;

/// <summary>
/// Assembler to convert between User entity and UserResource DTO
/// </summary>
public static class UserResourceAssembler
{
    public static UserResource ToResource(User user)
    {
        return new UserResource(
            user.Id,
            user.Email,
            user.Username,
            user.FirstName,
            user.LastName,
            user.CreatedAt
        );
    }

    public static IEnumerable<UserResource> ToResourceCollection(IEnumerable<User> users)
    {
        return users.Select(ToResource);
    }
}


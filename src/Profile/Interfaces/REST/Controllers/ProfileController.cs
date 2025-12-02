using Microsoft.AspNetCore.Mvc;
using Energix.API.Profile.Application.Internal.CommandServices;
using Energix.API.Profile.Application.Internal.QueryServices;
using Energix.API.Profile.Interfaces.REST.Resources;
using Energix.API.Profile.Interfaces.REST.Transform;

namespace Energix.API.Profile.Interfaces.REST.Controllers;

/// <summary>
/// Controller for managing user profile operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly ProfileCommandService _commandService;
    private readonly ProfileQueryService _queryService;

    public ProfileController(
        ProfileCommandService commandService,
        ProfileQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    /// <summary>
    /// Get user profile information
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User profile information</returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserProfileResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProfile(int userId)
    {
        var query = ProfileResourceAssembler.ToQuery(userId);
        var profile = await _queryService.HandleAsync(query);

        if (profile == null)
            return NotFound(new { message = $"User with ID {userId} not found" });

        var resource = profile.ToResource();
        return Ok(resource);
    }

    /// <summary>
    /// Update user profile information (partial update)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="resource">Profile information to update</param>
    /// <returns>Updated user profile</returns>
    [HttpPatch("{userId}")]
    [ProducesResponseType(typeof(UserProfileResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserProfile(
        int userId,
        [FromBody] UpdateUserProfileResource resource)
    {
        try
        {
            var command = resource.ToCommand(userId);
            var updatedProfile = await _commandService.HandleAsync(command);
            var profileResource = updatedProfile.ToResource();
            
            return Ok(profileResource);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="resource">Password change information</param>
    /// <returns>Success status</returns>
    [HttpPatch("{userId}/password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        int userId,
        [FromBody] ChangePasswordResource resource)
    {
        try
        {
            var command = resource.ToCommand(userId);
            await _commandService.HandleAsync(command);
            
            return Ok(new { message = "Password changed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}


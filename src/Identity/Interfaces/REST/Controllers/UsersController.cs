using Energix.API.Identity.Domain.Services;
using Energix.API.Identity.Infrastructure.Authorization.Attributes;
using Energix.API.Identity.Interfaces.REST.Assemblers;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.Identity.Interfaces.REST.Controllers;

/// <summary>
/// REST Controller for user management operations
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserQueryService _userQueryService;

    public UsersController(IUserQueryService userQueryService)
    {
        _userQueryService = userQueryService;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userQueryService.GetByIdAsync(id);
        
        if (user == null)
            return NotFound(new { error = "Usuario no encontrado" });

        var resource = UserResourceAssembler.ToResource(user);
        return Ok(resource);
    }

    /// <summary>
    /// Get current authenticated user
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { error = "Token inválido" });

        var user = await _userQueryService.GetByIdAsync(userId);
        
        if (user == null)
            return NotFound(new { error = "Usuario no encontrado" });

        var resource = UserResourceAssembler.ToResource(user);
        return Ok(resource);
    }
}


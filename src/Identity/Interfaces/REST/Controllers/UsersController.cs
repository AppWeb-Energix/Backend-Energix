using Energix.API.Identity.Domain.Services;
using Energix.API.Identity.Infrastructure.Authorization.Extensions;
using Energix.API.Identity.Interfaces.REST.Assemblers;
using Microsoft.AspNetCore.Authorization;
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
    public IActionResult GetCurrentUser()
    {
        // Get user from HttpContext.Items (populated by UserContextMiddleware)
        var user = HttpContext.GetAuthenticatedUser();
        
        if (user == null)
            return Unauthorized(new { error = "Usuario no autenticado o no encontrado" });

        var resource = UserResourceAssembler.ToResource(user);
        return Ok(resource);
    }
}


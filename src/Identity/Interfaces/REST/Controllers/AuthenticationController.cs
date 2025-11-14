using Energix.API.Identity.Domain.Services;
using Energix.API.Identity.Interfaces.REST.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.Identity.Interfaces.REST.Controllers;

/// <summary>
/// REST Controller for authentication operations (SignIn, SignUp)
/// </summary>
[ApiController]
[Route("api/v1/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserCommandService _userCommandService;

    public AuthenticationController(IUserCommandService userCommandService)
    {
        _userCommandService = userCommandService;
    }

    /// <summary>
    /// Sign in endpoint - authenticates user and returns JWT token
    /// </summary>
    [HttpPost("sign-in")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Request body es requerido" });

        var (success, message, token, user) = await _userCommandService.SignInAsync(
            request.Email,
            request.Password
        );

        if (!success)
            return Unauthorized(new { error = message });

        return Ok(new { token, user });
    }

    /// <summary>
    /// Sign up endpoint - registers a new user
    /// </summary>
    [HttpPost("sign-up")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Request body es requerido" });

        var (success, message, userId) = await _userCommandService.SignUpAsync(
            request.Email,
            request.Password,
            request.Username,
            request.FirstName,
            request.LastName
        );

        if (!success)
            return BadRequest(new { error = message });

        return Created($"/api/v1/users/{userId}", new { id = userId, message });
    }
}


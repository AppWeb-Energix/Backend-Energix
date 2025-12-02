using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Energix.API.Notifications.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IAlertRepository _alertRepository;

    public AlertsController(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    /// <summary>
    /// Extracts userId from JWT token
    /// Supports: JwtRegisteredClaimNames.Sub, "sub", "userId", ClaimTypes.NameIdentifier
    /// </summary>
    private int GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                       ?? User.FindFirst("sub")
                       ?? User.FindFirst("userId")
                       ?? User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            var availableClaims = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
            Console.WriteLine($"❌ Claims disponibles: {availableClaims}");
            throw new UnauthorizedAccessException("Token JWT inválido: no contiene userId válido");
        }

        Console.WriteLine($"✅ UserId extraído del token: {userId}");
        return userId;
    }

    /// <summary>
    /// Get all alerts for the authenticated user, ordered by date (newest first)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAlerts()
    {
        try
        {
            var userId = GetUserIdFromToken();

            var alerts = await _alertRepository.GetByUserIdAsync(userId);

            return Ok(alerts);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [ALERTS ERROR] {ex.Message}");
            return StatusCode(500, new { message = "Error al obtener alertas" });
        }
    }
}


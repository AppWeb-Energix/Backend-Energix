using System.Security.Claims;
using Energix.API.Identity.Domain.Entities;
using Energix.API.Identity.Domain.Services;

namespace Energix.API.Identity.Infrastructure.Authorization.Middleware;

/// <summary>
/// Middleware to load the authenticated user from the database and populate HttpContext.Items
/// This provides easy access to the full User entity in controllers without repeated DB queries
/// </summary>
public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserQueryService userQueryService)
    {
        // Only attempt to load user if authenticated
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Try to get userId from claims (supports both "sub" and NameIdentifier claim types)
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? context.User.FindFirst("sub")?.Value
                              ?? context.User.FindFirst("userId")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
            {
                // Load full user entity from database
                var user = await userQueryService.GetByIdAsync(userId);

                if (user != null)
                {
                    // Populate HttpContext.Items for easy access in controllers
                    context.Items["User"] = user;
                    context.Items["UserId"] = userId;
                }
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for registering the middleware
/// </summary>
public static class UserContextMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware to load authenticated user into HttpContext.Items
    /// Should be called after UseAuthentication() and before UseAuthorization()
    /// </summary>
    public static IApplicationBuilder UseUserContext(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserContextMiddleware>();
    }
}


using Energix.API.Identity.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Energix.API.Identity.Infrastructure.Authorization.Extensions;

/// <summary>
/// Extension methods for HttpContext to easily access the authenticated user
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the authenticated User entity from HttpContext.Items
    /// Returns null if user is not authenticated or not found
    /// </summary>
    public static User? GetAuthenticatedUser(this HttpContext context)
    {
        return context.Items["User"] as User;
    }

    /// <summary>
    /// Gets the authenticated user ID from HttpContext.Items
    /// Returns null if user is not authenticated
    /// </summary>
    public static int? GetAuthenticatedUserId(this HttpContext context)
    {
        return context.Items["UserId"] as int?;
    }

    /// <summary>
    /// Checks if a user is authenticated and loaded
    /// </summary>
    public static bool HasAuthenticatedUser(this HttpContext context)
    {
        return context.Items.ContainsKey("User") && context.Items["User"] != null;
    }
}


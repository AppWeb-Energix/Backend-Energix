using Energix.API.Identity.Infrastructure.Authorization.Attributes;

namespace Energix.API.Identity.Infrastructure.Authorization.Middleware;

/// <summary>
/// Middleware to handle custom authorization using [Authorize] and [AllowAnonymous] attributes
/// </summary>
public class RequestAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public RequestAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        
        // Check if endpoint has [AllowAnonymous] attribute
        var allowAnonymous = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
        
        if (allowAnonymous)
        {
            await _next(context);
            return;
        }

        // Check if endpoint has [Authorize] attribute
        var requiresAuth = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>() != null;
        
        if (requiresAuth)
        {
            // Check if user is authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "No autorizado. Token requerido." });
                return;
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for registering the middleware
/// </summary>
public static class RequestAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestAuthorizationMiddleware>();
    }
}


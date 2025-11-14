namespace Energix.API.Identity.Infrastructure.Authorization.Attributes;

/// <summary>
/// Attribute to mark endpoints that require authentication
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AuthorizeAttribute : Attribute
{
}


namespace Energix.API.Identity.Infrastructure.Authorization.Attributes;

/// <summary>
/// Attribute to mark endpoints that allow anonymous access (bypass authorization)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AllowAnonymousAttribute : Attribute
{
}


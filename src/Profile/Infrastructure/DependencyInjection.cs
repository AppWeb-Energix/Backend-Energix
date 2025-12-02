using Energix.API.Profile.Application.Internal.CommandServices;
using Energix.API.Profile.Application.Internal.QueryServices;
using Energix.API.Profile.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Energix.API.Profile.Infrastructure;

/// <summary>
/// Configuration for Profile bounded context dependency injection
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add Profile services to the service collection
    /// </summary>
    public static IServiceCollection AddProfileServices(this IServiceCollection services)
    {
        // Domain Services
        services.AddScoped<IProfileDomainService, ProfileDomainService>();
        
        // Application Services
        services.AddScoped<ProfileCommandService>();
        services.AddScoped<ProfileQueryService>();

        return services;
    }
}


using Energix.API.AdminManagement.Application.QueryServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energix.API.AdminManagement.Infrastructure;

/// <summary>
/// Dependency injection configuration for AdminManagement bounded context
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAdminManagementServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register Query Services
        services.AddScoped<SystemStatsQueryService>();

        return services;
    }
}


using Energix.API.Personalization.Domain.Repositories;
using Energix.API.Personalization.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Energix.API.Personalization.Infrastructure;

/// <summary>
/// Dependency injection configuration for Personalization module
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPersonalizationServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IPersonalizationRepository, PersonalizationRepository>();
        
        return services;
    }
}


using Energix.API.Personalization.Application.Internal.CommandServices;
using Energix.API.Personalization.Domain.Repositories;
using Energix.API.Personalization.Domain.Services;
using Energix.API.Personalization.Infrastructure.Persistence.EFC.Repositories;
using Energix.API.Personalization.Infrastructure.Services;
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
        
        // Register services
        services.AddScoped<IPersonalizationService, PersonalizationService>();
        
        // Register command services (Application layer)
        services.AddScoped<PersonalizationCommandService>();
        
        return services;
    }
}


// language: csharp
using Energix.API.DeviceManagement.Application.Internal.CommandServices;
using Energix.API.DeviceManagement.Application.Internal.QueryServices;
using Energix.API.DeviceManagement.Domain.Repositories;
using Energix.API.DeviceManagement.Domain.Services;
using Energix.API.DeviceManagement.Infrastructure.Persistence.EFC.Repositories;
using Energix.API.DeviceManagement.Infrastructure.Services;
using Energix.API.DeviceManagement.Application.Internal.EventHandlers.Devices;
using Energix.API.DeviceManagement.Application.Internal.EventHandlers.Zones;
using Microsoft.Extensions.DependencyInjection;

namespace Energix.API.DeviceManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDeviceManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IZoneRepository, ZoneRepository>();

        services.AddScoped<IPlanValidationService, PlanValidationService>();
        services.AddScoped<IDeviceNamingService, DeviceNamingService>();

        services.AddScoped<DeviceCommandService>();
        services.AddScoped<DeviceQueryService>();
        services.AddScoped<ZoneCommandService>();
        services.AddScoped<ZoneQueryService>();

        // Event handlers
        services.AddTransient<DeviceCreatedEventHandler>();
        services.AddTransient<DeviceAssignedToZoneEventHandler>();
        services.AddTransient<DeviceRemovedFromZoneEventHandler>();
        services.AddTransient<ZoneCreatedEventHandler>();
        services.AddTransient<ZoneDeletedEventHandler>();

        return services;
    }
}
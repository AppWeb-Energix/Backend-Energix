﻿using Energix.API.AdminManagement.Application.Services;
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
        // Register Application Services
        services.AddScoped<SystemHealthService>();
        services.AddScoped<AdminDashboardService>();
        services.AddScoped<SimpleAuditService>();

        return services;
    }
}


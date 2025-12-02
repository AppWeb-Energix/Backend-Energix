using Energix.API.Notifications.Domain.Repositories;
using Energix.API.Notifications.Infrastructure.Persistence.Repositories;


namespace Energix.API.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsServices(this IServiceCollection services)
    {
        services.AddScoped<IAlertRepository, AlertRepository>();

        return services;
    }
}


using Energix.Subscriptions.Application.Commands.AddPaymentMethods;
using Energix.Subscriptions.Application.Commands.CancelPlan;
using Energix.Subscriptions.Application.Commands.ChangeBillingPeriod;
using Energix.Subscriptions.Application.Commands.ChangePlan;
using Energix.Subscriptions.Application.Commands.RemovePaymentMethod;
using Energix.Subscriptions.Application.Commands.RenewPlan;
using Energix.Subscriptions.Application.Commands.SetDefaultPaymentMethod;
using Energix.Subscriptions.Application.Commands.Subscribe;
using Energix.Subscriptions.Application.Queries.GetAvailablePlans;
using Energix.Subscriptions.Application.Queries.GetDeviceLimit;
using Energix.Subscriptions.Application.Queries.GetPaymentMethodsByUser;
using Energix.Subscriptions.Application.Queries.GetPlanComparison;
using Energix.Subscriptions.Application.Queries.GetSubscriptionByUser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energix.Subscriptions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSubscriptionsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // No necesitamos registrar SubscriptionDbContext porque ahora usamos AppDbContext unificado
        
        // Command Handlers
        services.AddScoped<AddPaymentMethodCommandHandler>();
        services.AddScoped<ChangePlanCommandHandler>();
        services.AddScoped<ChangeBillingPeriodCommandHandler>();
        services.AddScoped<RemovePaymentMethodCommandHandler>();
        services.AddScoped<SetDefaultPaymentMethodCommandHandler>();
        services.AddScoped<RenewPlanCommandHandler>();
        services.AddScoped<CancelPlanCommandHandler>();
        services.AddScoped<SubscribeCommandHandler>();

        // Query Handlers
        services.AddScoped<GetPaymentMethodsByUserQueryHandler>();
        services.AddScoped<GetSubscriptionByUserQueryHandler>();
        services.AddScoped<GetAvailablePlansQueryHandler>();
        services.AddScoped<GetPlanComparisonQueryHandler>();
        services.AddScoped<GetDeviceLimitQueryHandler>();

        // Payment Gateway
        services.AddSingleton<PaymentGateway.IPaymentGateway, PaymentGateway.FakePaymentGateway>();

        return services;
    }
}


using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Infrastructure.Persistence.EFC.Configuration.Extensions;
using Energix.API.Identity.Domain.Entities;
using Energix.API.Identity.Infrastructure.Persistence.EFC.Configuration.Extensions;
using Energix.API.Notifications.Domain.Aggregates;
using Energix.API.Notifications.Infrastructure.Persistence.Configuration;
using Energix.API.Personalization.Domain.Model.Aggregates;
using Energix.API.Personalization.Infrastructure.Persistence.EFC.Configuration.Extensions;
using Energix.Subscriptions.Domain.Aggregates;
using Energix.Subscriptions.Infrastructure.Persistance.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Energix.API;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets - Device Management
    public DbSet<Device> Devices { get; set; } = null!;
    public DbSet<Zone> Zones { get; set; } = null!;
    
    // DbSets - Personalization
    public DbSet<PersonalizationAggregate> Personalizations { get; set; } = null!;
    
    // DbSets - Identity
    public DbSet<User> Users { get; set; } = null!;
    
    // DbSets - Subscriptions
    public DbSet<Subscription> Subscriptions { get; set; } = null!;
    public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
    
    // DbSets - Notifications
    public DbSet<Alert> Alerts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Device Management Configuration
        builder.ApplyDeviceManagementConfiguration();
        
        // Personalization Configuration
        builder.ApplyPersonalizationConfiguration();
        
        // Identity Configuration
        builder.ApplyIdentityConfiguration();
        
        // Subscriptions Configuration
        builder.ApplyConfigurationsFromAssembly(typeof(SubscriptionConfiguration).Assembly);
        
        // Notifications Configuration
        builder.ApplyConfiguration(new AlertEntityConfiguration());
    }
}
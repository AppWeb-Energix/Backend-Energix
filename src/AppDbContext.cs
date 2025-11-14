using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Infrastructure.Persistence.EFC.Configuration.Extensions;
using Energix.API.Personalization.Domain.Model.Aggregates;
using Energix.API.Personalization.Infrastructure.Persistence.EFC.Configuration.Extensions;
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Device Management Configuration
        builder.ApplyDeviceManagementConfiguration();
        
        // Personalization Configuration
        builder.ApplyPersonalizationConfiguration();
    }
}
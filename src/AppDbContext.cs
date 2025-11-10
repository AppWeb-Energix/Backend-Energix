using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Infrastructure.Persistence.EFC.Configuration.Extensions;
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Device Management Configuration
        builder.ApplyDeviceManagementConfiguration();
        
    }
}
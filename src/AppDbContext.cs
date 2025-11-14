using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.Identity.Domain.Entities;  // ⬅️ ESTA LÍNEA
using Energix.API.DeviceManagement.Infrastructure.Persistence.EFC.Configuration.Extensions;
using Energix.API.Identity.Infrastructure.Persistence.EFC.Configuration.Extensions;  // ⬅️ ESTA LÍNEA
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
    
    // DbSets - Identity
    public DbSet<User> Users { get; set; } = null!;  // ⬅️ ESTA LÍNEA

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Device Management Configuration
        builder.ApplyDeviceManagementConfiguration();
        
        // Identity Configuration
        builder.ApplyIdentityConfiguration();  // ⬅️ ESTA LÍNEA
    }
}
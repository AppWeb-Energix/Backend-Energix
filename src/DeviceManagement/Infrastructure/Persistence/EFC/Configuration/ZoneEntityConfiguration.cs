using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energix.API.DeviceManagement.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Entity Framework configuration for the aggregate zone
/// </summary>
public class ZoneEntityConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable("zones");

        builder.HasKey(z => z.Id);
        builder.Property(z => z.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(z => z.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(z => z.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(z => z.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Relationship: Zone -> Devices (One-to-Many)
        // It's already configured from the Device side, but you can add additional settings here
        builder.HasMany(z => z.Devices)
            .WithOne(d => d.Zone)
            .HasForeignKey(d => d.ZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(z => z.UserId)
            .HasDatabaseName("idx_zones_user_id");

        builder.HasIndex(z => new { z.UserId, z.Name })
            .IsUnique()
            .HasDatabaseName("idx_zones_user_name_unique");
    }
}
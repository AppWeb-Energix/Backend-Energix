using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energix.API.DeviceManagement.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Entity Framework configuration for the aggregate Device
/// </summary>
public class DeviceEntityConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(d => d.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(d => d.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        // Enum: Type (manual, plug, sensor) - saved as string
        builder.Property(d => d.Type)
            .HasColumnName("type")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToLowerString(),
                v => DeviceTypeExtensions.ParseDeviceType(v)
            )
            .IsRequired();

        // Enum: Status (on, off) - saved as string
        builder.Property(d => d.Status)
            .HasColumnName("status")
            .HasMaxLength(10)
            .HasConversion(
                v => v.ToLowerString(),
                v => DeviceStatusExtensions.ParseDeviceStatus(v)
            )
            .IsRequired();

        builder.Property(d => d.Online)
            .HasColumnName("online")
            .IsRequired();

        builder.Property(d => d.LinkedAt)
            .HasColumnName("linked_at")
            .IsRequired();

        builder.Property(d => d.ZoneId)
            .HasColumnName("zone_id")
            .IsRequired(false); // Nullable

        // Enum: DeviceKind (for manuals only) - saved as string
        builder.Property(d => d.DeviceKind)
            .HasColumnName("device_kind")
            .HasMaxLength(20)
            .HasConversion(
                v => v.HasValue ? v.Value.ToLowerString() : null,
                v => v != null ? DeviceKindExtensions.ParseDeviceKind(v) : (DeviceKind?)null
            )
            .IsRequired(false); // Nullable

        // Value Object: Metrics (owned type - separate columns)
        builder.OwnsOne(d => d.Metrics, metrics =>
        {
            metrics.Property(m => m.Monthly)
                .HasColumnName("metrics_monthly")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            metrics.Property(m => m.EstimatedCost)
                .HasColumnName("metrics_estimated_cost")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            metrics.Property(m => m.Tariff)
                .HasColumnName("metrics_tariff")
                .HasColumnType("decimal(10,2)")
                .IsRequired(false);

            metrics.Property(m => m.DailyAvg)
                .HasColumnName("metrics_daily_avg")
                .HasColumnType("decimal(10,2)")
                .IsRequired();
        });

        // Relationship: Device -> Zone (Many-to-One)
        builder.HasOne(d => d.Zone)
            .WithMany(z => z.Devices)
            .HasForeignKey(d => d.ZoneId)
            .OnDelete(DeleteBehavior.SetNull); // When deleting zone, zoneId = null

        // Indexes
        builder.HasIndex(d => d.UserId)
            .HasDatabaseName("idx_devices_user_id");

        builder.HasIndex(d => d.ZoneId)
            .HasDatabaseName("idx_devices_zone_id");

        builder.HasIndex(d => new { d.UserId, d.Type })
            .HasDatabaseName("idx_devices_user_type");
    }
}
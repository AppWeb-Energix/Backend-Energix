using Energix.API.Notifications.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energix.API.Notifications.Infrastructure.Persistence.Configuration;

/// <summary>
/// Entity Framework configuration for the aggregate Alert
/// </summary>
public class AlertEntityConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alerts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.DeviceId)
            .HasColumnName("device_id")
            .IsRequired(false);

        builder.Property(a => a.Type)
            .HasColumnName("type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Message)
            .HasColumnName("message")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Composite index for optimized queries: WHERE user_id = X ORDER BY created_at DESC
        builder.HasIndex(a => new { a.UserId, a.CreatedAt })
            .HasDatabaseName("ix_alerts_user_id_created_at");
    }
}


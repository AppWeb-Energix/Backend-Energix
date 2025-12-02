using Energix.API.AdminManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energix.API.AdminManagement.Infrastructure.Persistence.EFC.Configuration;

public class SimpleAuditLogConfiguration : IEntityTypeConfiguration<SimpleAuditLog>
{
    public void Configure(EntityTypeBuilder<SimpleAuditLog> builder)
    {
        builder.ToTable("simple_audit_logs");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedOnAdd();
        
        builder.Property(a => a.Message)
            .HasColumnName("message")
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}


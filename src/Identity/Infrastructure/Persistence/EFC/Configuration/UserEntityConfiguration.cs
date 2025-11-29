using Energix.API.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energix.API.Identity.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Entity Framework configuration for User entity
/// </summary>
public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Dni)
            .HasColumnName("dni")
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(u => u.District)
            .HasColumnName("district")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasColumnType("varchar(20)")
            .IsRequired()
            .HasDefaultValue("Customer");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);

        // Indexes
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("idx_users_username_unique");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("idx_users_email_unique");

        builder.HasIndex(u => u.Dni)
            .IsUnique()
            .HasDatabaseName("idx_users_dni_unique");
    }
}
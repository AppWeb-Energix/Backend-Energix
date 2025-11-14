using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Energix.API.Personalization.Domain.Model.Aggregates;

namespace Energix.API.Personalization.Infrastructure.Persistence.EFC.Configuration;

public class PersonalizationConfiguration : IEntityTypeConfiguration<PersonalizationAggregate>
{
    public void Configure(EntityTypeBuilder<PersonalizationAggregate> builder)
    {
        builder.ToTable("Personalizations");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(p => p.UserId)
            .IsRequired();
        
        builder.HasIndex(p => p.UserId)
            .IsUnique();
        
        builder.Property(p => p.KpiCurrent)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(p => p.KpiCost)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(p => p.KpiMonthly)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(p => p.ChartHourly)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(p => p.ChartMonthly)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(p => p.ChartDevice)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(p => p.CreatedAt)
            .IsRequired();
        
        builder.Property(p => p.UpdatedAt)
            .IsRequired();
    }
}


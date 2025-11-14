using Energix.Subscriptions.Domain.Aggregates;
using Energix.Subscriptions.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energix.Subscriptions.Infrastructure.Persistance.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethods");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Id)
            .ValueGeneratedNever();

        builder.Property(pm => pm.UserId)
            .IsRequired();

        builder.Property(pm => pm.CardHolderName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pm => pm.ExpiryDate)
            .IsRequired();

        builder.Property(pm => pm.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pm => pm.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pm => pm.CreatedAt)
            .IsRequired();

        builder.Property(pm => pm.UpdatedAt)
            .IsRequired(false);

        builder.OwnsOne(pm => pm.MaskedCardNumber, mcn =>
        {
            mcn.Property(x => x.Value)
                .HasColumnName("MaskedCardNumber")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.OwnsOne(pm => pm.CardBrand, cb =>
        {
            cb.Property(x => x.Name)
                .HasColumnName("CardBrand")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.HasIndex(pm => pm.UserId);
        builder.HasIndex(pm => new { pm.UserId, pm.IsDefault });
    }
}


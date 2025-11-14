using Energix.Subscriptions.Domain.Aggregates;
using Energix.Subscriptions.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energix.Subscriptions.Infrastructure.Persistance.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.PlanType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.BillingPeriod)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.StartDate)
            .IsRequired();

        builder.Property(s => s.EndDate)
            .IsRequired(false);

        builder.Property(s => s.NextBillingDate)
            .IsRequired(false);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.AutoRenew)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired(false);

        builder.OwnsOne(s => s.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("PriceAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.HasMany<PaymentMethod>("_paymentMethods")
            .WithOne()
            .HasForeignKey(pm => pm.UserId)
            .HasPrincipalKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation("_paymentMethods")!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(s => s.UserId)
            .IsUnique();
    }
}


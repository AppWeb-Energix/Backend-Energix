using Energix.Subscriptions.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Infrastructure.Persistance;

public class SubscriptionDbContext : DbContext
{
    public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Subscription> Subscriptions { get; set; } = null!;
    public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubscriptionDbContext).Assembly);
    }
}


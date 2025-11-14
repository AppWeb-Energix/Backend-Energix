using Microsoft.EntityFrameworkCore;

namespace Energix.API.Identity.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyIdentityConfiguration(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new UserEntityConfiguration());
        return builder;
    }
}
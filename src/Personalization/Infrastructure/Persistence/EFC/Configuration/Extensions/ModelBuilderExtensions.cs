using Microsoft.EntityFrameworkCore;

namespace Energix.API.Personalization.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyPersonalizationConfiguration(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new PersonalizationConfiguration());
    }
}


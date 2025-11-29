using Energix.API.AdminManagement.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Energix.API.AdminManagement.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyAdminManagementConfiguration(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new SimpleAuditLogConfiguration());
    }
}


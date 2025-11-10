using Microsoft.EntityFrameworkCore;

namespace Energix.API.DeviceManagement.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyDeviceManagementConfiguration(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new DeviceEntityConfiguration());
        builder.ApplyConfiguration(new ZoneEntityConfiguration());
        return builder;
    }
}
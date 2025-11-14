using Energix.Subscriptions.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Queries.GetDeviceLimit;

/// <summary>
/// Handler para verificar límites de dispositivos
/// </summary>
public class GetDeviceLimitQueryHandler
{
    private readonly SubscriptionDbContext _context;

    public GetDeviceLimitQueryHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceLimitResult> HandleAsync(
        GetDeviceLimitQuery query,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == query.UserId && s.IsActive, cancellationToken);

        if (subscription == null)
        {
            return new DeviceLimitResult
            {
                CanAddDevice = false,
                MaxDevices = 0,
                CurrentDevices = query.CurrentDeviceCount,
                RemainingSlots = 0,
                HasUnlimitedDevices = false,
                PlanType = "None",
                Message = "No se encontró suscripción activa"
            };
        }

        var planSchema = subscription.GetPlanSchema();
        var hasUnlimitedDevices = planSchema.MaxDevices == int.MaxValue;
        var canAddDevice = subscription.CanAddDevice(query.CurrentDeviceCount);
        var remainingSlots = subscription.GetRemainingDeviceSlots(query.CurrentDeviceCount);

        string message;
        if (canAddDevice)
        {
            message = hasUnlimitedDevices
                ? "Puedes agregar dispositivos ilimitados"
                : "Puedes agregar " + remainingSlots.ToString() + " dispositivo(s) más";
        }
        else
        {
            message = "Has alcanzado el límite de " + planSchema.MaxDevices.ToString() + " dispositivos. Considera mejorar tu plan.";
        }

        return new DeviceLimitResult
        {
            CanAddDevice = canAddDevice,
            MaxDevices = hasUnlimitedDevices ? -1 : planSchema.MaxDevices,
            CurrentDevices = query.CurrentDeviceCount,
            RemainingSlots = hasUnlimitedDevices ? -1 : remainingSlots,
            HasUnlimitedDevices = hasUnlimitedDevices,
            PlanType = $"{subscription.PlanType}",
            Message = message
        };
    }
}


using Energix.API;
using Energix.Subscriptions.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Energix.Subscriptions.Application.Queries.GetDeviceLimit;

public class GetDeviceLimitQueryHandler
{
    private readonly AppDbContext _context;

    public GetDeviceLimitQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, int? Limit, string Message)> HandleAsync(
        GetDeviceLimitQuery query,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Subscriptions
            .Where(s => s.UserId == query.UserId && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
        {
            return (false, null, "No se encontró una suscripción activa para este usuario");
        }

        var schema = subscription.GetPlanSchema();
        return (true, schema.MaxDevices, "Límite de dispositivos obtenido correctamente");
    }
}


using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Domain.Model.Queries.Zones;
using Energix.API.DeviceManagement.Domain.Repositories;

namespace Energix.API.DeviceManagement.Application.Internal.QueryServices;

/// <summary>
/// Application service for handling zone-related queries
/// </summary>
public class ZoneQueryService
{
    private readonly IZoneRepository _zoneRepository;

    public ZoneQueryService(IZoneRepository zoneRepository)
    {
        _zoneRepository = zoneRepository;
    }

    /// <summary>
    /// It retrieves all of a user's zones
    /// </summary>
    public async Task<IEnumerable<Zone>> Handle(GetZonesByUserIdQuery query)
    {
        return await _zoneRepository.FindByUserIdAsync(query.UserId);
    }

    /// <summary>
    /// Gets a specific zone by ID
    /// </summary>
    public async Task<Zone?> Handle(GetZoneByIdQuery query)
    {
        return await _zoneRepository.FindByIdAsync(query.ZoneId);
    }

    /// <summary>
    /// It captures all of a user's zones, including their devices
    /// </summary>
    public async Task<IEnumerable<Zone>> Handle(GetZonesWithDevicesQuery query)
    {
        return await _zoneRepository.FindByUserIdWithDevicesAsync(query.UserId);
    }
}
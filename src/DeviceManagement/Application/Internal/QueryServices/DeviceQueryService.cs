﻿using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Domain.Model.Queries.Devices;
using Energix.API.DeviceManagement.Domain.Repositories;
using Energix.API.DeviceManagement.Domain.Services;

namespace Energix.API.DeviceManagement.Application.Internal.QueryServices;

/// <summary>
/// Application service for handling device-related queries
/// </summary>
public class DeviceQueryService : IDeviceQueryService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IPlanValidationService _planValidationService;

    public DeviceQueryService(
        IDeviceRepository deviceRepository,
        IPlanValidationService planValidationService)
    {
        _deviceRepository = deviceRepository;
        _planValidationService = planValidationService;
    }
    /// <summary>
    /// Retrieves all of a user's devices (with optional filter by type)
    /// </summary>
    public async Task<IEnumerable<Device>> Handle(GetDevicesByUserIdQuery query)
    {
        if (query.Type.HasValue)
        {
            return await _deviceRepository.FindByUserIdAndTypeAsync(query.UserId, query.Type.Value);
        }
        
        return await _deviceRepository.FindByUserIdAsync(query.UserId);
    }

    /// <summary>
    /// Get a specific device by ID
    /// </summary>
    public async Task<Device?> Handle(GetDeviceByIdQuery query)
    {
        return await _deviceRepository.FindByIdAsync(query.DeviceId);
    }

    /// <summary>
    /// Retrieves devices assigned to a zone
    /// </summary>
    public async Task<IEnumerable<Device>> Handle(GetDevicesByZoneIdQuery query)
    {
        return await _deviceRepository.FindByZoneIdAsync(query.ZoneId);
    }

    /// <summary>
    /// Obtains unassigned zone devices from a user
    /// </summary>
    public async Task<IEnumerable<Device>> Handle(GetDevicesWithoutZoneQuery query)
    {
        return await _deviceRepository.FindWithoutZoneByUserIdAsync(query.UserId);
    }

    /// <summary>
    /// Count the number of devices a user has
    /// </summary>
    public async Task<int> Handle(CountDevicesByUserIdQuery query)
    {
        return await _deviceRepository.CountByUserIdAsync(query.UserId);
    }

    /// <summary>
    /// Check if a user can add more devices according to their plan
    /// </summary>
    public async Task<bool> Handle(CanAddDeviceQuery query)
    {
        var currentCount = await _deviceRepository.CountByUserIdAsync(query.UserId);
        return await _planValidationService.CanAddDeviceAsync(query.UserId, query.Plan, currentCount);
    }
}
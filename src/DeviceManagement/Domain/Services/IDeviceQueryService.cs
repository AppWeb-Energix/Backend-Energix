using Energix.API.DeviceManagement.Domain.Model.Aggregates;
using Energix.API.DeviceManagement.Domain.Model.Queries.Devices;

namespace Energix.API.DeviceManagement.Domain.Services;

/// <summary>
/// Service for querying devices
/// </summary>
public interface IDeviceQueryService
{
    /// <summary>
    /// Get all devices for a specific user (with optional filter by type)
    /// </summary>
    Task<IEnumerable<Device>> Handle(GetDevicesByUserIdQuery query);
    
    /// <summary>
    /// Get a specific device by ID
    /// </summary>
    Task<Device?> Handle(GetDeviceByIdQuery query);
}


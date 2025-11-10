using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Domain.Repositories;
using Energix.API.DeviceManagement.Domain.Services;

namespace Energix.API.DeviceManagement.Infrastructure.Services;

/// <summary>
/// Implementation of the automatic device naming service
/// </summary>
public class DeviceNamingService : IDeviceNamingService
{
    private readonly IDeviceRepository _deviceRepository;
    
    private const string DEVICE_NAME_PREFIX = "Dispositivo";
    private const int MAX_NAME_LENGTH = 50;
    private const int MIN_NAME_LENGTH = 1;

    public DeviceNamingService(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    /// <summary>
    /// Generates an automatic name for a device based on the current type and quantity.
    /// </summary>
    public async Task<string> GenerateDeviceNameAsync(int userId, DeviceType deviceType, int currentCount)
    {
        var nextNumber = await GetNextDeviceNumberAsync(userId, deviceType);
        return $"{DEVICE_NAME_PREFIX} {nextNumber}";
    }

    /// <summary>
    /// Validate whether a device name is valid
    /// </summary>
    public bool IsValidDeviceName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var trimmedName = name.Trim();
        
        if (trimmedName.Length < MIN_NAME_LENGTH || trimmedName.Length > MAX_NAME_LENGTH)
            return false;

        return true;
    }

    /// <summary>
    /// Get the next available number for a device
    /// </summary>
    public async Task<int> GetNextDeviceNumberAsync(int userId, DeviceType deviceType)
    {
        // Retrieve all user devices of the specified type
        var devices = await _deviceRepository.FindByUserIdAndTypeAsync(userId, deviceType);
        
        // Count how many devices you have
        var count = devices.Count();
        
        return count + 1;
    }
}
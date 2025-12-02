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
    public Task<string> GenerateDeviceNameAsync(int userId, DeviceType deviceType, int nextNumber, DeviceKind? deviceKind = null)
    {
        var prefix = deviceType switch
        {
            DeviceType.Manual when deviceKind.HasValue => GetManualPrefix(deviceKind.Value),
            DeviceType.Manual => "Dispositivo manual",
            DeviceType.Plug => "Enchufe",
            DeviceType.Sensor => "Sensor",
            _ => DEVICE_NAME_PREFIX
        };

        return Task.FromResult($"{prefix} {nextNumber}");
    }

    private static string GetManualPrefix(DeviceKind kind)
    {
        return kind switch
        {
            DeviceKind.Refrigerator => "Refrigerador",
            DeviceKind.Washer => "Lavadora",
            DeviceKind.Tv => "TV",
            DeviceKind.Pc => "PC",
            DeviceKind.Lights => "Luces",
            DeviceKind.Other => "Dispositivo",
            _ => DEVICE_NAME_PREFIX
        };
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
        var devices = await _deviceRepository.FindByUserIdAndTypeAsync(userId, deviceType);
        var count = devices.Count();
        return count + 1;
    }
}

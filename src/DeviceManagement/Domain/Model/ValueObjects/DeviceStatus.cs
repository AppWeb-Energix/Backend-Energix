namespace Energix.API.DeviceManagement.Domain.Model.ValueObjects;

/// <summary>
/// Device status
/// </summary>
public enum DeviceStatus
{
    /// <summary>
    /// Device off
    /// </summary>
    Off = 0,
    
    /// <summary>
    /// Device on
    /// </summary>
    On = 1
}

/// <summary>
/// Extension methods for DeviceStatus
/// </summary>
public static class DeviceStatusExtensions
{
    /// <summary>
    /// Convert DeviceStatus to a string for JSON
    /// </summary>
    public static string ToLowerString(this DeviceStatus status)
    {
        return status switch
        {
            DeviceStatus.Off => "off",
            DeviceStatus.On => "on",
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
    }

    /// <summary>
    /// Convert string to DeviceStatus
    /// </summary>
    public static DeviceStatus ParseDeviceStatus(string value)
    {
        return value?.ToLower() switch
        {
            "off" => DeviceStatus.Off,
            "on" => DeviceStatus.On,
            _ => throw new ArgumentException($"Estado de dispositivo inválido: {value}")
        };
    }
}
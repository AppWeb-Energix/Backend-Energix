namespace Energix.API.DeviceManagement.Domain.Model.ValueObjects;

/// <summary>
/// Device type according to user plan
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// Manual Device (Basic Plan) - Requires manual metrics
    /// </summary>
    Manual = 0,
    
    /// <summary>
    /// Smart plug device (Student Plan) - Automatic metering
    /// </summary>
    Plug = 1,
    
    /// <summary>
    /// Sensor device (Family Plan) - Advanced automatic measurement
    /// </summary>
    Sensor = 2
    
}

/// <summary>
/// Extension methods for DeviceType
/// </summary>
public static class DeviceTypeExtensions
{
    /// <summary>
    /// Convert DeviceType to string for JSON
    /// </summary>
    public static string ToLowerString(this DeviceType type)
    {
        return type switch
        {
            DeviceType.Manual => "manual",
            DeviceType.Plug => "plug",
            DeviceType.Sensor => "sensor",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    /// <summary>
    /// Convert string to DeviceType
    /// </summary>
    public static DeviceType ParseDeviceType(string value)
    {
        return value?.ToLower() switch
        {
            "manual" => DeviceType.Manual,
            "plug" => DeviceType.Plug,
            "sensor" => DeviceType.Sensor,
            _ => throw new ArgumentException($"Tipo de dispositivo inválido: {value}")
        };
    }
}


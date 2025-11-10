namespace Energix.API.DeviceManagement.Domain.Model.ValueObjects;

/// <summary>
/// Handheld device category
/// </summary>
public enum DeviceKind
{
    Refrigerator = 0,
        
    Washer = 1,
    
    Tv = 2,
    
    Pc = 3,
    
    Lights = 4,
    
    Other = 5
}

/// <summary>
/// Extension methods for DeviceStatus
/// </summary>
public static class DeviceKindExtensions
{
    /// <summary>
    /// Convert DeviceKind to a string for JSON
    /// </summary>
    public static string ToLowerString(this DeviceKind kind)
    {
        return kind switch
        {
            DeviceKind.Refrigerator => "refrigerator",
            DeviceKind.Washer => "washer",
            DeviceKind.Tv => "tv",
            DeviceKind.Pc => "pc",
            DeviceKind.Lights => "lights",
            DeviceKind.Other => "other",
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }

    /// <summary>
    /// Convert string to DeviceKind
    /// </summary>
    public static DeviceKind ParseDeviceKind(string value)
    {
        return value?.ToLower() switch
        {
            "refrigerator" => DeviceKind.Refrigerator,
            "washer" => DeviceKind.Washer,
            "tv" => DeviceKind.Tv,
            "pc" => DeviceKind.Pc,
            "lights" => DeviceKind.Lights,
            "other" => DeviceKind.Other,
            _ => throw new ArgumentException($"Tipo de dispositivo manual inválido: {value}")
        };
    }
}
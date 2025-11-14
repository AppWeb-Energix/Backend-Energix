namespace Energix.Subscriptions.Application.Queries.GetDeviceLimit;

/// <summary>
/// Resultado con información de límites de dispositivos
/// </summary>
public class DeviceLimitResult
{
    public bool CanAddDevice { get; set; }
    public int MaxDevices { get; set; }
    public int CurrentDevices { get; set; }
    public int RemainingSlots { get; set; }
    public bool HasUnlimitedDevices { get; set; }
    public string PlanType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}


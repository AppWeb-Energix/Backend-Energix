using Energix.API.DeviceManagement.Domain.Model.ValueObjects;

namespace Energix.API.DeviceManagement.Domain.Model.Aggregates;

/// <summary>
/// Device Aggregate Root
/// It represents a device linked to a user
/// </summary>
public class Device
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Name { get; private set; }
    public DeviceType Type { get; private set; }
    public DeviceStatus Status { get; private set; }
    public bool Online { get; private set; }
    public DateTime LinkedAt { get; private set; }
    public int? ZoneId { get; private set; }
    
    // For manual devices only (type = manual)
    public DeviceKind? DeviceKind { get; private set; }
    public DeviceMetrics? Metrics { get; private set; }
    
    // Navigation property (optional, para EF Core)
    public virtual Zone? Zone { get; private set; }
    
    protected Device()
    {
        Name = string.Empty;
    }
    public Device(
        int userId, 
        string name, 
        DeviceType type)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId debe ser mayor a 0", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name no puede estar vacío", nameof(name));
        
        if (type == DeviceType.Manual)
            throw new ArgumentException("Use el constructor con métricas para dispositivos manuales", nameof(type));

        UserId = userId;
        Name = name.Trim();
        Type = type;
        Status = DeviceStatus.Off;
        Online = false;
        LinkedAt = DateTime.UtcNow;
        ZoneId = null;
        DeviceKind = null;
        Metrics = null;
    }
    
    public Device(
        int userId,
        string name,
        DeviceKind deviceKind,
        DeviceMetrics metrics)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId debe ser mayor a 0", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name no puede estar vacío", nameof(name));
        
        if (metrics == null)
            throw new ArgumentNullException(nameof(metrics));

        UserId = userId;
        Name = name.Trim();
        Type = DeviceType.Manual;
        Status = DeviceStatus.Off;
        Online = false;
        LinkedAt = DateTime.UtcNow;
        ZoneId = null;
        DeviceKind = deviceKind;
        Metrics = metrics;
    }
    
    /// <summary>
    /// Change the device name
    /// </summary>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("El nuevo nombre no puede estar vacío", nameof(newName));

        Name = newName.Trim();
    }

    /// <summary>
    /// Turn on the device (plug/sensor only)
    /// </summary>
    public void TurnOn()
    {
        if (Type == DeviceType.Manual)
            throw new InvalidOperationException("No se puede encender un dispositivo manual");

        Status = DeviceStatus.On;
        Online = true;
    }

    /// <summary>
    /// Turn off the device (plug/sensor only)
    /// </summary>
    public void TurnOff()
    {
        if (Type == DeviceType.Manual)
            throw new InvalidOperationException("No se puede apagar un dispositivo manual");

        Status = DeviceStatus.Off;
        Online = false;
    }

    /// <summary>
    /// Toggle the device state (on/off)
    /// </summary>
    public void TogglePower()
    {
        if (Type == DeviceType.Manual)
            throw new InvalidOperationException("No se puede alternar el estado de un dispositivo manual");

        if (Status == DeviceStatus.On)
        {
            TurnOff();
        }
        else
        {
            TurnOn();
        }
    }

    /// <summary>
    /// Assign the device to a zone (family plan only)
    /// </summary>
    public void AssignToZone(int? zoneId)
    {
        ZoneId = zoneId;
    }

    /// <summary>
    /// Update metrics (manual devices only)
    /// </summary>
    public void UpdateMetrics(DeviceMetrics newMetrics)
    {
        if (Type != DeviceType.Manual)
            throw new InvalidOperationException("Solo los dispositivos manuales tienen métricas");

        if (newMetrics == null)
            throw new ArgumentNullException(nameof(newMetrics));

        Metrics = newMetrics;
    }

    /// <summary>
    /// Check if the device belongs to a zone
    /// </summary>
    public bool IsInZone() => ZoneId.HasValue;

    /// <summary>
    /// Check if the device is turned on
    /// </summary>
    public bool IsOn() => Status == DeviceStatus.On && Online;

    /// <summary>
    /// Check if the device is manual.
    /// </summary>
    public bool IsManual() => Type == DeviceType.Manual;

    /// <summary>
    /// Check if the device is automatic (plug or sensor)
    /// </summary>
    public bool IsAutomatic() => Type == DeviceType.Plug || Type == DeviceType.Sensor;
}
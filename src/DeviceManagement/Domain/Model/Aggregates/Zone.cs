namespace Energix.API.DeviceManagement.Domain.Model.Aggregates;

/// <summary>
/// Zone Aggregate Root
/// It represents an area for organizing devices (family plan only)
/// </summary>
public class Zone
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Navigation property - Devices assigned to this area
    public virtual ICollection<Device> Devices { get; private set; }
    
    protected Zone()
    {
        Name = string.Empty;
        Devices = new List<Device>();
    }
    
    public Zone(int userId, string name)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId debe ser mayor a 0", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name no puede estar vacío", nameof(name));

        if (name.Length > 50)
            throw new ArgumentException("Name no puede exceder 50 caracteres", nameof(name));

        UserId = userId;
        Name = name.Trim();
        CreatedAt = DateTime.UtcNow;
        Devices = new List<Device>();
    }

    /// <summary>
    /// Change the area name
    /// </summary>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("El nuevo nombre no puede estar vacío", nameof(newName));

        if (newName.Length > 50)
            throw new ArgumentException("El nombre no puede exceder 50 caracteres", nameof(newName));

        Name = newName.Trim();
    }

    /// <summary>
    /// Add a device to the zone (for navigation only)
    /// The actual assignment is done from the Device
    /// </summary>
    internal void AddDevice(Device device)
    {
        if (device == null)
            throw new ArgumentNullException(nameof(device));

        if (device.UserId != UserId)
            throw new InvalidOperationException("El dispositivo debe pertenecer al mismo usuario");

        if (!Devices.Contains(device))
        {
            Devices.Add(device);
        }
    }

    /// <summary>
    /// Remove a device from the area (for navigation only)
    /// The actual disassignment is done from the Device
    /// </summary>
    internal void RemoveDevice(Device device)
    {
        if (device == null)
            throw new ArgumentNullException(nameof(device));

        Devices.Remove(device);
    }

    /// <summary>
    /// It retrieves the number of assigned devices
    /// </summary>
    public int GetDeviceCount()
    {
        return Devices?.Count ?? 0;
    }

    /// <summary>
    /// Check if the area has assigned devices
    /// </summary>
    public bool HasDevices()
    {
        return GetDeviceCount() > 0;
    }

    /// <summary>
    /// It gets all the devices turned on in the area
    /// </summary>
    public IEnumerable<Device> GetActiveDevices()
    {
        return Devices?.Where(d => d.IsOn()) ?? Enumerable.Empty<Device>();
    }

    /// <summary>
    /// It gets all devices turned off in the area
    /// </summary>
    public IEnumerable<Device> GetInactiveDevices()
    {
        return Devices?.Where(d => !d.IsOn()) ?? Enumerable.Empty<Device>();
    }

    /// <summary>
    /// Check if the area belongs to a specific user
    /// </summary>
    public bool BelongsTo(int userId)
    {
        return UserId == userId;
    }
}
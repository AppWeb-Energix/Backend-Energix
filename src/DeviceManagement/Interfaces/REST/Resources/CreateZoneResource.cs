namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Resource to create a zone
/// </summary>
public record CreateZoneResource(
    int? UserId,
    string Name
);
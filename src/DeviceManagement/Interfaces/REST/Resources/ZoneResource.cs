namespace Energix.API.DeviceManagement.Interfaces.REST.Resources;

/// <summary>
/// Resource to represent an area in the responses
/// </summary>
public record ZoneResource(
    int Id,
    int UserId,
    string Name,
    DateTime CreatedAt
);
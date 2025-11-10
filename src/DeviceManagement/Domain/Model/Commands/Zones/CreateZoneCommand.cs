namespace Energix.API.DeviceManagement.Domain.Model.Commands.Zones;

/// <summary>
/// Command to create a zone (family plan only)
/// </summary>
public record CreateZoneCommand(
    int UserId,
    string Name
);
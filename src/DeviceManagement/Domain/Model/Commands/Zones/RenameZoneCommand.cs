namespace Energix.API.DeviceManagement.Domain.Model.Commands.Zones;

/// <summary>
/// Command to rename an area
/// </summary>
public record RenameZoneCommand(
    int ZoneId,
    string NewName,
    int UserId
);
namespace Energix.API.DeviceManagement.Domain.Model.Commands.Zones;

/// <summary>
/// Command to delete a zone
/// </summary>
public record DeleteZoneCommand(
    int ZoneId,
    int UserId
);
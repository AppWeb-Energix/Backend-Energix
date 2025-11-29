namespace Energix.API.AdminManagement.Domain;

/// <summary>
/// Aggregate Root for Admin Management bounded context
/// </summary>
public class AdminManagementAggregate
{
    public int Id { get; set; }
    public string SystemStatus { get; set; } = "Operational";
}


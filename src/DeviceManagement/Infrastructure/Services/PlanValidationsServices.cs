using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Domain.Services;

namespace Energix.API.DeviceManagement.Infrastructure.Services;

/// <summary>
/// Implementation of the business rule validation service by plan
/// </summary>
public class PlanValidationService : IPlanValidationService
{
    private const int BASIC_LIMIT = 2;
    private const int STUDENT_LIMIT = 2;
    private const int FAMILY_LIMIT = int.MaxValue; // Ilimitado

    /// <summary>
    /// Check if a user can add more devices based on their plan
    /// </summary>
    public Task<bool> CanAddDeviceAsync(int userId, PlanType plan, int currentDeviceCount)
    {
        var limit = GetDeviceLimit(plan);
        var canAdd = currentDeviceCount < limit;
        return Task.FromResult(canAdd);
    }

    /// <summary>
    /// You get the device limit according to the plan
    /// </summary>
    public int GetDeviceLimit(PlanType plan)
    {
        return plan switch
        {
            PlanType.Basic => BASIC_LIMIT,
            PlanType.Student => STUDENT_LIMIT,
            PlanType.Family => FAMILY_LIMIT,
            _ => throw new ArgumentException($"Plan type inválido: {plan}")
        };
    }

    /// <summary>
    /// You get the type of device allowed according to the plan
    /// </summary>
    public DeviceType GetDeviceTypeForPlan(PlanType plan)
    {
        return plan switch
        {
            PlanType.Basic => DeviceType.Manual,
            PlanType.Student => DeviceType.Plug,
            PlanType.Family => DeviceType.Sensor,
            _ => throw new ArgumentException($"Plan type inválido: {plan}")
        };
    }

    /// <summary>
    /// Check if a plan allows renaming devices
    /// </summary>
    public bool CanRenameDevice(PlanType plan)
    {
        return plan switch
        {
            PlanType.Basic => false,      // You cannot rename
            PlanType.Student => true,     // You can rename
            PlanType.Family => true,      // You can rename
            _ => throw new ArgumentException($"Plan type inválido: {plan}")
        };
    }

    /// <summary>
    /// Check if a plan allows toggle power (on/off)
    /// </summary>
    public bool CanTogglePower(PlanType plan)
    {
        return plan switch
        {
            PlanType.Basic => false,      // Can't control power
            PlanType.Student => true,     // Can control power
            PlanType.Family => true,      // Can control power
            _ => throw new ArgumentException($"Plan type inválido: {plan}")
        };
    }

    /// <summary>
    /// Check if a plan allows you to manage zones
    /// </summary>
    public bool CanManageZones(PlanType plan)
    {
        return plan switch
        {
            PlanType.Basic => false,      // Cannot manage zones
            PlanType.Student => false,    // Cannot manage zones
            PlanType.Family => true,      // Can manage zones
            _ => throw new ArgumentException($"Plan type inválido: {plan}")
        };
    }

    /// <summary>
    /// Check if a transaction is allowed for a specific plan
    /// </summary>
    public bool IsOperationAllowed(PlanType plan, string operation)
    {
        return operation?.ToLower() switch
        {
            "rename" => CanRenameDevice(plan),
            "togglepower" => CanTogglePower(plan),
            "managezones" => CanManageZones(plan),
            _ => throw new ArgumentException($"Operación desconocida: {operation}")
        };
    }
}
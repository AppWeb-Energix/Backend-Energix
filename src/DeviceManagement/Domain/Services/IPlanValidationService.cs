using Energix.API.DeviceManagement.Domain.Model.ValueObjects;

namespace Energix.API.DeviceManagement.Domain.Services;

/// <summary>
/// Domain service to validate business rules according to the user's plan
/// </summary>
public interface IPlanValidationService
{
    /// <summary>
    /// Check if a user can add more devices based on their plan.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="plan">User plan</param>
    /// <param name="currentDeviceCount">Current number of devices</param>
    /// <returns>True if you can add more, False if you've reached the limit</returns>
    Task<bool> CanAddDeviceAsync(int userId, PlanType plan, int currentDeviceCount);
    
    /// <summary>
    /// You get the device limit according to the plan
    /// </summary>
    /// <param name="plan">User plan</param>
    /// <returns>Maximum number of devices (int.MaxValue for unlimited)</returns>
    int GetDeviceLimit(PlanType plan);
    
    /// <summary>
    /// You get the type of device allowed according to the plan
    /// </summary>
    /// <param name="plan">User plan</param>
    /// <returns>Device type (manual, plug, sensor)</returns>
    DeviceType GetDeviceTypeForPlan(PlanType plan);
    
    /// <summary>
    /// Check if a plan allows renaming devices
    /// </summary>
    /// <param name="plan">User plan</param>
    /// <returns>True, it can rename</returns>
    bool CanRenameDevice(PlanType plan);
    
    /// <summary>
    /// Check if a plan allows toggle power (on/off)
    /// </summary>
    /// <param name="plan">User plan</param>
    /// <returns>True, it can control on/off</returns>
    bool CanTogglePower(PlanType plan);
    
    /// <summary>
    /// Check if a plan allows you to manage zones
    /// </summary>
    /// <param name="plan">User plan</param>
    /// <returns>True if you can create and assign zones</returns>
    bool CanManageZones(PlanType plan);
    
    /// <summary>
    /// Check if a transaction is allowed for a specific plan
    /// </summary>
    /// <param name="plan">User plan</param>
    /// <param name="operation">Operation name (rename, togglePower, manageZones)</param>
    /// <returns>True if the operation is allowed</returns>
    bool IsOperationAllowed(PlanType plan, string operation);
}
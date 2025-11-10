using Energix.API.DeviceManagement.Application.Internal.CommandServices;
using Energix.API.DeviceManagement.Application.Internal.QueryServices;
using Energix.API.DeviceManagement.Domain.Model.Commands;
using Energix.API.DeviceManagement.Domain.Model.Commands.Devices;
using Energix.API.DeviceManagement.Domain.Model.Queries;
using Energix.API.DeviceManagement.Domain.Model.Queries.Devices;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Interfaces.REST.Resources;
using Energix.API.DeviceManagement.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.DeviceManagement.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly DeviceCommandService _commandService;
    private readonly DeviceQueryService _queryService;

    public DevicesController(
        DeviceCommandService commandService,
        DeviceQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    /// <summary>
    /// It retrieves all of a user's devices
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="type">Device type (optional): manual, plug, sensor</param>
    [HttpGet]
    public async Task<IActionResult> GetDevicesByUserId(
        [FromQuery] int userId,
        [FromQuery] string? type = null)
    {
        DeviceType? deviceType = null;
        if (!string.IsNullOrEmpty(type))
        {
            try
            {
                deviceType = DeviceTypeExtensions.ParseDeviceType(type);
            }
            catch
            {
                return BadRequest(new { message = "Tipo de dispositivo inválido. Use: manual, plug, sensor" });
            }
        }

        var query = new GetDevicesByUserIdQuery(userId, deviceType);
        var devices = await _queryService.Handle(query);
        var resources = devices.Select(DeviceResourceFromEntityAssembler.ToResourceFromEntity);
        
        return Ok(resources);
    }

    /// <summary>
    /// Get a device by your ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDeviceById(int id)
    {
        var query = new GetDeviceByIdQuery(id);
        var device = await _queryService.Handle(query);

        if (device == null)
            return NotFound(new { message = "Dispositivo no encontrado" });

        var resource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
        return Ok(resource);
    }

    /// <summary>
    /// Create a new device
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="plan">User Plan: basic (manual), student (plug), family (sensor)
    /// NOTE: This parameter is temporary until the Subscriptions bounded context is implemented</param>
    /// <param name="resource">Device data
    /// - Plan basic: name is generated automatically, requires deviceKind, monthly, estimatedCost
    /// - Plan student/family: It allows you to customize the name; it does NOT require metric fields</param>
    [HttpPost]
    public async Task<IActionResult> CreateDevice(
        [FromQuery] int userId,
        [FromQuery] string plan,
        [FromBody] CreateDeviceResource resource)
    {
        PlanType userPlan;
        try
        {
            userPlan = PlanTypeExtensions.ParsePlanType(plan);
        }
        catch
        {
            return BadRequest(new { message = "Plan inválido. Use: basic, student, family" });
        }

        var command = CreateDeviceCommandFromResourceAssembler.ToCommandFromResource(resource, userId);
        var result = await _commandService.Handle(command, userPlan);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        var deviceResource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(result.Data!);
        return CreatedAtAction(nameof(GetDeviceById), new { id = result.Data!.Id }, deviceResource);
    }

    /// <summary>
    /// Update a device
    /// </summary>
    /// <param name="id">Device ID</param>
    /// <param name="plan">User Plan: basic, student, family
    /// NOTE: This parameter is temporary until the Subscriptions bounded context is implemented</param>
    /// <param name="resource">Fields to be updated according to plan permissions</param>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateDevice(
        int id,
        [FromQuery] string plan,
        [FromBody] UpdateDeviceResource resource)
    {
        PlanType userPlan;
        try
        {
            userPlan = PlanTypeExtensions.ParsePlanType(plan);
        }
        catch
        {
            return BadRequest(new { message = "Tu plan no permite renombrar dispositivos" });
        }

        var command = UpdateDeviceCommandFromResourceAssembler.ToCommandFromResource(resource, id);
        var result = await _commandService.Handle(command, userPlan);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        var deviceResource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(result.Data!);
        return Ok(deviceResource);
    }

    /// <summary>
    /// Remove (unlink) a device
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id, [FromQuery] int userId)
    {
        var command = new DeleteDeviceCommand(id, userId);
        var result = await _commandService.Handle(command);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return NoContent();
    }
}
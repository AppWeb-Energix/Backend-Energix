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

    private static string? NormalizePlan(string? plan) =>
        string.IsNullOrWhiteSpace(plan) ||
        string.Equals(plan, "undefined", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(plan, "null", StringComparison.OrdinalIgnoreCase)
            ? null
            : plan;

    private static PlanType ResolvePlanOrInfer(string? plan, string resourceType)
    {
        var normalizedPlan = NormalizePlan(plan);

        if (!string.IsNullOrEmpty(normalizedPlan))
            return PlanTypeExtensions.ParsePlanType(normalizedPlan);

        return resourceType.ToLowerInvariant() switch
        {
            "manual" => PlanType.Basic,
            "plug" => PlanType.Student,
            "sensor" => PlanType.Family,
            _ => PlanTypeExtensions.ParsePlanType(resourceType)
        };
    }

    private static PlanType ResolvePlanOrDefault(string? plan, PlanType defaultPlan)
    {
        var normalizedPlan = NormalizePlan(plan);
        return string.IsNullOrEmpty(normalizedPlan)
            ? defaultPlan
            : PlanTypeExtensions.ParsePlanType(normalizedPlan);
    }

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

    [HttpPost]
    public async Task<IActionResult> CreateDevice(
        [FromQuery] int? userId,
        [FromQuery] string? plan,
        [FromBody] CreateDeviceResource resource)
    {
        var resolvedUserId = userId ?? resource.UserId;
        if (resolvedUserId == null)
            return BadRequest(new { message = "userId es requerido (query o body)" });

        PlanType userPlan;
        try
        {
            userPlan = ResolvePlanOrInfer(plan, resource.Type);
        }
        catch
        {
            return BadRequest(new { message = "Plan inválido. Use: basic, student, family" });
        }

        var command = CreateDeviceCommandFromResourceAssembler.ToCommandFromResource(resource, resolvedUserId.Value);
        var result = await _commandService.Handle(command, userPlan);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        var deviceResource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(result.Data!);
        return CreatedAtAction(nameof(GetDeviceById), new { id = result.Data!.Id }, deviceResource);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateDevice(
        int id,
        [FromQuery] string? plan,
        [FromBody] UpdateDeviceResource resource)
    {
        PlanType userPlan;
        try
        {
            userPlan = ResolvePlanOrDefault(plan, PlanType.Family);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id, [FromQuery] int? userId)
    {
        var resolvedUserId = userId;

        if (resolvedUserId == null)
        {
            var device = await _queryService.Handle(new GetDeviceByIdQuery(id));
            resolvedUserId = device?.UserId;
        }

        if (resolvedUserId == null)
            return BadRequest(new { message = "userId es requerido para eliminar el dispositivo" });

        var command = new DeleteDeviceCommand(id, resolvedUserId.Value);
        var result = await _commandService.Handle(command);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return NoContent();
    }
}
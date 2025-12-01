using Energix.API.DeviceManagement.Application.Internal.CommandServices;
using Energix.API.DeviceManagement.Application.Internal.QueryServices;
using Energix.API.DeviceManagement.Domain.Model.Commands.Zones;
using Energix.API.DeviceManagement.Domain.Model.Queries.Zones;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Interfaces.REST.Resources;
using Energix.API.DeviceManagement.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.DeviceManagement.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ZonesController : ControllerBase
{
    private readonly ZoneCommandService _commandService;
    private readonly ZoneQueryService _queryService;

    public ZonesController(
        ZoneCommandService commandService,
        ZoneQueryService queryService)
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

    private static PlanType ResolvePlanOrDefault(string? plan, PlanType defaultPlan)
    {
        var normalizedPlan = NormalizePlan(plan);

        if (string.IsNullOrEmpty(normalizedPlan))
            return defaultPlan;

        try
        {
            return PlanTypeExtensions.ParsePlanType(normalizedPlan);
        }
        catch
        {
            return defaultPlan;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetZonesByUserId(
        [FromQuery] int userId,
        [FromQuery] bool includeDevices = false)
    {
        if (includeDevices)
        {
            var queryWithDevices = new GetZonesWithDevicesQuery(userId);
            var zonesWithDevices = await _queryService.Handle(queryWithDevices);
            var resourcesWithDevices = zonesWithDevices
                .Select(ZoneResourceFromEntityAssembler.ToResourceWithDevicesFromEntity);
            return Ok(resourcesWithDevices);
        }

        var query = new GetZonesByUserIdQuery(userId);
        var zones = await _queryService.Handle(query);
        var resources = zones.Select(ZoneResourceFromEntityAssembler.ToResourceFromEntity);

        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetZoneById(int id)
    {
        var query = new GetZoneByIdQuery(id);
        var zone = await _queryService.Handle(query);

        if (zone == null)
            return NotFound(new { message = "Zona no encontrada" });

        var resource = ZoneResourceFromEntityAssembler.ToResourceFromEntity(zone);
        return Ok(resource);
    }

    [HttpPost]
    public async Task<IActionResult> CreateZone(
        [FromQuery] int? userId,
        [FromQuery] string? plan,
        [FromBody] CreateZoneResource resource)
    {
        var resolvedUserId = userId ?? resource.UserId;
        if (resolvedUserId == null)
            return BadRequest(new { message = "userId es requerido (query o body)" });

        var userPlan = ResolvePlanOrDefault(plan, PlanType.Family);

        var command = CreateZoneCommandFromResourceAssembler.ToCommandFromResource(resource, resolvedUserId.Value);
        var result = await _commandService.Handle(command, userPlan);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        var zoneResource = ZoneResourceFromEntityAssembler.ToResourceFromEntity(result.Data!);
        return CreatedAtAction(nameof(GetZoneById), new { id = result.Data!.Id }, zoneResource);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateZone(
        int id,
        [FromQuery] int? userId,
        [FromBody] UpdateZoneResource resource)
    {
        var resolvedUserId = userId;

        if (resolvedUserId == null)
        {
            var zone = await _queryService.Handle(new GetZoneByIdQuery(id));
            resolvedUserId = zone?.UserId;
        }

        if (resolvedUserId == null)
            return BadRequest(new { message = "userId es requerido para actualizar la zona" });

        var command = RenameZoneCommandFromResourceAssembler.ToCommandFromResource(resource, id, resolvedUserId.Value);
        var result = await _commandService.Handle(command);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        var zoneResource = ZoneResourceFromEntityAssembler.ToResourceFromEntity(result.Data!);
        return Ok(zoneResource);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteZone(
        int id,
        [FromQuery] int? userId,
        [FromQuery] string? plan)
    {
        var resolvedUserId = userId;
        if (resolvedUserId == null)
        {
            var zone = await _queryService.Handle(new GetZoneByIdQuery(id));
            resolvedUserId = zone?.UserId;
        }

        if (resolvedUserId == null)
            return BadRequest(new { message = "userId es requerido para eliminar la zona" });

        var userPlan = ResolvePlanOrDefault(plan, PlanType.Family);

        var command = new DeleteZoneCommand(id, resolvedUserId.Value);
        var result = await _commandService.Handle(command, userPlan);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return NoContent();
    }
}
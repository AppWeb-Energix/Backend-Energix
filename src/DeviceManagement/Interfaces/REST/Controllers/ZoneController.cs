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

    /// <summary>
    /// It retrieves all of a user's zones
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="includeDevices">Include devices in the response</param>
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

    /// <summary>
    /// You get a zone by your ID
    /// </summary>
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

    /// <summary>
    /// Create a new zone
    /// </summary>
    /// <param name="userId">User ID (from query)</param>
    /// <param name="plan">User plan (from query): family</param>
    /// <param name="resource">Area data</param>
    [HttpPost]
    public async Task<IActionResult> CreateZone(
        [FromQuery] int userId,
        [FromQuery] string plan,
        [FromBody] CreateZoneResource resource)
    {
        PlanType userPlan;
        try
        {
            userPlan = PlanTypeExtensions.ParsePlanType(plan);
        }
        catch
        {
            return BadRequest(new { message = "Plan inválido. Use: family" });
        }

        var command = CreateZoneCommandFromResourceAssembler.ToCommandFromResource(resource, userId);
        var result = await _commandService.Handle(command, userPlan);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        var zoneResource = ZoneResourceFromEntityAssembler.ToResourceFromEntity(result.Data!);
        return CreatedAtAction(nameof(GetZoneById), new { id = result.Data!.Id }, zoneResource);
    }

    /// <summary>
    /// Update (rename) an area
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateZone(
        int id,
        [FromQuery] int userId,
        [FromBody] UpdateZoneResource resource)
    {
        var command = RenameZoneCommandFromResourceAssembler.ToCommandFromResource(resource, id, userId);
        var result = await _commandService.Handle(command);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        var zoneResource = ZoneResourceFromEntityAssembler.ToResourceFromEntity(result.Data!);
        return Ok(zoneResource);
    }

    /// <summary>
    /// Delete a zone
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteZone(
        int id,
        [FromQuery] int userId,
        [FromQuery] string plan)
    {
        PlanType userPlan;
        try
        {
            userPlan = PlanTypeExtensions.ParsePlanType(plan);
        }
        catch
        {
            return BadRequest(new { message = "Plan inválido. Use: family" });
        }

        var command = new DeleteZoneCommand(id, userId);
        var result = await _commandService.Handle(command, userPlan);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return NoContent();
    }
}
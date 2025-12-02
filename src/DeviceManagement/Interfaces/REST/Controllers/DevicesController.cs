using Energix. API.DeviceManagement.Application.Internal.CommandServices;
using Energix.API.DeviceManagement.Application.Internal.QueryServices;
using Energix. API.DeviceManagement.Domain.Model.Commands;
using Energix.API.DeviceManagement.Domain.Model.Commands. Devices;
using Energix.API.DeviceManagement. Domain.Model.Queries;
using Energix.API.DeviceManagement.Domain.Model. Queries.Devices;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix. API.DeviceManagement.Domain.Services;
using Energix.API.DeviceManagement. Interfaces.REST.Resources;
using Energix.API.DeviceManagement.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.DeviceManagement.Interfaces.REST. Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceCommandService _commandService;
    private readonly IDeviceQueryService _queryService;

    public DevicesController(
        IDeviceCommandService commandService,
        IDeviceQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    private static string? NormalizePlan(string? plan) =>
        string.IsNullOrWhiteSpace(plan) ||
        string.Equals(plan, "undefined", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(plan, "null", StringComparison. OrdinalIgnoreCase)
            ? null
            : plan;

    private static PlanType ResolvePlanOrInfer(string? plan, string resourceType)
    {
        var normalizedPlan = NormalizePlan(plan);

        if (!string. IsNullOrEmpty(normalizedPlan))
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
        DeviceType?  deviceType = null;
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

    [HttpGet("metrics/summary")]
    public async Task<IActionResult> GetManualDeviceMetricsSummary([FromQuery] int userId)
    {
        if (userId <= 0)
            return BadRequest(new { message = "userId es requerido" });

        var manualDevices = await _queryService.Handle(new GetDevicesByUserIdQuery(userId, DeviceType. Manual));
        var resource = DeviceMetricsSummaryResourceAssembler.ToResourceFromEntities(manualDevices);

        return Ok(resource);
    }

    /// <summary>
    /// Devuelve los labels, datasets y KPIs para graficar dispositivos manuales en el frontend.
    /// </summary>
    /// <param name="userId">Identificador del usuario dueño de los dispositivos manuales. </param>
    [HttpGet("metrics/chart")]
    public async Task<IActionResult> GetManualDeviceMetricsChart([FromQuery] int userId)
    {
        if (userId <= 0)
            return BadRequest(new { message = "userId es requerido" });

        var manualDevices = await _queryService.Handle(new GetDevicesByUserIdQuery(userId, DeviceType.Manual));
        var resource = ManualDeviceChartResourceAssembler.ToChartResource(manualDevices);

        return Ok(resource);
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
        [FromQuery] int?  userId,
        [FromQuery] string? plan,
        [FromBody] CreateDeviceResource resource)
    {
        try
        {
            // 🔍 LOGGING DETALLADO
            Console.WriteLine($"[DEVICES] === Iniciando CreateDevice ===");
            Console.WriteLine($"[DEVICES] userId (query): {userId}");
            Console.WriteLine($"[DEVICES] plan (query): {plan}");
            Console.WriteLine($"[DEVICES] resource. UserId: {resource?.UserId}");
            Console.WriteLine($"[DEVICES] resource.Type: {resource?.Type}");
            Console.WriteLine($"[DEVICES] resource.Name: {resource?.Name}");

            var resolvedUserId = userId ?? resource. UserId;
            if (resolvedUserId == null)
            {
                Console.WriteLine("[DEVICES ERROR] userId es null");
                return BadRequest(new { message = "userId es requerido (query o body)" });
            }

            Console.WriteLine($"[DEVICES] resolvedUserId: {resolvedUserId}");

            var command = CreateDeviceCommandFromResourceAssembler.ToCommandFromResource(resource, resolvedUserId.Value);
            Console.WriteLine($"[DEVICES] Command creado, ejecutando Handle...");
            
            var device = await _commandService.Handle(command);  // ✅ Solo 1 parámetro
            Console.WriteLine($"[DEVICES] Device creado exitosamente. ID: {device.Id}");

            var deviceResource = DeviceResourceFromEntityAssembler. ToResourceFromEntity(device);
            return CreatedAtAction(nameof(GetDeviceById), new { id = device.Id }, deviceResource);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEVICES FATAL ERROR] {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[DEVICES STACK] {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[DEVICES INNER] {ex.InnerException.Message}");
            }
            
            return StatusCode(500, new { 
                error = ex.Message, 
                type = ex.GetType(). Name,
                innerError = ex.InnerException?.Message 
            });
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateDevice(
        int id,
        [FromQuery] string? plan,
        [FromBody] UpdateDeviceResource resource)
    {
        try
        {
            var command = UpdateDeviceCommandFromResourceAssembler.ToCommandFromResource(resource, id);
            var device = await _commandService.Handle(command);  // ✅ Solo 1 parámetro

            if (device == null)
                return NotFound(new { message = "Dispositivo no encontrado" });

            var deviceResource = DeviceResourceFromEntityAssembler. ToResourceFromEntity(device);
            return Ok(deviceResource);
        }
        catch (Exception ex)
        {
            Console. WriteLine($"[UPDATE ERROR] {ex.Message}");
            return BadRequest(new { message = ex. Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id, [FromQuery] int? userId)
    {
        try
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
            var success = await _commandService.Handle(command);  // ✅ Retorna bool

            if (!success)
                return BadRequest(new { message = "No se pudo eliminar el dispositivo" });

            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DELETE ERROR] {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }
}
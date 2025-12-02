using System.Security.Claims;
using Energix.API.DeviceManagement.Application.Internal.CommandServices;
using Energix.API.DeviceManagement.Application.Internal.QueryServices;
using Energix.API.DeviceManagement.Domain.Model.Commands;
using Energix.API.DeviceManagement.Domain.Model.Commands.Devices;
using Energix.API.DeviceManagement.Domain.Model.Queries;
using Energix.API.DeviceManagement.Domain.Model.Queries.Devices;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Domain.Services;
using Energix.API.DeviceManagement.Interfaces.REST.Resources;
using Energix.API.DeviceManagement.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.DeviceManagement.Interfaces.REST.Controllers;

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

    /// <summary>
    /// Extrae el userId desde el token JWT del usuario autenticado
    /// </summary>
    private int? GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                       ?? User.FindFirst("sub")
                       ?? User.FindFirst("userId")
                       ?? User.FindFirst("id");

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
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

    /// <summary>
    /// Obtiene todos los dispositivos del usuario autenticado, opcionalmente filtrados por tipo
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetDevicesByUserId([FromQuery] string? type = null)
    {
        var userId = GetUserIdFromToken();

        if (userId == null)
            return Unauthorized(new { message = "Token JWT inválido o no contiene userId" });

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

        var query = new GetDevicesByUserIdQuery(userId.Value, deviceType);
        var devices = await _queryService.Handle(query);
        var resources = devices.Select(DeviceResourceFromEntityAssembler.ToResourceFromEntity);

        return Ok(resources);
    }

    /// <summary>
    /// Obtiene resumen de métricas de dispositivos manuales del usuario autenticado
    /// </summary>
    [HttpGet("metrics/summary")]
    [Authorize]
    public async Task<IActionResult> GetManualDeviceMetricsSummary()
    {
        var userId = GetUserIdFromToken();

        if (userId == null)
            return Unauthorized(new { message = "Token JWT inválido" });

        var manualDevices = await _queryService.Handle(
            new GetDevicesByUserIdQuery(userId.Value, DeviceType.Manual));
        
        var resource = DeviceMetricsSummaryResourceAssembler.ToResourceFromEntities(manualDevices);

        return Ok(resource);
    }

    /// <summary>
    /// Devuelve los labels, datasets y KPIs para graficar dispositivos manuales del usuario autenticado
    /// </summary>
    [HttpGet("metrics/chart")]
    [Authorize]
    public async Task<IActionResult> GetManualDeviceMetricsChart()
    {
        var userId = GetUserIdFromToken();

        if (userId == null)
            return Unauthorized(new { message = "Token JWT inválido" });

        var manualDevices = await _queryService.Handle(
            new GetDevicesByUserIdQuery(userId.Value, DeviceType.Manual));
        
        var resource = ManualDeviceChartResourceAssembler.ToChartResource(manualDevices);

        return Ok(resource);
    }

    /// <summary>
    /// Obtiene un dispositivo específico por su ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetDeviceById(int id)
    {
        var userId = GetUserIdFromToken();

        if (userId == null)
            return Unauthorized(new { message = "Token JWT inválido" });

        var query = new GetDeviceByIdQuery(id);
        var device = await _queryService.Handle(query);

        if (device == null)
            return NotFound(new { message = "Dispositivo no encontrado" });

        // Validar que el dispositivo pertenezca al usuario autenticado
        if (device.UserId != userId.Value)
            return Forbid();

        var resource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
        return Ok(resource);
    }

    /// <summary>
    /// Crea un nuevo dispositivo para el usuario autenticado
    /// </summary>
    /// <summary>
    /// Crea un nuevo dispositivo para el usuario autenticado
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateDevice(
        [FromBody] CreateDeviceResource resource,
        [FromQuery] string? plan = null)
    {
        try
        {
            var userId = GetUserIdFromToken();

            if (userId == null)
                return Unauthorized(new { message = "Token JWT inválido o no contiene userId" });

            Console.WriteLine($"[DEVICES] === Iniciando CreateDevice ===");
            Console.WriteLine($"[DEVICES] userId desde JWT: {userId}");
            Console.WriteLine($"[DEVICES] plan (query): {plan}");
            Console.WriteLine($"[DEVICES] resource.Type: {resource?.Type}");
            Console.WriteLine($"[DEVICES] resource.Name: {resource?.Name}");

            // NO modificar resource.UserId, pasar el userId del JWT al assembler
            var command = CreateDeviceCommandFromResourceAssembler.ToCommandFromResource(
                resource,
                userId.Value); // ← El assembler debe usar este userId en lugar del resource.UserId

            Console.WriteLine($"[DEVICES] Command creado, ejecutando Handle...");

            var device = await _commandService.Handle(command);
            Console.WriteLine($"[DEVICES] Device creado exitosamente. ID: {device.Id}");

            var deviceResource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
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

            return StatusCode(500, new
            {
                error = ex.Message,
                type = ex.GetType().Name,
                innerError = ex.InnerException?.Message
            });
        }
    }


    /// <summary>
    /// Actualiza un dispositivo existente del usuario autenticado
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateDevice(
        int id,
        [FromBody] UpdateDeviceResource resource,
        [FromQuery] string? plan = null)
    {
        try
        {
            var userId = GetUserIdFromToken();

            if (userId == null)
                return Unauthorized(new { message = "Token JWT inválido" });

            // Validar que el dispositivo pertenezca al usuario autenticado
            var existingDevice = await _queryService.Handle(new GetDeviceByIdQuery(id));
            
            if (existingDevice == null)
                return NotFound(new { message = "Dispositivo no encontrado" });

            if (existingDevice.UserId != userId.Value)
                return Forbid();

            var command = UpdateDeviceCommandFromResourceAssembler.ToCommandFromResource(resource, id);
            var device = await _commandService.Handle(command);

            if (device == null)
                return NotFound(new { message = "Dispositivo no encontrado" });

            var deviceResource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
            return Ok(deviceResource);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UPDATE ERROR] {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un dispositivo del usuario autenticado
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        try
        {
            var userId = GetUserIdFromToken();

            if (userId == null)
                return Unauthorized(new { message = "Token JWT inválido" });

            // Validar que el dispositivo exista y pertenezca al usuario
            var device = await _queryService.Handle(new GetDeviceByIdQuery(id));
            
            if (device == null)
                return NotFound(new { message = "Dispositivo no encontrado" });

            if (device.UserId != userId.Value)
                return Forbid();

            var command = new DeleteDeviceCommand(id, userId.Value);
            var success = await _commandService.Handle(command);

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

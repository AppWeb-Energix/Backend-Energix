using System.Security.Claims;
using Energix.API.DeviceManagement.Domain.Model.Commands.Devices;
using Energix.API.DeviceManagement.Domain.Model.Queries.Devices;
using Energix.API.DeviceManagement.Domain.Model.ValueObjects;
using Energix.API.DeviceManagement.Domain.Services;
using Energix.API.DeviceManagement.Interfaces.REST.Resources;
using Energix.API.DeviceManagement.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.DeviceManagement.Interfaces.REST.Controllers;

[Authorize] // ✅ Autenticación global para todo el controlador
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
    /// Soporta: "sub", "userId", ClaimTypes.NameIdentifier
    /// </summary>
    private int GetUserIdFromToken()
    {
        // ✅ Prioridad: sub (estándar JWT) > userId > NameIdentifier
        var userIdClaim = User.FindFirst("sub")
                       ?? User.FindFirst("userId")
                       ?? User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            var availableClaims = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
            Console.WriteLine($"❌ Claims disponibles: {availableClaims}");
            throw new UnauthorizedAccessException("Token JWT inválido: no contiene userId válido");
        }

        Console.WriteLine($"✅ UserId extraído del token: {userId}");
        return userId;
    }

    /// <summary>
    /// Extrae el PlanType desde el token JWT del usuario autenticado
    /// Lee claim "planType" con valores esperados: Basic, Student, FamilyPremium
    /// Mapea a PlanType interno: Basic, Student, Family
    /// </summary>
    private PlanType GetPlanFromToken()
    {
        var planTypeClaim = User.FindFirst("planType");

        if (planTypeClaim == null || string.IsNullOrWhiteSpace(planTypeClaim.Value))
        {
            Console.WriteLine($"⚠️ Claim 'planType' no encontrado en token, usando fallback: PlanType.Basic");
            return PlanType.Basic;
        }

        try
        {
            // Mapear valores del JWT al enum interno
            var planTypeValue = planTypeClaim.Value;
            Console.WriteLine($"📋 PlanType claim value: {planTypeValue}");

            var planType = PlanTypeExtensions.ParsePlanType(planTypeValue);
            Console.WriteLine($"✅ PlanType mapeado: {planType}");
            
            return planType;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error al parsear planType '{planTypeClaim.Value}': {ex.Message}. Usando fallback: PlanType.Basic");
            return PlanType.Basic;
        }
    }

    /// <summary>
    /// Obtiene todos los dispositivos del usuario autenticado, opcionalmente filtrados por tipo
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDevicesByUserId([FromQuery] string? type = null)
    {
        try
        {
            var userId = GetUserIdFromToken();

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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene resumen de métricas de dispositivos manuales del usuario autenticado
    /// </summary>
    [HttpGet("metrics/summary")]
    public async Task<IActionResult> GetManualDeviceMetricsSummary()
    {
        try
        {
            var userId = GetUserIdFromToken();

            var manualDevices = await _queryService.Handle(
                new GetDevicesByUserIdQuery(userId, DeviceType.Manual));

            var resource = DeviceMetricsSummaryResourceAssembler.ToResourceFromEntities(manualDevices);

            return Ok(resource);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Devuelve los labels, datasets y KPIs para graficar dispositivos manuales del usuario autenticado
    /// </summary>
    [HttpGet("metrics/chart")]
    public async Task<IActionResult> GetManualDeviceMetricsChart()
    {
        try
        {
            var userId = GetUserIdFromToken();

            var manualDevices = await _queryService.Handle(
                new GetDevicesByUserIdQuery(userId, DeviceType.Manual));

            var resource = ManualDeviceChartResourceAssembler.ToChartResource(manualDevices);

            return Ok(resource);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un dispositivo específico por su ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDeviceById(int id)
    {
        try
        {
            var userId = GetUserIdFromToken();

            var query = new GetDeviceByIdQuery(id);
            var device = await _queryService.Handle(query);

            if (device == null)
                return NotFound(new { message = "Dispositivo no encontrado" });

            // ✅ Validar que el dispositivo pertenezca al usuario autenticado
            if (device.UserId != userId)
                return Forbid();

            var resource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
            return Ok(resource);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Crea un nuevo dispositivo para el usuario autenticado
    /// El userId y planType se extraen del JWT - NO deben enviarse en el body
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceResource resource)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var userPlan = GetPlanFromToken();

            Console.WriteLine($"[DEVICES] 🔐 UserId desde JWT: {userId}");
            Console.WriteLine($"[DEVICES] 📋 PlanType desde JWT: {userPlan}");
            Console.WriteLine($"[DEVICES] 📝 Device Type: {resource.Type}");
            Console.WriteLine($"[DEVICES] 📝 Device Name: {resource.Name}");

            // ✅ El userId del JWT se pasa al comando, y el userPlan al servicio
            var command = CreateDeviceCommandFromResourceAssembler.ToCommandFromResource(resource, userId);
            var result = await _commandService.Handle(command, userPlan);

            if (!result.Success)
            {
                Console.WriteLine($"[DEVICES] ❌ Error al crear dispositivo: {result.ErrorMessage}");
                return BadRequest(new { message = result.ErrorMessage });
            }

            Console.WriteLine($"[DEVICES] ✅ Device creado exitosamente. ID: {result.Data!.Id}");

            var deviceResource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(result.Data!);
            return CreatedAtAction(nameof(GetDeviceById), new { id = result.Data!.Id }, deviceResource);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [DEVICES ERROR] {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[STACK] {ex.StackTrace}");

            if (ex.InnerException != null)
                Console.WriteLine($"[INNER] {ex.InnerException.Message}");

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
    public async Task<IActionResult> UpdateDevice(int id, [FromBody] UpdateDeviceResource resource)
    {
        try
        {
            var userId = GetUserIdFromToken();

            // ✅ Validar que el dispositivo pertenezca al usuario autenticado
            var existingDevice = await _queryService.Handle(new GetDeviceByIdQuery(id));

            if (existingDevice == null)
                return NotFound(new { message = "Dispositivo no encontrado" });

            if (existingDevice.UserId != userId)
                return Forbid();

            var command = UpdateDeviceCommandFromResourceAssembler.ToCommandFromResource(resource, id);
            var device = await _commandService.Handle(command);

            if (device == null)
                return NotFound(new { message = "Dispositivo no encontrado" });

            var deviceResource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
            return Ok(deviceResource);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [UPDATE ERROR] {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un dispositivo del usuario autenticado
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        try
        {
            var userId = GetUserIdFromToken();

            // ✅ Validar que el dispositivo exista y pertenezca al usuario
            var device = await _queryService.Handle(new GetDeviceByIdQuery(id));

            if (device == null)
                return NotFound(new { message = "Dispositivo no encontrado" });

            if (device.UserId != userId)
                return Forbid();

            var command = new DeleteDeviceCommand(id, userId);
            var success = await _commandService.Handle(command);

            if (!success)
                return BadRequest(new { message = "No se pudo eliminar el dispositivo" });

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [DELETE ERROR] {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }
}

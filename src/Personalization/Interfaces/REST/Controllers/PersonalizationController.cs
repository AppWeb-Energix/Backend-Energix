using Energix.API.Personalization.Domain.Model.Aggregates;
using Energix.API.Personalization.Domain.Repositories;
using Energix.API.Personalization.Interfaces.REST.Resources;
using Energix.API.Personalization.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

namespace Energix.API.Personalization.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PersonalizationController : ControllerBase
{
    private readonly IPersonalizationRepository _repository;

    public PersonalizationController(IPersonalizationRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Get personalization by user ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Personalization settings for the user</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByUserId(int id)
    {
        var personalization = await _repository.GetByUserIdAsync(id);
        
        if (personalization == null)
            return NotFound(new { message = "Personalización no encontrada para el usuario especificado" });
        
        var resource = PersonalizationResourceFromEntityAssembler.ToResourceFromEntity(personalization);
        return Ok(resource);
    }

    /// <summary>
    /// Create a new personalization configuration
    /// </summary>
    /// <param name="resource">Personalization settings to create</param>
    /// <returns>Created personalization settings</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePersonalizationResource resource)
    {
        // Check if personalization already exists for this user
        var existing = await _repository.GetByUserIdAsync(resource.UserId);
        if (existing != null)
        {
            return Conflict(new { message = $"Ya existe una personalización para el usuario {resource.UserId}" });
        }

        // Create new personalization
        var personalization = new PersonalizationAggregate
        {
            UserId = resource.UserId,
            KpiCurrent = resource.KpiCurrent,
            KpiCost = resource.KpiCost,
            KpiMonthly = resource.KpiMonthly,
            ChartHourly = resource.ChartHourly,
            ChartMonthly = resource.ChartMonthly,
            ChartDevice = resource.ChartDevice,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(personalization);
        var result = PersonalizationResourceFromEntityAssembler.ToResourceFromEntity(created);
        
        return CreatedAtAction(nameof(GetByUserId), new { id = created.UserId }, result);
    }

    /// <summary>
    /// Update personalization settings by user ID (complete update)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="resource">Complete personalization settings to update</param>
    /// <returns>Updated personalization settings</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateByUserId(int id, [FromBody] CreatePersonalizationResource resource)
    {
        // Validate that userId in route matches userId in body
        if (id != resource.UserId)
        {
            return BadRequest(new { message = "El ID de usuario en la URL no coincide con el del cuerpo de la solicitud" });
        }

        // Find personalization by user ID
        var existing = await _repository.GetByUserIdAsync(id);
        
        if (existing == null)
        {
            return NotFound(new { message = "Personalización no encontrada para el usuario especificado" });
        }

        // Update all fields
        existing.KpiCurrent = resource.KpiCurrent;
        existing.KpiCost = resource.KpiCost;
        existing.KpiMonthly = resource.KpiMonthly;
        existing.ChartHourly = resource.ChartHourly;
        existing.ChartMonthly = resource.ChartMonthly;
        existing.ChartDevice = resource.ChartDevice;
        existing.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _repository.UpdateAsync(existing);
        var result = PersonalizationResourceFromEntityAssembler.ToResourceFromEntity(updated);
        
        return Ok(result);
    }

    /// <summary>
    /// Delete personalization by user ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteByUserId(int id)
    {
        var personalization = await _repository.GetByUserIdAsync(id);
        
        if (personalization == null)
        {
            return NotFound(new { message = "Personalización no encontrada para el usuario especificado" });
        }
        
        var deleted = await _repository.DeleteAsync(personalization.Id);
        
        if (!deleted)
        {
            return StatusCode(500, new { message = "Error al eliminar la personalización" });
        }
        
        return NoContent();
    }
}

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
    /// Update personalization settings by user ID (partial update)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="resource">Personalization settings to update</param>
    /// <returns>Updated personalization settings</returns>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateByUserId(int id, [FromBody] UpdatePersonalizationResource resource)
    {
        // Find personalization by user ID
        var existing = await _repository.GetByUserIdAsync(id);
        if (existing == null)
            return NotFound(new { message = "Personalización no encontrada para el usuario especificado" });

        // Update only provided fields (partial update)
        if (resource.KpiCurrent.HasValue) existing.KpiCurrent = resource.KpiCurrent.Value;
        if (resource.KpiCost.HasValue) existing.KpiCost = resource.KpiCost.Value;
        if (resource.KpiMonthly.HasValue) existing.KpiMonthly = resource.KpiMonthly.Value;
        if (resource.ChartHourly.HasValue) existing.ChartHourly = resource.ChartHourly.Value;
        if (resource.ChartMonthly.HasValue) existing.ChartMonthly = resource.ChartMonthly.Value;
        if (resource.ChartDevice.HasValue) existing.ChartDevice = resource.ChartDevice.Value;

        var updated = await _repository.UpdateAsync(existing);
        var result = PersonalizationResourceFromEntityAssembler.ToResourceFromEntity(updated);
        
        return Ok(result);
    }
}

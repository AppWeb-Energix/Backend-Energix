using Energix.API.Personalization.Domain.Model.Aggregates;
using Energix.API.Personalization.Domain.Model.Commands;
using Energix.API.Personalization.Domain.Repositories;
using Energix.API.Personalization.Domain.Services;

namespace Energix.API.Personalization.Application.Internal.CommandServices;

/// <summary>
/// Application service for handling personalization commands
/// </summary>
public class PersonalizationCommandService
{
    private readonly IPersonalizationRepository _repository;
    private readonly IPersonalizationService _domainService;

    public PersonalizationCommandService(
        IPersonalizationRepository repository,
        IPersonalizationService domainService)
    {
        _repository = repository;
        _domainService = domainService;
    }

    /// <summary>
    /// Handle create personalization command
    /// </summary>
    public async Task<PersonalizationAggregate> HandleAsync(CreatePersonalizationCommand command)
    {
        // Validate if user can create personalization
        var canCreate = await _domainService.CanUserCreatePersonalizationAsync(command.UserId);
        if (!canCreate)
        {
            throw new InvalidOperationException($"User {command.UserId} already has a personalization configuration");
        }

        // Create entity from command
        var entity = new PersonalizationAggregate
        {
            UserId = command.UserId,
            KpiCurrent = command.KpiSettings.Current,
            KpiCost = command.KpiSettings.Cost,
            KpiMonthly = command.KpiSettings.Monthly,
            ChartHourly = command.ChartSettings.Hourly,
            ChartMonthly = command.ChartSettings.Monthly,
            ChartDevice = command.ChartSettings.Device
        };

        // Validate settings
        var isValid = _domainService.ValidatePersonalizationSettings(
            entity.KpiCurrent, entity.KpiCost, entity.KpiMonthly,
            entity.ChartHourly, entity.ChartMonthly, entity.ChartDevice);

        if (!isValid)
        {
            throw new InvalidOperationException("Invalid personalization settings. At least one KPI and one Chart setting must be enabled.");
        }

        return await _repository.CreateAsync(entity);
    }

    /// <summary>
    /// Handle update personalization command
    /// </summary>
    public async Task<PersonalizationAggregate> HandleAsync(UpdatePersonalizationCommand command)
    {
        var entity = await _repository.GetByIdAsync(command.PersonalizationId);
        if (entity == null)
        {
            throw new InvalidOperationException($"Personalization {command.PersonalizationId} not found");
        }

        // Update KPI settings if provided
        if (command.KpiSettings != null)
        {
            entity.KpiCurrent = command.KpiSettings.Current;
            entity.KpiCost = command.KpiSettings.Cost;
            entity.KpiMonthly = command.KpiSettings.Monthly;
        }

        // Update Chart settings if provided
        if (command.ChartSettings != null)
        {
            entity.ChartHourly = command.ChartSettings.Hourly;
            entity.ChartMonthly = command.ChartSettings.Monthly;
            entity.ChartDevice = command.ChartSettings.Device;
        }

        // Validate settings
        var isValid = _domainService.ValidatePersonalizationSettings(
            entity.KpiCurrent, entity.KpiCost, entity.KpiMonthly,
            entity.ChartHourly, entity.ChartMonthly, entity.ChartDevice);

        if (!isValid)
        {
            throw new InvalidOperationException("Invalid personalization settings. At least one KPI and one Chart setting must be enabled.");
        }

        return await _repository.UpdateAsync(entity);
    }

    /// <summary>
    /// Handle delete personalization command
    /// </summary>
    public async Task<bool> HandleDeleteAsync(int personalizationId)
    {
        return await _repository.DeleteAsync(personalizationId);
    }
}
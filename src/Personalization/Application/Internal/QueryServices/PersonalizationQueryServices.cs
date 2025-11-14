using Energix.API.Personalization.Domain.Model.Aggregates;
using Energix.API.Personalization.Domain.Model.Queries;
using Energix.API.Personalization.Domain.Repositories;

namespace Energix.API.Personalization.Application.Internal.QueryServices;

/// <summary>
/// Application service for handling personalization queries
/// </summary>
public class PersonalizationQueryServices
{
    private readonly IPersonalizationRepository _repository;

    public PersonalizationQueryServices(IPersonalizationRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handle get personalization by user ID query
    /// </summary>
    public async Task<PersonalizationAggregate?> HandleAsync(GetPersonalizationByUserIdQuery query)
    {
        return await _repository.GetByUserIdAsync(query.UserId);
    }

    /// <summary>
    /// Handle get personalization by ID query
    /// </summary>
    public async Task<PersonalizationAggregate?> HandleAsync(GetPersonalizationByIdQuery query)
    {
        return await _repository.GetByIdAsync(query.PersonalizationId);
    }
}
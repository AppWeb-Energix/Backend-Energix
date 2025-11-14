using Energix.API.Personalization.Domain.Model.Aggregates;

namespace Energix.API.Personalization.Domain.Repositories;

public interface IPersonalizationRepository
{
    Task<PersonalizationAggregate?> GetByUserIdAsync(int userId);
    Task<PersonalizationAggregate?> GetByIdAsync(int id);
    Task<PersonalizationAggregate> CreateAsync(PersonalizationAggregate entity);
    Task<PersonalizationAggregate> UpdateAsync(PersonalizationAggregate entity);
    Task<bool> DeleteAsync(int id);
}


using Energix.API;
using Energix.API.Personalization.Domain.Model.Aggregates;
using Energix.API.Personalization.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Energix.API.Personalization.Infrastructure.Persistence.EFC.Repositories;

public class PersonalizationRepository : IPersonalizationRepository
{
    private readonly AppDbContext _context;

    public PersonalizationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PersonalizationAggregate?> GetByUserIdAsync(int userId)
    {
        return await _context.Set<PersonalizationAggregate>()
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<PersonalizationAggregate?> GetByIdAsync(int id)
    {
        return await _context.Set<PersonalizationAggregate>()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PersonalizationAggregate> CreateAsync(PersonalizationAggregate entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.Set<PersonalizationAggregate>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<PersonalizationAggregate> UpdateAsync(PersonalizationAggregate entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<PersonalizationAggregate>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        
        _context.Set<PersonalizationAggregate>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}


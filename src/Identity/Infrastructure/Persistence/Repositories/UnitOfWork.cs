using Energix.API.Identity.Domain.Repositories;

namespace Energix.API.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// Unit of Work implementation using EF Core DbContext
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}


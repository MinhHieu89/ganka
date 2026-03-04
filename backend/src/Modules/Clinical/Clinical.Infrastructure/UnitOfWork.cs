using Clinical.Application.Interfaces;

namespace Clinical.Infrastructure;

/// <summary>
/// Unit of Work implementation wrapping <see cref="ClinicalDbContext"/>.
/// Coordinates persistence across all Clinical module repositories.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ClinicalDbContext _dbContext;

    public UnitOfWork(ClinicalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

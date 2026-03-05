using Pharmacy.Application.Interfaces;

namespace Pharmacy.Infrastructure;

/// <summary>
/// Unit of Work implementation wrapping <see cref="PharmacyDbContext"/>.
/// Coordinates persistence across all Pharmacy module repositories.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly PharmacyDbContext _dbContext;

    public UnitOfWork(PharmacyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

using Optical.Application.Interfaces;

namespace Optical.Infrastructure;

/// <summary>
/// Unit of Work implementation wrapping <see cref="OpticalDbContext"/>.
/// Coordinates persistence across all Optical module repositories.
/// Follows the same pattern as Pharmacy.Infrastructure.UnitOfWork.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OpticalDbContext _dbContext;

    public UnitOfWork(OpticalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

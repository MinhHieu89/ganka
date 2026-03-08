using Treatment.Application.Interfaces;

namespace Treatment.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation wrapping <see cref="TreatmentDbContext"/>.
/// Coordinates persistence across all Treatment module repositories.
/// Follows the same pattern as Optical.Infrastructure.UnitOfWork.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TreatmentDbContext _dbContext;

    public UnitOfWork(TreatmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

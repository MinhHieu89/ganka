using Scheduling.Application.Interfaces;

namespace Scheduling.Infrastructure;

/// <summary>
/// Unit of Work implementation wrapping <see cref="SchedulingDbContext"/>.
/// Coordinates persistence across all Scheduling module repositories.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SchedulingDbContext _dbContext;

    public UnitOfWork(SchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

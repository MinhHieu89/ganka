using Auth.Application.Interfaces;

namespace Auth.Infrastructure;

/// <summary>
/// Unit of Work implementation wrapping <see cref="AuthDbContext"/>.
/// Coordinates persistence across all Auth module repositories.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AuthDbContext _dbContext;

    public UnitOfWork(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

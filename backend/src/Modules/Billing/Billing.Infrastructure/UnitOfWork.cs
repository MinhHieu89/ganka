using Billing.Application.Interfaces;

namespace Billing.Infrastructure;

/// <summary>
/// Unit of Work implementation wrapping <see cref="BillingDbContext"/>.
/// Coordinates persistence across all Billing module repositories.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BillingDbContext _dbContext;

    public UnitOfWork(BillingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

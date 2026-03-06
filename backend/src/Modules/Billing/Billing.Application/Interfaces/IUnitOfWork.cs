namespace Billing.Application.Interfaces;

/// <summary>
/// Unit of Work abstraction for coordinating persistence across Billing repositories.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

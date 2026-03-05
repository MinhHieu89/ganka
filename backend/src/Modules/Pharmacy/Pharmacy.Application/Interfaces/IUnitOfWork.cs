namespace Pharmacy.Application.Interfaces;

/// <summary>
/// Unit of Work abstraction for coordinating persistence across Pharmacy repositories.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

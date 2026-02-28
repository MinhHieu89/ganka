namespace Auth.Application.Interfaces;

/// <summary>
/// Unit of Work abstraction for coordinating persistence across repositories.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

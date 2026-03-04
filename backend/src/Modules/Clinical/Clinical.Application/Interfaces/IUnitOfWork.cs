namespace Clinical.Application.Interfaces;

/// <summary>
/// Unit of Work abstraction for coordinating persistence across Clinical repositories.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

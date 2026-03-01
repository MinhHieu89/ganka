namespace Patient.Application.Interfaces;

/// <summary>
/// Unit of Work abstraction for coordinating persistence across Patient module repositories.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

namespace Optical.Application.Interfaces;

/// <summary>
/// Unit of Work abstraction for coordinating persistence across Optical repositories.
/// Wraps OpticalDbContext.SaveChangesAsync to provide a clean separation between
/// repository operations (change tracking) and actual persistence.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

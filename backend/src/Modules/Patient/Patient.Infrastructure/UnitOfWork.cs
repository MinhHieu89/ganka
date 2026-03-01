using Patient.Application.Interfaces;

namespace Patient.Infrastructure;

/// <summary>
/// Unit of Work implementation wrapping <see cref="PatientDbContext"/>.
/// Coordinates persistence across all Patient module repositories.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly PatientDbContext _dbContext;

    public UnitOfWork(PatientDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

using Auth.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ISystemSettingRepository"/>.
/// Simple key-value lookup for system configuration.
/// </summary>
public sealed class SystemSettingRepository : ISystemSettingRepository
{
    private readonly AuthDbContext _dbContext;

    public SystemSettingRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await _dbContext.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

        return setting?.Value;
    }
}

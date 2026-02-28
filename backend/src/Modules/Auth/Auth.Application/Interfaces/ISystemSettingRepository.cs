namespace Auth.Application.Interfaces;

/// <summary>
/// Read-only repository interface for SystemSetting key-value lookups.
/// </summary>
public interface ISystemSettingRepository
{
    Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);
}

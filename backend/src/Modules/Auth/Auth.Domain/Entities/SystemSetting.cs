using Shared.Domain;

namespace Auth.Domain.Entities;

/// <summary>
/// Key-value store for system configuration.
/// Used for admin-configurable settings like token lifetimes and session timeouts.
/// </summary>
public class SystemSetting : Entity
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private SystemSetting() { }

    public SystemSetting(string key, string value, string description)
    {
        Key = key;
        Value = value;
        Description = description;
    }

    public void UpdateValue(string value)
    {
        Value = value;
        SetUpdatedAt();
    }
}

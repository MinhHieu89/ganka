using System.Collections.Concurrent;
using Treatment.Application;
using Treatment.Application.Features;

namespace Treatment.Infrastructure.Services;

/// <summary>
/// In-memory implementation of <see cref="IOsdiTokenStore"/> using a ConcurrentDictionary.
/// Tokens expire after 24 hours. Expired tokens are lazily cleaned up on access.
/// Registered as a singleton in DI.
/// </summary>
public sealed class InMemoryOsdiTokenStore : IOsdiTokenStore
{
    private readonly ConcurrentDictionary<string, OsdiTokenInfo> _tokens = new();

    public void Register(string token, OsdiTokenInfo info)
    {
        _tokens[token] = info;
        CleanupExpired();
    }

    public OsdiTokenInfo? Get(string token)
    {
        if (!_tokens.TryGetValue(token, out var info))
            return null;

        if (info.ExpiresAt < DateTime.UtcNow)
        {
            _tokens.TryRemove(token, out _);
            return null;
        }

        return info;
    }

    public void Remove(string token)
    {
        _tokens.TryRemove(token, out _);
    }

    private void CleanupExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _tokens)
        {
            if (kvp.Value.ExpiresAt < now)
                _tokens.TryRemove(kvp.Key, out _);
        }
    }
}

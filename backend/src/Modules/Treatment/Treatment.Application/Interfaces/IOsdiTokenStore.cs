using Treatment.Application.Features;

namespace Treatment.Application;

/// <summary>
/// In-memory token store for OSDI self-fill QR tokens.
/// Tokens have a 24-hour TTL and are automatically cleaned up on access.
/// </summary>
public interface IOsdiTokenStore
{
    /// <summary>
    /// Registers a token with its associated package/session info.
    /// </summary>
    void Register(string token, OsdiTokenInfo info);

    /// <summary>
    /// Retrieves and validates a token. Returns null if not found or expired.
    /// </summary>
    OsdiTokenInfo? Get(string token);

    /// <summary>
    /// Removes a token after it has been consumed.
    /// </summary>
    void Remove(string token);
}

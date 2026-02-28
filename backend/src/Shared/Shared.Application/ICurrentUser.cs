namespace Shared.Application;

/// <summary>
/// Provides access to the currently authenticated user's identity and permissions.
/// Implemented in Shared.Infrastructure by reading from HttpContext claims.
/// </summary>
public interface ICurrentUser
{
    Guid UserId { get; }
    Guid BranchId { get; }
    string Email { get; }
    IReadOnlyList<string> Permissions { get; }
}

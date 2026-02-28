namespace Shared.Application;

/// <summary>
/// Provides the current branch (tenant) context for multi-tenant query filtering.
/// Implemented in Shared.Infrastructure by reading BranchId from HttpContext claims.
/// </summary>
public interface IBranchContext
{
    Guid CurrentBranchId { get; }
}

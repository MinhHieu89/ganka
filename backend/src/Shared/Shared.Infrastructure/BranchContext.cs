using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Application;

namespace Shared.Infrastructure;

/// <summary>
/// Reads the current branch (tenant) context from HttpContext JWT claims.
/// Used by EF Core global query filters for multi-tenant data isolation.
/// Registered as Scoped in the DI container.
/// </summary>
public sealed class BranchContext : IBranchContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BranchContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid CurrentBranchId =>
        Guid.TryParse(
            _httpContextAccessor.HttpContext?.User?.FindFirstValue("branch_id"),
            out var id)
            ? id
            : Guid.Empty;
}

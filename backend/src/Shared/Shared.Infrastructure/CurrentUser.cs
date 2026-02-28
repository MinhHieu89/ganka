using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Application;

namespace Shared.Infrastructure;

/// <summary>
/// Reads the currently authenticated user's identity from HttpContext JWT claims.
/// Registered as Scoped in the DI container.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : Guid.Empty;

    public Guid BranchId =>
        Guid.TryParse(User?.FindFirstValue("branch_id"), out var id)
            ? id
            : Guid.Empty;

    public string Email =>
        User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public IReadOnlyList<string> Permissions =>
        User?.FindAll("permission")
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly()
        ?? (IReadOnlyList<string>)Array.Empty<string>();
}

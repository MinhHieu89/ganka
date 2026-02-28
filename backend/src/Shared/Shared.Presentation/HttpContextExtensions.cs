using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Shared.Presentation;

/// <summary>
/// Extension methods for HttpContext to extract common claims and user information.
/// Centralizes claims parsing for all Presentation layer endpoints.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Attempts to extract the authenticated user's ID from the ClaimTypes.NameIdentifier claim.
    /// Returns true if the claim exists and is a valid Guid; otherwise returns false with Guid.Empty.
    /// </summary>
    public static bool TryGetUserId(this HttpContext httpContext, out Guid userId)
    {
        var claim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (claim is not null && Guid.TryParse(claim, out userId))
            return true;

        userId = Guid.Empty;
        return false;
    }
}

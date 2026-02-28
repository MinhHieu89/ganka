using System.Security.Claims;
using Auth.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Auth.Application.Endpoints;

/// <summary>
/// Authenticated endpoint for logging out.
/// Revokes the specified refresh token.
/// </summary>
public static class LogoutEndpoint
{
    [WolverinePost("/api/auth/logout")]
    [Authorize]
    public static async Task<IResult> Logout(
        [FromBody] LogoutRequest request,
        [FromServices] IAuthService authService,
        ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var result = await authService.LogoutAsync(userId, request.RefreshToken);

        if (result.IsFailure)
            return Results.Problem(result.Error.Description, statusCode: 400);

        return Results.Ok();
    }
}

public sealed record LogoutRequest(string RefreshToken);

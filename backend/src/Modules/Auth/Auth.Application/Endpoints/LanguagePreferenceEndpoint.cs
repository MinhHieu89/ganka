using System.Security.Claims;
using Auth.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Auth.Application.Endpoints;

/// <summary>
/// Endpoint for updating the current user's language preference.
/// </summary>
public static class LanguagePreferenceEndpoint
{
    [WolverinePut("/api/auth/language")]
    [Authorize]
    public static async Task<IResult> UpdateLanguage(
        [FromBody] UpdateLanguageRequest request,
        [FromServices] IAuthService authService,
        ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        // Validate language
        if (request.Language is not ("vi" or "en"))
            return Results.BadRequest(new { error = "Language must be 'vi' or 'en'." });

        var result = await authService.UpdateLanguagePreferenceAsync(userId, request.Language);

        if (result.IsFailure)
            return Results.Problem(result.Error.Description, statusCode: 400);

        return Results.Ok();
    }
}

public sealed record UpdateLanguageRequest(string Language);

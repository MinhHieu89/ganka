using Auth.Application.Services;
using Auth.Contracts.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Auth.Application.Endpoints;

/// <summary>
/// Public endpoint for refreshing JWT tokens.
/// Uses refresh token rotation with family-based theft detection.
/// </summary>
public static class RefreshTokenEndpoint
{
    [WolverinePost("/api/auth/refresh")]
    public static async Task<IResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        [FromServices] IAuthService authService)
    {
        var result = await authService.RefreshTokenAsync(request);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Error.Unauthorized" => Results.Unauthorized(),
                _ => Results.Problem(result.Error.Description, statusCode: 400)
            };
        }

        return Results.Ok(result.Value);
    }
}

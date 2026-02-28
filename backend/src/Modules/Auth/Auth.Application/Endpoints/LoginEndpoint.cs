using Auth.Application.Services;
using Auth.Contracts.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Auth.Application.Endpoints;

/// <summary>
/// Public endpoint for user authentication.
/// Returns JWT access token and refresh token on valid credentials.
/// </summary>
public static class LoginEndpoint
{
    [WolverinePost("/api/auth/login")]
    public static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IAuthService authService,
        HttpContext httpContext)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var result = await authService.LoginAsync(request, ipAddress);

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

using System.Security.Claims;
using Auth.Application.Services;
using Auth.Contracts.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Auth.Application.Endpoints;

/// <summary>
/// Admin endpoints for user management and current user info.
/// </summary>
public static class UserEndpoints
{
    [WolverineGet("/api/admin/users")]
    [Authorize]
    public static async Task<IResult> GetUsers(
        [FromServices] IUserService userService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await userService.GetUsersAsync(page, pageSize);

        if (result.IsFailure)
            return Results.Problem(result.Error.Description, statusCode: 400);

        var (users, totalCount) = result.Value;
        return Results.Ok(new { data = users, totalCount, page, pageSize });
    }

    [WolverinePost("/api/admin/users")]
    [Authorize]
    public static async Task<IResult> CreateUser(
        [FromBody] CreateUserCommand command,
        [FromServices] IUserService userService)
    {
        var result = await userService.CreateUserAsync(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Error.Conflict" => Results.Conflict(new { error = result.Error.Description }),
                _ => Results.Problem(result.Error.Description, statusCode: 400)
            };
        }

        return Results.Created($"/api/admin/users/{result.Value}", new { Id = result.Value });
    }

    [WolverinePut("/api/admin/users/{id}")]
    [Authorize]
    public static async Task<IResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserCommand command,
        [FromServices] IUserService userService)
    {
        var result = await userService.UpdateUserAsync(id, command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Error.NotFound" => Results.NotFound(),
                _ => Results.Problem(result.Error.Description, statusCode: 400)
            };
        }

        return Results.Ok();
    }

    [WolverinePut("/api/admin/users/{id}/roles")]
    [Authorize]
    public static async Task<IResult> AssignRoles(
        Guid id,
        [FromBody] AssignRolesCommand command,
        [FromServices] IUserService userService)
    {
        var result = await userService.AssignRolesAsync(id, command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Error.NotFound" => Results.NotFound(),
                _ => Results.Problem(result.Error.Description, statusCode: 400)
            };
        }

        return Results.Ok();
    }

    [WolverineGet("/api/auth/me")]
    [Authorize]
    public static async Task<IResult> GetCurrentUser(
        ClaimsPrincipal principal,
        [FromServices] IAuthService authService)
    {
        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var result = await authService.GetUserByIdAsync(userId);

        if (result.IsFailure)
            return Results.NotFound();

        return Results.Ok(result.Value);
    }
}

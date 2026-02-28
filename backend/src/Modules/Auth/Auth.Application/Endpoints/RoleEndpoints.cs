using Auth.Application.Services;
using Auth.Contracts.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Auth.Application.Endpoints;

/// <summary>
/// Admin endpoints for role management.
/// </summary>
public static class RoleEndpoints
{
    [WolverineGet("/api/admin/roles")]
    [Authorize]
    public static async Task<IResult> GetRoles(
        [FromServices] IRoleService roleService)
    {
        var result = await roleService.GetRolesAsync();

        if (result.IsFailure)
            return Results.Problem(result.Error.Description, statusCode: 400);

        return Results.Ok(result.Value);
    }

    [WolverinePost("/api/admin/roles")]
    [Authorize]
    public static async Task<IResult> CreateRole(
        [FromBody] CreateRoleCommand command,
        [FromServices] IRoleService roleService)
    {
        var result = await roleService.CreateRoleAsync(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Error.Conflict" => Results.Conflict(new { error = result.Error.Description }),
                _ => Results.Problem(result.Error.Description, statusCode: 400)
            };
        }

        return Results.Created($"/api/admin/roles/{result.Value}", new { Id = result.Value });
    }

    [WolverinePut("/api/admin/roles/{id}/permissions")]
    [Authorize]
    public static async Task<IResult> UpdateRolePermissions(
        Guid id,
        [FromBody] UpdateRolePermissionsCommand command,
        [FromServices] IRoleService roleService)
    {
        var commandWithId = command with { RoleId = id };
        var result = await roleService.UpdateRolePermissionsAsync(commandWithId);

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
}

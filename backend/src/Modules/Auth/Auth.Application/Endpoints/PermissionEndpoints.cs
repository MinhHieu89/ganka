using Auth.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Auth.Application.Endpoints;

/// <summary>
/// Admin endpoint for listing all permissions grouped by module.
/// </summary>
public static class PermissionEndpoints
{
    [WolverineGet("/api/admin/permissions")]
    [Authorize]
    public static async Task<IResult> GetPermissions(
        [FromServices] IPermissionService permissionService)
    {
        var result = await permissionService.GetPermissionsGroupedByModuleAsync();

        if (result.IsFailure)
            return Results.Problem(result.Error.Description, statusCode: 400);

        return Results.Ok(result.Value);
    }
}

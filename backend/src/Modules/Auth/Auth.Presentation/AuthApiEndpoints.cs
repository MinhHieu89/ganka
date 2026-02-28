using System.Security.Claims;
using Auth.Application.Features;
using Auth.Contracts.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain;
using Wolverine;

namespace Auth.Presentation;

/// <summary>
/// Extension methods for mapping Auth module Minimal API endpoints.
/// Auth-flow endpoints (login, logout, refresh, me, language) and admin-flow endpoints.
/// </summary>
public static class AuthApiEndpoints
{
    /// <summary>
    /// Maps all Auth module Minimal API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapAuthApiEndpoints(this IEndpointRouteBuilder app)
    {
        MapAuthFlowEndpoints(app);
        MapAdminUserEndpoints(app);
        MapAdminRoleEndpoints(app);
        MapAdminPermissionEndpoints(app);

        return app;
    }

    private static void MapAuthFlowEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            LoginCommand command,
            IMessageBus bus,
            HttpContext httpContext) =>
        {
            var enriched = command with { IpAddress = httpContext.Connection.RemoteIpAddress?.ToString() };
            var result = await bus.InvokeAsync<Result<LoginResponse>>(enriched);

            if (result.IsFailure)
            {
                return result.Error.Code switch
                {
                    "Error.Unauthorized" => Results.Problem(
                        detail: result.Error.Description,
                        title: "Unauthorized",
                        statusCode: 401),
                    _ => Results.Problem(result.Error.Description, statusCode: 400)
                };
            }

            return Results.Ok(result.Value);
        });

        app.MapPost("/api/auth/logout", async (
            IMessageBus bus,
            HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var result = await bus.InvokeAsync<Result>(new LogoutCommand(userId));

            if (result.IsFailure)
                return Results.Problem(result.Error.Description, statusCode: 400);

            return Results.Ok();
        })
        .RequireAuthorization();

        app.MapPost("/api/auth/refresh", async (
            RefreshTokenCommand command,
            IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<Result<RefreshTokenResponse>>(command);

            if (result.IsFailure)
            {
                return result.Error.Code switch
                {
                    "Error.Unauthorized" => Results.Unauthorized(),
                    _ => Results.Problem(result.Error.Description, statusCode: 400)
                };
            }

            return Results.Ok(result.Value);
        });

        app.MapGet("/api/auth/me", async (
            IMessageBus bus,
            HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var result = await bus.InvokeAsync<Result<UserDto>>(new GetCurrentUserQuery(userId));

            if (result.IsFailure)
                return Results.NotFound();

            return Results.Ok(result.Value);
        })
        .RequireAuthorization();

        app.MapPut("/api/auth/language", async (
            UpdateLanguageCommand command,
            IMessageBus bus,
            HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var enriched = command with { UserId = userId };
            var result = await bus.InvokeAsync<Result>(enriched);

            if (result.IsFailure)
                return Results.Problem(result.Error.Description, statusCode: 400);

            return Results.Ok();
        })
        .RequireAuthorization();
    }

    private static void MapAdminUserEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/users", async (
            IMessageBus bus,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default) =>
        {
            var response = await bus.InvokeAsync<GetUsersResponse>(
                new GetUsersQuery(page, pageSize), cancellationToken);

            return Results.Ok(new { data = response.Users, totalCount = response.TotalCount, page = response.Page, pageSize = response.PageSize });
        })
        .RequireAuthorization();

        app.MapPost("/api/admin/users", async (
            CreateUserCommand command,
            IMessageBus bus,
            CancellationToken cancellationToken = default) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error.Code switch
                {
                    "Error.Conflict" => Results.Conflict(new { error = result.Error.Description }),
                    _ => Results.Problem(result.Error.Description, statusCode: 400)
                };
            }

            return Results.Created($"/api/admin/users/{result.Value}", new { Id = result.Value });
        })
        .RequireAuthorization();

        app.MapPut("/api/admin/users/{id}", async (
            Guid id,
            UpdateUserCommand command,
            IMessageBus bus,
            CancellationToken cancellationToken = default) =>
        {
            var commandWithId = command with { UserId = id };
            var result = await bus.InvokeAsync<Result>(commandWithId, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error.Code switch
                {
                    "Error.NotFound" => Results.NotFound(),
                    _ => Results.Problem(result.Error.Description, statusCode: 400)
                };
            }

            return Results.Ok();
        })
        .RequireAuthorization();

        app.MapPut("/api/admin/users/{id}/roles", async (
            Guid id,
            AssignRolesCommand command,
            IMessageBus bus,
            CancellationToken cancellationToken = default) =>
        {
            var commandWithId = command with { UserId = id };
            var result = await bus.InvokeAsync<Result>(commandWithId, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error.Code switch
                {
                    "Error.NotFound" => Results.NotFound(),
                    _ => Results.Problem(result.Error.Description, statusCode: 400)
                };
            }

            return Results.Ok();
        })
        .RequireAuthorization();
    }

    private static void MapAdminRoleEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/roles", async (
            IMessageBus bus,
            CancellationToken cancellationToken = default) =>
        {
            var roles = await bus.InvokeAsync<List<RoleDto>>(new GetRolesQuery(), cancellationToken);
            return Results.Ok(roles);
        })
        .RequireAuthorization();

        app.MapPost("/api/admin/roles", async (
            CreateRoleCommand command,
            IMessageBus bus,
            CancellationToken cancellationToken = default) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error.Code switch
                {
                    "Error.Conflict" => Results.Conflict(new { error = result.Error.Description }),
                    _ => Results.Problem(result.Error.Description, statusCode: 400)
                };
            }

            return Results.Created($"/api/admin/roles/{result.Value}", new { Id = result.Value });
        })
        .RequireAuthorization();

        app.MapPut("/api/admin/roles/{id}/permissions", async (
            Guid id,
            UpdateRolePermissionsCommand command,
            IMessageBus bus,
            CancellationToken cancellationToken = default) =>
        {
            var commandWithId = command with { RoleId = id };
            var result = await bus.InvokeAsync<Result>(commandWithId, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error.Code switch
                {
                    "Error.NotFound" => Results.NotFound(),
                    _ => Results.Problem(result.Error.Description, statusCode: 400)
                };
            }

            return Results.Ok();
        })
        .RequireAuthorization();
    }

    private static void MapAdminPermissionEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/permissions", async (
            IMessageBus bus,
            CancellationToken cancellationToken = default) =>
        {
            var permissions = await bus.InvokeAsync<List<PermissionGroupDto>>(
                new GetPermissionsQuery(), cancellationToken);
            return Results.Ok(permissions);
        })
        .RequireAuthorization();
    }
}

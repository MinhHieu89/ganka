using Auth.Application.Features;
using Auth.Contracts.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Auth.Presentation;

/// <summary>
/// Extension methods for mapping Auth module Minimal API endpoints.
/// Auth-flow endpoints (login, logout, refresh, me, language) and admin-flow endpoints.
/// Uses route groups to consolidate shared concerns (prefixes, authorization).
/// </summary>
public static class AuthApiEndpoints
{
    /// <summary>
    /// Maps all Auth module Minimal API endpoints using route groups.
    /// </summary>
    public static IEndpointRouteBuilder MapAuthApiEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/api/auth");
        var adminGroup = app.MapGroup("/api/admin").RequireAuthorization();

        MapAuthFlowEndpoints(authGroup);
        MapAdminUserEndpoints(adminGroup);
        MapAdminRoleEndpoints(adminGroup);
        MapAdminPermissionEndpoints(adminGroup);

        return app;
    }

    private static void MapAuthFlowEndpoints(RouteGroupBuilder authGroup)
    {
        authGroup.MapPost("/login", async (LoginCommand command, IMessageBus bus, HttpContext ctx) =>
        {
            var enriched = command with { IpAddress = ctx.Connection.RemoteIpAddress?.ToString() };
            var result = await bus.InvokeAsync<Result<LoginResponse>>(enriched);
            return result.ToHttpResult();
        });

        authGroup.MapPost("/logout", async (IMessageBus bus, HttpContext ctx) =>
        {
            if (!ctx.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var result = await bus.InvokeAsync<Result>(new LogoutCommand(userId));
            return result.ToHttpResult();
        }).RequireAuthorization();

        authGroup.MapPost("/refresh", async (RefreshTokenCommand command, IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<Result<RefreshTokenResponse>>(command);
            return result.ToHttpResult();
        });

        authGroup.MapGet("/me", async (IMessageBus bus, HttpContext ctx) =>
        {
            if (!ctx.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var result = await bus.InvokeAsync<Result<UserDto>>(new GetCurrentUserQuery(userId));
            return result.ToHttpResult();
        }).RequireAuthorization();

        authGroup.MapPut("/language", async (UpdateLanguageCommand command, IMessageBus bus, HttpContext ctx) =>
        {
            if (!ctx.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var enriched = command with { UserId = userId };
            var result = await bus.InvokeAsync<Result>(enriched);
            return result.ToHttpResult();
        }).RequireAuthorization();
    }

    private static void MapAdminUserEndpoints(RouteGroupBuilder adminGroup)
    {
        adminGroup.MapGet("/users", async (IMessageBus bus, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
        {
            var response = await bus.InvokeAsync<GetUsersResponse>(new GetUsersQuery(page, pageSize), ct);
            return Results.Ok(new { data = response.Users, totalCount = response.TotalCount, page = response.Page, pageSize = response.PageSize });
        });

        adminGroup.MapPost("/users", async (CreateUserCommand command, IMessageBus bus, CancellationToken ct = default) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/admin/users");
        });

        adminGroup.MapPut("/users/{id}", async (Guid id, UpdateUserCommand command, IMessageBus bus, CancellationToken ct = default) =>
        {
            var result = await bus.InvokeAsync<Result>(command with { UserId = id }, ct);
            return result.ToHttpResult();
        });

        adminGroup.MapPut("/users/{id}/roles", async (Guid id, AssignRolesCommand command, IMessageBus bus, CancellationToken ct = default) =>
        {
            var result = await bus.InvokeAsync<Result>(command with { UserId = id }, ct);
            return result.ToHttpResult();
        });
    }

    private static void MapAdminRoleEndpoints(RouteGroupBuilder adminGroup)
    {
        adminGroup.MapGet("/roles", async (IMessageBus bus, CancellationToken ct = default) =>
        {
            var roles = await bus.InvokeAsync<List<RoleDto>>(new GetRolesQuery(), ct);
            return Results.Ok(roles);
        });

        adminGroup.MapPost("/roles", async (CreateRoleCommand command, IMessageBus bus, CancellationToken ct = default) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/admin/roles");
        });

        adminGroup.MapPut("/roles/{id}/permissions", async (Guid id, UpdateRolePermissionsCommand command, IMessageBus bus, CancellationToken ct = default) =>
        {
            var result = await bus.InvokeAsync<Result>(command with { RoleId = id }, ct);
            return result.ToHttpResult();
        });
    }

    private static void MapAdminPermissionEndpoints(RouteGroupBuilder adminGroup)
    {
        adminGroup.MapGet("/permissions", async (IMessageBus bus, CancellationToken ct = default) =>
        {
            var permissions = await bus.InvokeAsync<List<PermissionGroupDto>>(new GetPermissionsQuery(), ct);
            return Results.Ok(permissions);
        });
    }
}

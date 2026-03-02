using Auth.Application.Features;
using Auth.Contracts.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Auth.Presentation;

/// <summary>
/// Extension methods for mapping Auth module Minimal API endpoints.
/// Auth-flow endpoints (login, logout, refresh, me, language) and admin-flow endpoints.
/// Uses route groups to consolidate shared concerns (prefixes, authorization).
/// Refresh tokens are stored in HTTP-only cookies (not in JSON response bodies).
/// </summary>
public static class AuthApiEndpoints
{
    private const string RefreshTokenCookieName = "refresh_token";
    private const string CookiePath = "/api/auth";
    private const int RememberMeMaxAgeDays = 30;

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
        authGroup.MapPost("/login", async (LoginCommand command, IMessageBus bus, HttpContext ctx, IWebHostEnvironment env) =>
        {
            var enriched = command with { IpAddress = ctx.Connection.RemoteIpAddress?.ToString() };
            var result = await bus.InvokeAsync<Result<LoginResponse>>(enriched);

            if (!result.IsSuccess)
                return result.ToHttpResult();

            var loginResponse = result.Value;

            // Set refresh token as HTTP-only cookie
            SetRefreshTokenCookie(ctx, loginResponse.RefreshToken, command.RememberMe, env);

            // Return response WITHOUT refreshToken in the JSON body
            return Results.Ok(new
            {
                accessToken = loginResponse.AccessToken,
                expiresAt = loginResponse.ExpiresAt,
                user = loginResponse.User
            });
        });

        authGroup.MapPost("/logout", async (IMessageBus bus, HttpContext ctx) =>
        {
            if (!ctx.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var result = await bus.InvokeAsync<Result>(new LogoutCommand(userId));

            if (result.IsSuccess)
            {
                // Clear the refresh token cookie
                ClearRefreshTokenCookie(ctx);
            }

            return result.ToHttpResult();
        }).RequireAuthorization();

        authGroup.MapPost("/refresh", async (IMessageBus bus, HttpContext ctx, IWebHostEnvironment env) =>
        {
            // Read refresh token from cookie instead of request body
            var refreshTokenValue = ctx.Request.Cookies[RefreshTokenCookieName];

            if (string.IsNullOrEmpty(refreshTokenValue))
                return Results.Problem(
                    detail: "Unauthorized",
                    title: "Unauthorized",
                    statusCode: 401);

            var command = new RefreshTokenCommand(refreshTokenValue);
            var result = await bus.InvokeAsync<Result<RefreshTokenResponse>>(command);

            if (!result.IsSuccess)
                return result.ToHttpResult();

            var refreshResponse = result.Value;

            // The handler returns the new refresh token value and the RememberMe flag
            // is preserved on the new token entity. We need to determine RememberMe
            // from the response context. The RefreshTokenHandler copies RememberMe
            // from old to new token, so we read it via the handler.
            // Since we can't easily get RememberMe from the response, we'll look it up
            // by reading the new token's RememberMe from the domain.
            // Actually, the simplest approach: include RememberMe in the response.
            // But to avoid changing RefreshTokenResponse, we can use the convention
            // that the handler already preserves it. We need it here though.
            // Solution: Add RememberMe to RefreshTokenResponse.

            // For now, the response includes RememberMe -- see updated RefreshTokenResponse
            SetRefreshTokenCookie(ctx, refreshResponse.RefreshToken, refreshResponse.RememberMe, env);

            // Return response WITHOUT refreshToken in the JSON body
            return Results.Ok(new
            {
                accessToken = refreshResponse.AccessToken,
                expiresAt = refreshResponse.ExpiresAt,
                user = refreshResponse.User
            });
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

    /// <summary>
    /// Sets the refresh token as an HTTP-only cookie on the response.
    /// </summary>
    private static void SetRefreshTokenCookie(HttpContext ctx, string refreshToken, bool rememberMe, IWebHostEnvironment env)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = CookiePath
        };

        if (rememberMe)
        {
            cookieOptions.MaxAge = TimeSpan.FromDays(RememberMeMaxAgeDays);
        }
        // No MaxAge = session cookie (expires when browser closes)

        ctx.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
    }

    /// <summary>
    /// Clears the refresh token cookie by deleting it.
    /// </summary>
    private static void ClearRefreshTokenCookie(HttpContext ctx)
    {
        ctx.Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = CookiePath
        });
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

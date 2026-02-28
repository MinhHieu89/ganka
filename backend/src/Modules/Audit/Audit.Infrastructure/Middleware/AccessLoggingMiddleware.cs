using System.Security.Claims;
using Audit.Domain.Entities;
using Audit.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Audit.Infrastructure.Middleware;

/// <summary>
/// ASP.NET Core middleware that logs all HTTP requests to the audit.AccessLogs table.
/// For authenticated requests: captures UserId, UserEmail, resource path, and response status code.
/// For login endpoints: logs Login/LoginFailed based on response status.
/// Uses fire-and-forget pattern to avoid blocking the response pipeline.
/// </summary>
public sealed class AccessLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AccessLoggingMiddleware> _logger;

    private static readonly HashSet<string> LoginPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/login",
        "/api/auth/refresh"
    };

    private static readonly HashSet<string> LogoutPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/logout"
    };

    public AccessLoggingMiddleware(RequestDelegate next, ILogger<AccessLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Let the request flow through the pipeline first
        await _next(context);

        // Fire-and-forget: log access asynchronously without blocking the response
        _ = LogAccessAsync(context);
    }

    private async Task LogAccessAsync(HttpContext context)
    {
        try
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Skip logging for non-API paths (Swagger, health checks, static files)
            if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
                return;

            var userId = GetUserId(context);
            var userEmail = GetUserEmail(context);
            var branchId = GetBranchId(context);
            var action = DetermineAction(path, context.Response.StatusCode);
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var statusCode = context.Response.StatusCode;

            // Truncate user agent to prevent excessive storage
            if (userAgent.Length > 1024)
                userAgent = userAgent[..1024];

            var accessLog = AccessLog.Create(
                userId,
                userEmail,
                action,
                path,
                ipAddress,
                userAgent,
                statusCode,
                branchId);

            using var scope = context.RequestServices.CreateScope();
            var auditDbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
            auditDbContext.AccessLogs.Add(accessLog);
            await auditDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Access logging must never break the application -- swallow and log errors
            _logger.LogError(ex, "Failed to write access log entry");
        }
    }

    private static AccessAction DetermineAction(string path, int statusCode)
    {
        if (LoginPaths.Contains(path))
            return statusCode is >= 200 and < 300 ? AccessAction.Login : AccessAction.LoginFailed;

        if (LogoutPaths.Contains(path))
            return AccessAction.Logout;

        // GET requests to patient/clinical endpoints are record views
        if (path.Contains("/patients/", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/clinical/", StringComparison.OrdinalIgnoreCase))
            return AccessAction.ViewRecord;

        return AccessAction.ApiRequest;
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private static string? GetUserEmail(HttpContext context)
    {
        return context.User.FindFirstValue(ClaimTypes.Email);
    }

    private static Guid? GetBranchId(HttpContext context)
    {
        var claim = context.User.FindFirstValue("branch_id");
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

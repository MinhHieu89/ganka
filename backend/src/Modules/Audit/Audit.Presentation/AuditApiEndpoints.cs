using Audit.Application.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Presentation;
using Wolverine;

namespace Audit.Presentation;

/// <summary>
/// Extension methods for mapping Audit module Minimal API endpoints.
/// All audit endpoints require authorization and are prefixed with /api/admin.
/// Uses route groups to consolidate shared concerns (prefix, authorization).
/// </summary>
public static class AuditApiEndpoints
{
    /// <summary>
    /// Maps all Audit module Minimal API endpoints using route groups.
    /// </summary>
    public static IEndpointRouteBuilder MapAuditApiEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/admin").RequireAuthorization();

        // Audit logs: query with filtering and cursor-based pagination
        adminGroup.MapGet("/audit-logs", async ([AsParameters] GetAuditLogsQuery query, IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<GetAuditLogsResponse>(query);
            return Results.Ok(result);
        });

        // Audit logs: export as CSV file download
        adminGroup.MapGet("/audit-logs/export", async ([AsParameters] ExportAuditLogsQuery query, IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<ExportAuditLogsResponse>(query);
            return Results.File(result.FileContents, "text/csv", result.FileName);
        });

        // Access logs: query with filtering and cursor-based pagination
        adminGroup.MapGet("/access-logs", async ([AsParameters] GetAccessLogsQuery query, IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<GetAccessLogsResponse>(query);
            return Results.Ok(result);
        });

        return app;
    }
}

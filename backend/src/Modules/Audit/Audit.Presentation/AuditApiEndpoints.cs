using Audit.Application.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Audit.Presentation;

/// <summary>
/// Extension methods for mapping Audit module Minimal API endpoints.
/// All audit endpoints require authorization and are prefixed with /api/admin.
/// </summary>
public static class AuditApiEndpoints
{
    /// <summary>
    /// Maps all Audit module Minimal API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapAuditApiEndpoints(this IEndpointRouteBuilder app)
    {
        // Audit logs: query with filtering and cursor-based pagination
        app.MapGet("/api/admin/audit-logs", async (
            [AsParameters] GetAuditLogsQuery query,
            IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<GetAuditLogsResponse>(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        // Audit logs: export as CSV file download
        app.MapGet("/api/admin/audit-logs/export", async (
            [AsParameters] ExportAuditLogsQuery query,
            IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<ExportAuditLogsResponse>(query);
            return Results.File(result.FileContents, "text/csv", result.FileName);
        }).RequireAuthorization();

        // Access logs: query with filtering and cursor-based pagination
        app.MapGet("/api/admin/access-logs", async (
            [AsParameters] GetAccessLogsQuery query,
            IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<GetAccessLogsResponse>(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        return app;
    }
}

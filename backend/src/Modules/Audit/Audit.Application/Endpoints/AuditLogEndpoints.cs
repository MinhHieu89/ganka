using System.Globalization;
using System.Text;
using System.Text.Json;
using Audit.Contracts.Dtos;
using Audit.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace Audit.Application.Endpoints;

/// <summary>
/// Admin endpoints for querying and exporting audit log entries.
/// Supports filtering by user, action, entity name, and date range
/// with cursor-based pagination for performance.
/// </summary>
public static class AuditLogEndpoints
{
    /// <summary>
    /// Query audit logs with filtering and cursor-based pagination.
    /// Cursor is based on (Timestamp, Id) for stable ordering.
    /// </summary>
    [WolverineGet("/api/admin/audit-logs")]
    public static async Task<IResult> GetAuditLogs(
        [FromServices] IAuditReadContext readContext,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? actionType = null,
        [FromQuery] string? entityName = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] DateTime? cursorTimestamp = null,
        [FromQuery] Guid? cursorId = null,
        [FromQuery] int pageSize = 50)
    {
        // Clamp page size to prevent abuse
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = readContext.AuditLogs;

        // Apply filters
        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrEmpty(actionType) && Enum.TryParse<AuditAction>(actionType, true, out var action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrEmpty(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (dateFrom.HasValue)
            query = query.Where(a => a.Timestamp >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(a => a.Timestamp <= dateTo.Value);

        // Cursor-based pagination: get records older than the cursor
        if (cursorTimestamp.HasValue && cursorId.HasValue)
        {
            query = query.Where(a =>
                a.Timestamp < cursorTimestamp.Value ||
                (a.Timestamp == cursorTimestamp.Value && a.Id.CompareTo(cursorId.Value) < 0));
        }

        // Order by timestamp descending, then by Id descending for stable cursor
        var results = await query
            .OrderByDescending(a => a.Timestamp)
            .ThenByDescending(a => a.Id)
            .Take(pageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.Timestamp,
                a.UserEmail,
                a.EntityName,
                a.EntityId,
                a.Action.ToString(),
                DeserializeChanges(a.Changes)))
            .ToListAsync();

        // Build cursor for next page
        var nextCursor = results.Count == pageSize && results.Count > 0
            ? new { timestamp = results[^1].Timestamp, id = results[^1].Id }
            : null;

        return Results.Ok(new
        {
            data = results,
            nextCursor,
            pageSize
        });
    }

    /// <summary>
    /// Export audit logs as CSV file download with the same filter parameters.
    /// </summary>
    [WolverineGet("/api/admin/audit-logs/export")]
    public static async Task<IResult> ExportAuditLogs(
        [FromServices] IAuditReadContext readContext,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? actionType = null,
        [FromQuery] string? entityName = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var query = readContext.AuditLogs;

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrEmpty(actionType) && Enum.TryParse<AuditAction>(actionType, true, out var action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrEmpty(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (dateFrom.HasValue)
            query = query.Where(a => a.Timestamp >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(a => a.Timestamp <= dateTo.Value);

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Take(10000) // Cap export to prevent OOM
            .Select(a => new
            {
                a.Id,
                a.Timestamp,
                a.UserEmail,
                a.EntityName,
                a.EntityId,
                Action = a.Action.ToString(),
                a.Changes
            })
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Id,Timestamp,UserEmail,EntityName,EntityId,Action,Changes");

        foreach (var log in logs)
        {
            var escapedChanges = log.Changes.Replace("\"", "\"\"");
            csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "{0},{1:O},{2},{3},{4},{5},\"{6}\"",
                log.Id,
                log.Timestamp,
                log.UserEmail,
                log.EntityName,
                log.EntityId,
                log.Action,
                escapedChanges));
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return Results.File(bytes, "text/csv", $"audit-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
    }

    private static IReadOnlyList<AuditChangeDto> DeserializeChanges(string changesJson)
    {
        if (string.IsNullOrEmpty(changesJson))
            return Array.Empty<AuditChangeDto>();

        try
        {
            return JsonSerializer.Deserialize<List<AuditChangeDto>>(changesJson, JsonOptions)
                ?? (IReadOnlyList<AuditChangeDto>)Array.Empty<AuditChangeDto>();
        }
        catch
        {
            return Array.Empty<AuditChangeDto>();
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}

using Audit.Contracts.Dtos;
using Audit.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace Audit.Application.Endpoints;

/// <summary>
/// Admin endpoints for querying access log entries.
/// </summary>
public static class AccessLogEndpoints
{
    /// <summary>
    /// Query access logs with filtering and cursor-based pagination.
    /// </summary>
    [WolverineGet("/api/admin/access-logs")]
    public static async Task<IResult> GetAccessLogs(
        [FromServices] IAuditReadContext readContext,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] DateTime? cursorTimestamp = null,
        [FromQuery] Guid? cursorId = null,
        [FromQuery] int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = readContext.AccessLogs;

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrEmpty(action) && Enum.TryParse<AccessAction>(action, true, out var accessAction))
            query = query.Where(a => a.Action == accessAction);

        if (dateFrom.HasValue)
            query = query.Where(a => a.Timestamp >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(a => a.Timestamp <= dateTo.Value);

        if (cursorTimestamp.HasValue && cursorId.HasValue)
        {
            query = query.Where(a =>
                a.Timestamp < cursorTimestamp.Value ||
                (a.Timestamp == cursorTimestamp.Value && a.Id.CompareTo(cursorId.Value) < 0));
        }

        var results = await query
            .OrderByDescending(a => a.Timestamp)
            .ThenByDescending(a => a.Id)
            .Take(pageSize)
            .Select(a => new AccessLogDto(
                a.Id,
                a.Timestamp,
                a.UserEmail,
                a.Action.ToString(),
                a.Resource,
                a.IpAddress,
                a.StatusCode))
            .ToListAsync();

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
}

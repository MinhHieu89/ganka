using Audit.Application.Interfaces;
using Audit.Contracts.Dtos;
using Audit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Audit.Application.Features;

/// <summary>
/// Query access logs with filtering and cursor-based pagination.
/// </summary>
public sealed record GetAccessLogsQuery(
    Guid? UserId = null,
    string? Action = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    DateTime? CursorTimestamp = null,
    Guid? CursorId = null,
    int PageSize = 50);

public sealed record GetAccessLogsResponse(
    IReadOnlyList<AccessLogDto> Data,
    object? NextCursor,
    int PageSize);

public sealed class GetAccessLogsHandler
{
    private readonly IAuditReadRepository _repository;

    public GetAccessLogsHandler(IAuditReadRepository repository)
        => _repository = repository;

    public async Task<GetAccessLogsResponse> Handle(GetAccessLogsQuery query)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var dbQuery = _repository.AccessLogs;

        if (query.UserId.HasValue)
            dbQuery = dbQuery.Where(a => a.UserId == query.UserId.Value);

        if (!string.IsNullOrEmpty(query.Action) && Enum.TryParse<AccessAction>(query.Action, true, out var accessAction))
            dbQuery = dbQuery.Where(a => a.Action == accessAction);

        if (query.DateFrom.HasValue)
            dbQuery = dbQuery.Where(a => a.Timestamp >= query.DateFrom.Value);

        if (query.DateTo.HasValue)
            dbQuery = dbQuery.Where(a => a.Timestamp <= query.DateTo.Value);

        // Cursor-based pagination: get records older than the cursor
        if (query.CursorTimestamp.HasValue && query.CursorId.HasValue)
        {
            dbQuery = dbQuery.Where(a =>
                a.Timestamp < query.CursorTimestamp.Value ||
                (a.Timestamp == query.CursorTimestamp.Value && a.Id.CompareTo(query.CursorId.Value) < 0));
        }

        var results = await dbQuery
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

        return new GetAccessLogsResponse(results, nextCursor, pageSize);
    }
}

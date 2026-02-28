using System.Text.Json;
using Audit.Application.Interfaces;
using Audit.Contracts.Dtos;
using Audit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Audit.Application.Features;

/// <summary>
/// Query audit logs with filtering and cursor-based pagination.
/// Cursor is based on (Timestamp, Id) for stable ordering.
/// </summary>
public sealed record GetAuditLogsQuery(
    Guid? UserId = null,
    string? ActionType = null,
    string? EntityName = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    DateTime? CursorTimestamp = null,
    Guid? CursorId = null,
    int PageSize = 50);

public sealed record GetAuditLogsResponse(
    IReadOnlyList<AuditLogDto> Data,
    object? NextCursor,
    int PageSize);

public sealed class GetAuditLogsHandler
{
    private readonly IAuditReadRepository _repository;

    public GetAuditLogsHandler(IAuditReadRepository repository)
        => _repository = repository;

    public async Task<GetAuditLogsResponse> Handle(GetAuditLogsQuery query)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var dbQuery = _repository.AuditLogs;

        // Apply filters
        if (query.UserId.HasValue)
            dbQuery = dbQuery.Where(a => a.UserId == query.UserId.Value);

        if (!string.IsNullOrEmpty(query.ActionType) && Enum.TryParse<AuditAction>(query.ActionType, true, out var action))
            dbQuery = dbQuery.Where(a => a.Action == action);

        if (!string.IsNullOrEmpty(query.EntityName))
            dbQuery = dbQuery.Where(a => a.EntityName == query.EntityName);

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

        // Order by timestamp descending, then by Id descending for stable cursor
        var results = await dbQuery
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

        return new GetAuditLogsResponse(results, nextCursor, pageSize);
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

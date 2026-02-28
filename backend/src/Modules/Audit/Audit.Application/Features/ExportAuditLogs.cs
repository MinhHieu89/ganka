using System.Globalization;
using System.Text;
using Audit.Application.Interfaces;
using Audit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Audit.Application.Features;

/// <summary>
/// Export audit logs as CSV with the same filter parameters as GetAuditLogs.
/// </summary>
public sealed record ExportAuditLogsQuery(
    Guid? UserId = null,
    string? ActionType = null,
    string? EntityName = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null);

public sealed record ExportAuditLogsResponse(byte[] FileContents, string FileName);

public sealed class ExportAuditLogsHandler
{
    private readonly IAuditReadRepository _repository;

    public ExportAuditLogsHandler(IAuditReadRepository repository)
        => _repository = repository;

    public async Task<ExportAuditLogsResponse> Handle(ExportAuditLogsQuery query)
    {
        var dbQuery = _repository.AuditLogs;

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

        var logs = await dbQuery
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
        var fileName = $"audit-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return new ExportAuditLogsResponse(bytes, fileName);
    }
}

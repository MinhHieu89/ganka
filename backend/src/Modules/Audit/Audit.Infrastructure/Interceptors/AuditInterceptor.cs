using System.Text.Json;
using Audit.Domain.Entities;
using Audit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application;
using Shared.Domain;

namespace Audit.Infrastructure.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that automatically captures field-level changes
/// on all IAuditable entities and writes them to the audit.AuditLogs table.
///
/// Flow:
/// 1. SavingChangesAsync: Iterate ChangeTracker for IAuditable entities, capture old/new values
/// 2. SavedChangesAsync: After successful save, write audit entries to AuditDbContext
///
/// This ensures audit records only exist for successfully committed changes.
/// Registered on ALL module DbContexts in the Bootstrapper.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    private List<AuditEntry>? _pendingEntries;

    public AuditInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null || eventData.Context is AuditDbContext)
        {
            // Don't audit the audit context itself to avoid infinite recursion
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        _pendingEntries = CaptureAuditEntries(eventData.Context.ChangeTracker);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_pendingEntries is { Count: > 0 })
        {
            await WriteAuditEntriesAsync(_pendingEntries, cancellationToken);
            _pendingEntries = null;
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        // Discard pending entries on failure -- no audit record for failed saves
        _pendingEntries = null;
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _pendingEntries = null;
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private List<AuditEntry> CaptureAuditEntries(ChangeTracker changeTracker)
    {
        var entries = new List<AuditEntry>();

        // Get current user info for the audit record
        var currentUser = _serviceProvider.GetService<ICurrentUser>();
        var userId = currentUser?.UserId ?? Guid.Empty;
        var userEmail = currentUser?.Email ?? "system";
        var branchId = currentUser?.BranchId ?? Guid.Empty;

        foreach (var entry in changeTracker.Entries())
        {
            // Only audit entities that implement IAuditable
            if (entry.Entity is not IAuditable)
                continue;

            // Only track Added, Modified, Deleted states
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            var entityName = entry.Entity.GetType().Name;
            var entityId = GetEntityId(entry);
            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Modified => AuditAction.Updated,
                EntityState.Deleted => AuditAction.Deleted,
                _ => AuditAction.Updated
            };

            var changes = CaptureFieldChanges(entry);

            entries.Add(new AuditEntry(
                userId,
                userEmail,
                entityName,
                entityId,
                action,
                changes,
                branchId));
        }

        return entries;
    }

    private static string GetEntityId(EntityEntry entry)
    {
        var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties;
        if (keyProperties is null || keyProperties.Count == 0)
            return string.Empty;

        if (keyProperties.Count == 1)
            return entry.CurrentValues[keyProperties[0]]?.ToString() ?? string.Empty;

        // Composite key -- join with underscore
        return string.Join("_",
            keyProperties.Select(p => entry.CurrentValues[p]?.ToString() ?? string.Empty));
    }

    private static List<FieldChange> CaptureFieldChanges(EntityEntry entry)
    {
        var changes = new List<FieldChange>();

        switch (entry.State)
        {
            case EntityState.Added:
                foreach (var property in entry.CurrentValues.Properties)
                {
                    var currentValue = entry.CurrentValues[property];
                    if (currentValue is not null)
                    {
                        changes.Add(new FieldChange(
                            property.Name,
                            null,
                            currentValue.ToString()));
                    }
                }
                break;

            case EntityState.Modified:
                foreach (var property in entry.OriginalValues.Properties)
                {
                    var originalValue = entry.OriginalValues[property];
                    var currentValue = entry.CurrentValues[property];

                    // Only record properties that actually changed
                    if (!Equals(originalValue, currentValue))
                    {
                        changes.Add(new FieldChange(
                            property.Name,
                            originalValue?.ToString(),
                            currentValue?.ToString()));
                    }
                }
                break;

            case EntityState.Deleted:
                foreach (var property in entry.OriginalValues.Properties)
                {
                    var originalValue = entry.OriginalValues[property];
                    if (originalValue is not null)
                    {
                        changes.Add(new FieldChange(
                            property.Name,
                            originalValue.ToString(),
                            null));
                    }
                }
                break;
        }

        return changes;
    }

    private async Task WriteAuditEntriesAsync(
        List<AuditEntry> entries,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var auditDbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        foreach (var entry in entries)
        {
            var changesJson = JsonSerializer.Serialize(entry.Changes, JsonOptions);

            var auditLog = AuditLog.Create(
                entry.UserId,
                entry.UserEmail,
                entry.EntityName,
                entry.EntityId,
                entry.Action,
                changesJson,
                entry.BranchId);

            auditDbContext.AuditLogs.Add(auditLog);
        }

        await auditDbContext.SaveChangesAsync(cancellationToken);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Internal record to hold captured audit data between SavingChanges and SavedChanges.
    /// </summary>
    internal sealed record AuditEntry(
        Guid UserId,
        string UserEmail,
        string EntityName,
        string EntityId,
        AuditAction Action,
        List<FieldChange> Changes,
        Guid BranchId);

    /// <summary>
    /// Represents a single field-level change with old and new values.
    /// </summary>
    internal sealed record FieldChange(
        string PropertyName,
        string? OldValue,
        string? NewValue);
}

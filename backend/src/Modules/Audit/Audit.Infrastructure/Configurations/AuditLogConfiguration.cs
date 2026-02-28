using Audit.Domain.Entities;
using Audit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for AuditLog entity.
/// Audit logs are append-only -- UPDATE and DELETE permissions should be revoked
/// at the SQL Server level for the application user:
///
/// -- SQL Server migration script for production:
/// -- DENY UPDATE, DELETE ON [audit].[AuditLogs] TO [app_user];
/// -- GRANT SELECT, INSERT ON [audit].[AuditLogs] TO [app_user];
/// </summary>
public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.UserEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.Changes)
            .IsRequired();

        builder.Property(a => a.BranchId)
            .IsRequired();

        // Composite index for efficient query: filter by time range, user, and entity type
        builder.HasIndex(a => new { a.Timestamp, a.UserId, a.EntityName })
            .IsDescending(true, false, false)
            .HasDatabaseName("IX_AuditLogs_Timestamp_UserId_EntityName");

        // Index for per-record audit history lookups
        builder.HasIndex(a => new { a.EntityId, a.EntityName })
            .HasDatabaseName("IX_AuditLogs_EntityId_EntityName");

        // Index for cursor-based pagination (Timestamp DESC, Id)
        builder.HasIndex(a => new { a.Timestamp, a.Id })
            .IsDescending(true, true)
            .HasDatabaseName("IX_AuditLogs_Timestamp_Id_Cursor");
    }
}

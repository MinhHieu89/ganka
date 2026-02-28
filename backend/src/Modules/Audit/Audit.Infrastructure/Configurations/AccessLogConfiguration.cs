using Audit.Domain.Entities;
using Audit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for AccessLog entity.
/// Access logs are append-only -- UPDATE and DELETE permissions should be revoked
/// at the SQL Server level for the application user:
///
/// -- SQL Server migration script for production:
/// -- DENY UPDATE, DELETE ON [audit].[AccessLogs] TO [app_user];
/// -- GRANT SELECT, INSERT ON [audit].[AccessLogs] TO [app_user];
/// </summary>
public sealed class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
{
    public void Configure(EntityTypeBuilder<AccessLog> builder)
    {
        builder.ToTable("AccessLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.UserEmail)
            .HasMaxLength(256);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.Resource)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(a => a.IpAddress)
            .IsRequired()
            .HasMaxLength(45); // IPv6 max length

        builder.Property(a => a.UserAgent)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(a => a.StatusCode)
            .IsRequired();

        // Index for querying by time range and user
        builder.HasIndex(a => new { a.Timestamp, a.UserId })
            .IsDescending(true, false)
            .HasDatabaseName("IX_AccessLogs_Timestamp_UserId");

        // Index for filtering login attempts
        builder.HasIndex(a => a.Action)
            .HasDatabaseName("IX_AccessLogs_Action");

        // Index for cursor-based pagination
        builder.HasIndex(a => new { a.Timestamp, a.Id })
            .IsDescending(true, true)
            .HasDatabaseName("IX_AccessLogs_Timestamp_Id_Cursor");
    }
}

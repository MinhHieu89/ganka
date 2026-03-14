using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treatment.Domain.Entities;

namespace Treatment.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for TreatmentSession.
/// Child entity of TreatmentPackage aggregate. Maps to "TreatmentSessions" table.
/// Configures backing field navigation for _consumables collection.
/// </summary>
public class TreatmentSessionConfiguration : IEntityTypeConfiguration<TreatmentSession>
{
    public void Configure(EntityTypeBuilder<TreatmentSession> builder)
    {
        builder.ToTable("TreatmentSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TreatmentPackageId)
            .IsRequired();

        builder.Property(x => x.SessionNumber)
            .IsRequired();

        // SessionStatus enum stored as int
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ParametersJson)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.OsdiScore)
            .IsRequired(false)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.OsdiSeverity)
            .HasMaxLength(20);

        builder.Property(x => x.ClinicalNotes)
            .HasMaxLength(4000);

        builder.Property(x => x.PerformedById)
            .IsRequired();

        builder.Property(x => x.VisitId)
            .IsRequired(false);

        builder.Property(x => x.ScheduledAt)
            .IsRequired(false);

        builder.Property(x => x.CompletedAt)
            .IsRequired(false);

        builder.Property(x => x.IntervalOverrideReason)
            .HasMaxLength(500);

        // Backing field navigation for the private _consumables collection
        builder.Navigation(x => x.Consumables)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // One-to-many: TreatmentSession -> SessionConsumables (cascade delete)
        builder.HasMany(x => x.Consumables)
            .WithOne()
            .HasForeignKey(x => x.TreatmentSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance indexes
        builder.HasIndex(x => x.TreatmentPackageId);
        builder.HasIndex(x => x.CompletedAt);
    }
}

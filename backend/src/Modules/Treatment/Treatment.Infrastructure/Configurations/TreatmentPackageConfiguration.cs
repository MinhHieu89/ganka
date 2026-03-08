using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treatment.Domain.Entities;

namespace Treatment.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for TreatmentPackage aggregate root.
/// Maps to "TreatmentPackages" table in the treatment schema.
/// Configures backing field navigations for _sessions and _versions collections,
/// one-to-one relationship for CancellationRequest, and ignores computed properties.
/// </summary>
public class TreatmentPackageConfiguration : IEntityTypeConfiguration<TreatmentPackage>
{
    public void Configure(EntityTypeBuilder<TreatmentPackage> builder)
    {
        builder.ToTable("TreatmentPackages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProtocolTemplateId)
            .IsRequired();

        builder.Property(x => x.PatientId)
            .IsRequired();

        builder.Property(x => x.PatientName)
            .IsRequired()
            .HasMaxLength(200);

        // Enum conversions stored as int
        builder.Property(x => x.TreatmentType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.PricingMode)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.TotalSessions)
            .IsRequired();

        builder.Property(x => x.PackagePrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.SessionPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.MinIntervalDays)
            .IsRequired();

        builder.Property(x => x.ParametersJson)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.VisitId)
            .IsRequired(false);

        builder.Property(x => x.CreatedById)
            .IsRequired();

        builder.Property(x => x.UpdatedById)
            .IsRequired(false);

        // BranchId value object stored as Guid column
        builder.Property(x => x.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Ignore computed properties -- not stored as DB columns
        builder.Ignore(x => x.SessionsCompleted);
        builder.Ignore(x => x.SessionsRemaining);
        builder.Ignore(x => x.IsComplete);

        // Backing field navigation for the private _sessions collection
        builder.Navigation(x => x.Sessions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Backing field navigation for the private _versions collection
        builder.Navigation(x => x.Versions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // One-to-many: TreatmentPackage -> TreatmentSessions (cascade delete)
        builder.HasMany(x => x.Sessions)
            .WithOne()
            .HasForeignKey(x => x.TreatmentPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: TreatmentPackage -> ProtocolVersions (cascade delete)
        builder.HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey(x => x.TreatmentPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-one: TreatmentPackage -> CancellationRequest (optional, cascade delete)
        // HasOne must come before Navigation configuration so EF Core discovers the relationship first
        builder.HasOne(x => x.CancellationRequest)
            .WithOne()
            .HasForeignKey<CancellationRequest>(x => x.TreatmentPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Backing field navigation for the private _cancellationRequest field
        builder.Navigation(x => x.CancellationRequest)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Performance indexes
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.TreatmentType);
        builder.HasIndex(x => x.ProtocolTemplateId);
    }
}

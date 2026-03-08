using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treatment.Domain.Entities;

namespace Treatment.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for ProtocolVersion.
/// Child entity of TreatmentPackage. Maps to "ProtocolVersions" table.
/// Stores JSON snapshots of package state before and after mid-course modifications (TRT-07).
/// </summary>
public class ProtocolVersionConfiguration : IEntityTypeConfiguration<ProtocolVersion>
{
    public void Configure(EntityTypeBuilder<ProtocolVersion> builder)
    {
        builder.ToTable("ProtocolVersions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TreatmentPackageId)
            .IsRequired();

        builder.Property(x => x.VersionNumber)
            .IsRequired();

        builder.Property(x => x.PreviousJson)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.CurrentJson)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.ChangeDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ChangedById)
            .IsRequired();

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        // Performance index
        builder.HasIndex(x => x.TreatmentPackageId);
    }
}

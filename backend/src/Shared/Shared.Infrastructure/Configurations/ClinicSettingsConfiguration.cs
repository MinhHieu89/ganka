using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Entities;

namespace Shared.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for the ClinicSettings entity.
/// Uses the "reference" schema consistent with ReferenceDbContext.
/// </summary>
public class ClinicSettingsConfiguration : IEntityTypeConfiguration<ClinicSettings>
{
    public void Configure(EntityTypeBuilder<ClinicSettings> builder)
    {
        builder.ToTable("ClinicSettings", "reference");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.ClinicName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cs => cs.ClinicNameVi)
            .HasMaxLength(200);

        builder.Property(cs => cs.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(cs => cs.Phone)
            .HasMaxLength(100);

        builder.Property(cs => cs.Fax)
            .HasMaxLength(100);

        builder.Property(cs => cs.LicenseNumber)
            .HasMaxLength(100);

        builder.Property(cs => cs.Tagline)
            .HasMaxLength(300);

        builder.Property(cs => cs.LogoBlobUrl)
            .HasMaxLength(500);

        builder.Property(cs => cs.Email)
            .HasMaxLength(100);

        builder.Property(cs => cs.Website)
            .HasMaxLength(100);

        // BranchId stored as Guid column with conversion
        builder.Property(cs => cs.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v))
            .IsRequired();

        // Index on BranchId for multi-branch queries
        builder.HasIndex(cs => cs.BranchId)
            .HasDatabaseName("IX_ClinicSettings_BranchId");
    }
}

using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems", "clinical");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.DrugPrescriptionId).IsRequired();

        // Optional FK to drug catalog (null = off-catalog)
        builder.Property(p => p.DrugCatalogItemId);

        builder.Property(p => p.DrugName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.GenericName)
            .HasMaxLength(200);

        builder.Property(p => p.Strength)
            .HasMaxLength(50);

        builder.Property(p => p.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Dosage)
            .HasMaxLength(500);

        builder.Property(p => p.DosageOverride)
            .HasMaxLength(500);

        builder.Property(p => p.Frequency)
            .HasMaxLength(100);

        // Form and Route stored as int columns (no enum conversion -- cross-module boundary)
        builder.Property(p => p.Form);
        builder.Property(p => p.Route);

        builder.Property(p => p.SortOrder)
            .HasDefaultValue(0);

        // Performance index on DrugPrescriptionId
        builder.HasIndex(p => p.DrugPrescriptionId);
    }
}

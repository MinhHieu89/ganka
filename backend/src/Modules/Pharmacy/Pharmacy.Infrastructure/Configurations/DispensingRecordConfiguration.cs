using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for DispensingRecord and its children.
/// DispensingRecord is the aggregate root for a complete dispensing event.
/// DispensingLine and BatchDeduction are configured as separate entity types
/// with backing field access for private collection navigation.
/// </summary>
public class DispensingRecordConfiguration : IEntityTypeConfiguration<DispensingRecord>
{
    public void Configure(EntityTypeBuilder<DispensingRecord> builder)
    {
        builder.ToTable("DispensingRecords");

        builder.HasKey(r => r.Id);

        // Cross-module FK to Clinical DrugPrescription.Id (no navigation property)
        builder.Property(r => r.PrescriptionId)
            .IsRequired();

        // Cross-module FK to Clinical Visit.Id
        builder.Property(r => r.VisitId)
            .IsRequired();

        // Cross-module FK to Patient.Id
        builder.Property(r => r.PatientId)
            .IsRequired();

        // Denormalized patient name for audit without cross-module joins
        builder.Property(r => r.PatientName)
            .IsRequired()
            .HasMaxLength(200);

        // Cross-module FK to Auth User.Id (the dispensing pharmacist)
        builder.Property(r => r.DispensedById)
            .IsRequired();

        builder.Property(r => r.DispensedAt)
            .IsRequired();

        // Override reason for expired prescription dispensing (PHR-07); null for in-window prescriptions
        builder.Property(r => r.OverrideReason)
            .HasMaxLength(500);

        builder.Property(r => r.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Backing field access for the Lines private collection
        builder.Navigation(r => r.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // One-to-many: DispensingRecord -> DispensingLines (cascade delete)
        builder.HasMany(r => r.Lines)
            .WithOne()
            .HasForeignKey(l => l.DispensingRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core entity configuration for DispensingLine.
/// DispensingLine is a child of DispensingRecord. One line per prescription item.
/// Always created via DispensingRecord.AddLine() — never directly instantiated.
/// </summary>
public class DispensingLineConfiguration : IEntityTypeConfiguration<DispensingLine>
{
    public void Configure(EntityTypeBuilder<DispensingLine> builder)
    {
        builder.ToTable("DispensingLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.DispensingRecordId)
            .IsRequired();

        // Cross-module FK to Clinical PrescriptionItem.Id
        builder.Property(l => l.PrescriptionItemId)
            .IsRequired();

        // Cross-module FK to Pharmacy DrugCatalogItem.Id
        builder.Property(l => l.DrugCatalogItemId)
            .IsRequired();

        // Denormalized drug name for audit without joins
        builder.Property(l => l.DrugName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Quantity)
            .IsRequired();

        // DispensingStatus enum stored as int
        builder.Property(l => l.Status)
            .IsRequired()
            .HasConversion<int>();

        // Backing field access for the BatchDeductions private collection
        builder.Navigation(l => l.BatchDeductions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // One-to-many: DispensingLine -> BatchDeductions for FEFO multi-batch allocation
        builder.HasMany(l => l.BatchDeductions)
            .WithOne()
            .HasForeignKey(bd => bd.DispensingLineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core entity configuration for BatchDeduction.
/// BatchDeduction is a shared child entity linked to either a DispensingLine
/// or an OtcSaleLine (exactly one FK is set; the other is null).
/// Supports multi-batch FEFO deductions: one dispensing/OTC line -> many BatchDeductions.
/// </summary>
public class BatchDeductionConfiguration : IEntityTypeConfiguration<BatchDeduction>
{
    public void Configure(EntityTypeBuilder<BatchDeduction> builder)
    {
        builder.ToTable("BatchDeductions");

        builder.HasKey(bd => bd.Id);

        // Nullable FK to DispensingLine — set for dispensing, null for OTC sales
        builder.Property(bd => bd.DispensingLineId);

        // Nullable FK to OtcSaleLine — set for OTC sales, null for dispensing
        builder.Property(bd => bd.OtcSaleLineId);

        // FK to the DrugBatch from which stock was deducted
        builder.Property(bd => bd.DrugBatchId)
            .IsRequired();

        // Batch number denormalized for audit without join to DrugBatches
        builder.Property(bd => bd.BatchNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(bd => bd.Quantity)
            .IsRequired();
    }
}

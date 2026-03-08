using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treatment.Domain.Entities;

namespace Treatment.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for CancellationRequest.
/// One-to-one child of TreatmentPackage. Maps to "CancellationRequests" table.
/// Tracks the cancellation approval workflow: Requested -> Approved/Rejected (TRT-09).
/// </summary>
public class CancellationRequestConfiguration : IEntityTypeConfiguration<CancellationRequest>
{
    public void Configure(EntityTypeBuilder<CancellationRequest> builder)
    {
        builder.ToTable("CancellationRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TreatmentPackageId)
            .IsRequired();

        // CancellationRequestStatus enum stored as int
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.DeductionPercent)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.RefundAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.RequestedById)
            .IsRequired();

        builder.Property(x => x.RequestedAt)
            .IsRequired();

        builder.Property(x => x.ProcessedById)
            .IsRequired(false);

        builder.Property(x => x.ProcessedAt)
            .IsRequired(false);

        builder.Property(x => x.ProcessingNote)
            .HasMaxLength(1000);

        // Performance index
        builder.HasIndex(x => x.TreatmentPackageId)
            .IsUnique();
    }
}

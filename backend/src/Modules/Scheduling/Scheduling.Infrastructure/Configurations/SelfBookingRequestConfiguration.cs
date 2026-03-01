using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Configurations;

public class SelfBookingRequestConfiguration : IEntityTypeConfiguration<SelfBookingRequest>
{
    public void Configure(EntityTypeBuilder<SelfBookingRequest> builder)
    {
        builder.ToTable("SelfBookingRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.PatientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.Email)
            .HasMaxLength(200);

        builder.Property(r => r.PreferredTimeSlot)
            .HasMaxLength(50);

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        builder.Property(r => r.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(500);

        builder.Property(r => r.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Unique index on ReferenceNumber for status check lookups
        builder.HasIndex(r => r.ReferenceNumber)
            .IsUnique();

        // Composite index on Phone + Status for rate limit queries
        builder.HasIndex(r => new { r.Phone, r.Status });
    }
}

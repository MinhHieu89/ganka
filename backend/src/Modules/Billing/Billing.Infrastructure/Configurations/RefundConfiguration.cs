using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for Refund entity.
/// Amount uses precision(18, 0) for VND.
/// </summary>
public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("Refunds");

        builder.Property(r => r.Amount).HasPrecision(18, 0);

        builder.Property(r => r.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.RejectionReason).HasMaxLength(500);
        builder.Property(r => r.Notes).HasMaxLength(500);

        builder.HasIndex(r => r.InvoiceId);
    }
}

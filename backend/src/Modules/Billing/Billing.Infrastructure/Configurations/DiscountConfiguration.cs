using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for Discount entity.
/// Value uses precision(18, 2) for percentage discounts (e.g., 10.5%).
/// CalculatedAmount uses precision(18, 0) for VND.
/// </summary>
public sealed class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("Discounts");

        builder.Property(d => d.Value).HasPrecision(18, 2);
        builder.Property(d => d.CalculatedAmount).HasPrecision(18, 0);

        builder.Property(d => d.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(d => d.RejectionReason).HasMaxLength(500);

        builder.HasIndex(d => d.InvoiceId);
    }
}

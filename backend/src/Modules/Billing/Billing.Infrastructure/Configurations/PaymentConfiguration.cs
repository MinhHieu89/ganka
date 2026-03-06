using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for Payment entity.
/// All monetary fields use precision(18, 0) for VND.
/// </summary>
public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.Property(p => p.Amount).HasPrecision(18, 0);

        builder.Property(p => p.ReferenceNumber).HasMaxLength(100);
        builder.Property(p => p.CardLast4).HasMaxLength(4);
        builder.Property(p => p.CardType).HasMaxLength(20);
        builder.Property(p => p.Notes).HasMaxLength(500);

        builder.HasIndex(p => p.InvoiceId);
        builder.HasIndex(p => p.CashierShiftId);
        builder.HasIndex(p => p.TreatmentPackageId)
            .HasFilter("[TreatmentPackageId] IS NOT NULL");
    }
}

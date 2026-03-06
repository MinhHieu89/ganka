using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for Invoice aggregate root.
/// Uses PropertyAccessMode.Field for navigation properties to support DDD backing fields.
/// All monetary fields use precision(18, 0) for VND (no decimal places).
/// </summary>
public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.Property(i => i.InvoiceNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(i => i.InvoiceNumber).IsUnique();

        builder.Property(i => i.PatientName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.SubTotal).HasPrecision(18, 0);
        builder.Property(i => i.DiscountTotal).HasPrecision(18, 0);
        builder.Property(i => i.TotalAmount).HasPrecision(18, 0);
        builder.Property(i => i.PaidAmount).HasPrecision(18, 0);

        builder.Property(i => i.RowVersion).IsRowVersion();

        builder.HasMany(i => i.LineItems)
            .WithOne()
            .HasForeignKey(li => li.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne()
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Discounts)
            .WithOne()
            .HasForeignKey(d => d.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Refunds)
            .WithOne()
            .HasForeignKey(r => r.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.LineItems).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(i => i.Payments).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(i => i.Discounts).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(i => i.Refunds).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(i => !i.IsDeleted);

        builder.HasIndex(i => i.VisitId);
        builder.HasIndex(i => i.PatientId);
        builder.HasIndex(i => i.CashierShiftId);
    }
}

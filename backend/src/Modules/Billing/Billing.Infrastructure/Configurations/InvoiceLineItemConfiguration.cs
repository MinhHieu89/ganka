using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for InvoiceLineItem child entity.
/// All monetary fields use precision(18, 0) for VND.
/// </summary>
public sealed class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("InvoiceLineItems");

        builder.Property(li => li.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(li => li.DescriptionVi)
            .HasMaxLength(500);

        builder.Property(li => li.UnitPrice).HasPrecision(18, 0);
        builder.Property(li => li.LineTotal).HasPrecision(18, 0);

        builder.Property(li => li.SourceType).HasMaxLength(50);
    }
}

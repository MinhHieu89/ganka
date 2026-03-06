using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for SupplierDrugPrice.
/// Configures the "SupplierDrugPrices" table in the pharmacy schema.
/// The unique index on (SupplierId, DrugCatalogItemId) enforces one price entry
/// per supplier-drug combination.
/// </summary>
public class SupplierDrugPriceConfiguration : IEntityTypeConfiguration<SupplierDrugPrice>
{
    public void Configure(EntityTypeBuilder<SupplierDrugPrice> builder)
    {
        builder.ToTable("SupplierDrugPrices");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SupplierId)
            .IsRequired();

        builder.Property(s => s.DrugCatalogItemId)
            .IsRequired();

        builder.Property(s => s.DefaultPurchasePrice)
            .IsRequired()
            .HasPrecision(18, 2);

        // Enforce uniqueness: one default price per supplier-drug pair
        builder.HasIndex(s => new { s.SupplierId, s.DrugCatalogItemId })
            .IsUnique();
    }
}

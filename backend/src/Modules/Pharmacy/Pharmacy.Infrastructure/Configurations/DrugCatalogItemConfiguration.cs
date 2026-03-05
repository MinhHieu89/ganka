using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;

namespace Pharmacy.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for DrugCatalogItem.
/// Configures the "DrugCatalogItems" table in the pharmacy schema with
/// Vietnamese_CI_AI collation on name fields for accent-insensitive search.
/// </summary>
public class DrugCatalogItemConfiguration : IEntityTypeConfiguration<DrugCatalogItem>
{
    public void Configure(EntityTypeBuilder<DrugCatalogItem> builder)
    {
        builder.ToTable("DrugCatalogItems");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200)
            .UseCollation("Vietnamese_CI_AI");

        builder.Property(d => d.NameVi)
            .IsRequired()
            .HasMaxLength(200)
            .UseCollation("Vietnamese_CI_AI");

        builder.Property(d => d.GenericName)
            .IsRequired()
            .HasMaxLength(200)
            .UseCollation("Vietnamese_CI_AI");

        builder.Property(d => d.Strength)
            .HasMaxLength(50);

        builder.Property(d => d.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.DefaultDosageTemplate)
            .HasMaxLength(500);

        builder.Property(d => d.Form)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.Route)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Search performance indexes
        builder.HasIndex(d => d.Name);
        builder.HasIndex(d => d.GenericName);
    }
}

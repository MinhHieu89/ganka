using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for ServiceCatalogItem entity.
/// Uses billing schema, unique Code index, VND precision for Price.
/// </summary>
public sealed class ServiceCatalogItemConfiguration : IEntityTypeConfiguration<ServiceCatalogItem>
{
    public void Configure(EntityTypeBuilder<ServiceCatalogItem> builder)
    {
        builder.ToTable("ServiceCatalogItems");

        builder.Property(s => s.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        builder.Property(s => s.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(s => s.Code).IsUnique();

        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.NameVi)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Price)
            .HasPrecision(18, 0);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Patient.Domain.Entities;

namespace Patient.Infrastructure.Configurations;

public class AllergyCatalogItemConfiguration : IEntityTypeConfiguration<AllergyCatalogItem>
{
    public void Configure(EntityTypeBuilder<AllergyCatalogItem> builder)
    {
        builder.ToTable("AllergyCatalogItems");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.NameVi)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Category)
            .HasMaxLength(100);

        builder.HasIndex(a => a.Name)
            .IsUnique();
    }
}

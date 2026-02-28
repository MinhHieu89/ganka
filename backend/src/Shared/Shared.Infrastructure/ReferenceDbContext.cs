using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Shared.Infrastructure;

/// <summary>
/// EF Core DbContext for cross-module reference data (e.g., ICD-10 codes).
/// Uses the "reference" schema for data that is shared across all modules.
/// </summary>
public class ReferenceDbContext : DbContext
{
    public ReferenceDbContext(DbContextOptions<ReferenceDbContext> options) : base(options)
    {
    }

    public DbSet<Icd10Code> Icd10Codes => Set<Icd10Code>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reference");

        modelBuilder.Entity<Icd10Code>(builder =>
        {
            builder.ToTable("Icd10Codes");

            builder.HasKey(c => c.Code);

            builder.Property(c => c.Code)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(c => c.DescriptionEn)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(c => c.DescriptionVi)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(c => c.Category)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.RequiresLaterality)
                .IsRequired();

            builder.Property(c => c.IsFavorite)
                .IsRequired()
                .HasDefaultValue(false);

            // Index for searching by category
            builder.HasIndex(c => c.Category)
                .HasDatabaseName("IX_Icd10Codes_Category");

            // Index for full-text-like search on description
            builder.HasIndex(c => c.DescriptionEn)
                .HasDatabaseName("IX_Icd10Codes_DescriptionEn");
        });

        base.OnModelCreating(modelBuilder);
    }
}

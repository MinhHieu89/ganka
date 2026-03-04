using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class VisitDiagnosisConfiguration : IEntityTypeConfiguration<VisitDiagnosis>
{
    public void Configure(EntityTypeBuilder<VisitDiagnosis> builder)
    {
        builder.ToTable("VisitDiagnoses", "clinical");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.VisitId).IsRequired();

        builder.Property(d => d.Icd10Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.DescriptionEn)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.DescriptionVi)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.Laterality)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.SortOrder).IsRequired();

        // Prevent duplicate diagnosis + laterality per visit
        builder.HasIndex(d => new { d.VisitId, d.Icd10Code, d.Laterality })
            .IsUnique();
    }
}

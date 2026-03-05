using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class DryEyeAssessmentConfiguration : IEntityTypeConfiguration<DryEyeAssessment>
{
    public void Configure(EntityTypeBuilder<DryEyeAssessment> builder)
    {
        builder.ToTable("DryEyeAssessments", "clinical");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.VisitId).IsRequired();

        // Tear Break-Up Time per eye (seconds) - precision(5,2)
        builder.Property(d => d.OdTbut).HasPrecision(5, 2);
        builder.Property(d => d.OsTbut).HasPrecision(5, 2);

        // Schirmer test per eye (mm) - precision(5,2)
        builder.Property(d => d.OdSchirmer).HasPrecision(5, 2);
        builder.Property(d => d.OsSchirmer).HasPrecision(5, 2);

        // Meibomian gland grading per eye (int, no precision needed)
        // OdMeibomianGrading and OsMeibomianGrading are int? by default

        // Tear meniscus height per eye (mm) - precision(5,2)
        builder.Property(d => d.OdTearMeniscus).HasPrecision(5, 2);
        builder.Property(d => d.OsTearMeniscus).HasPrecision(5, 2);

        // Staining score per eye (int, no precision needed)
        // OdStaining and OsStaining are int? by default

        // OSDI score - precision(7,2) for range 0.00-100.00
        builder.Property(d => d.OsdiScore).HasPrecision(7, 2);

        // OSDI severity stored as int
        builder.Property(d => d.OsdiSeverity)
            .HasConversion<int?>();

        // Performance index on VisitId
        builder.HasIndex(d => d.VisitId);
    }
}

using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits", "clinical");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.PatientId).IsRequired();

        builder.Property(v => v.PatientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.DoctorId).IsRequired();

        builder.Property(v => v.DoctorName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.CurrentStage)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.VisitDate).IsRequired();

        builder.Property(v => v.ExaminationNotes)
            .HasColumnType("nvarchar(max)");

        builder.Property(v => v.HasAllergies)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(v => v.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Optimistic concurrency via RowVersion
        builder.Property(v => v.RowVersion)
            .IsRowVersion();

        // Navigation collections
        builder.HasMany(v => v.Refractions)
            .WithOne()
            .HasForeignKey(r => r.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Diagnoses)
            .WithOne()
            .HasForeignKey(d => d.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Amendments)
            .WithOne()
            .HasForeignKey(a => a.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance indexes
        builder.HasIndex(v => v.PatientId);
        builder.HasIndex(v => v.DoctorId);
        builder.HasIndex(v => new { v.CurrentStage, v.Status });
    }
}

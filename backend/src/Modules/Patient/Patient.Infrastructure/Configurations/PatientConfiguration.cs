using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Patient.Infrastructure.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Domain.Entities.Patient>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(200)
            .UseCollation("Vietnamese_CI_AI");

        builder.Property(p => p.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(p => p.Phone)
            .IsUnique();

        builder.Property(p => p.PatientCode)
            .HasMaxLength(15);

        builder.HasIndex(p => p.PatientCode)
            .IsUnique()
            .HasFilter("[PatientCode] IS NOT NULL");

        builder.Property(p => p.PatientType)
            .IsRequired();

        builder.Property(p => p.Address)
            .HasMaxLength(500);

        builder.Property(p => p.Cccd)
            .HasMaxLength(20);

        builder.Property(p => p.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        // Year-scoped sequence for patient code generation
        builder.HasIndex(p => new { p.Year, p.SequenceNumber })
            .IsUnique()
            .HasFilter("[SequenceNumber] > 0");

        builder.Property(p => p.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Allergy collection navigation
        builder.HasMany(p => p.Allergies)
            .WithOne()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Intake form fields
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.Occupation).HasMaxLength(200);
        builder.Property(p => p.OcularHistory).HasMaxLength(2000);
        builder.Property(p => p.SystemicHistory).HasMaxLength(2000);
        builder.Property(p => p.CurrentMedications).HasMaxLength(2000);
        builder.Property(p => p.ScreenTimeHours).HasPrecision(4, 1);
        builder.Property(p => p.WorkEnvironment).HasMaxLength(100);
        builder.Property(p => p.ContactLensUsage).HasMaxLength(100);
        builder.Property(p => p.LifestyleNotes).HasMaxLength(2000);

        // Concurrency token
        builder.Property(p => p.RowVersion)
            .IsRowVersion();
    }
}

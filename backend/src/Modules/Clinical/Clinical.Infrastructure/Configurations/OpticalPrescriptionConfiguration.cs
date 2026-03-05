using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class OpticalPrescriptionConfiguration : IEntityTypeConfiguration<OpticalPrescription>
{
    public void Configure(EntityTypeBuilder<OpticalPrescription> builder)
    {
        builder.ToTable("OpticalPrescriptions", "clinical");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.VisitId).IsRequired();

        // OD (right eye) distance Rx -- precision(5,2) following RefractionConfiguration pattern
        builder.Property(o => o.OdSph).HasPrecision(5, 2);
        builder.Property(o => o.OdCyl).HasPrecision(5, 2);
        builder.Property(o => o.OdAdd).HasPrecision(5, 2);

        // OS (left eye) distance Rx
        builder.Property(o => o.OsSph).HasPrecision(5, 2);
        builder.Property(o => o.OsCyl).HasPrecision(5, 2);
        builder.Property(o => o.OsAdd).HasPrecision(5, 2);

        // Interpupillary distance
        builder.Property(o => o.FarPd).HasPrecision(5, 2);
        builder.Property(o => o.NearPd).HasPrecision(5, 2);

        // Near Rx override fields
        builder.Property(o => o.NearOdSph).HasPrecision(5, 2);
        builder.Property(o => o.NearOdCyl).HasPrecision(5, 2);
        builder.Property(o => o.NearOsSph).HasPrecision(5, 2);
        builder.Property(o => o.NearOsCyl).HasPrecision(5, 2);

        // LensType stored as int
        builder.Property(o => o.LensType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.Notes)
            .HasMaxLength(500);

        builder.Property(o => o.PrescribedAt).IsRequired();

        // Performance index on VisitId
        builder.HasIndex(o => o.VisitId);
    }
}

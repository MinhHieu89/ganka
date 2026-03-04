using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class RefractionConfiguration : IEntityTypeConfiguration<Refraction>
{
    public void Configure(EntityTypeBuilder<Refraction> builder)
    {
        builder.ToTable("Refractions", "clinical");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.VisitId).IsRequired();

        builder.Property(r => r.Type)
            .IsRequired()
            .HasConversion<int>();

        // OD (right eye) refraction values
        builder.Property(r => r.OdSph).HasPrecision(5, 2);
        builder.Property(r => r.OdCyl).HasPrecision(5, 2);
        builder.Property(r => r.OdAxis).HasPrecision(5, 2);
        builder.Property(r => r.OdAdd).HasPrecision(5, 2);
        builder.Property(r => r.OdPd).HasPrecision(5, 2);

        // OS (left eye) refraction values
        builder.Property(r => r.OsSph).HasPrecision(5, 2);
        builder.Property(r => r.OsCyl).HasPrecision(5, 2);
        builder.Property(r => r.OsAxis).HasPrecision(5, 2);
        builder.Property(r => r.OsAdd).HasPrecision(5, 2);
        builder.Property(r => r.OsPd).HasPrecision(5, 2);

        // Visual acuity
        builder.Property(r => r.UcvaOd).HasPrecision(5, 2);
        builder.Property(r => r.UcvaOs).HasPrecision(5, 2);
        builder.Property(r => r.BcvaOd).HasPrecision(5, 2);
        builder.Property(r => r.BcvaOs).HasPrecision(5, 2);

        // IOP
        builder.Property(r => r.IopOd).HasPrecision(5, 2);
        builder.Property(r => r.IopOs).HasPrecision(5, 2);
        builder.Property(r => r.IopMethod)
            .HasConversion<int?>();

        // Axial length
        builder.Property(r => r.AxialLengthOd).HasPrecision(5, 2);
        builder.Property(r => r.AxialLengthOs).HasPrecision(5, 2);

        // One refraction per type per visit
        builder.HasIndex(r => new { r.VisitId, r.Type })
            .IsUnique();
    }
}

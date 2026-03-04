using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class DoctorIcd10FavoriteConfiguration : IEntityTypeConfiguration<DoctorIcd10Favorite>
{
    public void Configure(EntityTypeBuilder<DoctorIcd10Favorite> builder)
    {
        builder.ToTable("DoctorIcd10Favorites", "clinical");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.DoctorId).IsRequired();

        builder.Property(f => f.Icd10Code)
            .IsRequired()
            .HasMaxLength(20);

        // One favorite per doctor per ICD-10 code
        builder.HasIndex(f => new { f.DoctorId, f.Icd10Code })
            .IsUnique();
    }
}

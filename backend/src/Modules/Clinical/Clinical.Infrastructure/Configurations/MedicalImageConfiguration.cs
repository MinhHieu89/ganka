using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class MedicalImageConfiguration : IEntityTypeConfiguration<MedicalImage>
{
    public void Configure(EntityTypeBuilder<MedicalImage> builder)
    {
        builder.ToTable("MedicalImages", "clinical");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.VisitId).IsRequired();

        // FK to user who uploaded -- no navigation property, just the FK column
        builder.Property(m => m.UploadedById).IsRequired();

        // ImageType stored as int, required
        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<int>();

        // EyeTag stored as int, nullable
        builder.Property(m => m.EyeTag)
            .HasConversion<int?>();

        builder.Property(m => m.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.BlobName)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(m => m.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Description)
            .HasMaxLength(500);

        // NO navigation property from Visit to MedicalImage (kept separate from aggregate)
        // MedicalImage has its own repository for independent queries

        // Composite index on (VisitId, Type) for same-type image queries
        builder.HasIndex(m => new { m.VisitId, m.Type });

        // Index on VisitId for general visit-level image queries
        builder.HasIndex(m => m.VisitId);
    }
}

using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class ImagingServiceConfiguration : IEntityTypeConfiguration<ImagingService>
{
    public void Configure(EntityTypeBuilder<ImagingService> builder)
    {
        builder.ToTable("ImagingService", "clinical");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ImagingRequestId).IsRequired();

        builder.Property(s => s.ServiceName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.EyeScope)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(s => s.TechnicianNote)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(s => s.ImagingRequestId);
    }
}

using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class ImagingRequestConfiguration : IEntityTypeConfiguration<ImagingRequest>
{
    public void Configure(EntityTypeBuilder<ImagingRequest> builder)
    {
        builder.ToTable("ImagingRequest", "clinical");

        builder.HasKey(ir => ir.Id);

        builder.Property(ir => ir.VisitId).IsRequired();
        builder.Property(ir => ir.DoctorId).IsRequired();
        builder.Property(ir => ir.RequestedAt).IsRequired();

        builder.Property(ir => ir.DoctorNote)
            .HasColumnType("nvarchar(max)");

        builder.HasMany(ir => ir.Services)
            .WithOne()
            .HasForeignKey(s => s.ImagingRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(ir => ir.Services)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(ir => ir.VisitId);
    }
}

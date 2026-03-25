using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class OpticalOrderConfiguration : IEntityTypeConfiguration<OpticalOrder>
{
    public void Configure(EntityTypeBuilder<OpticalOrder> builder)
    {
        builder.ToTable("OpticalOrder", "clinical");

        builder.HasKey(oo => oo.Id);

        builder.Property(oo => oo.VisitId).IsRequired();

        builder.Property(oo => oo.LensType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oo => oo.FrameCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oo => oo.LensCostPerUnit)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(oo => oo.FrameCost)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(oo => oo.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(oo => oo.ConsultantId).IsRequired();

        builder.Property(oo => oo.ConsultantName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oo => oo.ConfirmedAt).IsRequired();

        builder.HasIndex(oo => oo.VisitId);
    }
}

using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class PharmacyDispensingConfiguration : IEntityTypeConfiguration<PharmacyDispensing>
{
    public void Configure(EntityTypeBuilder<PharmacyDispensing> builder)
    {
        builder.ToTable("PharmacyDispensing", "clinical");

        builder.HasKey(pd => pd.Id);

        builder.Property(pd => pd.VisitId).IsRequired();
        builder.Property(pd => pd.PharmacistId).IsRequired();

        builder.Property(pd => pd.PharmacistName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pd => pd.DispensedAt).IsRequired();

        builder.Property(pd => pd.DispenseNote)
            .HasColumnType("nvarchar(max)");

        builder.HasMany(pd => pd.LineItems)
            .WithOne()
            .HasForeignKey(li => li.PharmacyDispensingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(pd => pd.LineItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(pd => pd.VisitId);
    }
}

using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class DrugPrescriptionConfiguration : IEntityTypeConfiguration<DrugPrescription>
{
    public void Configure(EntityTypeBuilder<DrugPrescription> builder)
    {
        builder.ToTable("DrugPrescriptions", "clinical");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.VisitId).IsRequired();

        builder.Property(d => d.Notes)
            .HasMaxLength(1000);

        builder.Property(d => d.PrescriptionCode)
            .HasMaxLength(20);

        builder.Property(d => d.PrescribedAt).IsRequired();

        // Items collection with cascade delete
        builder.HasMany(d => d.Items)
            .WithOne()
            .HasForeignKey(i => i.DrugPrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Backing field access mode for Items collection
        builder.Navigation(d => d.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Performance index on VisitId
        builder.HasIndex(d => d.VisitId);
    }
}

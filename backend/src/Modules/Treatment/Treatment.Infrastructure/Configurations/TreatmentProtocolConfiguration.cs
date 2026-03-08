using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treatment.Domain.Entities;

namespace Treatment.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for TreatmentProtocol aggregate root.
/// Maps to "TreatmentProtocols" table in the treatment schema.
/// Configures indexes on TreatmentType and IsActive for protocol listing queries.
/// </summary>
public class TreatmentProtocolConfiguration : IEntityTypeConfiguration<TreatmentProtocol>
{
    public void Configure(EntityTypeBuilder<TreatmentProtocol> builder)
    {
        builder.ToTable("TreatmentProtocols");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        // TreatmentType enum stored as int
        builder.Property(x => x.TreatmentType)
            .IsRequired()
            .HasConversion<int>();

        // PricingMode enum stored as int
        builder.Property(x => x.PricingMode)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.DefaultSessionCount)
            .IsRequired();

        builder.Property(x => x.DefaultPackagePrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.DefaultSessionPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.CancellationDeductionPercent)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.MinIntervalDays)
            .IsRequired();

        builder.Property(x => x.MaxIntervalDays)
            .IsRequired();

        builder.Property(x => x.DefaultParametersJson)
            .HasMaxLength(4000);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.IsActive)
            .IsRequired();

        // BranchId value object stored as Guid column
        builder.Property(x => x.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Performance indexes
        builder.HasIndex(x => x.TreatmentType);
        builder.HasIndex(x => x.IsActive);
    }
}

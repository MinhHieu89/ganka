using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treatment.Domain.Entities;

namespace Treatment.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for SessionConsumable.
/// Child entity of TreatmentSession. Maps to "SessionConsumables" table.
/// Links to Pharmacy.ConsumableItem via ConsumableItemId for inventory tracking.
/// </summary>
public class SessionConsumableConfiguration : IEntityTypeConfiguration<SessionConsumable>
{
    public void Configure(EntityTypeBuilder<SessionConsumable> builder)
    {
        builder.ToTable("SessionConsumables");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TreatmentSessionId)
            .IsRequired();

        builder.Property(x => x.ConsumableItemId)
            .IsRequired();

        builder.Property(x => x.ConsumableName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Quantity)
            .IsRequired();

        // Performance index on ConsumableItemId for cross-module queries
        builder.HasIndex(x => x.ConsumableItemId);
    }
}

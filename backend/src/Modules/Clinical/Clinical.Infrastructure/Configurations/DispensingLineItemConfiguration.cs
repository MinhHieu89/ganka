using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class DispensingLineItemConfiguration : IEntityTypeConfiguration<DispensingLineItem>
{
    public void Configure(EntityTypeBuilder<DispensingLineItem> builder)
    {
        builder.ToTable("DispensingLineItem", "clinical");

        builder.HasKey(li => li.Id);

        builder.Property(li => li.PharmacyDispensingId).IsRequired();

        builder.Property(li => li.DrugName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(li => li.Instruction)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(li => li.PharmacyDispensingId);
    }
}

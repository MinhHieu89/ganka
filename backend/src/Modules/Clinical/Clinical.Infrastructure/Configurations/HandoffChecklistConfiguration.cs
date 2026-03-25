using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class HandoffChecklistConfiguration : IEntityTypeConfiguration<HandoffChecklist>
{
    public void Configure(EntityTypeBuilder<HandoffChecklist> builder)
    {
        builder.ToTable("HandoffChecklist", "clinical");

        builder.HasKey(hc => hc.Id);

        builder.Property(hc => hc.VisitId).IsRequired();
        builder.Property(hc => hc.CompletedById).IsRequired();

        builder.Property(hc => hc.CompletedByName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(hc => hc.CompletedAt).IsRequired();

        builder.HasIndex(hc => hc.VisitId);
    }
}

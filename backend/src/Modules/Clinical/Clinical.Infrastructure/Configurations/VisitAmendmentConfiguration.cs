using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class VisitAmendmentConfiguration : IEntityTypeConfiguration<VisitAmendment>
{
    public void Configure(EntityTypeBuilder<VisitAmendment> builder)
    {
        builder.ToTable("VisitAmendments", "clinical");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.VisitId).IsRequired();

        builder.Property(a => a.AmendedById).IsRequired();

        builder.Property(a => a.AmendedByName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Reason)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.FieldChangesJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.AmendedAt).IsRequired();
    }
}

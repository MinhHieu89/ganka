using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class StageSkipConfiguration : IEntityTypeConfiguration<StageSkip>
{
    public void Configure(EntityTypeBuilder<StageSkip> builder)
    {
        builder.ToTable("StageSkip", "clinical");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.VisitId).IsRequired();

        builder.Property(s => s.Stage)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.Reason)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.FreeTextNote)
            .HasMaxLength(200);

        builder.Property(s => s.ActorId).IsRequired();

        builder.Property(s => s.ActorName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.SkippedAt).IsRequired();

        builder.HasIndex(s => s.VisitId);
    }
}

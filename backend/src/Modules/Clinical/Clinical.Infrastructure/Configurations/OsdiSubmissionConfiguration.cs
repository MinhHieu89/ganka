using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class OsdiSubmissionConfiguration : IEntityTypeConfiguration<OsdiSubmission>
{
    public void Configure(EntityTypeBuilder<OsdiSubmission> builder)
    {
        builder.ToTable("OsdiSubmissions", "clinical");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.VisitId).IsRequired(false);

        builder.Property(o => o.SubmittedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.AnswersJson)
            .HasMaxLength(500);

        builder.Property(o => o.Score)
            .HasPrecision(7, 2);

        // Severity stored as int
        builder.Property(o => o.Severity)
            .HasConversion<int>();

        // PublicToken: nullable, unique filtered index (IS NOT NULL)
        builder.Property(o => o.PublicToken)
            .HasMaxLength(100);

        builder.HasIndex(o => o.PublicToken)
            .IsUnique()
            .HasFilter("[PublicToken] IS NOT NULL");

        // Performance index on VisitId
        builder.HasIndex(o => o.VisitId);
    }
}

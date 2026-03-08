using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optical.Domain.Entities;
using System.Text.Json;

namespace Optical.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for WarrantyClaim.
/// Stores warranty claims for delivered glasses orders.
/// DocumentUrls are serialized as JSON to nvarchar(max) using a value converter.
/// </summary>
public class WarrantyClaimConfiguration : IEntityTypeConfiguration<WarrantyClaim>
{
    public void Configure(EntityTypeBuilder<WarrantyClaim> builder)
    {
        builder.ToTable("WarrantyClaims");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.GlassesOrderId)
            .IsRequired();

        builder.Property(x => x.ClaimDate)
            .IsRequired();

        // WarrantyResolution enum stored as int
        builder.Property(x => x.Resolution)
            .IsRequired()
            .HasConversion<int>();

        // WarrantyApprovalStatus enum stored as int
        builder.Property(x => x.ApprovalStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.AssessmentNotes)
            .IsRequired()
            .HasMaxLength(2000);

        // Nullable discount amount for Discount resolution type
        builder.Property(x => x.DiscountAmount)
            .IsRequired(false)
            .HasColumnType("decimal(18,2)");

        // Cross-module reference to Identity user who approved/rejected
        builder.Property(x => x.ApprovedById)
            .IsRequired(false);

        builder.Property(x => x.ApprovedAt)
            .IsRequired(false);

        // DocumentUrls: IReadOnlyList<string> (backed by _documentUrls) stored as JSON
        // Uses backing field access so EF Core uses the private _documentUrls List<string>.
        // ValueComparer uses IReadOnlyList<string> to match the property's declared type.
        var documentUrlsComparer = new ValueComparer<IReadOnlyList<string>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            urls => urls.Aggregate(0, (hash, url) => HashCode.Combine(hash, url.GetHashCode())),
            urls => urls.ToList());

        builder.Property(x => x.DocumentUrls)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                urls => JsonSerializer.Serialize(urls, (JsonSerializerOptions?)null),
                json => (IReadOnlyList<string>)(JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>()),
                documentUrlsComparer)
            .HasField("_documentUrls")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Ignore computed property (not stored in DB)
        builder.Ignore(x => x.RequiresApproval);

        // Performance indexes
        builder.HasIndex(x => x.GlassesOrderId);
        builder.HasIndex(x => x.ApprovalStatus);
    }
}

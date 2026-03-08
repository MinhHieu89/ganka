using Microsoft.EntityFrameworkCore;
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

        // DocumentUrls: List<string> stored as JSON in nvarchar(max) column
        // Uses System.Text.Json for serialization/deserialization
        builder.Property(x => x.DocumentUrls)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                urls => JsonSerializer.Serialize(urls, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>());

        // Ignore computed property (not stored in DB)
        builder.Ignore(x => x.RequiresApproval);

        // Performance indexes
        builder.HasIndex(x => x.GlassesOrderId);
        builder.HasIndex(x => x.ApprovalStatus);
    }
}

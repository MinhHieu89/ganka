using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optical.Domain.Entities;

namespace Optical.Infrastructure.Configurations;

/// <summary>
/// EF Core entity configuration for StocktakingSession and its StocktakingItem children.
/// StocktakingSession tracks a full barcode-based inventory count session.
/// A unique composite index on (StocktakingSessionId, Barcode) in StocktakingItem prevents
/// duplicate barcode scans within a session (RESEARCH.md pitfall 5 — concurrent scan duplication).
/// </summary>
public class StocktakingSessionConfiguration : IEntityTypeConfiguration<StocktakingSession>
{
    public void Configure(EntityTypeBuilder<StocktakingSession> builder)
    {
        builder.ToTable("StocktakingSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        // StocktakingStatus enum stored as int
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.StartedById)
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .IsRequired(false);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        // BranchId value object stored as Guid column
        builder.Property(x => x.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Ignore computed properties — derived from Items collection
        builder.Ignore(x => x.TotalItemsScanned);
        builder.Ignore(x => x.DiscrepancyCount);

        // Configure backing field access for the private _items collection
        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // One-to-many: StocktakingSession -> StocktakingItems (cascade delete)
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.StocktakingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance index on Status for filtering InProgress sessions
        builder.HasIndex(x => x.Status);
    }
}

/// <summary>
/// EF Core entity configuration for StocktakingItem.
/// Each item represents a barcode scan + physical count within a stocktaking session.
/// The unique composite index on (StocktakingSessionId, Barcode) enforces the upsert behavior
/// at the database level — prevents duplicate entries for the same barcode in the same session.
/// </summary>
public class StocktakingItemConfiguration : IEntityTypeConfiguration<StocktakingItem>
{
    public void Configure(EntityTypeBuilder<StocktakingItem> builder)
    {
        builder.ToTable("StocktakingItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StocktakingSessionId)
            .IsRequired();

        // EAN-13 barcode is exactly 13 characters
        builder.Property(x => x.Barcode)
            .IsRequired()
            .HasMaxLength(13);

        // Optional FK to Frame catalog item
        builder.Property(x => x.FrameId)
            .IsRequired(false);

        // Optional human-readable frame name for display (null if barcode unrecognized)
        builder.Property(x => x.FrameName)
            .IsRequired(false)
            .HasMaxLength(300);

        builder.Property(x => x.PhysicalCount)
            .IsRequired();

        builder.Property(x => x.SystemCount)
            .IsRequired();

        // CRITICAL: Unique composite index prevents duplicate barcode scans per session.
        // This enforces the upsert pattern at the DB level (RESEARCH.md pitfall 5).
        builder.HasIndex(x => new { x.StocktakingSessionId, x.Barcode })
            .IsUnique();

        // Ignore computed property — derived from PhysicalCount - SystemCount
        builder.Ignore(x => x.Discrepancy);
    }
}

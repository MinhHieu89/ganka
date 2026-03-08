using Shared.Domain;

namespace Optical.Domain.Entities;

/// <summary>
/// Entity representing a single barcode scan entry during a stocktaking session.
/// Records the physical count observed by staff compared to the system's expected inventory count.
/// Child entity of <see cref="StocktakingSession"/>; managed via the session's <c>RecordItem</c> upsert method.
/// </summary>
public class StocktakingItem : Entity
{
    /// <summary>Foreign key to the parent StocktakingSession.</summary>
    public Guid StocktakingSessionId { get; private set; }

    /// <summary>
    /// EAN-13 barcode scanned during the physical count.
    /// Used as the upsert key — scanning the same barcode twice updates rather than duplicates.
    /// </summary>
    public string Barcode { get; private set; } = string.Empty;

    /// <summary>
    /// Optional FK to the Frame entity resolved by matching the barcode.
    /// Null when the scanned barcode is not found in the frame catalog.
    /// </summary>
    public Guid? FrameId { get; private set; }

    /// <summary>
    /// Human-readable display name of the frame resolved from the barcode.
    /// E.g., "Rayban RB5154 Black 52-18-140". Null when barcode is unrecognized.
    /// </summary>
    public string? FrameName { get; private set; }

    /// <summary>
    /// Number of units physically counted by staff during the stocktaking session.
    /// Can be updated via <see cref="UpdatePhysicalCount"/> to correct mistakes.
    /// </summary>
    public int PhysicalCount { get; private set; }

    /// <summary>
    /// Number of units the system expected at the time of the barcode scan.
    /// Snapshot of current stock at scan time — not updated when physical count is corrected.
    /// </summary>
    public int SystemCount { get; private set; }

    /// <summary>
    /// Discrepancy between the physical count and the system count.
    /// Positive value = overage (more on shelf than expected).
    /// Negative value = shortage (fewer on shelf than expected).
    /// Zero = counts match.
    /// </summary>
    public int Discrepancy => PhysicalCount - SystemCount;

    /// <summary>Private parameterless constructor for EF Core materialization.</summary>
    private StocktakingItem() { }

    /// <summary>
    /// Factory method for creating a new stocktaking item from a barcode scan.
    /// Called internally by <see cref="StocktakingSession.RecordItem"/> — do not use directly.
    /// </summary>
    /// <param name="sessionId">The stocktaking session this item belongs to.</param>
    /// <param name="barcode">EAN-13 barcode scanned during the physical count.</param>
    /// <param name="frameId">Resolved frame FK from the catalog. Null if barcode not found.</param>
    /// <param name="frameName">Human-readable frame name. Null if barcode not found.</param>
    /// <param name="physicalCount">Number of units physically counted by staff.</param>
    /// <param name="systemCount">System's expected count at time of scan.</param>
    public static StocktakingItem Create(
        Guid sessionId,
        string barcode,
        Guid? frameId,
        string? frameName,
        int physicalCount,
        int systemCount)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("StocktakingSessionId is required.", nameof(sessionId));

        if (string.IsNullOrWhiteSpace(barcode))
            throw new ArgumentException("Barcode is required.", nameof(barcode));

        if (physicalCount < 0)
            throw new ArgumentException("Physical count cannot be negative.", nameof(physicalCount));

        if (systemCount < 0)
            throw new ArgumentException("System count cannot be negative.", nameof(systemCount));

        return new StocktakingItem
        {
            StocktakingSessionId = sessionId,
            Barcode = barcode,
            FrameId = frameId,
            FrameName = frameName,
            PhysicalCount = physicalCount,
            SystemCount = systemCount
        };
    }

    /// <summary>
    /// Updates the physical count for this item. Used to correct an incorrect count during stocktaking.
    /// System count (inventory snapshot) is never changed through this method.
    /// </summary>
    /// <param name="newCount">The corrected physical count (must be non-negative).</param>
    /// <exception cref="ArgumentException">Thrown when newCount is negative.</exception>
    public void UpdatePhysicalCount(int newCount)
    {
        if (newCount < 0)
            throw new ArgumentException("Physical count cannot be negative.", nameof(newCount));

        PhysicalCount = newCount;
        SetUpdatedAt();
    }
}

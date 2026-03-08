using Optical.Domain.Enums;
using Shared.Domain;

namespace Optical.Domain.Entities;

/// <summary>
/// AggregateRoot representing a barcode-based stocktaking session in the optical center.
/// Staff scan frame barcodes and enter physical counts; the session compares against system inventory.
/// Uses upsert pattern for item recording to prevent duplicate counts from scanning the same barcode twice
/// (see RESEARCH.md Pitfall 5 — concurrent scan duplication).
/// </summary>
public class StocktakingSession : AggregateRoot, IAuditable
{
    private readonly List<StocktakingItem> _items = new();

    /// <summary>
    /// Session display name (e.g., "Monthly Stocktake - March 2026").
    /// Used to identify this session in reports and history.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Current lifecycle status of the session (InProgress, Completed, Cancelled).</summary>
    public StocktakingStatus Status { get; private set; }

    /// <summary>FK to the staff member who initiated this session.</summary>
    public Guid StartedById { get; private set; }

    /// <summary>UTC timestamp when the session was completed. Null while InProgress or if Cancelled.</summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>Optional general notes about this stocktaking session.</summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// All barcode scan entries recorded in this session.
    /// Exposed as read-only — mutate only via <see cref="RecordItem"/>.
    /// </summary>
    public IReadOnlyList<StocktakingItem> Items => _items.AsReadOnly();

    /// <summary>Private parameterless constructor for EF Core materialization.</summary>
    private StocktakingSession() { }

    /// <summary>
    /// Factory method for starting a new stocktaking session.
    /// Sets Status to <see cref="StocktakingStatus.InProgress"/> on creation.
    /// </summary>
    /// <param name="name">Display name for this session (e.g., "Monthly Stocktake - March 2026").</param>
    /// <param name="startedById">The staff member who initiated the session.</param>
    /// <param name="branchId">The branch performing the stocktake (multi-tenant isolation).</param>
    public static StocktakingSession Create(string name, Guid startedById, BranchId branchId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Session name is required.", nameof(name));

        if (startedById == Guid.Empty)
            throw new ArgumentException("StartedById is required.", nameof(startedById));

        var session = new StocktakingSession
        {
            Name = name,
            StartedById = startedById,
            Status = StocktakingStatus.InProgress
        };

        session.SetBranchId(branchId);
        return session;
    }

    /// <summary>
    /// Records a barcode scan with a physical count using an upsert pattern.
    /// If the barcode has already been scanned in this session, the existing item's
    /// physical count is updated instead of creating a duplicate entry.
    /// This prevents double-counting when the same item is scanned twice.
    /// </summary>
    /// <param name="barcode">EAN-13 barcode scanned from the frame.</param>
    /// <param name="physicalCount">Number of units physically counted by staff.</param>
    /// <param name="systemCount">System's expected inventory count at time of scan.</param>
    /// <param name="frameId">Resolved frame FK from catalog. Null if barcode not recognized.</param>
    /// <param name="frameName">Human-readable frame name. Null if barcode not recognized.</param>
    /// <returns>The created or updated StocktakingItem for this barcode.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the session is not InProgress.</exception>
    public StocktakingItem RecordItem(
        string barcode,
        int physicalCount,
        int systemCount,
        Guid? frameId = null,
        string? frameName = null)
    {
        if (Status != StocktakingStatus.InProgress)
            throw new InvalidOperationException(
                $"Cannot record items in a session with status '{Status}'. Session must be InProgress.");

        if (string.IsNullOrWhiteSpace(barcode))
            throw new ArgumentException("Barcode is required.", nameof(barcode));

        // UPSERT: if the barcode was already scanned, update its count rather than duplicating
        var existing = _items.FirstOrDefault(i => i.Barcode == barcode);
        if (existing is not null)
        {
            existing.UpdatePhysicalCount(physicalCount);
            SetUpdatedAt();
            return existing;
        }

        // New barcode — add a new item entry
        var item = StocktakingItem.Create(Id, barcode, frameId, frameName, physicalCount, systemCount);
        _items.Add(item);
        SetUpdatedAt();
        return item;
    }

    /// <summary>
    /// Completes the stocktaking session, locking it against further scan entries.
    /// Sets Status to <see cref="StocktakingStatus.Completed"/> and records the completion timestamp.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the session is already Completed or Cancelled.</exception>
    public void Complete()
    {
        if (Status == StocktakingStatus.Completed)
            throw new InvalidOperationException("Stocktaking session is already completed.");

        if (Status == StocktakingStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled stocktaking session.");

        Status = StocktakingStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Cancels the stocktaking session, discarding its results.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the session is already Completed.</exception>
    public void Cancel()
    {
        if (Status == StocktakingStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed stocktaking session.");

        Status = StocktakingStatus.Cancelled;
        SetUpdatedAt();
    }

    /// <summary>
    /// Total number of unique barcodes scanned in this session.
    /// </summary>
    public int TotalItemsScanned => _items.Count;

    /// <summary>
    /// Number of items where the physical count does not match the system count.
    /// Used in discrepancy reports to highlight items that need investigation.
    /// </summary>
    public int DiscrepancyCount => _items.Count(i => i.PhysicalCount != i.SystemCount);
}

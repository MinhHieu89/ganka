using Optical.Domain.Enums;
using Shared.Domain;

namespace Optical.Domain.Entities;

/// <summary>
/// Represents a custom lens order placed with a supplier per patient prescription.
/// Used for unusual power combinations not available in bulk stock.
/// Supports orders to Essilor, Hoya, and Viet Phap suppliers.
/// Linked to the glasses order that triggered the custom lens requirement.
/// </summary>
public class LensOrder : Entity, IAuditable
{
    /// <summary>Foreign key to the LensCatalogItem that defines the lens type.</summary>
    public Guid LensCatalogItemId { get; private set; }

    /// <summary>
    /// Foreign key to the supplier who will fulfill this order.
    /// Cross-module reference to Pharmacy.Supplier entity (shared supplier entity).
    /// </summary>
    public Guid SupplierId { get; private set; }

    /// <summary>Foreign key to the glasses order that triggered this custom lens requirement.</summary>
    public Guid GlassesOrderId { get; private set; }

    /// <summary>The patient for whom this lens is being ordered.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>Spherical power from patient prescription (e.g., -2.00, +1.50).</summary>
    public decimal Sph { get; private set; }

    /// <summary>Cylinder power from patient prescription (e.g., -0.75). 0 for sphere-only.</summary>
    public decimal Cyl { get; private set; }

    /// <summary>
    /// Addition power from prescription for bifocal/progressive lenses (e.g., +2.00).
    /// Null for single vision prescriptions.
    /// </summary>
    public decimal? Add { get; private set; }

    /// <summary>
    /// Cylinder axis in degrees (1-180) from patient prescription.
    /// Null when Cyl is 0 (no astigmatism).
    /// </summary>
    public decimal? Axis { get; private set; }

    /// <summary>
    /// Coating options requested for this custom lens order.
    /// [Flags] enum — multiple coatings can be combined.
    /// </summary>
    public LensCoating RequestedCoatings { get; private set; }

    /// <summary>
    /// Order status. Simple string — not a complex state machine.
    /// Valid values: "Ordered", "Received", "Cancelled"
    /// </summary>
    public string Status { get; private set; } = LensOrderStatus.Ordered;

    /// <summary>Timestamp when the lens was received from the supplier. Null until received.</summary>
    public DateTime? ReceivedAt { get; private set; }

    /// <summary>
    /// Optional notes for this order.
    /// Used for cancellation reasons, special instructions, or supplier communications.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>Private parameterless constructor for EF Core materialization.</summary>
    private LensOrder() { }

    /// <summary>
    /// Factory method for creating a new custom lens order for a patient prescription.
    /// </summary>
    /// <param name="lensCatalogItemId">The lens type being ordered.</param>
    /// <param name="supplierId">The supplier to order from (Essilor, Hoya, Viet Phap).</param>
    /// <param name="glassesOrderId">The glasses order that requires this custom lens.</param>
    /// <param name="patientId">The patient for whom the lens is being ordered.</param>
    /// <param name="sph">Spherical power from prescription.</param>
    /// <param name="cyl">Cylinder power from prescription.</param>
    /// <param name="add">Addition power for bifocal/progressive. Null for single vision.</param>
    /// <param name="axis">Cylinder axis in degrees. Null when no astigmatism correction.</param>
    /// <param name="requestedCoatings">Coating options requested for this lens.</param>
    public static LensOrder Create(
        Guid lensCatalogItemId,
        Guid supplierId,
        Guid glassesOrderId,
        Guid patientId,
        decimal sph,
        decimal cyl,
        decimal? add,
        decimal? axis,
        LensCoating requestedCoatings)
    {
        if (lensCatalogItemId == Guid.Empty)
            throw new ArgumentException("LensCatalogItemId is required.", nameof(lensCatalogItemId));

        if (supplierId == Guid.Empty)
            throw new ArgumentException("SupplierId is required.", nameof(supplierId));

        if (glassesOrderId == Guid.Empty)
            throw new ArgumentException("GlassesOrderId is required.", nameof(glassesOrderId));

        if (patientId == Guid.Empty)
            throw new ArgumentException("PatientId is required.", nameof(patientId));

        return new LensOrder
        {
            LensCatalogItemId = lensCatalogItemId,
            SupplierId = supplierId,
            GlassesOrderId = glassesOrderId,
            PatientId = patientId,
            Sph = sph,
            Cyl = cyl,
            Add = add,
            Axis = axis,
            RequestedCoatings = requestedCoatings,
            Status = LensOrderStatus.Ordered
        };
    }

    /// <summary>
    /// Marks the lens order as received from the supplier.
    /// Sets the received timestamp and transitions status to "Received".
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the order is not in "Ordered" status.</exception>
    public void MarkReceived()
    {
        if (Status != LensOrderStatus.Ordered)
            throw new InvalidOperationException(
                $"Cannot mark lens order as received. Current status is '{Status}'.");

        Status = LensOrderStatus.Received;
        ReceivedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Cancels the lens order, recording the cancellation reason in Notes.
    /// </summary>
    /// <param name="reason">Reason for cancellation. Appended to existing notes if any.</param>
    /// <exception cref="InvalidOperationException">Thrown if the order is already received or cancelled.</exception>
    public void Cancel(string reason)
    {
        if (Status == LensOrderStatus.Received)
            throw new InvalidOperationException("Cannot cancel a lens order that has already been received.");

        if (Status == LensOrderStatus.Cancelled)
            throw new InvalidOperationException("Lens order is already cancelled.");

        Status = LensOrderStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes)
            ? $"Cancellation reason: {reason}"
            : $"{Notes}\nCancellation reason: {reason}";

        SetUpdatedAt();
    }
}

/// <summary>
/// Constant strings for LensOrder status values.
/// Using constants instead of enum for flexibility with string storage.
/// </summary>
public static class LensOrderStatus
{
    public const string Ordered = "Ordered";
    public const string Received = "Received";
    public const string Cancelled = "Cancelled";
}

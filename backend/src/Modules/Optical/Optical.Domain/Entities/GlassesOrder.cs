using Optical.Domain.Enums;
using Optical.Domain.Events;
using Shared.Domain;

namespace Optical.Domain.Entities;

/// <summary>
/// Aggregate root representing the full lifecycle of a glasses order.
/// Tracks status transitions (Ordered -> Processing -> Received -> Ready -> Delivered),
/// payment confirmation, delivery timestamp (for warranty calculation), and order items.
/// </summary>
public class GlassesOrder : AggregateRoot, IAuditable
{
    // --- Properties ---

    /// <summary>The patient this order belongs to.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>Denormalized patient name for display without cross-module query.</summary>
    public string PatientName { get; private set; } = string.Empty;

    /// <summary>The clinical visit that originated this order.</summary>
    public Guid VisitId { get; private set; }

    /// <summary>The optical prescription issued during the visit (Clinical module).</summary>
    public Guid OpticalPrescriptionId { get; private set; }

    /// <summary>Current lifecycle status. Starts at Ordered.</summary>
    public GlassesOrderStatus Status { get; private set; }

    /// <summary>Whether lenses are processed in-house or outsourced to a supplier lab.</summary>
    public ProcessingType ProcessingType { get; private set; }

    /// <summary>
    /// Set by the UpdateOrderStatus handler after verifying full payment via Billing integration (OPT-04).
    /// The entity itself does NOT enforce payment — that is the application layer's responsibility.
    /// </summary>
    public bool IsPaymentConfirmed { get; private set; }

    /// <summary>When the order is expected to be ready for pickup.</summary>
    public DateTime? EstimatedDeliveryDate { get; private set; }

    /// <summary>
    /// Set to UTC now when status transitions to Delivered.
    /// Used as the base date for 12-month warranty calculation.
    /// </summary>
    public DateTime? DeliveredAt { get; private set; }

    /// <summary>Total price of the order including all items.</summary>
    public decimal TotalPrice { get; private set; }

    /// <summary>Reference to a preset combo package, if the order uses one. Null for custom combos.</summary>
    public Guid? ComboPackageId { get; private set; }

    /// <summary>Free-text notes from optical staff.</summary>
    public string? Notes { get; private set; }

    // --- Items collection ---

    private readonly List<GlassesOrderItem> _items = new();

    /// <summary>Frame and lens line items on this order.</summary>
    public IReadOnlyList<GlassesOrderItem> Items => _items.AsReadOnly();

    // --- State machine ---

    /// <summary>
    /// Defines the legal forward-only status transitions for a glasses order.
    /// Any transition not listed here is rejected by <see cref="TransitionTo"/>.
    /// </summary>
    private static readonly Dictionary<GlassesOrderStatus, GlassesOrderStatus[]> AllowedTransitions = new()
    {
        [GlassesOrderStatus.Ordered]    = [GlassesOrderStatus.Processing],
        [GlassesOrderStatus.Processing] = [GlassesOrderStatus.Received],
        [GlassesOrderStatus.Received]   = [GlassesOrderStatus.Ready],
        [GlassesOrderStatus.Ready]      = [GlassesOrderStatus.Delivered],
    };

    // --- Factory method ---

    /// <summary>
    /// Creates a new glasses order. Status starts at <see cref="GlassesOrderStatus.Ordered"/>.
    /// </summary>
    public static GlassesOrder Create(
        Guid patientId,
        string patientName,
        Guid visitId,
        Guid opticalPrescriptionId,
        ProcessingType processingType,
        DateTime? estimatedDeliveryDate,
        decimal totalPrice,
        Guid? comboPackageId,
        string? notes,
        BranchId branchId)
    {
        var order = new GlassesOrder
        {
            PatientId             = patientId,
            PatientName           = patientName,
            VisitId               = visitId,
            OpticalPrescriptionId = opticalPrescriptionId,
            ProcessingType        = processingType,
            Status                = GlassesOrderStatus.Ordered,
            IsPaymentConfirmed    = false,
            EstimatedDeliveryDate = estimatedDeliveryDate,
            TotalPrice            = totalPrice,
            ComboPackageId        = comboPackageId,
            Notes                 = notes,
        };

        order.SetBranchId(branchId);
        return order;
    }

    // --- Behaviour methods ---

    /// <summary>
    /// Transitions the order to <paramref name="newStatus"/>.
    /// Validates against the allowed transition table and raises a domain event.
    /// Sets <see cref="DeliveredAt"/> when transitioning to <see cref="GlassesOrderStatus.Delivered"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the requested transition is not allowed from the current status.
    /// </exception>
    public void TransitionTo(GlassesOrderStatus newStatus)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition from {Status} to {newStatus}. " +
                $"Allowed transitions from {Status}: " +
                $"{(allowed is null ? "none (terminal status)" : string.Join(", ", allowed))}.");
        }

        var oldStatus = Status;

        if (newStatus == GlassesOrderStatus.Delivered)
        {
            DeliveredAt = DateTime.UtcNow;
        }

        Status = newStatus;
        SetUpdatedAt();
        AddDomainEvent(new GlassesOrderStatusChangedEvent(Id, oldStatus, newStatus));
    }

    /// <summary>
    /// Marks the order as payment-confirmed. Called by the application handler after verifying
    /// full payment via the Billing module (OPT-04). Does not change order status.
    /// </summary>
    public void ConfirmPayment()
    {
        IsPaymentConfirmed = true;
        SetUpdatedAt();
    }

    /// <summary>
    /// Adds a frame or lens line item to this order.
    /// </summary>
    public void AddItem(GlassesOrderItem item)
    {
        _items.Add(item);
    }

    /// <summary>
    /// Raises the <see cref="GlassesOrderCreatedEvent"/> domain event with the current
    /// order state and line items. Should be called after all items have been added.
    /// </summary>
    public void RaiseCreatedEvent()
    {
        AddDomainEvent(new GlassesOrderCreatedEvent(
            OrderId: Id,
            VisitId: VisitId,
            PatientId: PatientId,
            PatientName: PatientName,
            Items: _items.Select(i => new GlassesOrderCreatedEvent.OrderLineDto(
                Description: i.ItemDescription,
                DescriptionVi: i.ItemDescriptionVi,
                UnitPrice: i.UnitPrice,
                Quantity: i.Quantity)).ToList(),
            BranchId: BranchId.Value));
    }

    // --- Computed properties ---

    /// <summary>
    /// True if the order has been delivered and is within the 12-month warranty window.
    /// Warranty is calculated from <see cref="DeliveredAt"/>, not the order creation date.
    /// </summary>
    public bool IsUnderWarranty =>
        DeliveredAt.HasValue && DeliveredAt.Value.AddMonths(12) > DateTime.UtcNow;

    /// <summary>
    /// True if the estimated delivery date has passed and the order has not yet been delivered.
    /// Used by the overdue order alert dashboard.
    /// </summary>
    public bool IsOverdue =>
        EstimatedDeliveryDate.HasValue
        && Status != GlassesOrderStatus.Delivered
        && DateTime.UtcNow > EstimatedDeliveryDate.Value;

    // --- EF Core ---

    /// <summary>Private parameterless constructor required by EF Core for materialisation.</summary>
    private GlassesOrder() { }
}

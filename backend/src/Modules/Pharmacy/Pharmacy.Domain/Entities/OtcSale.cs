using Pharmacy.Domain.Events;
using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Aggregate root representing a walk-in over-the-counter (OTC) sale at the pharmacy.
/// Does not require a prescription. Customer linkage is optional — sales can be processed
/// for a registered patient, an anonymous walk-in, or both (patient record + name override).
///
/// OTC sales deduct stock via the same FEFO/batch mechanism as prescription dispensing.
/// Payment collection is deferred to Phase 7 (Billing).
/// </summary>
public class OtcSale : AggregateRoot, IAuditable
{
    /// <summary>
    /// Optional link to an existing Patient record (PAT-02 walk-in customers or registered patients).
    /// Null for fully anonymous sales.
    /// </summary>
    public Guid? PatientId { get; private set; }

    /// <summary>
    /// Optional customer name for display and reporting when patient is anonymous
    /// or when the staff wants to record a name without linking to a patient record.
    /// </summary>
    public string? CustomerName { get; private set; }

    /// <summary>Foreign key to the pharmacist/staff user (Auth module) who processed this sale.</summary>
    public Guid SoldById { get; private set; }

    /// <summary>UTC timestamp when the sale was processed.</summary>
    public DateTime SoldAt { get; private set; }

    /// <summary>Optional notes about the sale (e.g., special instructions or reasons).</summary>
    public string? Notes { get; private set; }

    private readonly List<OtcSaleLine> _lines = [];

    /// <summary>
    /// All drug lines included in this OTC sale.
    /// Each line represents one drug item with its quantity and price snapshot.
    /// </summary>
    public IReadOnlyCollection<OtcSaleLine> Lines => _lines.AsReadOnly();

    /// <summary>Private constructor for EF Core materialization.</summary>
    private OtcSale() { }

    /// <summary>
    /// Factory method for creating a new OTC sale.
    /// Call AddLine() after creation to add drug items.
    /// </summary>
    /// <param name="patientId">Optional patient record to link this sale to. Null for anonymous.</param>
    /// <param name="customerName">Optional customer name for anonymous or named walk-in customers.</param>
    /// <param name="soldById">The staff member processing this sale.</param>
    /// <param name="notes">Optional notes about the sale.</param>
    /// <param name="branchId">The branch where the sale occurs (multi-tenant isolation).</param>
    public static OtcSale Create(
        Guid? patientId,
        string? customerName,
        Guid soldById,
        string? notes,
        BranchId branchId)
    {
        var sale = new OtcSale
        {
            PatientId = patientId,
            CustomerName = customerName?.Trim(),
            SoldById = soldById,
            SoldAt = DateTime.UtcNow,
            Notes = notes?.Trim()
        };

        sale.SetBranchId(branchId);
        return sale;
    }

    /// <summary>
    /// Adds a drug line to this OTC sale.
    /// Creates an OtcSaleLine child entity and appends it to this aggregate's collection.
    /// Call AddBatchDeduction() on the returned line to record FEFO batch allocations.
    /// </summary>
    /// <param name="drugCatalogItemId">The Pharmacy DrugCatalogItem being sold.</param>
    /// <param name="drugName">Drug name denormalized for audit records.</param>
    /// <param name="quantity">Quantity being sold (must be positive).</param>
    /// <param name="unitPrice">Selling price per unit at the time of sale (price snapshot).</param>
    /// <returns>The created OtcSaleLine for adding batch deductions.</returns>
    public OtcSaleLine AddLine(
        Guid drugCatalogItemId,
        string drugName,
        int quantity,
        decimal unitPrice)
    {
        var line = OtcSaleLine.Create(Id, drugCatalogItemId, drugName, quantity, unitPrice);
        _lines.Add(line);
        return line;
    }

    /// <summary>
    /// Raises an OtcSaleCompletedEvent domain event with the provided drug line items.
    /// Called by the handler after all sale lines are processed and enriched with
    /// catalog data (Vietnamese name) for downstream billing integration.
    /// </summary>
    /// <param name="items">Enriched drug line items with names, quantities, and unit prices.</param>
    public void RaiseSaleCompletedEvent(List<OtcSaleCompletedEvent.DrugLineDto> items)
    {
        AddDomainEvent(new OtcSaleCompletedEvent(
            OtcSaleId: Id,
            PatientId: PatientId,
            CustomerName: CustomerName,
            Items: items,
            BranchId: BranchId.Value));
    }
}

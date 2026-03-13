using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Events;
using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Aggregate root representing a complete dispensing event for a single HIS prescription.
/// Created by the pharmacist when they dispense (or intentionally skip) drugs for a patient.
///
/// Each line in the prescription becomes a DispensingLine (Dispensed or Skipped).
/// OverrideReason is required when dispensing an expired prescription (PHR-07).
/// </summary>
public class DispensingRecord : AggregateRoot, IAuditable
{
    /// <summary>
    /// Foreign key to the Clinical DrugPrescription.Id this record fulfils.
    /// The Clinical module is the source of truth for prescription data.
    /// </summary>
    public Guid PrescriptionId { get; private set; }

    /// <summary>Foreign key to the Clinical Visit this prescription belongs to.</summary>
    public Guid VisitId { get; private set; }

    /// <summary>Foreign key to the patient receiving the dispensed drugs.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>
    /// Patient name denormalized from Patient module for audit records without cross-module joins.
    /// </summary>
    public string PatientName { get; private set; } = string.Empty;

    /// <summary>Foreign key to the pharmacist user (Auth module) who performed the dispensing.</summary>
    public Guid DispensedById { get; private set; }

    /// <summary>UTC timestamp when the dispensing event occurred.</summary>
    public DateTime DispensedAt { get; private set; }

    /// <summary>
    /// Override reason provided by the pharmacist when dispensing an expired prescription (PHR-07).
    /// Null when the prescription was within its 7-day validity window.
    /// </summary>
    public string? OverrideReason { get; private set; }

    private readonly List<DispensingLine> _lines = [];

    /// <summary>
    /// All dispensing lines for this record — one line per prescription item.
    /// Each line is either Dispensed (with batch deductions) or Skipped.
    /// </summary>
    public IReadOnlyCollection<DispensingLine> Lines => _lines.AsReadOnly();

    /// <summary>Private constructor for EF Core materialization.</summary>
    private DispensingRecord() { }

    /// <summary>
    /// Factory method for creating a new dispensing record.
    /// Call AddLine() after creation to add each prescription drug line.
    /// </summary>
    /// <param name="prescriptionId">The Clinical DrugPrescription.Id being dispensed.</param>
    /// <param name="visitId">The Clinical Visit this prescription belongs to.</param>
    /// <param name="patientId">The patient receiving the drugs.</param>
    /// <param name="patientName">Patient name (denormalized for audit).</param>
    /// <param name="dispensedById">The pharmacist performing the dispensing.</param>
    /// <param name="overrideReason">Override reason if the prescription is expired (PHR-07). Null for valid prescriptions.</param>
    /// <param name="branchId">The branch where dispensing occurs (multi-tenant isolation).</param>
    public static DispensingRecord Create(
        Guid prescriptionId,
        Guid visitId,
        Guid patientId,
        string patientName,
        Guid dispensedById,
        string? overrideReason,
        BranchId branchId)
    {
        if (string.IsNullOrWhiteSpace(patientName))
            throw new ArgumentException("Patient name is required.", nameof(patientName));

        var record = new DispensingRecord
        {
            PrescriptionId = prescriptionId,
            VisitId = visitId,
            PatientId = patientId,
            PatientName = patientName,
            DispensedById = dispensedById,
            DispensedAt = DateTime.UtcNow,
            OverrideReason = overrideReason
        };

        record.SetBranchId(branchId);
        return record;
    }

    /// <summary>
    /// Adds a dispensing line for one prescription item.
    /// Creates a DispensingLine child entity and appends it to this aggregate's collection.
    /// Call AddBatchDeduction() on the returned line to record FEFO batch allocations.
    /// </summary>
    /// <param name="prescriptionItemId">The Clinical PrescriptionItem.Id this line fulfils.</param>
    /// <param name="drugCatalogItemId">The Pharmacy DrugCatalogItem being dispensed.</param>
    /// <param name="drugName">Drug name (denormalized for audit).</param>
    /// <param name="quantity">Quantity prescribed on this line.</param>
    /// <param name="status">Dispensed or Skipped.</param>
    /// <returns>The created DispensingLine for adding batch deductions.</returns>
    public DispensingLine AddLine(
        Guid prescriptionItemId,
        Guid drugCatalogItemId,
        string drugName,
        int quantity,
        DispensingStatus status)
    {
        var line = DispensingLine.Create(
            Id,
            prescriptionItemId,
            drugCatalogItemId,
            drugName,
            quantity,
            status);

        _lines.Add(line);
        return line;
    }

    /// <summary>
    /// Raises a DrugDispensedEvent domain event with the provided drug line items.
    /// Called by the handler after all dispensing lines are processed and enriched with
    /// catalog data (Vietnamese name, selling price) for downstream billing integration.
    /// </summary>
    /// <param name="items">Enriched drug line items with names, quantities, and unit prices.</param>
    public void RaiseDispensedEvent(List<DrugDispensedEvent.DrugLineDto> items)
    {
        AddDomainEvent(new DrugDispensedEvent(
            VisitId: VisitId,
            PatientId: PatientId,
            PatientName: PatientName,
            Items: items));
    }
}

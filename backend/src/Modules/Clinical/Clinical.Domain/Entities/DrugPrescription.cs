using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Drug prescription as a Visit child entity. Contains a collection of PrescriptionItems.
/// Subject to Visit.EnsureEditable() guard via Visit.AddDrugPrescription().
/// PrescriptionCode is a 14-char auto-generated identifier per MOH Circular 26/2025.
/// Notes field stores "Loi dan" (doctor's advice), required per MOH.
/// </summary>
public class DrugPrescription : Entity
{
    public Guid VisitId { get; private set; }
    public string? Notes { get; private set; }
    public string? PrescriptionCode { get; private set; }
    public DateTime PrescribedAt { get; private set; }

    private readonly List<PrescriptionItem> _items = [];
    public IReadOnlyCollection<PrescriptionItem> Items => _items.AsReadOnly();

    private DrugPrescription() { }

    /// <summary>
    /// Factory method for creating a new drug prescription for a visit.
    /// </summary>
    public static DrugPrescription Create(Guid visitId, string? notes)
    {
        return new DrugPrescription
        {
            VisitId = visitId,
            Notes = notes,
            PrescribedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adds a prescription item and assigns its SortOrder based on current item count.
    /// </summary>
    public void AddItem(PrescriptionItem item)
    {
        _items.Add(item);
        SetUpdatedAt();
    }

    /// <summary>
    /// Removes a prescription item by ID.
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Prescription item {itemId} not found.");
        _items.Remove(item);
        SetUpdatedAt();
    }

    /// <summary>
    /// Updates the doctor's advice notes (Loi dan).
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        SetUpdatedAt();
    }

    /// <summary>
    /// Generates a 14-character prescription code.
    /// Format: date prefix (yyyyMMdd) + 6-char sequence (padded).
    /// </summary>
    public void GeneratePrescriptionCode()
    {
        var datePrefix = PrescribedAt.ToString("yyyyMMdd");
        var sequence = Id.ToString("N")[..6].ToUpperInvariant();
        PrescriptionCode = $"{datePrefix}{sequence}";
    }
}

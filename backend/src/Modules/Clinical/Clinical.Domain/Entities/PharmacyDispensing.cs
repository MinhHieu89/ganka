using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Pharmacy dispensing record for a visit. Contains line items for each prescribed drug.
/// Each line item tracks whether it was dispensed.
/// </summary>
public class PharmacyDispensing : Entity
{
    public Guid VisitId { get; private set; }
    public Guid PharmacistId { get; private set; }
    public string PharmacistName { get; private set; } = string.Empty;
    public DateTime DispensedAt { get; private set; }
    public string? DispenseNote { get; private set; }

    private readonly List<DispensingLineItem> _lineItems = [];
    public IReadOnlyCollection<DispensingLineItem> LineItems => _lineItems.AsReadOnly();

    private PharmacyDispensing() { }

    public static PharmacyDispensing Create(Guid visitId, Guid pharmacistId, string pharmacistName,
        List<(string DrugName, int Quantity, string Instruction)> items, string? dispenseNote = null)
    {
        var dispensing = new PharmacyDispensing
        {
            VisitId = visitId,
            PharmacistId = pharmacistId,
            PharmacistName = pharmacistName,
            DispensedAt = DateTime.UtcNow,
            DispenseNote = dispenseNote
        };

        foreach (var (drugName, quantity, instruction) in items)
        {
            dispensing._lineItems.Add(DispensingLineItem.Create(dispensing.Id, drugName, quantity, instruction));
        }

        return dispensing;
    }
}

/// <summary>
/// Individual drug line item in a pharmacy dispensing record.
/// </summary>
public class DispensingLineItem : Entity
{
    public Guid PharmacyDispensingId { get; private set; }
    public string DrugName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public string Instruction { get; private set; } = string.Empty;
    public bool IsDispensed { get; private set; }

    private DispensingLineItem() { }

    public static DispensingLineItem Create(Guid pharmacyDispensingId, string drugName, int quantity, string instruction)
    {
        return new DispensingLineItem
        {
            PharmacyDispensingId = pharmacyDispensingId,
            DrugName = drugName,
            Quantity = quantity,
            Instruction = instruction,
            IsDispensed = false
        };
    }

    public void MarkDispensed()
    {
        IsDispensed = true;
        SetUpdatedAt();
    }
}

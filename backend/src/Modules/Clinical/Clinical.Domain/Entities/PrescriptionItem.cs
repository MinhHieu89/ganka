using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Individual drug line in a prescription. Tracks both catalog-linked and off-catalog drugs.
/// DrugCatalogItemId is nullable: null means off-catalog (doctor typed drug manually).
/// Form and Route are stored as plain ints to avoid cross-module domain dependency on Pharmacy.Domain enums.
/// </summary>
public class PrescriptionItem : Entity
{
    public Guid DrugPrescriptionId { get; private set; }
    public Guid? DrugCatalogItemId { get; private set; }
    public string DrugName { get; private set; } = string.Empty;
    public string? GenericName { get; private set; }
    public string? Strength { get; private set; }
    public int Form { get; private set; }
    public int Route { get; private set; }
    public string? Dosage { get; private set; }
    public string? DosageOverride { get; private set; }
    public int Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public string? Frequency { get; private set; }
    public int? DurationDays { get; private set; }
    public bool IsOffCatalog { get; private set; }
    public bool HasAllergyWarning { get; private set; }
    public int SortOrder { get; private set; }

    private PrescriptionItem() { }

    /// <summary>
    /// Creates a prescription item linked to a drug catalog entry.
    /// </summary>
    public static PrescriptionItem CreateFromCatalog(
        Guid drugPrescriptionId,
        Guid drugCatalogItemId,
        string drugName,
        string? genericName,
        string? strength,
        int form,
        int route,
        string? dosage,
        string? dosageOverride,
        int quantity,
        string unit,
        string? frequency,
        int? durationDays,
        bool hasAllergyWarning,
        int sortOrder)
    {
        return new PrescriptionItem
        {
            DrugPrescriptionId = drugPrescriptionId,
            DrugCatalogItemId = drugCatalogItemId,
            DrugName = drugName,
            GenericName = genericName,
            Strength = strength,
            Form = form,
            Route = route,
            Dosage = dosage,
            DosageOverride = dosageOverride,
            Quantity = quantity,
            Unit = unit,
            Frequency = frequency,
            DurationDays = durationDays,
            IsOffCatalog = false,
            HasAllergyWarning = hasAllergyWarning,
            SortOrder = sortOrder
        };
    }

    /// <summary>
    /// Creates an off-catalog prescription item (doctor typed drug name manually).
    /// DrugCatalogItemId is null and IsOffCatalog is true.
    /// </summary>
    public static PrescriptionItem CreateOffCatalog(
        Guid drugPrescriptionId,
        string drugName,
        string? genericName,
        string? strength,
        int form,
        int route,
        string? dosage,
        string? dosageOverride,
        int quantity,
        string unit,
        string? frequency,
        int? durationDays,
        bool hasAllergyWarning,
        int sortOrder)
    {
        return new PrescriptionItem
        {
            DrugPrescriptionId = drugPrescriptionId,
            DrugCatalogItemId = null,
            DrugName = drugName,
            GenericName = genericName,
            Strength = strength,
            Form = form,
            Route = route,
            Dosage = dosage,
            DosageOverride = dosageOverride,
            Quantity = quantity,
            Unit = unit,
            Frequency = frequency,
            DurationDays = durationDays,
            IsOffCatalog = true,
            HasAllergyWarning = hasAllergyWarning,
            SortOrder = sortOrder
        };
    }
}

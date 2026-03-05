using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// Represents a drug entry in the clinic's pharmacy catalog.
/// Stores drug metadata for prescription writing. The Clinical module
/// references this catalog via Pharmacy.Contracts cross-module queries.
/// Phase 6 will add supplier, price, batch tracking, and stock level fields.
/// </summary>
public class DrugCatalogItem : AggregateRoot, IAuditable
{
    /// <summary>English drug name (e.g., "Tobramycin 0.3%")</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Vietnamese drug name (e.g., "Tobramycin 0,3%")</summary>
    public string NameVi { get; private set; } = string.Empty;

    /// <summary>International Nonproprietary Name (e.g., "Tobramycin")</summary>
    public string GenericName { get; private set; } = string.Empty;

    /// <summary>Pharmaceutical form (drops, tablet, ointment, etc.)</summary>
    public DrugForm Form { get; private set; }

    /// <summary>Drug strength (e.g., "0.3%", "500mg"). Null if not applicable.</summary>
    public string? Strength { get; private set; }

    /// <summary>Route of administration (topical, oral, IM, etc.)</summary>
    public DrugRoute Route { get; private set; }

    /// <summary>Dispensing unit (e.g., "Chai", "Hộp", "Tuýp")</summary>
    public string Unit { get; private set; } = string.Empty;

    /// <summary>Default dosage instruction template (e.g., "1-2 giọt x 4 lần/ngày")</summary>
    public string? DefaultDosageTemplate { get; private set; }

    /// <summary>Whether the catalog item is active. Inactive items are hidden from search.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Private constructor for EF Core materialization.</summary>
    private DrugCatalogItem() { }

    /// <summary>
    /// Factory method for creating a new drug catalog item.
    /// </summary>
    public static DrugCatalogItem Create(
        string name,
        string nameVi,
        string genericName,
        DrugForm form,
        string? strength,
        DrugRoute route,
        string unit,
        string? defaultDosageTemplate,
        BranchId branchId)
    {
        var item = new DrugCatalogItem
        {
            Name = name,
            NameVi = nameVi,
            GenericName = genericName,
            Form = form,
            Strength = strength,
            Route = route,
            Unit = unit,
            DefaultDosageTemplate = defaultDosageTemplate,
            IsActive = true
        };

        item.SetBranchId(branchId);
        return item;
    }

    /// <summary>
    /// Updates editable fields of the drug catalog item.
    /// </summary>
    public void Update(
        string name,
        string nameVi,
        string genericName,
        DrugForm form,
        string? strength,
        DrugRoute route,
        string unit,
        string? defaultDosageTemplate)
    {
        Name = name;
        NameVi = nameVi;
        GenericName = genericName;
        Form = form;
        Strength = strength;
        Route = route;
        Unit = unit;
        DefaultDosageTemplate = defaultDosageTemplate;

        SetUpdatedAt();
    }

    /// <summary>
    /// Soft-deactivates the catalog item, hiding it from drug search results.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Re-activates a previously deactivated catalog item.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}

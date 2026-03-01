using Shared.Domain;

namespace Patient.Domain.Entities;

/// <summary>
/// Reference data for allergy autocomplete suggestions.
/// Contains both English and Vietnamese names for common ophthalmology-relevant allergies.
/// </summary>
public class AllergyCatalogItem : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string NameVi { get; private set; } = string.Empty;
    public string? Category { get; private set; }

    private AllergyCatalogItem() { }

    public static AllergyCatalogItem Create(string name, string nameVi, string? category = null)
    {
        return new AllergyCatalogItem
        {
            Name = name,
            NameVi = nameVi,
            Category = category
        };
    }
}

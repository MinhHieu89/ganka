using Clinical.Contracts.Dtos;
using Patient.Contracts.Dtos;
using Wolverine;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for cross-referencing a drug against patient allergies.
/// Queries patient allergies via IMessageBus (Patient module) and checks
/// drug name and generic name against allergy names (case-insensitive).
/// Returns matching allergies for frontend warning display.
/// </summary>
public static class CheckDrugAllergyHandler
{
    public static async Task<List<AllergyDto>> Handle(
        CheckDrugAllergyQuery query,
        IMessageBus bus,
        CancellationToken ct)
    {
        var allergies = await bus.InvokeAsync<List<AllergyDto>>(
            new GetPatientAllergiesQuery(query.PatientId), ct);

        if (allergies is null || allergies.Count == 0)
            return [];

        var matches = new List<AllergyDto>();

        foreach (var allergy in allergies)
        {
            var allergyName = allergy.Name;

            // Check if drug name contains the allergy name (case-insensitive)
            if (query.DrugName.Contains(allergyName, StringComparison.OrdinalIgnoreCase) ||
                allergyName.Contains(query.DrugName, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(allergy);
                continue;
            }

            // Check if generic name matches the allergy name (case-insensitive)
            if (!string.IsNullOrEmpty(query.GenericName) &&
                (query.GenericName.Contains(allergyName, StringComparison.OrdinalIgnoreCase) ||
                 allergyName.Contains(query.GenericName, StringComparison.OrdinalIgnoreCase)))
            {
                matches.Add(allergy);
            }
        }

        return matches;
    }
}

using Optical.Contracts.Queries;
using Shared.Domain;
using Wolverine;

namespace Optical.Application.Features.Prescriptions;

/// <summary>
/// Query to compare two prescriptions side by side with change indicators.
/// Returns null if either prescription is not found in the patient's history.
/// </summary>
public sealed record GetPrescriptionComparisonQuery(Guid PatientId, Guid PrescriptionId1, Guid PrescriptionId2);

/// <summary>
/// Side-by-side comparison of two optical prescriptions with per-field change indicators.
/// Older and Newer are determined by VisitDate, regardless of query parameter order.
/// </summary>
public sealed record PrescriptionComparisonDto(
    OpticalPrescriptionHistoryDto Older,
    OpticalPrescriptionHistoryDto Newer,
    List<FieldChange> Changes);

/// <summary>
/// A single field change between two prescriptions.
/// Direction: "increased", "decreased", or "same".
/// </summary>
public sealed record FieldChange(
    string FieldName,
    string? OldValue,
    string? NewValue,
    string Direction);

/// <summary>
/// Wolverine static handler for comparing two optical prescriptions side-by-side.
/// Retrieves full prescription history from Clinical module, finds both prescriptions,
/// determines which is older/newer by VisitDate, and computes per-field changes.
/// Returns null if either prescription ID is not found.
/// </summary>
public static class GetPrescriptionComparisonHandler
{
    /// <summary>Fields included in the comparison, in order.</summary>
    private static readonly string[] ComparedFields =
    [
        "SphOd", "CylOd", "AxisOd", "AddOd",
        "SphOs", "CylOs", "AxisOs", "AddOs",
        "Pd"
    ];

    public static async Task<Result<PrescriptionComparisonDto>> Handle(
        GetPrescriptionComparisonQuery query,
        IMessageBus bus,
        CancellationToken ct)
    {
        var history = await bus.InvokeAsync<List<OpticalPrescriptionHistoryDto>?>(
            new GetPatientOpticalPrescriptionsQuery(query.PatientId), ct);

        if (history is null || history.Count == 0)
            return Result.Failure<PrescriptionComparisonDto>(
                Error.NotFound("Prescriptions", query.PatientId));

        var rx1 = history.FirstOrDefault(p => p.Id == query.PrescriptionId1);
        var rx2 = history.FirstOrDefault(p => p.Id == query.PrescriptionId2);

        if (rx1 is null || rx2 is null)
            return Result.Failure<PrescriptionComparisonDto>(
                Error.NotFound("Prescription", rx1 is null ? query.PrescriptionId1 : query.PrescriptionId2));

        // Determine which is older/newer by VisitDate
        var older = rx1.VisitDate <= rx2.VisitDate ? rx1 : rx2;
        var newer = rx1.VisitDate <= rx2.VisitDate ? rx2 : rx1;

        var changes = ComputeChanges(older, newer);

        return new PrescriptionComparisonDto(older, newer, changes);
    }

    private static List<FieldChange> ComputeChanges(
        OpticalPrescriptionHistoryDto older,
        OpticalPrescriptionHistoryDto newer)
    {
        var changes = new List<FieldChange>();

        foreach (var field in ComparedFields)
        {
            var oldVal = GetField(older, field);
            var newVal = GetField(newer, field);

            if (oldVal == newVal)
                continue; // no change — skip

            var oldStr = oldVal?.ToString("0.00") ?? null;
            var newStr = newVal?.ToString("0.00") ?? null;

            string direction;
            if (oldVal is null || newVal is null)
                direction = "changed";
            else if (newVal > oldVal)
                direction = "increased";
            else
                direction = "decreased";

            changes.Add(new FieldChange(field, oldStr, newStr, direction));
        }

        return changes;
    }

    private static decimal? GetField(OpticalPrescriptionHistoryDto dto, string fieldName) =>
        fieldName switch
        {
            "SphOd" => dto.SphOd,
            "CylOd" => dto.CylOd,
            "AxisOd" => dto.AxisOd,
            "AddOd" => dto.AddOd,
            "SphOs" => dto.SphOs,
            "CylOs" => dto.CylOs,
            "AxisOs" => dto.AxisOs,
            "AddOs" => dto.AddOs,
            "Pd" => dto.Pd,
            _ => null
        };
}

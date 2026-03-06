using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine static handler for the cross-module GetPendingPrescriptionsQuery.
/// Called by Pharmacy.Infrastructure via IMessageBus to retrieve the pharmacy dispensing queue.
///
/// Returns all drug prescriptions from the Clinical module along with their visit context.
/// The Pharmacy module is responsible for filtering out already-dispensed prescriptions.
///
/// IsExpired/DaysRemaining are based on a 7-day validity window from PrescribedAt.
/// </summary>
public static class GetPendingPrescriptionsHandler
{
    /// <summary>
    /// Prescription validity window: 7 days from PrescribedAt.
    /// After 7 days, prescriptions are considered expired for dispensing purposes.
    /// </summary>
    private const int PrescriptionValidityDays = 7;

    public static async Task<List<ClinicalPendingPrescriptionDto>> Handle(
        GetPendingPrescriptionsQuery query,
        IVisitRepository visitRepository,
        CancellationToken ct)
    {
        var prescriptionsWithVisits = await visitRepository.GetPrescriptionsWithVisitsAsync(
            query.PatientId, ct);

        var now = DateTime.UtcNow;

        return prescriptionsWithVisits
            .Select(pw =>
            {
                var expiryDate = pw.Prescription.PrescribedAt.AddDays(PrescriptionValidityDays);
                var daysRemaining = (int)(expiryDate - now).TotalDays;
                var isExpired = daysRemaining < 0;

                return new ClinicalPendingPrescriptionDto(
                    PrescriptionId: pw.Prescription.Id,
                    VisitId: pw.Visit.Id,
                    PatientId: pw.Visit.PatientId,
                    PatientName: pw.Visit.PatientName,
                    PrescriptionCode: pw.Prescription.PrescriptionCode,
                    PrescribedAt: pw.Prescription.PrescribedAt,
                    IsExpired: isExpired,
                    DaysRemaining: Math.Max(0, daysRemaining),
                    Items: pw.Prescription.Items.Select(item => new ClinicalPendingPrescriptionItemDto(
                        PrescriptionItemId: item.Id,
                        DrugCatalogItemId: item.DrugCatalogItemId,
                        DrugName: item.DrugName,
                        Quantity: item.Quantity,
                        Unit: item.Unit,
                        Dosage: item.Dosage,
                        IsOffCatalog: item.IsOffCatalog)).ToList());
            })
            .ToList();
    }
}

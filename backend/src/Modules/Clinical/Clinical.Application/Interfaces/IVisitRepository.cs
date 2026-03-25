using Clinical.Domain.Entities;
using Optical.Contracts.Queries;

namespace Clinical.Application.Interfaces;

/// <summary>
/// Repository interface for the Visit aggregate root.
/// </summary>
public interface IVisitRepository
{
    /// <summary>
    /// Gets a visit by ID without related entities.
    /// </summary>
    Task<Visit?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a visit by ID with all related entities (Refractions, Diagnoses, Amendments).
    /// </summary>
    Task<Visit?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets active visits for the workflow dashboard.
    /// Returns visits that are Draft/Amended or were Signed within the last 24 hours.
    /// </summary>
    Task<List<Visit>> GetActiveVisitsAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds a new visit to the repository.
    /// </summary>
    Task AddAsync(Visit visit, CancellationToken ct = default);

    /// <summary>
    /// Checks if a patient already has an active (non-signed) visit.
    /// </summary>
    Task<bool> HasActiveVisitForPatientAsync(Guid patientId, CancellationToken ct = default);

    /// <summary>
    /// Explicitly adds a Refraction entity to the change tracker as Added.
    /// Required because adding through Visit aggregate backing field does not register with EF Core.
    /// </summary>
    void AddRefraction(Refraction refraction);

    /// <summary>
    /// Explicitly adds a VisitDiagnosis entity to the change tracker as Added.
    /// </summary>
    void AddDiagnosis(VisitDiagnosis diagnosis);

    /// <summary>
    /// Explicitly adds a VisitAmendment entity to the change tracker as Added.
    /// </summary>
    void AddAmendment(VisitAmendment amendment);

    /// <summary>
    /// Explicitly adds a DryEyeAssessment entity to the change tracker as Added.
    /// Required because adding through Visit aggregate backing field does not register with EF Core.
    /// </summary>
    void AddDryEyeAssessment(DryEyeAssessment assessment);

    /// <summary>
    /// Explicitly adds a DrugPrescription entity to the change tracker as Added.
    /// Required because adding through Visit aggregate backing field does not register with EF Core.
    /// </summary>
    void AddDrugPrescription(DrugPrescription prescription);

    /// <summary>
    /// Explicitly adds a PrescriptionItem entity to the change tracker as Added.
    /// Required because adding through Visit aggregate backing field does not register with EF Core.
    /// </summary>
    void AddPrescriptionItem(PrescriptionItem item);

    /// <summary>
    /// Explicitly adds an OpticalPrescription entity to the change tracker as Added.
    /// Required because adding through Visit aggregate backing field does not register with EF Core.
    /// </summary>
    void AddOpticalPrescription(OpticalPrescription prescription);

    /// <summary>
    /// Removes OpticalPrescription entities for a visit from the change tracker.
    /// Used when SetOpticalPrescription clears existing before adding new.
    /// </summary>
    void RemoveOpticalPrescriptions(IEnumerable<OpticalPrescription> prescriptions);

    /// <summary>
    /// Gets all dry eye assessments for a patient across visits, for trend chart display.
    /// Includes Visit navigation for VisitDate, ordered by VisitDate.
    /// </summary>
    Task<List<DryEyeAssessment>> GetDryEyeAssessmentsByPatientAsync(Guid patientId, CancellationToken ct = default);

    /// <summary>
    /// Gets the dry eye assessment for a specific visit.
    /// </summary>
    Task<DryEyeAssessment?> GetDryEyeAssessmentByVisitAsync(Guid visitId, CancellationToken ct = default);

    /// <summary>
    /// Gets pending drug prescriptions not yet dispensed, with optional patient filter.
    /// A prescription is "pending" when its visit has a DrugPrescription with no corresponding
    /// dispensing record in the Pharmacy module. This query returns the clinical-side data;
    /// the Pharmacy module checks which ones are already dispensed.
    /// </summary>
    Task<List<(DrugPrescription Prescription, Visit Visit)>> GetPrescriptionsWithVisitsAsync(
        Guid? patientId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all optical prescriptions for a patient across visits, ordered by visit date descending.
    /// Returns OpticalPrescriptionHistoryDto mapped from OpticalPrescription joined with Visit.
    /// Used for cross-module query handler GetPatientOpticalPrescriptionsQuery.
    /// </summary>
    Task<List<OpticalPrescriptionHistoryDto>> GetOpticalPrescriptionsByPatientIdAsync(
        Guid patientId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all visits for a patient, including diagnoses for primary diagnosis text.
    /// Used for patient visit history query (D-13/D-15).
    /// </summary>
    Task<List<Visit>> GetVisitsByPatientIdAsync(Guid patientId, CancellationToken ct = default);

    /// <summary>
    /// Gets active visits (Draft/Amended) plus completed-today visits for the done column (D-04/D-10).
    /// </summary>
    Task<List<Visit>> GetActiveVisitsIncludingDoneTodayAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets dry eye assessments with their visit dates for a patient, ordered by visit date ascending.
    /// Joins DryEyeAssessments with Visits in a single query to avoid N+1.
    /// Optionally filters to visits on or after the specified cutoff date.
    /// </summary>
    Task<List<(DryEyeAssessment Assessment, DateTime VisitDate)>> GetMetricHistoryAsync(
        Guid patientId,
        DateTime? since,
        CancellationToken ct = default);
}

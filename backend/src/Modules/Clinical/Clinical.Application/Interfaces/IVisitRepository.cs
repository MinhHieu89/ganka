using Clinical.Domain.Entities;

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
    /// Gets all dry eye assessments for a patient across visits, for trend chart display.
    /// Includes Visit navigation for VisitDate, ordered by VisitDate.
    /// </summary>
    Task<List<DryEyeAssessment>> GetDryEyeAssessmentsByPatientAsync(Guid patientId, CancellationToken ct = default);

    /// <summary>
    /// Gets the dry eye assessment for a specific visit.
    /// </summary>
    Task<DryEyeAssessment?> GetDryEyeAssessmentByVisitAsync(Guid visitId, CancellationToken ct = default);
}

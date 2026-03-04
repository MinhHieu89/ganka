using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Visit aggregate root. Represents a clinical visit record for a patient.
/// Implements immutability after sign-off via EnsureEditable guard.
/// Uses denormalized PatientName/DoctorName to avoid cross-module joins.
/// HasAllergies is denormalized from Patient for allergy warning icon on Kanban cards.
/// </summary>
public class Visit : AggregateRoot, IAuditable
{
    public Guid PatientId { get; private set; }
    public string PatientName { get; private set; } = string.Empty;
    public Guid DoctorId { get; private set; }
    public string DoctorName { get; private set; } = string.Empty;
    public Guid? AppointmentId { get; private set; }
    public WorkflowStage CurrentStage { get; private set; }
    public VisitStatus Status { get; private set; }
    public DateTime VisitDate { get; private set; }
    public string? ExaminationNotes { get; private set; }
    public bool HasAllergies { get; private set; }
    public DateTime? SignedAt { get; private set; }
    public Guid? SignedById { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private readonly List<Refraction> _refractions = [];
    public IReadOnlyCollection<Refraction> Refractions => _refractions.AsReadOnly();

    private readonly List<VisitDiagnosis> _diagnoses = [];
    public IReadOnlyCollection<VisitDiagnosis> Diagnoses => _diagnoses.AsReadOnly();

    private readonly List<VisitAmendment> _amendments = [];
    public IReadOnlyCollection<VisitAmendment> Amendments => _amendments.AsReadOnly();

    private Visit() { }

    /// <summary>
    /// Factory method for creating a new clinical visit.
    /// Sets initial stage to Reception and status to Draft.
    /// </summary>
    public static Visit Create(
        Guid patientId,
        string patientName,
        Guid doctorId,
        string doctorName,
        BranchId branchId,
        bool hasAllergies,
        Guid? appointmentId = null)
    {
        var visit = new Visit
        {
            PatientId = patientId,
            PatientName = patientName,
            DoctorId = doctorId,
            DoctorName = doctorName,
            AppointmentId = appointmentId,
            CurrentStage = WorkflowStage.Reception,
            Status = VisitStatus.Draft,
            VisitDate = DateTime.UtcNow,
            HasAllergies = hasAllergies
        };

        visit.SetBranchId(branchId);
        return visit;
    }

    /// <summary>
    /// Advances the visit to a new workflow stage.
    /// Validates that the new stage is a valid progression.
    /// </summary>
    public void AdvanceStage(WorkflowStage newStage)
    {
        if (newStage <= CurrentStage)
            throw new InvalidOperationException(
                $"Cannot move to stage {newStage}. Current stage is {CurrentStage}.");

        CurrentStage = newStage;
        SetUpdatedAt();
    }

    /// <summary>
    /// Signs off the visit, making it immutable.
    /// Only a doctor can sign off a visit.
    /// </summary>
    public void SignOff(Guid doctorId)
    {
        if (Status == VisitStatus.Signed)
            throw new InvalidOperationException("Visit is already signed off.");

        Status = VisitStatus.Signed;
        SignedAt = DateTime.UtcNow;
        SignedById = doctorId;
        SetUpdatedAt();
    }

    /// <summary>
    /// Updates examination notes. Requires the visit to be editable.
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        EnsureEditable();
        ExaminationNotes = notes;
        SetUpdatedAt();
    }

    /// <summary>
    /// Adds a refraction record to the visit. Requires the visit to be editable.
    /// </summary>
    public void AddRefraction(Refraction refraction)
    {
        EnsureEditable();
        _refractions.Add(refraction);
        SetUpdatedAt();
    }

    /// <summary>
    /// Adds a diagnosis to the visit. Requires the visit to be editable.
    /// </summary>
    public void AddDiagnosis(VisitDiagnosis diagnosis)
    {
        EnsureEditable();
        _diagnoses.Add(diagnosis);
        SetUpdatedAt();
    }

    /// <summary>
    /// Removes a diagnosis from the visit by ID. Requires the visit to be editable.
    /// </summary>
    public void RemoveDiagnosis(Guid diagnosisId)
    {
        EnsureEditable();
        var diagnosis = _diagnoses.FirstOrDefault(d => d.Id == diagnosisId);
        if (diagnosis is null)
            throw new InvalidOperationException($"Diagnosis with ID {diagnosisId} not found.");

        _diagnoses.Remove(diagnosis);
        SetUpdatedAt();
    }

    /// <summary>
    /// Starts an amendment workflow, allowing edits to a signed visit.
    /// </summary>
    public void StartAmendment()
    {
        if (Status != VisitStatus.Signed)
            throw new InvalidOperationException("Only signed visits can be amended.");

        Status = VisitStatus.Amended;
        SetUpdatedAt();
    }

    /// <summary>
    /// Guard method: throws if the visit is signed and not in amendment mode.
    /// Must be called before any mutation operation.
    /// </summary>
    private void EnsureEditable()
    {
        if (Status == VisitStatus.Signed)
            throw new InvalidOperationException(
                "Cannot modify a signed visit. Use amendment workflow.");
    }
}

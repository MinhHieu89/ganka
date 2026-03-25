using Clinical.Domain.Enums;
using Clinical.Domain.Events;
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

    // Parallel track statuses (post-Cashier)
    public TrackStatus DrugTrackStatus { get; private set; } = TrackStatus.NotApplicable;
    public TrackStatus GlassesTrackStatus { get; private set; } = TrackStatus.NotApplicable;

    // Branching flags
    public bool ImagingRequested { get; private set; }
    public bool RefractionSkipped { get; private set; }
    public bool HasGlassesPrescription { get; private set; }

    private readonly List<Refraction> _refractions = [];
    public IReadOnlyCollection<Refraction> Refractions => _refractions.AsReadOnly();

    private readonly List<VisitDiagnosis> _diagnoses = [];
    public IReadOnlyCollection<VisitDiagnosis> Diagnoses => _diagnoses.AsReadOnly();

    private readonly List<DryEyeAssessment> _dryEyeAssessments = [];
    public IReadOnlyCollection<DryEyeAssessment> DryEyeAssessments => _dryEyeAssessments.AsReadOnly();

    private readonly List<VisitAmendment> _amendments = [];
    public IReadOnlyCollection<VisitAmendment> Amendments => _amendments.AsReadOnly();

    private readonly List<DrugPrescription> _drugPrescriptions = [];
    public IReadOnlyCollection<DrugPrescription> DrugPrescriptions => _drugPrescriptions.AsReadOnly();

    private readonly List<OpticalPrescription> _opticalPrescriptions = [];
    public IReadOnlyCollection<OpticalPrescription> OpticalPrescriptions => _opticalPrescriptions.AsReadOnly();

    // New child entity collections for workflow
    private readonly List<ImagingRequest> _imagingRequests = [];
    public IReadOnlyCollection<ImagingRequest> ImagingRequests => _imagingRequests.AsReadOnly();

    private readonly List<StageSkip> _stageSkips = [];
    public IReadOnlyCollection<StageSkip> StageSkips => _stageSkips.AsReadOnly();

    private readonly List<VisitPayment> _visitPayments = [];
    public IReadOnlyCollection<VisitPayment> VisitPayments => _visitPayments.AsReadOnly();

    private readonly List<PharmacyDispensing> _pharmacyDispensings = [];
    public IReadOnlyCollection<PharmacyDispensing> PharmacyDispensings => _pharmacyDispensings.AsReadOnly();

    private readonly List<OpticalOrder> _opticalOrders = [];
    public IReadOnlyCollection<OpticalOrder> OpticalOrders => _opticalOrders.AsReadOnly();

    private readonly List<HandoffChecklist> _handoffChecklists = [];
    public IReadOnlyCollection<HandoffChecklist> HandoffChecklists => _handoffChecklists.AsReadOnly();

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
        visit.AddDomainEvent(new VisitCreatedEvent(
            visit.Id, patientId, patientName, doctorId, doctorName, branchId.Value));
        return visit;
    }

    /// <summary>
    /// Advances the visit to a new workflow stage.
    /// Validates branching logic for imaging loop, glasses flow, and parallel tracks.
    /// Enum values do NOT match flow order (e.g., OpticalCenter(8) -> Cashier(6) is valid).
    /// </summary>
    public void AdvanceStage(WorkflowStage newStage)
    {
        ValidateStageTransition(newStage);
        CurrentStage = newStage;
        SetUpdatedAt();
    }

    private void ValidateStageTransition(WorkflowStage newStage)
    {
        switch (CurrentStage)
        {
            case WorkflowStage.DoctorExam:
                // Branching: if imaging requested -> must go to Imaging, else -> Prescription
                if (ImagingRequested && newStage == WorkflowStage.Prescription)
                    throw new InvalidOperationException(
                        "Cannot skip to Prescription when imaging has been requested. Must go through Imaging -> DoctorReviewsResults first.");
                if (!ImagingRequested && newStage == WorkflowStage.Imaging)
                    throw new InvalidOperationException(
                        "Cannot advance to Imaging when no imaging has been requested.");
                if (newStage != WorkflowStage.Imaging && newStage != WorkflowStage.Prescription)
                    ValidateForwardProgression(newStage);
                break;

            case WorkflowStage.OpticalCenter:
                // OpticalCenter(8) -> Cashier(6) is a valid "backward" jump in enum values
                if (newStage == WorkflowStage.Cashier)
                    break; // Allow this specific transition
                ValidateForwardProgression(newStage);
                break;

            default:
                ValidateForwardProgression(newStage);
                break;
        }
    }

    private void ValidateForwardProgression(WorkflowStage newStage)
    {
        if (newStage <= CurrentStage)
            throw new InvalidOperationException(
                $"Cannot move to stage {newStage}. Current stage is {CurrentStage}.");
    }

    /// <summary>
    /// Allowed stage reversal transitions per D-07.
    /// Cashier(6) and Pharmacy(7) have NO entries, so they always fail.
    /// </summary>
    private static readonly Dictionary<WorkflowStage, HashSet<WorkflowStage>> AllowedReversals = new()
    {
        [WorkflowStage.RefractionVA] = [WorkflowStage.Reception],
        [WorkflowStage.DoctorExam] = [WorkflowStage.RefractionVA],
        [WorkflowStage.Imaging] = [WorkflowStage.DoctorExam],
        [WorkflowStage.DoctorReviewsResults] = [WorkflowStage.Imaging, WorkflowStage.DoctorExam],
        [WorkflowStage.Prescription] = [WorkflowStage.DoctorExam, WorkflowStage.DoctorReviewsResults],
    };

    private static bool IsReversalAllowed(WorkflowStage current, WorkflowStage target)
        => AllowedReversals.TryGetValue(current, out var allowed) && allowed.Contains(target);

    /// <summary>
    /// Reverses the visit to an earlier workflow stage with a mandatory reason.
    /// Only certain reversals are allowed per the AllowedReversals table (D-07).
    /// Cashier and post-Cashier stages cannot be reversed.
    /// </summary>
    public void ReverseStage(WorkflowStage targetStage, string reason)
    {
        if (targetStage >= CurrentStage)
            throw new InvalidOperationException("Target stage must be earlier than current stage.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("Reason is required for stage reversal.");
        if (!IsReversalAllowed(CurrentStage, targetStage))
            throw new InvalidOperationException($"Reversal from {CurrentStage} to {targetStage} is not allowed.");

        CurrentStage = targetStage;
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
    /// Cancels the visit. Only Draft visits can be cancelled.
    /// Raises VisitCancelledEvent for billing invoice voiding.
    /// </summary>
    public void Cancel()
    {
        if (Status != VisitStatus.Draft)
            throw new InvalidOperationException(
                "Only Draft visits can be cancelled.");

        Status = VisitStatus.Cancelled;
        SetUpdatedAt();
        AddDomainEvent(new VisitCancelledEvent(Id, BranchId.Value));
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
    /// Adds a dry eye assessment to the visit. Requires the visit to be editable.
    /// </summary>
    public void AddDryEyeAssessment(DryEyeAssessment assessment)
    {
        EnsureEditable();
        _dryEyeAssessments.Add(assessment);
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
    /// Sets the specified diagnosis as primary, demoting the current primary to secondary.
    /// Idempotent: if the target is already primary, no changes are made.
    /// Requires the visit to be editable.
    /// </summary>
    public void SetPrimaryDiagnosis(Guid diagnosisId)
    {
        EnsureEditable();
        var target = _diagnoses.FirstOrDefault(d => d.Id == diagnosisId)
            ?? throw new InvalidOperationException($"Diagnosis with ID {diagnosisId} not found.");
        if (target.Role == DiagnosisRole.Primary) return; // already primary

        // Demote current primary to secondary
        var currentPrimary = _diagnoses.FirstOrDefault(d => d.Role == DiagnosisRole.Primary);
        currentPrimary?.SetRole(DiagnosisRole.Secondary);

        // Promote target to primary
        target.SetRole(DiagnosisRole.Primary);
        target.SetSortOrder(0);

        // Re-sort remaining secondaries
        var sortOrder = 1;
        foreach (var d in _diagnoses.Where(d => d.Id != diagnosisId).OrderBy(d => d.SortOrder))
        {
            d.SetSortOrder(sortOrder++);
        }
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
    /// Adds a drug prescription to the visit. Requires the visit to be editable.
    /// Raises DrugPrescriptionAddedEvent for billing integration (prescribe -> pay -> dispense flow).
    /// </summary>
    public void AddDrugPrescription(DrugPrescription prescription)
    {
        EnsureEditable();
        _drugPrescriptions.Add(prescription);
        SetUpdatedAt();

        AddDomainEvent(new DrugPrescriptionAddedEvent(
            Id, PatientId, PatientName, BranchId.Value,
            prescription.Items.Select(i =>
                new DrugPrescriptionAddedEvent.PrescribedDrugDto(
                    i.DrugName, i.DrugCatalogItemId, i.Quantity)).ToList()));
    }

    /// <summary>
    /// Removes a drug prescription by ID. Requires the visit to be editable.
    /// </summary>
    public void RemoveDrugPrescription(Guid prescriptionId)
    {
        EnsureEditable();
        var rx = _drugPrescriptions.FirstOrDefault(p => p.Id == prescriptionId)
            ?? throw new InvalidOperationException($"Drug prescription {prescriptionId} not found.");

        var drugNames = rx.Items.Select(i => i.DrugName).ToList();
        _drugPrescriptions.Remove(rx);
        SetUpdatedAt();

        AddDomainEvent(new DrugPrescriptionRemovedEvent(Id, BranchId.Value, drugNames));
    }

    /// <summary>
    /// Sets the optical prescription for the visit. Only one allowed per visit.
    /// Clears any existing optical prescription first. Requires the visit to be editable.
    /// </summary>
    public void SetOpticalPrescription(OpticalPrescription prescription)
    {
        EnsureEditable();
        _opticalPrescriptions.Clear();
        _opticalPrescriptions.Add(prescription);
        SetUpdatedAt();
    }

    /// <summary>
    /// Starts an amendment workflow, allowing edits to a signed visit.
    /// Adds the amendment record to the visit's amendment chain.
    /// </summary>
    public void StartAmendment(VisitAmendment amendment)
    {
        if (Status != VisitStatus.Signed)
            throw new InvalidOperationException("Only signed visits can be amended.");

        Status = VisitStatus.Amended;
        _amendments.Add(amendment);
        SetUpdatedAt();
    }

    // ===================== Imaging Request =====================

    /// <summary>
    /// Requests imaging during DoctorExam stage. Sets ImagingRequested flag and creates an ImagingRequest entity.
    /// </summary>
    public void RequestImaging(Guid doctorId, string? note, List<string> serviceNames)
    {
        if (CurrentStage != WorkflowStage.DoctorExam)
            throw new InvalidOperationException("Imaging can only be requested during DoctorExam stage.");

        ImagingRequested = true;
        var request = ImagingRequest.Create(Id, doctorId, note, serviceNames);
        _imagingRequests.Add(request);
        SetUpdatedAt();
    }

    // ===================== Refraction Skip =====================

    /// <summary>
    /// Skips the RefractionVA stage with a mandatory reason.
    /// Creates a StageSkip audit entity.
    /// </summary>
    public void SkipRefraction(SkipReason reason, string? freeTextNote, Guid actorId, string actorName)
    {
        if (CurrentStage != WorkflowStage.RefractionVA)
            throw new InvalidOperationException("Can only skip refraction when at RefractionVA stage.");

        RefractionSkipped = true;
        var skip = StageSkip.Create(Id, WorkflowStage.RefractionVA, reason, freeTextNote, actorId, actorName);
        _stageSkips.Add(skip);
        SetUpdatedAt();
    }

    /// <summary>
    /// Undoes a refraction skip. Marks the latest StageSkip as undone.
    /// </summary>
    public void UndoRefractionSkip()
    {
        if (!RefractionSkipped)
            throw new InvalidOperationException("Refraction has not been skipped.");
        if (CurrentStage != WorkflowStage.RefractionVA)
            throw new InvalidOperationException("Can only undo refraction skip when at RefractionVA stage.");

        RefractionSkipped = false;
        var latestSkip = _stageSkips
            .Where(s => s.Stage == WorkflowStage.RefractionVA && !s.IsUndone)
            .OrderByDescending(s => s.SkippedAt)
            .FirstOrDefault();
        latestSkip?.MarkUndone();
        SetUpdatedAt();
    }

    // ===================== Post-Payment Tracks =====================

    /// <summary>
    /// Activates post-payment tracks after Cashier payment.
    /// Sets DrugTrackStatus and GlassesTrackStatus based on what was prescribed.
    /// </summary>
    public void ActivatePostPaymentTracks(bool hasDrugs, bool hasGlasses)
    {
        if (CurrentStage != WorkflowStage.Cashier)
            throw new InvalidOperationException("Can only activate post-payment tracks at Cashier stage.");

        DrugTrackStatus = hasDrugs ? TrackStatus.Pending : TrackStatus.NotApplicable;
        GlassesTrackStatus = hasGlasses ? TrackStatus.Pending : TrackStatus.NotApplicable;
        SetUpdatedAt();
    }

    /// <summary>
    /// Marks the drug dispensing track as completed.
    /// </summary>
    public void CompleteDrugTrack()
    {
        DrugTrackStatus = TrackStatus.Completed;
        SetUpdatedAt();
    }

    /// <summary>
    /// Marks the glasses processing track as completed.
    /// </summary>
    public void CompleteGlassesTrack()
    {
        GlassesTrackStatus = TrackStatus.Completed;
        SetUpdatedAt();
    }

    /// <summary>
    /// Returns true when all active tracks are completed (or not applicable).
    /// A visit is complete when there's nothing left to do post-payment.
    /// </summary>
    public bool IsComplete =>
        (DrugTrackStatus is TrackStatus.Completed or TrackStatus.NotApplicable) &&
        (GlassesTrackStatus is TrackStatus.Completed or TrackStatus.NotApplicable);

    // ===================== Workflow Child Entity Additions =====================

    /// <summary>
    /// Adds a payment record to the visit. Used by the ConfirmVisitPayment handler.
    /// </summary>
    public void AddVisitPayment(VisitPayment payment)
    {
        _visitPayments.Add(payment);
        SetUpdatedAt();
    }

    /// <summary>
    /// Adds a pharmacy dispensing record to the visit.
    /// </summary>
    public void AddPharmacyDispensing(PharmacyDispensing dispensing)
    {
        _pharmacyDispensings.Add(dispensing);
        SetUpdatedAt();
    }

    /// <summary>
    /// Adds an optical order to the visit.
    /// </summary>
    public void AddOpticalOrder(OpticalOrder order)
    {
        _opticalOrders.Add(order);
        SetUpdatedAt();
    }

    /// <summary>
    /// Adds a handoff checklist to the visit.
    /// </summary>
    public void AddHandoffChecklist(HandoffChecklist checklist)
    {
        _handoffChecklists.Add(checklist);
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets HasGlassesPrescription flag when an optical prescription is set.
    /// Used by SignOffVisit to determine post-prescription routing.
    /// </summary>
    public void SetHasGlassesPrescription(bool value)
    {
        HasGlassesPrescription = value;
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

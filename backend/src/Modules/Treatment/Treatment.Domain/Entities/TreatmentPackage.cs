using Shared.Domain;
using Treatment.Domain.Enums;
using Treatment.Domain.Events;
using Treatment.Domain.Models;

namespace Treatment.Domain.Entities;

/// <summary>
/// Aggregate root representing a per-patient treatment course.
/// Tracks sessions completed/remaining, enforces status transitions,
/// supports auto-completion (TRT-04), protocol versioning (TRT-07),
/// cancellation workflow (TRT-09), and refund calculation.
/// </summary>
public class TreatmentPackage : AggregateRoot, IAuditable
{
    // --- Backing fields ---

    private readonly List<TreatmentSession> _sessions = [];
    private readonly List<ProtocolVersion> _versions = [];
    private CancellationRequest? _cancellationRequest;

    // --- Properties ---

    /// <summary>FK to the TreatmentProtocol template this package was created from.</summary>
    public Guid ProtocolTemplateId { get; private set; }

    /// <summary>FK to the patient this treatment course belongs to.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>Denormalized patient name for display without cross-module query.</summary>
    public string PatientName { get; private set; } = string.Empty;

    /// <summary>Type of treatment (IPL, LLLT, LidCare).</summary>
    public TreatmentType TreatmentType { get; private set; }

    /// <summary>Current lifecycle status of the package.</summary>
    public PackageStatus Status { get; private set; }

    /// <summary>Total number of sessions in this package (1-6).</summary>
    public int TotalSessions { get; private set; }

    /// <summary>Pricing model: per-session or per-package.</summary>
    public PricingMode PricingMode { get; private set; }

    /// <summary>Total package price (used when PricingMode is PerPackage).</summary>
    public decimal PackagePrice { get; private set; }

    /// <summary>Price per individual session (used when PricingMode is PerSession).</summary>
    public decimal SessionPrice { get; private set; }

    /// <summary>Minimum number of days between consecutive sessions.</summary>
    public int MinIntervalDays { get; private set; }

    /// <summary>Default structured parameters from the template (JSON).</summary>
    public string ParametersJson { get; private set; } = string.Empty;

    /// <summary>Optional link to the originating clinical visit.</summary>
    public Guid? VisitId { get; private set; }

    /// <summary>Doctor who created this treatment package.</summary>
    public Guid CreatedById { get; private set; }

    /// <summary>Last user who updated this package.</summary>
    public Guid? UpdatedById { get; private set; }

    // --- Computed properties (NOT stored in DB) ---

    /// <summary>Read-only collection of sessions in this package.</summary>
    public IReadOnlyList<TreatmentSession> Sessions => _sessions.AsReadOnly();

    /// <summary>Read-only collection of protocol version snapshots.</summary>
    public IReadOnlyList<ProtocolVersion> Versions => _versions.AsReadOnly();

    /// <summary>Current cancellation request, if any.</summary>
    public CancellationRequest? CancellationRequest => _cancellationRequest;

    /// <summary>Number of completed sessions in this package.</summary>
    public int SessionsCompleted => _sessions.Count(s => s.Status == SessionStatus.Completed);

    /// <summary>Number of remaining sessions to complete the package.</summary>
    public int SessionsRemaining => TotalSessions - SessionsCompleted;

    /// <summary>Whether all sessions have been completed.</summary>
    public bool IsComplete => SessionsCompleted >= TotalSessions;

    // --- Constructors ---

    /// <summary>Private parameterless constructor required by EF Core for materialisation.</summary>
    private TreatmentPackage() { }

    // --- Factory method ---

    /// <summary>
    /// Creates a new treatment package for a patient. Status starts at Active.
    /// </summary>
    public static TreatmentPackage Create(
        Guid protocolTemplateId,
        Guid patientId,
        string patientName,
        TreatmentType treatmentType,
        int totalSessions,
        PricingMode pricingMode,
        decimal packagePrice,
        decimal sessionPrice,
        int minIntervalDays,
        string parametersJson,
        Guid? visitId,
        Guid createdById,
        BranchId branchId)
    {
        if (totalSessions < 1 || totalSessions > 6)
            throw new ArgumentOutOfRangeException(
                nameof(totalSessions), "Total sessions must be between 1 and 6.");

        var package = new TreatmentPackage
        {
            ProtocolTemplateId = protocolTemplateId,
            PatientId = patientId,
            PatientName = patientName,
            TreatmentType = treatmentType,
            Status = PackageStatus.Active,
            TotalSessions = totalSessions,
            PricingMode = pricingMode,
            PackagePrice = packagePrice,
            SessionPrice = sessionPrice,
            MinIntervalDays = minIntervalDays,
            ParametersJson = parametersJson,
            VisitId = visitId,
            CreatedById = createdById
        };

        package.SetBranchId(branchId);
        return package;
    }

    // --- Behaviour methods ---

    /// <summary>
    /// Records a completed treatment session within this package.
    /// If the package becomes complete after this session, auto-transitions to Completed status (TRT-04).
    /// Always raises TreatmentSessionCompletedEvent with consumable list for inventory deduction (TRT-11).
    /// </summary>
    /// <returns>The created TreatmentSession entity.</returns>
    public TreatmentSession RecordSession(
        string parametersJson,
        decimal? osdiScore,
        string? osdiSeverity,
        string? clinicalNotes,
        Guid performedById,
        Guid? visitId,
        DateTime? scheduledAt,
        string? intervalOverrideReason,
        List<(Guid ConsumableItemId, string ConsumableName, int Quantity)> consumables)
    {
        EnsureActive();

        if (SessionsCompleted >= TotalSessions)
            throw new InvalidOperationException(
                "Cannot record more sessions than the total allowed for this package.");

        // Enforce minimum interval between sessions (TRT-05)
        if (MinIntervalDays > 0)
        {
            var lastCompletedSession = _sessions
                .Where(s => s.Status != SessionStatus.Cancelled)
                .OrderByDescending(s => s.CompletedAt ?? s.ScheduledAt ?? s.CreatedAt)
                .FirstOrDefault();

            if (lastCompletedSession is not null)
            {
                var lastSessionDate = lastCompletedSession.CompletedAt
                    ?? lastCompletedSession.ScheduledAt
                    ?? lastCompletedSession.CreatedAt;
                var currentDate = scheduledAt ?? DateTime.UtcNow;
                var daysSinceLastSession = (currentDate - lastSessionDate).TotalDays;

                if (daysSinceLastSession < MinIntervalDays && string.IsNullOrWhiteSpace(intervalOverrideReason))
                    throw new InvalidOperationException(
                        $"Cannot record session: minimum interval of {MinIntervalDays} days between sessions has not been met " +
                        $"({daysSinceLastSession:F0} days since last session). Provide an interval override reason to proceed.");
            }
        }

        // Session numbering excludes cancelled sessions (TRT-02)
        var sessionNumber = _sessions.Count(s => s.Status != SessionStatus.Cancelled) + 1;

        var session = TreatmentSession.Create(
            treatmentPackageId: Id,
            sessionNumber: sessionNumber,
            parametersJson: parametersJson,
            osdiScore: osdiScore,
            osdiSeverity: osdiSeverity,
            clinicalNotes: clinicalNotes,
            performedById: performedById,
            visitId: visitId,
            scheduledAt: scheduledAt,
            intervalOverrideReason: intervalOverrideReason);

        // Add consumables to the session
        foreach (var (consumableItemId, consumableName, quantity) in consumables)
        {
            session.AddConsumable(consumableItemId, consumableName, quantity);
        }

        // Complete the session
        session.Complete();

        _sessions.Add(session);
        SetUpdatedAt();

        // Raise session completed event with consumable info for Pharmacy module (TRT-11)
        var consumableUsages = consumables
            .Select(c => new ConsumableUsageInfo(c.ConsumableItemId, c.Quantity))
            .ToList();

        // Compute session fee amount based on pricing mode
        var sessionFeeAmount = PricingMode switch
        {
            PricingMode.PerSession => SessionPrice,
            PricingMode.PerPackage => PackagePrice / TotalSessions,
            _ => 0m
        };

        AddDomainEvent(new TreatmentSessionCompletedEvent(
            PackageId: Id,
            SessionId: session.Id,
            PatientId: PatientId,
            PatientName: PatientName,
            TreatmentType: TreatmentType,
            Consumables: consumableUsages,
            VisitId: VisitId,
            SessionFeeAmount: sessionFeeAmount,
            BranchId: BranchId.Value));

        // Auto-complete package if all sessions are done (TRT-04)
        if (IsComplete)
        {
            Status = PackageStatus.Completed;
            AddDomainEvent(new TreatmentPackageCompletedEvent(
                PackageId: Id,
                PatientId: PatientId,
                TreatmentType: TreatmentType));
        }

        return session;
    }

    /// <summary>
    /// Modifies the treatment package mid-course (TRT-07).
    /// Creates a ProtocolVersion snapshot of the current state before applying changes.
    /// </summary>
    public void Modify(
        int? totalSessions,
        string? parametersJson,
        int? minIntervalDays,
        string changeDescription,
        Guid changedById,
        string reason)
    {
        EnsureModifiable();

        // Change detection: track whether any value actually changes
        var hasChanges = false;

        // Capture current state as JSON snapshot
        var previousJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            TotalSessions,
            ParametersJson,
            MinIntervalDays
        });

        // Apply changes
        if (totalSessions.HasValue && totalSessions.Value != TotalSessions)
        {
            if (totalSessions.Value < 1 || totalSessions.Value > 6)
                throw new ArgumentOutOfRangeException(
                    nameof(totalSessions), "Total sessions must be between 1 and 6.");

            if (totalSessions.Value < SessionsCompleted)
                throw new InvalidOperationException(
                    $"Cannot reduce total sessions to {totalSessions.Value} because {SessionsCompleted} sessions have already been completed.");

            TotalSessions = totalSessions.Value;
            hasChanges = true;
        }

        if (parametersJson is not null && parametersJson != ParametersJson)
        {
            ParametersJson = parametersJson;
            hasChanges = true;
        }

        if (minIntervalDays.HasValue && minIntervalDays.Value != MinIntervalDays)
        {
            MinIntervalDays = minIntervalDays.Value;
            hasChanges = true;
        }

        // Only create version snapshot and raise event if something actually changed
        if (hasChanges)
        {
            // Capture new state as JSON snapshot
            var currentJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                TotalSessions,
                ParametersJson,
                MinIntervalDays
            });

            // Create version snapshot
            var versionNumber = _versions.Count + 1;
            var version = ProtocolVersion.Create(
                treatmentPackageId: Id,
                versionNumber: versionNumber,
                previousJson: previousJson,
                currentJson: currentJson,
                changeDescription: changeDescription,
                changedById: changedById,
                reason: reason);

            _versions.Add(version);
            SetUpdatedAt();

            // Auto-complete package if TotalSessions now equals completed count (TRT-04)
            if (IsComplete && Status == PackageStatus.Active)
            {
                Status = PackageStatus.Completed;
                AddDomainEvent(new TreatmentPackageCompletedEvent(
                    PackageId: Id,
                    PatientId: PatientId,
                    TreatmentType: TreatmentType));
            }
        }
    }

    /// <summary>
    /// Pauses the treatment package. Only allowed from Active status.
    /// </summary>
    public void Pause()
    {
        EnsureActive();
        Status = PackageStatus.Paused;
        SetUpdatedAt();
    }

    /// <summary>
    /// Resumes a paused treatment package. Only allowed from Paused status.
    /// </summary>
    public void Resume()
    {
        if (Status != PackageStatus.Paused)
            throw new InvalidOperationException(
                $"Cannot resume a package in '{Status}' status. Only Paused packages can be resumed.");

        Status = PackageStatus.Active;
        SetUpdatedAt();
    }

    /// <summary>
    /// Requests cancellation of the treatment package (TRT-09).
    /// Creates a CancellationRequest and transitions status to PendingCancellation.
    /// </summary>
    public void RequestCancellation(
        string reason,
        decimal deductionPercent,
        Guid requestedById)
    {
        EnsureModifiable();

        // Validate deduction percentage is within allowed range (TRT-09: 10-20%)
        if (deductionPercent < 10 || deductionPercent > 20)
            throw new ArgumentOutOfRangeException(
                nameof(deductionPercent),
                $"Deduction percentage must be between 10% and 20%. Received: {deductionPercent}%.");

        if (_cancellationRequest is not null && _cancellationRequest.Status == CancellationRequestStatus.Requested)
            throw new InvalidOperationException(
                "A cancellation request is already pending for this package.");

        var refundAmount = CalculateRefundAmount(deductionPercent);

        _cancellationRequest = CancellationRequest.Create(
            treatmentPackageId: Id,
            reason: reason,
            deductionPercent: deductionPercent,
            refundAmount: refundAmount,
            requestedById: requestedById);

        Status = PackageStatus.PendingCancellation;
        SetUpdatedAt();
    }

    /// <summary>
    /// Approves the pending cancellation request. Transitions status to Cancelled.
    /// If a deduction override is provided, recalculates the refund amount before approving.
    /// </summary>
    public void ApproveCancellation(Guid processedById, string? note, decimal? deductionPercentOverride = null)
    {
        if (Status != PackageStatus.PendingCancellation)
            throw new InvalidOperationException(
                $"Cannot approve cancellation for a package in '{Status}' status.");

        if (_cancellationRequest is null)
            throw new InvalidOperationException(
                "No cancellation request found to approve.");

        // If manager provides a different deduction percentage, recalculate refund
        if (deductionPercentOverride.HasValue &&
            deductionPercentOverride.Value != _cancellationRequest.DeductionPercent)
        {
            var newRefund = CalculateRefundAmount(deductionPercentOverride.Value);
            _cancellationRequest.UpdateDeduction(deductionPercentOverride.Value, newRefund);
        }

        _cancellationRequest.Approve(processedById, note);
        Status = PackageStatus.Cancelled;
        SetUpdatedAt();
    }

    /// <summary>
    /// Rejects the pending cancellation request. Transitions status back to Active.
    /// </summary>
    public void RejectCancellation(Guid processedById, string? note)
    {
        if (Status != PackageStatus.PendingCancellation)
            throw new InvalidOperationException(
                $"Cannot reject cancellation for a package in '{Status}' status.");

        if (_cancellationRequest is null)
            throw new InvalidOperationException(
                "No cancellation request found to reject.");

        _cancellationRequest.Reject(processedById, note);
        Status = PackageStatus.Active;
        SetUpdatedAt();
    }

    /// <summary>
    /// Marks the package as switched (TRT-08).
    /// Used when patient switches from one treatment type to another mid-course.
    /// </summary>
    public void MarkAsSwitched()
    {
        EnsureModifiable();
        Status = PackageStatus.Switched;
        SetUpdatedAt();
    }

    /// <summary>
    /// Calculates the refund amount based on remaining sessions and deduction percentage.
    /// </summary>
    /// <param name="deductionPercent">Deduction percentage (0-100).</param>
    /// <returns>Calculated refund amount after deduction.</returns>
    public decimal CalculateRefundAmount(decimal deductionPercent)
    {
        if (deductionPercent < 0 || deductionPercent > 100)
            throw new ArgumentOutOfRangeException(
                nameof(deductionPercent), "Deduction percentage must be between 0 and 100.");

        var deductionFactor = 1 - deductionPercent / 100;

        return PricingMode switch
        {
            PricingMode.PerSession => SessionsRemaining * SessionPrice * deductionFactor,
            PricingMode.PerPackage => PackagePrice * SessionsRemaining / TotalSessions * deductionFactor,
            _ => throw new InvalidOperationException($"Unknown pricing mode: {PricingMode}")
        };
    }

    // --- Guard methods ---

    /// <summary>
    /// Guard: throws if the package is not in Active status.
    /// </summary>
    private void EnsureActive()
    {
        if (Status != PackageStatus.Active)
            throw new InvalidOperationException(
                $"Cannot perform this operation on a package in '{Status}' status. Package must be Active.");
    }

    /// <summary>
    /// Guard: throws if the package is not in a modifiable status (Active or Paused).
    /// </summary>
    private void EnsureModifiable()
    {
        if (Status != PackageStatus.Active && Status != PackageStatus.Paused)
            throw new InvalidOperationException(
                $"Cannot modify a package in '{Status}' status. Package must be Active or Paused.");
    }

    // --- EF Core ---

    /// <summary>Private parameterless constructor required by EF Core for materialisation.</summary>
    // Note: The private TreatmentPackage() constructor is defined above.
}

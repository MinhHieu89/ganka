using Treatment.Domain.Enums;

namespace Treatment.Domain.Entities;

/// <summary>
/// Represents a single treatment session within a TreatmentPackage.
/// Records device parameters, OSDI score (TRT-03), clinical notes,
/// and consumables used (TRT-11). Child entity of TreatmentPackage aggregate.
/// </summary>
public class TreatmentSession
{
    // --- Backing field ---

    private readonly List<SessionConsumable> _consumables = [];

    // --- Properties ---

    /// <summary>Unique identifier for this session.</summary>
    public Guid Id { get; private set; }

    /// <summary>FK to the parent TreatmentPackage.</summary>
    public Guid TreatmentPackageId { get; private set; }

    /// <summary>1-based session number within the package.</summary>
    public int SessionNumber { get; private set; }

    /// <summary>Current status of this session.</summary>
    public SessionStatus Status { get; private set; }

    /// <summary>Actual device parameters used during this session (JSON). May differ from template defaults.</summary>
    public string ParametersJson { get; private set; } = string.Empty;

    /// <summary>OSDI score recorded during or after the session (nullable).</summary>
    public decimal? OsdiScore { get; private set; }

    /// <summary>OSDI severity classification (e.g., "Normal", "Mild", "Moderate", "Severe").</summary>
    public string? OsdiSeverity { get; private set; }

    /// <summary>Freeform text observations from the clinician.</summary>
    public string? ClinicalNotes { get; private set; }

    /// <summary>Doctor or technician who performed this session.</summary>
    public Guid PerformedById { get; private set; }

    /// <summary>Optional link to a clinical visit.</summary>
    public Guid? VisitId { get; private set; }

    /// <summary>When the session was scheduled.</summary>
    public DateTime? ScheduledAt { get; private set; }

    /// <summary>When the session was completed.</summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>When the session record was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Reason for overriding the minimum interval between sessions, if applicable.</summary>
    public string? IntervalOverrideReason { get; private set; }

    // --- Computed properties ---

    /// <summary>Read-only collection of consumables used during this session.</summary>
    public IReadOnlyList<SessionConsumable> Consumables => _consumables.AsReadOnly();

    // --- Constructors ---

    /// <summary>Private parameterless constructor required by EF Core for materialisation.</summary>
    private TreatmentSession() { }

    // --- Factory method ---

    /// <summary>
    /// Creates a new treatment session. Status starts at Scheduled.
    /// </summary>
    public static TreatmentSession Create(
        Guid treatmentPackageId,
        int sessionNumber,
        string parametersJson,
        decimal? osdiScore,
        string? osdiSeverity,
        string? clinicalNotes,
        Guid performedById,
        Guid? visitId,
        DateTime? scheduledAt,
        string? intervalOverrideReason)
    {
        return new TreatmentSession
        {
            Id = Guid.NewGuid(),
            TreatmentPackageId = treatmentPackageId,
            SessionNumber = sessionNumber,
            Status = SessionStatus.Scheduled,
            ParametersJson = parametersJson,
            OsdiScore = osdiScore,
            OsdiSeverity = osdiSeverity,
            ClinicalNotes = clinicalNotes,
            PerformedById = performedById,
            VisitId = visitId,
            ScheduledAt = scheduledAt,
            IntervalOverrideReason = intervalOverrideReason,
            CreatedAt = DateTime.UtcNow
        };
    }

    // --- Behaviour methods ---

    /// <summary>
    /// Completes the session. Sets status to Completed and records completion timestamp.
    /// </summary>
    public void Complete()
    {
        if (Status == SessionStatus.Completed)
            throw new InvalidOperationException("Session is already completed.");

        if (Status == SessionStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled session.");

        Status = SessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a consumable item used during this session (TRT-11).
    /// </summary>
    public void AddConsumable(Guid consumableItemId, string consumableName, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity), "Consumable quantity must be greater than zero.");

        var consumable = SessionConsumable.Create(
            treatmentSessionId: Id,
            consumableItemId: consumableItemId,
            consumableName: consumableName,
            quantity: quantity);

        _consumables.Add(consumable);
    }
}

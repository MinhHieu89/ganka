using Shared.Domain;

namespace Treatment.Domain.Entities;

/// <summary>
/// Captures a snapshot of a TreatmentPackage's state before and after a mid-course modification.
/// Each modification produces a new ProtocolVersion, providing a full audit trail of changes (TRT-07).
/// </summary>
public class ProtocolVersion : Entity
{
    /// <summary>FK to the owning TreatmentPackage.</summary>
    public Guid TreatmentPackageId { get; private set; }

    /// <summary>Sequential version number within the package (1, 2, 3...).</summary>
    public int VersionNumber { get; private set; }

    /// <summary>JSON snapshot of the package state before the modification.</summary>
    public string PreviousJson { get; private set; } = default!;

    /// <summary>JSON snapshot of the package state after the modification.</summary>
    public string CurrentJson { get; private set; } = default!;

    /// <summary>Human-readable description of what changed, e.g., "Session count changed from 4 to 6".</summary>
    public string ChangeDescription { get; private set; } = default!;

    /// <summary>The doctor/staff member who made the change.</summary>
    public Guid ChangedById { get; private set; }

    /// <summary>Reason for the modification.</summary>
    public string Reason { get; private set; } = default!;

    private ProtocolVersion() { }

    /// <summary>
    /// Creates a new protocol version snapshot.
    /// </summary>
    /// <param name="treatmentPackageId">The owning package ID.</param>
    /// <param name="versionNumber">Sequential version number within the package.</param>
    /// <param name="previousJson">JSON snapshot of previous state.</param>
    /// <param name="currentJson">JSON snapshot of new state.</param>
    /// <param name="changeDescription">Human-readable change description.</param>
    /// <param name="changedById">ID of the staff member who made the change.</param>
    /// <param name="reason">Reason for the modification.</param>
    public static ProtocolVersion Create(
        Guid treatmentPackageId,
        int versionNumber,
        string previousJson,
        string currentJson,
        string changeDescription,
        Guid changedById,
        string reason)
    {
        return new ProtocolVersion
        {
            TreatmentPackageId = treatmentPackageId,
            VersionNumber = versionNumber,
            PreviousJson = previousJson,
            CurrentJson = currentJson,
            ChangeDescription = changeDescription,
            ChangedById = changedById,
            Reason = reason
        };
    }
}

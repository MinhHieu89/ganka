using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Handoff checklist completed at ReturnGlasses stage.
/// Verifies prescription accuracy, frame correctness, and patient fit confirmation.
/// </summary>
public class HandoffChecklist : Entity
{
    public Guid VisitId { get; private set; }
    public bool PrescriptionVerified { get; private set; }
    public bool FrameCorrect { get; private set; }
    public bool PatientConfirmedFit { get; private set; }
    public Guid CompletedById { get; private set; }
    public string CompletedByName { get; private set; } = string.Empty;
    public DateTime CompletedAt { get; private set; }

    private HandoffChecklist() { }

    public static HandoffChecklist Create(Guid visitId, bool prescriptionVerified,
        bool frameCorrect, bool patientConfirmedFit, Guid completedById, string completedByName)
    {
        return new HandoffChecklist
        {
            VisitId = visitId,
            PrescriptionVerified = prescriptionVerified,
            FrameCorrect = frameCorrect,
            PatientConfirmedFit = patientConfirmedFit,
            CompletedById = completedById,
            CompletedByName = completedByName,
            CompletedAt = DateTime.UtcNow
        };
    }
}

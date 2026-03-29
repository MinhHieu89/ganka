using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Records when a workflow stage is skipped (e.g., PreExam skip for follow-ups).
/// Captures the reason, actor, and supports undo.
/// </summary>
public class StageSkip : Entity
{
    public Guid VisitId { get; private set; }
    public WorkflowStage Stage { get; private set; }
    public SkipReason Reason { get; private set; }
    public string? FreeTextNote { get; private set; }
    public Guid ActorId { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    public DateTime SkippedAt { get; private set; }
    public bool IsUndone { get; private set; }

    private StageSkip() { }

    public static StageSkip Create(Guid visitId, WorkflowStage stage, SkipReason reason,
        string? freeTextNote, Guid actorId, string actorName)
    {
        if (freeTextNote is not null && freeTextNote.Length > 200)
            throw new InvalidOperationException("Free text note must be 200 characters or fewer.");

        return new StageSkip
        {
            VisitId = visitId,
            Stage = stage,
            Reason = reason,
            FreeTextNote = freeTextNote,
            ActorId = actorId,
            ActorName = actorName,
            SkippedAt = DateTime.UtcNow,
            IsUndone = false
        };
    }

    public void MarkUndone()
    {
        IsUndone = true;
        SetUpdatedAt();
    }
}

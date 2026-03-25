# Phase 13: Clinical Workflow Overhaul - Research

**Researched:** 2026-03-25
**Domain:** Clinical workflow management, kanban board, table views, visit lifecycle, patient history
**Confidence:** HIGH

## Summary

Phase 13 enhances the existing clinical workflow system across both frontend and backend. The codebase is mature and well-structured -- all major patterns are already established (CQRS with Wolverine, @dnd-kit kanban, React Query, shadcn/ui components, TanStack Router). The work splits into six domains: (1) expanding the kanban from 5 grouped columns to 9 individual columns (8 stages + Done), (2) adding a table view alternative, (3) enabling bidirectional stage transitions with audit trail, (4) auto-advancing workflow on doctor sign-off, (5) building a patient visit history tab, and (6) folding in 4 smaller todos (patient name links, OSDI answer viewing, optical Rx auto-expand, realtime OSDI updates).

The backend needs a new `ReverseWorkflowStage` command with reason and audit trail, modifications to the `Visit.AdvanceStage()` domain method to support backward movement, a new domain event for sign-off auto-advance, and a new `GetPatientVisitHistory` query endpoint. The frontend work is primarily component modification and creation using existing patterns and libraries.

**Primary recommendation:** Implement backend changes first (reverse stage, auto-advance on sign-off, visit history query), then frontend changes (kanban expansion, table view, reversal dialog, history tab, folded todos). All patterns already exist in the codebase -- no new libraries needed.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Each of the 8 workflow stages gets its own kanban column (1:1 mapping). No more grouping stages into combined columns.
- **D-02:** Column order: Reception, Refraction/VA, Doctor Exam, Diagnostics, Doctor Reads, Rx, Cashier, Pharmacy/Optical.
- **D-03:** Horizontal scroll for overflow -- all 8 columns at comfortable width, scroll to see off-screen columns (Trello/Jira pattern).
- **D-04:** A 9th "Done" column shows completed visits for the current day, then they disappear.
- **D-05:** Toggle button with grid/list icons in the dashboard toolbar header. Remember last selection per user via localStorage.
- **D-06:** Table view shows a full patient list with sortable columns: Patient, Doctor, Stage, Wait Time, Visit Time, Status. Sortable and filterable.
- **D-07:** Specific stages allow backward movement (not all stages). Claude's discretion on which backward transitions make clinical sense.
- **D-08:** Claude's discretion on role-based restrictions for who can reverse stages.
- **D-09:** Stage reversal requires a mandatory reason (audit trail), similar to how amendments require a reason.
- **D-10:** A visit is considered "done" when it reaches PharmacyOptical = 7. Shows in Done column for current day.
- **D-11:** Doctor sign-off automatically advances the visit to the next workflow stage. No manual stage advancement needed after sign-off.
- **D-12:** Staff can skip stages that aren't needed (e.g., jump from DoctorExam directly to Rx, skipping Diagnostics and DoctorReads).
- **D-13:** New "Visit History" tab added to the patient profile page.
- **D-14:** 2-column layout: vertical timeline on the left, visit details panel on the right.
- **D-15:** Timeline cards show compact summary: date, doctor name, primary diagnosis, visit status badge.
- **D-16:** Detail panel shows the same sections as existing VisitDetailPage in read-only mode.
- **D-17:** Patient name in visit kanban cards and visit detail page links to patient profile page.
- **D-18:** OSDI questionnaire answers visible in the visit detail page (currently only shows scores).
- **D-19:** Optical Prescription section auto-expands when data exists.
- **D-20:** Realtime OSDI score update on visit detail page when questionnaire is completed.

### Claude's Discretion
- Which specific backward stage transitions are allowed (based on clinical best practices research)
- Role-based restrictions for stage reversal
- Walk-in patient creation improvements (if any UX issues found during implementation)
- Table view filtering options and column widths
- Timeline card styling and spacing
- How to handle the Done column filtering (time-based or day-based cutoff)
- Loading states and error handling for all new features

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope.
- "Chart view for dry eye metrics across all visits" -- Already addressed by dry eye chart feature in prior phases
- "Print all pharmacy labels at once" -- Out of scope, belongs in pharmacy module improvements
- "Auto focus search field when opening Add Drug form" -- Minor UX fix, not related to workflow overhaul
</user_constraints>

## Project Constraints (from CLAUDE.md)

- **TDD Required:** Write failing tests first, then implement (red-green-refactor). At least 80% code coverage.
- **shadcn/ui First:** Use shadcn/ui components where applicable. Mantine as fallback with wrappers.
- **Free Libraries Only:** Must ask before using paid libraries.
- **Migrations Required:** When changing models, create and run migrations to update database schema.
- **UI-SPEC Exists:** A `13-UI-SPEC.md` design contract exists and must be followed for all visual/interaction decisions.
- **Bilingual:** All UI text must be in both Vietnamese and English (Vietnamese primary).
- **Context7 MCP:** Use for library documentation lookups.
- **DOC-01:** Must create Vietnamese user stories documentation for all features.

## Standard Stack

### Core (already in project)
| Library | Purpose | Why Standard |
|---------|---------|--------------|
| @dnd-kit/core + @dnd-kit/sortable | Kanban drag-and-drop | Already in use, supports constraints for backward-only drops |
| @tanstack/react-query v5 | Server state management | Already in use, query key factory pattern established |
| @tanstack/react-router | File-based routing | Already in use for all routes |
| shadcn/ui | UI components (Table, Tabs, ToggleGroup, Dialog, ScrollArea) | Project standard per CLAUDE.md |
| @tabler/icons-react | Icons | Project standard |
| react-i18next | Internationalization | Already in use for bilingual support |
| Wolverine | CQRS message bus (backend) | Already in use for all commands/queries |
| NSubstitute + FluentAssertions + xUnit | Backend testing | Already in use in Clinical.Unit.Tests |

### No New Libraries Needed
This phase requires no new dependencies. All required UI components (Table, ToggleGroup, ScrollArea) are available in shadcn/ui. The existing @dnd-kit setup handles all drag-and-drop scenarios including constrained drops.

## Architecture Patterns

### Existing Project Structure (relevant directories)
```
frontend/src/features/clinical/
  api/clinical-api.ts          # React Query hooks + API functions
  components/
    WorkflowDashboard.tsx      # Main kanban board (modify)
    KanbanColumn.tsx           # Column container (modify)
    PatientCard.tsx            # Draggable card (modify)
    SignOffSection.tsx          # Sign-off UI (auto-advance is backend)
    VisitDetailPage.tsx        # Visit detail (modify for D-17, D-18)
    OpticalPrescriptionSection.tsx  # Auto-expand (D-19)
    OsdiAnswersSection.tsx     # Already exists (D-18 already done)
    OsdiSection.tsx            # Already includes OsdiAnswersSection
  hooks/use-osdi-hub.ts       # SignalR for OSDI updates (D-20)

frontend/src/features/patient/components/
  PatientProfilePage.tsx       # Tab container (add Visit History tab)

backend/src/Modules/Clinical/
  Clinical.Domain/Entities/Visit.cs           # Aggregate root (modify AdvanceStage)
  Clinical.Domain/Enums/WorkflowStage.cs      # 8 stages enum (no change)
  Clinical.Domain/Enums/VisitStatus.cs        # 4 status enum (no change)
  Clinical.Application/Features/              # CQRS handlers (add new)
  Clinical.Application/Interfaces/            # Repository interfaces (extend)
  Clinical.Contracts/Dtos/                    # Commands & queries (add new)
  Clinical.Presentation/ClinicalApiEndpoints.cs  # Minimal API endpoints (add new)
```

### Pattern 1: CQRS Command/Query (Backend)
**What:** Record-based commands and queries dispatched via Wolverine message bus.
**When to use:** All backend operations.
**Example (existing pattern):**
```csharp
// Contract (in Clinical.Contracts/Dtos/)
public record ReverseWorkflowStageCommand(
    Guid VisitId,
    int TargetStage,
    string Reason);

// Handler (in Clinical.Application/Features/)
public static class ReverseWorkflowStageHandler
{
    public static async Task<Result> Handle(
        ReverseWorkflowStageCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct) { ... }
}

// Endpoint (in Clinical.Presentation/)
group.MapPut("/{visitId:guid}/reverse-stage", async (...) =>
{
    var result = await bus.InvokeAsync<Result>(enriched, ct);
    return result.ToHttpResult();
}).RequirePermissions(Permissions.Clinical.Update);
```

### Pattern 2: React Query Hook (Frontend)
**What:** Query key factory + hook wrapping API call.
**When to use:** All frontend data fetching and mutations.
**Example (existing pattern):**
```typescript
// In clinical-api.ts
export const clinicalKeys = {
  patientVisitHistory: (patientId: string) =>
    [...clinicalKeys.all, "patient-visit-history", patientId] as const,
}

export function usePatientVisitHistory(patientId: string | undefined) {
  return useQuery({
    queryKey: clinicalKeys.patientVisitHistory(patientId ?? ""),
    queryFn: () => getPatientVisitHistory(patientId!),
    enabled: !!patientId,
  })
}
```

### Pattern 3: Domain Method with Validation (Backend)
**What:** Business logic encapsulated in the aggregate root with guard clauses.
**When to use:** All state changes on Visit entity.
**Example (new method needed):**
```csharp
// In Visit.cs -- new method for backward transition
public void ReverseStage(WorkflowStage targetStage, string reason)
{
    if (targetStage >= CurrentStage)
        throw new InvalidOperationException("Target stage must be earlier than current stage.");
    if (!IsReversalAllowed(CurrentStage, targetStage))
        throw new InvalidOperationException($"Reversal from {CurrentStage} to {targetStage} is not allowed.");
    if (string.IsNullOrWhiteSpace(reason))
        throw new InvalidOperationException("Reason is required for stage reversal.");

    CurrentStage = targetStage;
    SetUpdatedAt();
    AddDomainEvent(new WorkflowStageReversedEvent(Id, targetStage, reason));
}
```

### Anti-Patterns to Avoid
- **Modifying AdvanceStage to handle both directions:** Keep forward (AdvanceStage) and backward (ReverseStage) as separate methods. They have different validation rules and audit requirements.
- **Filtering Done visits in frontend:** The Done column filter (today's completed visits) should be handled by the backend query, not by frontend filtering of the full active visits list.
- **Duplicating VisitDetailPage sections:** Reuse existing section components with a `readOnly` prop rather than creating separate read-only versions.

## Backward Stage Transitions (Claude's Discretion - D-07)

Based on ophthalmology clinic workflow analysis, the following backward transitions make clinical sense:

### Allowed Backward Transitions

| From Stage | Allowed Back To | Clinical Justification |
|------------|----------------|----------------------|
| RefractionVA (1) | Reception (0) | Patient arrived but needs to wait; wrong patient pulled |
| DoctorExam (2) | RefractionVA (1) | Doctor requests additional refraction measurements |
| Diagnostics (3) | DoctorExam (2) | Doctor needs to re-examine before ordering different tests |
| DoctorReads (4) | Diagnostics (3) | Additional diagnostic tests needed after initial review |
| DoctorReads (4) | DoctorExam (2) | Doctor wants to re-examine patient after seeing results |
| Rx (5) | DoctorExam (2) | Doctor reconsiders after starting to write prescription |
| Rx (5) | DoctorReads (4) | Need to review diagnostic results again before finalizing Rx |

### Never Allowed Backward Transitions

| From Stage | Cannot Go Back To | Reason |
|------------|------------------|--------|
| Cashier (6) | Any earlier stage | Payment may have been initiated; financial audit trail at risk |
| PharmacyOptical (7) | Any earlier stage | Drugs may have been dispensed; cannot undo physical actions |
| Any stage | More than 2 steps back (except to DoctorExam) | Typically indicates a process error, not a clinical need |

### Implementation

```csharp
private static readonly Dictionary<WorkflowStage, HashSet<WorkflowStage>> AllowedReversals = new()
{
    [WorkflowStage.RefractionVA] = [WorkflowStage.Reception],
    [WorkflowStage.DoctorExam] = [WorkflowStage.RefractionVA],
    [WorkflowStage.Diagnostics] = [WorkflowStage.DoctorExam],
    [WorkflowStage.DoctorReads] = [WorkflowStage.Diagnostics, WorkflowStage.DoctorExam],
    [WorkflowStage.Rx] = [WorkflowStage.DoctorExam, WorkflowStage.DoctorReads],
};

private static bool IsReversalAllowed(WorkflowStage current, WorkflowStage target)
    => AllowedReversals.TryGetValue(current, out var allowed) && allowed.Contains(target);
```

**Confidence: MEDIUM** -- Based on general ophthalmology workflow knowledge. Real clinic feedback may adjust these rules. The implementation is table-driven, so changes are trivial.

### Role-Based Restrictions (Claude's Discretion - D-08)

| Role | Can Reverse? | Rationale |
|------|-------------|-----------|
| Doctor | Yes (all allowed reversals) | Doctors have full clinical authority |
| Manager | Yes (all allowed reversals) | Managers oversee clinic operations |
| Technician | Yes, only RefractionVA -> Reception | Technicians handle early-stage workflow |
| Nurse | Yes, only RefractionVA -> Reception, DoctorExam -> RefractionVA | Nurses coordinate between technician and doctor |
| Cashier, Optical Staff, Accountant | No | These roles operate after clinical decisions are made |

**Implementation:** Use existing permission system. Add `Clinical.ReverseStage` permission. Assign to Doctor and Manager roles by default. Frontend hides reversal UI for roles without permission.

## Auto-Advance on Sign-Off (D-11)

### Design Decision: Backend Domain Event

The sign-off auto-advance should be a domain-level behavior, not a separate API call from the frontend.

**Approach:** When `Visit.SignOff()` is called and the visit is at DoctorExam (2), automatically advance to the next logical stage. The sign-off happens during the doctor's exam workflow, so:

- If at DoctorExam (2) and no diagnostics needed: advance to Rx (5)
- If at DoctorExam (2) with diagnostics pending: advance to Diagnostics (3)
- If at DoctorReads (4): advance to Rx (5)
- If at any other stage: advance to next sequential stage

**Simpler approach (recommended):** Always advance to the next sequential stage (`CurrentStage + 1`). The frontend already allows stage skipping (D-12), so if the doctor signs off at DoctorExam, the visit goes to Diagnostics. Staff can then skip forward if diagnostics aren't needed.

**Implementation in SignOffVisitHandler:**
```csharp
// After successful sign-off, auto-advance to next stage
if (visit.CurrentStage < WorkflowStage.PharmacyOptical)
{
    var nextStage = (WorkflowStage)(visit.CurrentStage + 1);
    visit.AdvanceStage(nextStage);
}
```

**Confidence: HIGH** -- This is a simple sequential advance matching the existing AdvanceStage pattern.

## Done Column Filtering (D-04, D-10)

### Backend Query Change

Currently `GetActiveVisitsAsync` returns Draft/Amended visits or Signed within 24 hours. Change to:

1. **Active visits:** All visits where `CurrentStage < PharmacyOptical` AND status is Draft or Amended
2. **Done visits (today):** Visits where `CurrentStage == PharmacyOptical` AND `VisitDate` is today (UTC date match)

The `ActiveVisitDto` already has `currentStage`, so the frontend can separate them into the Done column. But the backend should also return these "done today" visits in the active visits query.

**Recommended approach:** Add a `bool IsCompleted` field to `ActiveVisitDto` so the frontend can distinguish without stage-number logic.

## Patient Visit History (D-13 through D-16)

### New Backend Endpoint

```
GET /api/clinical/patients/{patientId}/visit-history
```

Returns a list of visit summaries ordered by date descending:

```csharp
public record PatientVisitHistoryDto(
    Guid VisitId,
    DateTime VisitDate,
    string DoctorName,
    int Status,
    string? PrimaryDiagnosisText,  // First diagnosis descriptionVi or descriptionEn
    int CurrentStage);
```

The detail panel reuses the existing `GET /api/clinical/{visitId}` endpoint which already returns `VisitDetailDto` with all sections.

### New Repository Method

```csharp
Task<List<Visit>> GetVisitsByPatientIdAsync(Guid patientId, CancellationToken ct = default);
```

### Frontend Architecture

- New `VisitHistoryTab` component with 2-column layout
- Left: `VisitTimeline` component with `VisitTimelineCard` items
- Right: Reuse existing section components (RefractionSection, DryEyeSection, DiagnosisSection, etc.) with `readOnly` mode
- Selected visit ID stored in component state
- Detail fetched via existing `useVisitById` hook

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Table sorting/filtering | Custom sort logic | shadcn Table + useMemo sort | Consistent with existing table patterns in the project |
| Drag-and-drop constraints | Custom drag validation | @dnd-kit collision detection + onDragEnd guard | Already handles this pattern in WorkflowDashboard |
| View persistence | Custom storage wrapper | Direct localStorage with JSON | Simple key-value, no need for abstraction |
| Timeline layout | CSS grid from scratch | Flexbox with fixed-width left panel | Same pattern as existing 2-column layouts in project |
| Audit trail for reversals | Separate audit table | WorkflowStageReversedEvent domain event + existing audit infrastructure | Audit module already captures all domain events via AUD-01 |

## Common Pitfalls

### Pitfall 1: Drag-and-drop backward validation timing
**What goes wrong:** Card drops on a non-allowed backward column and the mutation fires before validation.
**Why it happens:** @dnd-kit fires onDragEnd with the target column; validation must happen before calling the mutation.
**How to avoid:** In `handleDragEnd`, check if the target column is a backward move. If so, open the StageReversalDialog instead of immediately mutating. Only call the reverse mutation after the dialog confirms.
**Warning signs:** Visits appearing in wrong columns momentarily, then snapping back.

### Pitfall 2: Sign-off auto-advance race condition
**What goes wrong:** Frontend invalidates visit query after sign-off, but the auto-advance hasn't committed yet.
**Why it happens:** The sign-off and auto-advance happen in the same SaveChanges call, so this should not be an issue. But if they're separate calls, race conditions occur.
**How to avoid:** Keep sign-off + auto-advance in a single transaction (single `SaveChangesAsync` call in `SignOffVisitHandler`). The frontend just invalidates queries after the response.
**Warning signs:** Visit showing as signed but still at the old stage.

### Pitfall 3: Done column showing yesterday's visits
**What goes wrong:** Visits completed late in the day (UTC) appear in the wrong day's Done column.
**Why it happens:** UTC date comparison vs. local timezone (Vietnam, UTC+7).
**How to avoid:** Filter Done column by visit date in the clinic's timezone (UTC+7), not UTC. The backend should accept a timezone offset or use a configured clinic timezone.
**Warning signs:** Visits disappearing from Done at 5 PM instead of midnight local time.

### Pitfall 4: Visit history tab loading all visit details upfront
**What goes wrong:** Loading full visit details for every visit in history causes slow page load.
**Why it happens:** Eager loading all sections for all visits.
**How to avoid:** Timeline loads only summary data (PatientVisitHistoryDto). Detail panel loads full VisitDetailDto only for the selected visit, using the existing `useVisitById` hook with the selected visit ID.
**Warning signs:** Long loading time when opening visit history tab for patients with many visits.

### Pitfall 5: AdvanceStage domain method blocks backward transitions
**What goes wrong:** Existing `Visit.AdvanceStage()` throws when `newStage <= CurrentStage`.
**Why it happens:** The guard clause on line 87 of Visit.cs: `if (newStage <= CurrentStage) throw`.
**How to avoid:** Add a separate `ReverseStage()` method rather than modifying `AdvanceStage()`. This keeps forward and backward logic separate and maintains existing test coverage.
**Warning signs:** 500 errors when attempting stage reversal through the existing advance endpoint.

### Pitfall 6: PatientCard click handler conflicts with drag
**What goes wrong:** Clicking the patient name link triggers both navigation and drag start.
**Why it happens:** The click event on the Link fires after the pointer sensor threshold (8px) is not met.
**How to avoid:** The existing PointerSensor with `activationConstraint: { distance: 8 }` already handles this -- a simple click (< 8px movement) fires the click event, not drag. The patient name Link should use `e.stopPropagation()` to prevent the card click handler from also firing.
**Warning signs:** Clicking patient name navigates to visit detail instead of patient profile.

## Code Examples

### Kanban Column Configuration (Updated for 9 columns)
```typescript
// Source: Existing WorkflowDashboard.tsx pattern, expanded per D-01/D-02
const KANBAN_COLUMNS: ColumnDef[] = [
  { id: "reception", titleKey: "workflow.stages.reception", stages: [0], colorAccent: "stone" },
  { id: "refraction-va", titleKey: "workflow.stages.refractionVa", stages: [1], colorAccent: "blue" },
  { id: "doctor-exam", titleKey: "workflow.stages.doctorExam", stages: [2], colorAccent: "emerald" },
  { id: "diagnostics", titleKey: "workflow.stages.diagnostics", stages: [3], colorAccent: "cyan" },
  { id: "doctor-reads", titleKey: "workflow.stages.doctorReads", stages: [4], colorAccent: "teal" },
  { id: "rx", titleKey: "workflow.stages.rx", stages: [5], colorAccent: "amber" },
  { id: "cashier", titleKey: "workflow.stages.cashier", stages: [6], colorAccent: "orange" },
  { id: "pharmacy-optical", titleKey: "workflow.stages.pharmacyOptical", stages: [7], colorAccent: "violet" },
  { id: "done", titleKey: "workflow.done", stages: [], colorAccent: "muted" },
]
```

### Stage Reversal Dialog Trigger (in handleDragEnd)
```typescript
const handleDragEnd = useCallback((event: DragEndEvent) => {
  setActiveCard(null)
  const { active, over } = event
  if (!over) return

  const visitId = active.id as string
  const targetColumnId = resolveTargetColumn(over.id as string)
  if (!targetColumnId) return

  const sourceColumnId = findVisitColumn(visitId)
  if (sourceColumnId === targetColumnId) return

  const targetCol = getColumnDef(targetColumnId)
  if (!targetCol) return
  const newStage = targetCol.stages[0]
  const visit = activeVisits?.find(v => v.id === visitId)
  if (!visit) return

  if (newStage < visit.currentStage) {
    // Backward: open reversal dialog
    setReversalInfo({ visitId, currentStage: visit.currentStage, targetStage: newStage })
  } else {
    // Forward: advance directly
    advanceStageMutation.mutate({ visitId, newStage })
  }
}, [activeVisits, findVisitColumn, advanceStageMutation])
```

### Visit History Timeline Card
```typescript
// Source: UI-SPEC.md pattern for VisitTimelineCard
interface VisitTimelineCardProps {
  visit: PatientVisitHistoryDto
  isSelected: boolean
  onClick: () => void
}

function VisitTimelineCard({ visit, isSelected, onClick }: VisitTimelineCardProps) {
  return (
    <Card
      className={cn(
        "cursor-pointer p-3 space-y-1",
        isSelected && "ring-2 ring-primary"
      )}
      onClick={onClick}
    >
      <div className="flex items-center justify-between">
        <span className="text-sm font-semibold">
          {new Date(visit.visitDate).toLocaleDateString()}
        </span>
        <Badge variant={statusVariant(visit.status)}>
          {statusLabel(visit.status)}
        </Badge>
      </div>
      <p className="text-xs text-muted-foreground">{visit.doctorName}</p>
      {visit.primaryDiagnosisText && (
        <p className="text-xs truncate">{visit.primaryDiagnosisText}</p>
      )}
    </Card>
  )
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| 5 grouped kanban columns | 8+1 individual columns | This phase | 1:1 stage-to-column mapping for clarity |
| Forward-only stage progression | Bidirectional with audit | This phase | Clinical flexibility with accountability |
| Manual advance after sign-off | Auto-advance on sign-off | This phase | Reduces manual clicks, fewer stuck visits |
| No visit history in patient profile | Dedicated Visit History tab | This phase | Complete patient journey view |

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit + FluentAssertions + NSubstitute |
| Config file | `backend/tests/Clinical.Unit.Tests/Clinical.Unit.Tests.csproj` |
| Quick run command | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~ReverseWorkflow\|ClassName~SignOff\|ClassName~VisitHistory" --no-build` |
| Full suite command | `dotnet test backend/tests/Clinical.Unit.Tests` |

### Phase Requirements to Test Map
| Behavior | Test Type | Automated Command | File Exists? |
|----------|-----------|-------------------|-------------|
| Stage reversal: allowed transitions succeed | unit | `dotnet test --filter "ClassName~ReverseWorkflowStageHandlerTests"` | Wave 0 |
| Stage reversal: blocked transitions fail | unit | `dotnet test --filter "ClassName~ReverseWorkflowStageHandlerTests"` | Wave 0 |
| Stage reversal: reason required | unit | `dotnet test --filter "ClassName~ReverseWorkflowStageHandlerTests"` | Wave 0 |
| Sign-off auto-advance: next stage set | unit | `dotnet test --filter "ClassName~SignOffVisitHandlerTests"` | Modify existing |
| Sign-off auto-advance: at last stage, no advance | unit | `dotnet test --filter "ClassName~SignOffVisitHandlerTests"` | Modify existing |
| Visit history query: returns visits for patient | unit | `dotnet test --filter "ClassName~GetPatientVisitHistoryHandlerTests"` | Wave 0 |
| Visit history query: ordered by date desc | unit | `dotnet test --filter "ClassName~GetPatientVisitHistoryHandlerTests"` | Wave 0 |
| Domain: ReverseStage validates allowed transitions | unit | `dotnet test --filter "ClassName~VisitDomainTests"` | Wave 0 |
| Domain: ReverseStage requires reason | unit | `dotnet test --filter "ClassName~VisitDomainTests"` | Wave 0 |
| Active visits query: includes done-today visits | unit | `dotnet test --filter "ClassName~GetActiveVisitsHandlerTests"` | Modify existing |

### Sampling Rate
- **Per task commit:** `dotnet test backend/tests/Clinical.Unit.Tests --no-build -x`
- **Per wave merge:** `dotnet test backend/tests/Clinical.Unit.Tests`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `backend/tests/Clinical.Unit.Tests/Features/ReverseWorkflowStageHandlerTests.cs` -- covers stage reversal handler
- [ ] `backend/tests/Clinical.Unit.Tests/Features/GetPatientVisitHistoryHandlerTests.cs` -- covers visit history query
- [ ] `backend/tests/Clinical.Unit.Tests/Domain/VisitReverseStageTests.cs` -- covers domain-level reversal validation

## Open Questions

1. **Timezone handling for Done column**
   - What we know: Clinic is in Vietnam (UTC+7). VisitDate is stored as UTC DateTime.
   - What's unclear: Whether to filter by UTC date or local date for "today's done visits."
   - Recommendation: Filter by Vietnam timezone (UTC+7). Store clinic timezone in configuration. Convert VisitDate to local before date comparison.

2. **Existing OSDI answers display (D-18)**
   - What we know: `OsdiAnswersSection` component already exists and is already included in `OsdiSection.tsx` (line 145). The existing component already has a collapsible toggle.
   - What's unclear: Whether D-18 is already fully implemented or needs additional work.
   - Recommendation: Verify existing implementation during planning. May already be complete.

3. **Realtime OSDI update (D-20)**
   - What we know: `useOsdiHub` hook already exists and is used in `VisitDetailPage.tsx` for SignalR-based updates.
   - What's unclear: Whether the existing implementation already handles score updates on the visit detail page.
   - Recommendation: Verify existing implementation. The hook already invalidates the visit query, which should update OSDI data.

## Sources

### Primary (HIGH confidence)
- Existing codebase: `Visit.cs`, `WorkflowDashboard.tsx`, `clinical-api.ts`, `PatientProfilePage.tsx`, `SignOffSection.tsx`, `ClinicalApiEndpoints.cs`
- Existing tests: `AdvanceWorkflowStageHandlerTests.cs`
- UI-SPEC: `13-UI-SPEC.md`
- CONTEXT: `13-CONTEXT.md`

### Secondary (MEDIUM confidence)
- [AAO - 5 Tips for Improving Workflows](https://www.aao.org/young-ophthalmologists/yo-info/article/5-tips-improving-workflows-clinical-practice) -- General ophthalmology workflow patterns
- [IHE Eye Care Workflow](https://wiki.ihe.net/index.php/Eye_Care_Workflow) -- Standard eye care workflow stages

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- All libraries already in use, no new dependencies
- Architecture: HIGH -- All patterns already established in codebase, this is extension of existing work
- Pitfalls: HIGH -- Most pitfalls identified from direct code analysis
- Backward transitions: MEDIUM -- Based on clinical domain knowledge, may need adjustment from real clinic feedback

**Research date:** 2026-03-25
**Valid until:** 2026-04-25 (stable -- all patterns are established, no fast-moving dependencies)

# Phase 3: Clinical Workflow & Examination - Research

**Researched:** 2026-03-04
**Domain:** Clinical visit lifecycle, ophthalmology refraction data, ICD-10 diagnosis, real-time workflow dashboard
**Confidence:** HIGH

## Summary

Phase 3 builds the clinical visit core: visit creation (from appointment check-in or walk-in), a Kanban workflow dashboard, refraction data recording with OD/OS layout, ICD-10 diagnosis with laterality enforcement, visit sign-off with immutability, and amendment workflow. The existing Clinical module scaffold (ClinicalDbContext with "clinical" schema) provides the starting point, and established codebase patterns (AggregateRoot + IAuditable, Wolverine handlers, Repository + UnitOfWork, React Hook Form + Zod, TanStack Query, Popover/Command combobox) dictate exactly how to build this.

The biggest technical challenges are: (1) the Kanban board with drag-and-drop requiring a new library (@dnd-kit), (2) the ICD-10 per-doctor favorites requiring a separate junction table (the current `Icd10Code.IsFavorite` is a global boolean, not per-user), (3) the amendment workflow requiring field-level diff capture with an immutable audit chain, and (4) the real-time dashboard update mechanism.

**Primary recommendation:** Follow all established module patterns exactly. Use @dnd-kit/core + @dnd-kit/sortable for Kanban drag-and-drop. Use polling (not SignalR) for dashboard updates in v1. Create a DoctorIcd10Favorite junction table to replace the global IsFavorite field. Model amendments as a linked list of VisitAmendment entities pointing to the parent Visit.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Kanban board layout with columns for each workflow stage
- Patient cards show: name, appointment time, assigned doctor, wait time, allergy warning icon
- Click card to open visit details
- Drag-and-drop on desktop + action button on each card for advancing stages
- Visit created from appointment check-in: staff clicks "Check in" on confirmed appointment, creates Visit record, moves patient to Reception on Kanban
- Walk-in visits: staff creates manually via "New Visit" button (select existing patient or register new)
- Visit detail page is a single scrollable page with collapsible card sections (not tabbed), like a medical chart
- Sections: Patient Info, Refraction, Examination Notes (free-text), Diagnosis, Sign-off
- "Sign Off Visit" button at bottom of visit page with confirmation dialog
- After sign-off, all fields become read-only
- Amendment: Doctor clicks "Amend" on signed visit, fields become editable, must enter mandatory reason, system auto-captures field-level diff
- Side-by-side OD/OS layout for refraction
- Tabs per refraction type: Manifest | Autorefraction | Cycloplegic
- Each tab has its own OD/OS side-by-side form
- Only filled tabs get saved, indicator (*) on tabs that have data
- Fields per eye: SPH, CYL, AXIS, ADD, PD
- Visual Acuity: decimal notation (0.1 to 2.0), Vietnamese standard
- Both UCVA and BCVA recorded per eye
- IOP per eye with method notation
- Axial Length per eye
- ICD-10 combobox with Popover/Command pattern (same as allergy selector)
- Search by code or description in Vietnamese/English
- Multiple diagnoses per visit
- Per-doctor favorites with star icon toggle, pinned codes at top
- Laterality enforcement: auto-prompt OD/OS/OU, OU adds two records (.1 + .2), block .9
- Primary/secondary diagnosis designation

### Claude's Discretion
- Kanban stage grouping (all 8 vs grouped 4-5)
- Examination notes section structure (free-text vs semi-structured)
- Exact refraction input validation ranges
- IOP measurement method options (Goldmann, Non-contact, etc.)
- Loading states and error handling
- Kanban real-time update mechanism

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| CLN-01 | Doctor can create electronic visit record linked to patient and doctor, immutable after sign-off | Visit entity as AggregateRoot + IAuditable, IsSigned boolean, sign-off domain method that locks record |
| CLN-02 | Corrections to signed visits create amendment records with reason, field-level changes, original preserved | VisitAmendment entity with JSON diff, mandatory reason field, linked list pattern |
| CLN-03 | Staff can track visit workflow status through 8 stages | WorkflowStage enum, Visit.CurrentStage property, AdvanceStage/SetStage domain methods |
| CLN-04 | Dashboard shows all active patients and current workflow stage in real-time | Kanban board with @dnd-kit, polling-based refresh every 5-10 seconds |
| REF-01 | Record refraction data: SPH, CYL, AXIS, ADD, PD per eye | Refraction entity owned by Visit, RefractionType enum, per-eye value objects |
| REF-02 | Record VA (with/without correction), IOP (with method/time), Axial Length per eye | Fields on Refraction entity, IOP method enum, decimal VA values |
| REF-03 | Support manifest, autorefraction, and cycloplegic refraction types | RefractionType enum + tabs in frontend, multiple Refraction records per Visit |
| DX-01 | Search and select ICD-10 codes in Vietnamese and English with favorites | Popover/Command combobox pattern, DoctorIcd10Favorite junction table, search endpoint on ReferenceDbContext |
| DX-02 | Enforce ICD-10 laterality for ophthalmology codes (no unspecified eye) | Frontend laterality selector when RequiresLaterality=true, backend validation blocks .9 codes |
</phase_requirements>

## Standard Stack

### Core (Backend - already in project)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET 10 / ASP.NET Core | 10.0.* | Web framework | Already in use |
| EF Core | 10.0.* | ORM with ClinicalDbContext | Already scaffolded |
| WolverineFx | 5.* | Message bus, handler discovery | Already configured with Clinical.Application.Marker |
| FluentValidation | 12.* | Command validation | Established pattern |
| xUnit + NSubstitute + FluentAssertions | Latest | Unit testing | TDD per CLAUDE.md |

### Core (Frontend - already in project)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| React | 19.x | UI framework | Already in use |
| TanStack Router | 1.114.x | File-based routing | Already in use |
| TanStack Query | 5.x | Server state management | Already in use |
| TanStack Table | 8.x | Table/data grid | Already in use |
| React Hook Form + Zod | 7.x / 3.x | Form validation | Already in use |
| shadcn/ui | Latest | Component library | Already in use |
| @tabler/icons-react | 3.x | Icons | Already in use |
| i18next | 24.x | Internationalization | Already in use |
| openapi-fetch | 0.13.x | Typed API client | Already in use |

### New Dependencies (this phase)
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| @dnd-kit/core | ^6.1 | Drag-and-drop primitives | Kanban board drag interactions |
| @dnd-kit/sortable | ^8.0 | Sortable list preset | Moving cards between columns |
| @dnd-kit/utilities | ^3.2 | CSS transform helpers | Visual drag feedback |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| @dnd-kit | @hello-pangea/dnd | Higher-level API but less flexible; @dnd-kit better fits custom Kanban with shadcn Cards |
| @dnd-kit | @dnd-kit/react (0.3.x) | New API, pre-1.0, less documentation; legacy @dnd-kit/core is proven |
| Polling | SignalR | SignalR is real-time but adds significant complexity; 2-doctor clinic doesn't need sub-second updates |

**Installation (frontend):**
```bash
npm install @dnd-kit/core @dnd-kit/sortable @dnd-kit/utilities
```

**No new backend NuGet packages needed** -- all required packages already in Directory.Packages.props.

## Architecture Patterns

### Recommended Project Structure

**Backend:**
```
backend/src/Modules/Clinical/
  Clinical.Domain/
    Entities/
      Visit.cs               # AggregateRoot - visit record
      VisitAmendment.cs       # Entity - amendment chain
      Refraction.cs           # Entity - refraction data per type
      VisitDiagnosis.cs       # Entity - ICD-10 diagnosis per visit
    Enums/
      WorkflowStage.cs        # 8-stage workflow enum
      VisitStatus.cs          # Draft, Signed, Amended
      RefractionType.cs       # Manifest, Autorefraction, Cycloplegic
      Laterality.cs           # OD, OS, OU
      IopMethod.cs            # Goldmann, NonContact, etc.
    ValueObjects/
      EyeRefraction.cs        # SPH, CYL, AXIS, ADD, PD per eye
      VisualAcuity.cs         # UCVA + BCVA per eye
      IntraocularPressure.cs  # IOP value + method per eye
  Clinical.Application/
    Features/
      CreateVisit.cs          # From check-in or walk-in
      GetVisitById.cs
      UpdateVisitRefraction.cs
      UpdateVisitNotes.cs
      AddVisitDiagnosis.cs
      RemoveVisitDiagnosis.cs
      SignOffVisit.cs
      AmendVisit.cs
      AdvanceWorkflowStage.cs
      GetActiveVisits.cs      # Dashboard query
      SearchIcd10Codes.cs     # ICD-10 search endpoint
      ToggleIcd10Favorite.cs  # Per-doctor favorite
      GetDoctorFavorites.cs
    Interfaces/
      IVisitRepository.cs
      IVisitAmendmentRepository.cs
      IDoctorIcd10FavoriteRepository.cs
      IUnitOfWork.cs
    IoC.cs
    Marker.cs (existing)
  Clinical.Contracts/
    Dtos/
      VisitDto.cs
      VisitDetailDto.cs
      RefractionDto.cs
      VisitDiagnosisDto.cs
      ActiveVisitDto.cs       # Kanban card data
      Icd10SearchResultDto.cs
  Clinical.Infrastructure/
    ClinicalDbContext.cs (existing - add DbSets)
    Configurations/
      VisitConfiguration.cs
      VisitAmendmentConfiguration.cs
      RefractionConfiguration.cs
      VisitDiagnosisConfiguration.cs
      DoctorIcd10FavoriteConfiguration.cs
    Repositories/
      VisitRepository.cs
      VisitAmendmentRepository.cs
      DoctorIcd10FavoriteRepository.cs
    UnitOfWork.cs
    IoC.cs
    Migrations/
  Clinical.Presentation/
    ClinicalApiEndpoints.cs
    IoC.cs
```

**Frontend:**
```
frontend/src/
  features/clinical/
    api/
      clinical-api.ts         # API functions + TanStack Query hooks
    components/
      WorkflowDashboard.tsx    # Main Kanban board page
      KanbanColumn.tsx         # Single workflow stage column
      PatientCard.tsx          # Draggable patient card
      VisitDetailPage.tsx      # Scrollable visit page
      PatientInfoSection.tsx   # Collapsible patient info card
      RefractionSection.tsx    # OD/OS side-by-side + tabs
      RefractionForm.tsx       # Per-type refraction input form
      ExaminationNotesSection.tsx  # Free-text notes
      DiagnosisSection.tsx     # ICD-10 diagnosis list
      Icd10Combobox.tsx        # Popover/Command ICD-10 selector
      SignOffSection.tsx       # Sign-off button + dialog
      AmendmentDialog.tsx      # Amendment reason input
      VisitAmendmentHistory.tsx # Amendment trail display
    hooks/
      useVisits.ts             # TanStack Query hooks
  app/routes/_authenticated/
    clinical/
      index.tsx                # Workflow dashboard route
    visits/
      $visitId.tsx             # Visit detail route
  public/locales/
    en/clinical.json
    vi/clinical.json
```

### Pattern 1: Visit Aggregate Root with Immutability

**What:** Visit as an AggregateRoot that transitions through workflow stages and becomes immutable after sign-off.
**When to use:** Core pattern for the entire visit lifecycle.

```csharp
// Clinical.Domain/Entities/Visit.cs
public class Visit : AggregateRoot, IAuditable
{
    public Guid PatientId { get; private set; }
    public string PatientName { get; private set; } = string.Empty;
    public Guid DoctorId { get; private set; }
    public string DoctorName { get; private set; } = string.Empty;
    public Guid? AppointmentId { get; private set; }  // null for walk-ins
    public WorkflowStage CurrentStage { get; private set; }
    public VisitStatus Status { get; private set; }
    public DateTime VisitDate { get; private set; }
    public string? ExaminationNotes { get; private set; }
    public DateTime? SignedAt { get; private set; }
    public Guid? SignedById { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private readonly List<Refraction> _refractions = [];
    public IReadOnlyCollection<Refraction> Refractions => _refractions.AsReadOnly();

    private readonly List<VisitDiagnosis> _diagnoses = [];
    public IReadOnlyCollection<VisitDiagnosis> Diagnoses => _diagnoses.AsReadOnly();

    private readonly List<VisitAmendment> _amendments = [];
    public IReadOnlyCollection<VisitAmendment> Amendments => _amendments.AsReadOnly();

    // Factory method
    public static Visit Create(
        Guid patientId, string patientName,
        Guid doctorId, string doctorName,
        BranchId branchId, Guid? appointmentId = null) { ... }

    // Workflow
    public void AdvanceStage(WorkflowStage newStage) { ... }

    // Sign-off - makes record immutable
    public void SignOff(Guid doctorId)
    {
        if (Status == VisitStatus.Signed)
            throw new InvalidOperationException("Visit is already signed off.");
        Status = VisitStatus.Signed;
        SignedAt = DateTime.UtcNow;
        SignedById = doctorId;
        SetUpdatedAt();
    }

    // Guard: throws if signed and not in amendment mode
    private void EnsureEditable()
    {
        if (Status == VisitStatus.Signed)
            throw new InvalidOperationException("Cannot modify a signed visit. Use amendment workflow.");
    }
}
```

### Pattern 2: Amendment Chain for Immutable Record Corrections

**What:** VisitAmendment entities capture field-level diffs with mandatory reason, preserving the original record.
**When to use:** When a doctor needs to correct a signed visit.

```csharp
// Clinical.Domain/Entities/VisitAmendment.cs
public class VisitAmendment : Entity, IAuditable
{
    public Guid VisitId { get; private set; }
    public Guid AmendedById { get; private set; }
    public string AmendedByName { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public string FieldChangesJson { get; private set; } = string.Empty;  // JSON: [{field, oldValue, newValue}]
    public DateTime AmendedAt { get; private set; }

    public static VisitAmendment Create(
        Guid visitId, Guid amendedById, string amendedByName,
        string reason, List<FieldChange> changes) { ... }
}

public record FieldChange(string FieldName, string? OldValue, string? NewValue);
```

### Pattern 3: Per-Doctor ICD-10 Favorites (Junction Table)

**What:** Replace the global `IsFavorite` on `Icd10Code` with a per-doctor junction table.
**When to use:** ICD-10 diagnosis selection -- each doctor has their own pinned favorites.

**Important:** The existing `Icd10Code.IsFavorite` field in Shared.Domain is a global boolean that cannot support per-doctor favorites. Create a new `DoctorIcd10Favorite` entity in Clinical.Domain instead:

```csharp
// Clinical.Domain/Entities/DoctorIcd10Favorite.cs (in Clinical module, not Shared)
public class DoctorIcd10Favorite : Entity
{
    public Guid DoctorId { get; private set; }
    public string Icd10Code { get; private set; } = string.Empty;

    public static DoctorIcd10Favorite Create(Guid doctorId, string icd10Code) { ... }
}
```

The `DoctorIcd10Favorite` lives in the Clinical module's schema. Search queries join `reference.Icd10Codes` with `clinical.DoctorIcd10Favorites` to show pinned codes at the top for the current doctor.

### Pattern 4: Kanban Dashboard with Polling

**What:** Workflow dashboard polls for active visits and renders a Kanban board with @dnd-kit.
**When to use:** CLN-03 and CLN-04 requirements.

```typescript
// Kanban refresh pattern using TanStack Query polling
const { data: activeVisits } = useQuery({
  queryKey: ["clinical", "active-visits"],
  queryFn: () => getActiveVisits(),
  refetchInterval: 5000,  // Poll every 5 seconds
  refetchIntervalInBackground: false,  // Stop when tab not visible
})
```

**Kanban stage grouping recommendation (Claude's Discretion):** Group 8 stages into 5 columns for a boutique clinic with 2 doctors:

| Column | Stages Contained | Rationale |
|--------|-----------------|-----------|
| Reception | Reception | Entry point |
| Testing | Refraction/VA | Technician work |
| Doctor | Doctor Exam, Diagnostics, Doctor Reads | Doctor's workflow |
| Processing | Rx, Cashier | Post-exam processing |
| Complete | Pharmacy/Optical | Exit point |

This reduces visual clutter while the underlying `WorkflowStage` enum still has all 8 values for precise tracking. The column headers can show grouped stage names. Each card still shows its exact stage.

### Pattern 5: Refraction OD/OS Side-by-Side Layout

**What:** Refraction data entered in side-by-side OD (right) / OS (left) layout with tabs per refraction type.
**When to use:** REF-01, REF-02, REF-03 requirements.

```typescript
// Frontend structure for refraction form
// Each refraction type (Manifest, Auto, Cycloplegic) has its own record
// with OD/OS side-by-side fields

interface RefractionFormValues {
  od: {  // Right eye
    sph: number | null
    cyl: number | null
    axis: number | null
    add: number | null
    pd: number | null
  }
  os: {  // Left eye
    sph: number | null
    cyl: number | null
    axis: number | null
    add: number | null
    pd: number | null
  }
  ucvaOd: number | null  // Uncorrected VA right
  ucvaOs: number | null
  bcvaOd: number | null  // Best-corrected VA right
  bcvaOs: number | null
  iopOd: number | null
  iopOs: number | null
  iopMethod: string      // "Goldmann" | "NonContact" | "iCare" | "Tonopen"
  axialLengthOd: number | null
  axialLengthOs: number | null
}
```

### Anti-Patterns to Avoid

- **Tabbed visit page:** CONTEXT.md explicitly says "single scrollable page with collapsible card sections, not tabbed". Do NOT use Tabs component for the visit detail page. Use Card with Collapsible from shadcn/ui.
- **Global ICD-10 favorites:** Do NOT use the existing `Icd10Code.IsFavorite` field. It is per-record, not per-user. Create a junction table instead.
- **Cross-module joins:** Do NOT join ClinicalDbContext to PatientDbContext. Denormalize PatientName/DoctorName on the Visit entity (same as Appointment pattern).
- **SignalR for v1:** Do NOT add SignalR complexity. Polling is sufficient for 2-doctor clinic.
- **Modifying signed visits directly:** The Visit entity MUST guard against modifications when Status == Signed. All changes go through the amendment workflow.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Drag and drop | Custom mouse/touch event handlers | @dnd-kit/core + @dnd-kit/sortable | Accessibility, touch support, keyboard support, collision detection |
| Field-level diff | Manual comparison logic | JSON serialization of old/new values + structured FieldChange records | Edge cases with nulls, types, nested objects |
| ICD-10 search | Full-text search engine | EF Core LIKE queries on Code + DescriptionEn + DescriptionVi | ~150 ophthalmology codes is small enough for LIKE; no Elasticsearch needed |
| Collapsible sections | Custom show/hide toggle | shadcn/ui Collapsible component | Already available, accessible, animated |
| Form validation | Manual field checking | FluentValidation (backend) + Zod (frontend) | Established pattern, consistent error handling |

**Key insight:** The ICD-10 code dataset is small (~150 codes). No need for full-text search infrastructure. A simple `LIKE '%query%'` on both description columns, with results grouped by category and favorites pinned to top, will be fast and sufficient.

## Common Pitfalls

### Pitfall 1: Icd10Code.IsFavorite Is Global, Not Per-User
**What goes wrong:** Building favorites using the existing `IsFavorite` boolean on `Icd10Code` makes all doctors share the same favorites.
**Why it happens:** The field was added in Phase 1 as a placeholder. CONTEXT.md explicitly says "Per-doctor favorites."
**How to avoid:** Create `DoctorIcd10Favorite` junction table in Clinical schema. Query favorites filtered by current user's DoctorId. The existing `IsFavorite` field can remain but should be ignored (or removed in a cleanup migration).
**Warning signs:** If you see code setting `Icd10Code.IsFavorite` directly, it is wrong.

### Pitfall 2: Modifying Visit After Sign-Off Without Amendment
**What goes wrong:** Forgetting to check `Status == Signed` before allowing edits leads to mutable "immutable" records.
**Why it happens:** Multiple update endpoints (refraction, notes, diagnosis) all need the guard.
**How to avoid:** Add `EnsureEditable()` guard method on Visit entity. Call it at the start of every mutation method. The amendment workflow temporarily creates a new amendment record, applies changes, and re-signs.
**Warning signs:** Any update handler that doesn't check visit status first.

### Pitfall 3: Laterality .9 Codes Slipping Through
**What goes wrong:** Doctor selects a laterality-required ICD-10 code but the system allows unspecified (.9) eye designation.
**Why it happens:** Backend validation doesn't enforce laterality rules, or frontend doesn't prompt.
**How to avoid:** Frontend: when `RequiresLaterality=true`, show inline OD/OS/OU selector before adding diagnosis. Backend: validate that codes ending in .9 are rejected when the base code has `RequiresLaterality=true`. When OU is selected, store two VisitDiagnosis records (.1 for right, .2 for left).
**Warning signs:** VisitDiagnosis records with .9 suffix codes where base code has RequiresLaterality=true.

### Pitfall 4: Cross-Module DB Dependencies
**What goes wrong:** ClinicalDbContext tries to reference PatientDbContext entities, creating circular dependencies.
**Why it happens:** Need patient data on the visit.
**How to avoid:** Denormalize PatientName on Visit entity (same as Appointment.PatientName pattern). For allergy display, query Patient module API at read-time, don't store in Clinical schema.
**Warning signs:** Clinical.Infrastructure referencing Patient.Infrastructure or Patient.Domain.

### Pitfall 5: Drag-and-Drop Without Touch Fallback
**What goes wrong:** Kanban works on desktop but not on tablets.
**Why it happens:** @dnd-kit uses PointerSensor by default, which works for both but needs proper configuration.
**How to avoid:** Configure both PointerSensor and TouchSensor as sensors. Also provide action buttons on each card for stage advancement as a non-DnD alternative (CONTEXT.md requirement).
**Warning signs:** Kanban board that only works with mouse.

### Pitfall 6: @dnd-kit/core Peer Dependency with React 19
**What goes wrong:** npm install fails or warns about React 19 peer dependency.
**Why it happens:** @dnd-kit/core may declare peer dependency on React 16-18.
**How to avoid:** Use `--legacy-peer-deps` flag or add to `.npmrc`. The library works fine with React 19; it does not use deprecated APIs like `findDOMNode`.
**Warning signs:** npm ERESOLVE peer dependency warnings during install.

### Pitfall 7: Amendment Diff Missing Nested Changes
**What goes wrong:** Amendment captures top-level field changes but misses changes within refraction or diagnosis sub-entities.
**Why it happens:** Diff logic only compares Visit-level fields.
**How to avoid:** Structure FieldChange records with dotted paths: "refractions.manifest.od.sph" for nested values. Serialize the before/after snapshot of the full visit to compute the diff.
**Warning signs:** Amendments that say "no changes detected" when refraction values were modified.

## Code Examples

### Established Handler Pattern (from Scheduling module)
```csharp
// Source: backend/src/Modules/Scheduling/Scheduling.Application/Features/BookAppointment.cs
public static class CreateVisitHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateVisitCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateVisitCommand> validator,
        ICurrentUser currentUser,
        ILogger<CreateVisitCommand> logger,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<Guid>.Failure(Error.ValidationWithDetails(errors));
        }
        // ... create Visit aggregate, save, return Id
    }
}
```

### Established Endpoint Pattern (from Scheduling module)
```csharp
// Source: backend/src/Modules/Scheduling/Scheduling.Presentation/SchedulingApiEndpoints.cs
public static class ClinicalApiEndpoints
{
    public static IEndpointRouteBuilder MapClinicalApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clinical").RequireAuthorization();
        // ... map endpoints
        return app;
    }
}
```

### Established Combobox Pattern (from AllergyForm)
```typescript
// Source: frontend/src/features/patient/components/AllergyForm.tsx
// The ICD-10 combobox follows this exact pattern:
// - Popover + Command with shouldFilter={false}
// - External filtering via API or local
// - CommandGroup for category grouping
// - CommandItem with check icon for selected state
// - Input wrapped in div inside PopoverTrigger
```

### Established API Hook Pattern (from patient-api)
```typescript
// Source: frontend/src/features/patient/api/patient-api.ts
// TanStack Query hooks with queryKey factories:
const clinicalKeys = {
  all: ["clinical"] as const,
  visits: () => [...clinicalKeys.all, "visits"] as const,
  visit: (id: string) => [...clinicalKeys.visits(), id] as const,
  activeVisits: () => [...clinicalKeys.all, "active-visits"] as const,
  icd10Search: (query: string) => [...clinicalKeys.all, "icd10", query] as const,
}
```

### @dnd-kit Kanban Board Pattern
```typescript
// Kanban board with @dnd-kit/core + @dnd-kit/sortable
import {
  DndContext,
  DragOverlay,
  PointerSensor,
  TouchSensor,
  useSensor,
  useSensors,
  closestCorners,
} from "@dnd-kit/core"
import { SortableContext, verticalListSortingStrategy } from "@dnd-kit/sortable"

function WorkflowDashboard() {
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } }),
    useSensor(TouchSensor, { activationConstraint: { delay: 200, tolerance: 5 } }),
  )

  return (
    <DndContext
      sensors={sensors}
      collisionDetection={closestCorners}
      onDragEnd={handleDragEnd}
    >
      <div className="flex gap-4 overflow-x-auto">
        {columns.map(column => (
          <KanbanColumn key={column.id} column={column}>
            <SortableContext
              items={column.visits.map(v => v.id)}
              strategy={verticalListSortingStrategy}
            >
              {column.visits.map(visit => (
                <PatientCard key={visit.id} visit={visit} />
              ))}
            </SortableContext>
          </KanbanColumn>
        ))}
      </div>
      <DragOverlay>{/* Active card preview */}</DragOverlay>
    </DndContext>
  )
}
```

### Collapsible Card Section Pattern
```typescript
// Collapsible card for visit detail sections
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/Card"
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/shared/components/Collapsible"
import { IconChevronDown } from "@tabler/icons-react"

function VisitSection({ title, children, defaultOpen = true }) {
  return (
    <Collapsible defaultOpen={defaultOpen}>
      <Card>
        <CollapsibleTrigger asChild>
          <CardHeader className="cursor-pointer flex flex-row items-center justify-between">
            <CardTitle>{title}</CardTitle>
            <IconChevronDown className="h-4 w-4 transition-transform" />
          </CardHeader>
        </CollapsibleTrigger>
        <CollapsibleContent>
          <CardContent>{children}</CardContent>
        </CollapsibleContent>
      </Card>
    </Collapsible>
  )
}
```

**Note:** Check if shadcn/ui Collapsible is already installed. If not, run `npx shadcn@latest add collapsible` to add it, then create the wrapper in `@/shared/components/Collapsible.tsx` per the established wrapper pattern.

## Discretion Recommendations

### Kanban Stage Grouping: 5 Columns
**Recommendation:** Group the 8 workflow stages into 5 Kanban columns.

| Column Label | Backend Stages | Color Indicator |
|-------------|---------------|-----------------|
| Reception | Reception | Gray |
| Testing | Refraction/VA | Blue |
| Doctor | Doctor Exam, Diagnostics, Doctor Reads | Green |
| Processing | Rx, Cashier | Orange |
| Done | Pharmacy/Optical | Muted |

**Rationale:** A boutique clinic with 2 doctors would have at most ~15-20 patients at any time. 8 columns would be too sparse and require horizontal scrolling. 5 columns fit on a standard monitor. Each card still displays its exact stage as a badge.

### Examination Notes: Free-Text with Markdown
**Recommendation:** Use a Textarea for free-text examination notes. Do not add semi-structured fields for v1.

**Rationale:** Ophthalmology examinations are highly variable. The structured data is captured in refraction and diagnosis sections. The notes section serves as a catch-all for observations, findings, and clinical impressions. Doctors should be free to write whatever they need. Semi-structured templates belong in Phase 4 (Dry Eye template) and beyond.

### Refraction Validation Ranges
**Recommendation:**

| Field | Range | Step | Unit |
|-------|-------|------|------|
| SPH | -30.00 to +30.00 | 0.25 | Diopter |
| CYL | -10.00 to +10.00 | 0.25 | Diopter |
| AXIS | 1 to 180 | 1 | Degrees |
| ADD | +0.25 to +4.00 | 0.25 | Diopter |
| PD | 20 to 80 | 0.5 | mm |
| VA (UCVA/BCVA) | 0.01 to 2.0 | 0.01 | Decimal |
| IOP | 1 to 60 | 1 | mmHg |
| Axial Length | 15.0 to 40.0 | 0.01 | mm |

### IOP Measurement Methods
**Recommendation:** Enum values:
- Goldmann (gold standard applanation)
- NonContact (air puff)
- iCare (rebound tonometry)
- Tonopen (portable applanation)
- Other

### Real-Time Update Mechanism: Polling
**Recommendation:** TanStack Query `refetchInterval: 5000` (5 seconds) for the active visits dashboard query.

**Rationale:** With 2 doctors and ~8 staff, a 5-second polling interval generates ~12 requests/minute to the dashboard endpoint. This is negligible server load. SignalR adds hub infrastructure, client connection management, and reconnection logic. Not worth the complexity for v1. The `refetchIntervalInBackground: false` option stops polling when the tab is not visible, saving resources.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| react-beautiful-dnd | @dnd-kit or @hello-pangea/dnd | 2023 (Atlassian deprecated rbd) | Must use maintained library |
| SignalR for all real-time | Polling for simple cases, SignalR for complex | Ongoing | Polling is simpler for low-frequency updates |
| Global IsFavorite on reference data | Per-user junction table | This phase | Required for multi-doctor favorites |

**Deprecated/outdated:**
- react-beautiful-dnd: Deprecated by Atlassian. Do not use.
- @dnd-kit/core global IsFavorite: The Phase 1 placeholder field on Icd10Code should not be used for per-doctor favorites.

## Open Questions

1. **Should we remove `IsFavorite` from `Icd10Code`?**
   - What we know: It was added in Phase 1 as a placeholder. Per-doctor favorites use a junction table.
   - What is unclear: Whether removing it requires a migration that touches the reference schema.
   - Recommendation: Leave it for now (default false, unused). Remove in a cleanup phase to avoid touching shared infrastructure.

2. **Collapsible component availability in shadcn/ui**
   - What we know: shadcn/ui has a Collapsible primitive. Need to verify it is installed.
   - What is unclear: Whether it was added during Phase 1.2 shadcn setup.
   - Recommendation: Check at implementation time. If missing, `npx shadcn@latest add collapsible` and create wrapper.

3. **@dnd-kit peer dependency with React 19**
   - What we know: @dnd-kit/core works with React 19 functionally (no findDOMNode usage). Peer deps may warn.
   - What is unclear: Whether npm install will fail or just warn.
   - Recommendation: If peer dep error, add `legacy-peer-deps=true` to `.npmrc` or use `--legacy-peer-deps` flag.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.x + NSubstitute 5.x + FluentAssertions 8.x |
| Config file | backend/tests/ directory with per-module test projects |
| Quick run command | `dotnet test backend/tests/Clinical.Unit.Tests --no-build -x` |
| Full suite command | `dotnet test backend/Ganka28.slnx` |

### Phase Requirements to Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| CLN-01 | Create visit, sign-off makes immutable | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~CreateVisitHandlerTests" -x` | Wave 0 |
| CLN-01 | Signed visit rejects modifications | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~SignOffVisitHandlerTests" -x` | Wave 0 |
| CLN-02 | Amendment captures field-level diff | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~AmendVisitHandlerTests" -x` | Wave 0 |
| CLN-03 | Workflow stage advancement | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~AdvanceWorkflowStageHandlerTests" -x` | Wave 0 |
| CLN-04 | Active visits query returns grouped by stage | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~GetActiveVisitsHandlerTests" -x` | Wave 0 |
| REF-01 | Refraction data persistence per eye | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~UpdateRefractionHandlerTests" -x` | Wave 0 |
| REF-02 | VA, IOP, Axial Length recording | unit | Covered by UpdateRefractionHandlerTests | Wave 0 |
| REF-03 | Multiple refraction types per visit | unit | Covered by UpdateRefractionHandlerTests | Wave 0 |
| DX-01 | ICD-10 search in Vi/En with favorites | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~SearchIcd10CodesHandlerTests" -x` | Wave 0 |
| DX-02 | Laterality .9 rejection | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~AddVisitDiagnosisHandlerTests" -x` | Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test backend/tests/Clinical.Unit.Tests --no-build -x`
- **Per wave merge:** `dotnet test backend/Ganka28.slnx`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `backend/tests/Clinical.Unit.Tests/` project -- create test project, add to solution
- [ ] Test project NuGet references: xunit, NSubstitute, FluentAssertions, Bogus
- [ ] Reference Clinical.Application, Clinical.Domain, Shared.Domain

## Sources

### Primary (HIGH confidence)
- Existing codebase: Scheduling module (handler pattern, endpoint pattern, entity pattern)
- Existing codebase: Patient module (AllergyForm combobox pattern, TanStack Query hooks)
- Existing codebase: Shared.Domain (AggregateRoot, Entity, IAuditable, Result, Error, Icd10Code)
- Existing codebase: ReferenceDbContext (ICD-10 seed data, ~150 ophthalmology codes)
- Existing codebase: ClinicalDbContext scaffold (clinical schema, registered in Program.cs)

### Secondary (MEDIUM confidence)
- [@dnd-kit official docs](https://dndkit.com/) -- drag-and-drop API, sensors, collision detection
- [shadcn/ui + dnd-kit Kanban example](https://github.com/Georgegriff/react-dnd-kit-tailwind-shadcn-ui) -- reference implementation
- [ASP.NET Core SignalR docs](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction) -- for future real-time reference (not v1)
- [@dnd-kit/react npm](https://www.npmjs.com/package/@dnd-kit/react) -- v0.3.2, pre-stable (avoid for now)

### Tertiary (LOW confidence)
- Refraction validation ranges -- based on standard ophthalmology practice, not verified against Vietnamese clinical standards. May need adjustment per user feedback.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all libraries are established in the project or well-documented
- Architecture: HIGH -- follows exact patterns from Scheduling/Patient modules
- Refraction domain: MEDIUM -- validation ranges based on general ophthalmology knowledge
- @dnd-kit React 19 compat: MEDIUM -- works functionally, peer deps may warn
- Pitfalls: HIGH -- based on direct codebase analysis

**Research date:** 2026-03-04
**Valid until:** 2026-04-04 (stable domain, 30-day validity)

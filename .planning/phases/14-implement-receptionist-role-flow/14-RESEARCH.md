# Phase 14: Implement Receptionist Role Flow - Research

**Researched:** 2026-03-28
**Domain:** Full-stack role-based workflow (ASP.NET modular monolith + React SPA)
**Confidence:** HIGH

## Summary

Phase 14 implements the complete receptionist front-desk workflow across 5 screens: a role-based dashboard (SCR-002a), patient intake form (SCR-003), appointment booking page (SCR-004), check-in/walk-in dialogs (SCR-005), and context-menu actions (SCR-006). The work spans both backend (entity modifications, new handlers, role seeding, migrations) and frontend (new feature module, 17 new components, route modifications).

The backend changes are substantial but well-contained: Appointment entity needs 9 new fields + nullable PatientId, Visit entity needs 4 new fields + updated Cancel method, Patient entity needs ~8 new fields for intake data (email, occupation, medical/ocular history, lifestyle), AppointmentStatus enum needs NoShow, and a new Receptionist role must be seeded. The frontend work is the bulk -- building a complete `features/receptionist/` module with its own API client, hooks, components, and Zod schemas.

**Primary recommendation:** Split into backend-first (entity changes, migrations, handlers, role seeding) then frontend (dashboard, intake form, booking page, dialogs, actions) with the backend providing all API endpoints before frontend integration begins.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- D-01: Same `/dashboard` route, role-based rendering -- detect Receptionist role and render a completely different view. No separate route.
- D-02: Existing sidebar Dashboard item used -- no new sidebar entries.
- D-03: Table-only view for receptionist (no kanban/table toggle).
- D-04: Polling with manual refresh button for real-time updates. No WebSocket. Polling intervals: 30s for KPI cards, 15s for patient table.
- D-05: Dialog modals for all check-in and visit creation popups.
- D-06: Full intake form opens when checking in incomplete patient. Pre-filled with existing data.
- D-07: All 4 form sections collapsible but all expanded by default.
- D-08: "Luu & Chuyen tien kham" button auto-advances patient to Pre-Exam stage (skips Waiting queue).
- D-09: Full page route at `/appointment/new` for appointment booking.
- D-10: Hardcode 30-min slot duration. Use existing ClinicSchedule entity.
- D-11: Guest bookings store GuestName, GuestPhone, GuestReason on Appointment record. Do NOT create patient record until check-in. PatientId becomes nullable.
- D-12: Slot capacity is doctor-based. If doctor selected: 1 patient per doctor per 30-min slot. If "BS nao trong": allow freely.
- D-13: Dropdown menu (three-dot) at end of each row. Actions vary by status.
- D-14: Cancel/No-Show: required dropdown reason / optional text note.
- D-15: "Dat hen lai" checkbox navigates to `/appointment/new` with patient pre-filled.
- D-19: Full responsive: desktop >1024px, tablet 768-1024px, mobile <768px.

### Claude's Discretion
- Internal component structure and file organization within features/receptionist/
- API endpoint naming and request/response DTO shapes (following existing patterns)
- Query invalidation strategy for polling + manual refresh
- How to map receptionist 4-status model to clinical 11-stage pipeline in queries

### Deferred Ideas (OUT OF SCOPE)
- Keyboard shortcuts (N/H// on dashboard, Ctrl+S/Ctrl+Enter on forms)
- Auto-save draft on intake form
- Notification sounds for new appointments / status changes
- Per-doctor schedule filtering on booking grid
- SMS/Zalo appointment reminders
- Appointment booking limit config (currently hardcoded 3 months max)
</user_constraints>

## Project Constraints (from CLAUDE.md)

- **TDD strictly**: Write failing tests first, then implement (red-green-refactor). At least 80% code coverage.
- **shadcn/ui first**: Use shadcn components where applicable. Mantine only if shadcn cannot meet the requirement.
- **Migrations required**: When models change, create and run migrations.
- **Free libraries only**: No paid libraries without approval.
- **No unnecessary placeholders**: Only add placeholder where it makes sense.
- **Backend errors**: Check logs before debugging 500 errors.
- **Lock file issues**: Stop backend before continuing if lock file error.
- **DOC-01**: Must create Vietnamese user stories documentation.

## Standard Stack

### Core (already in project)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET 10 | 10.0.102 | Backend runtime | Project standard |
| Wolverine | (project) | CQRS message bus | Established handler pattern |
| FluentValidation | (project) | Command validation | Every handler uses it |
| EF Core | (project) | ORM + migrations | Domain entity persistence |
| React | 19.0.0 | Frontend framework | Project standard |
| TanStack Query | 5.64.2 | Server state / polling | Existing API pattern with refetchInterval |
| TanStack Router | 1.114.3 | File-based routing | Existing route pattern |
| TanStack Table | 8.21.2 | Data table | Used in clinical dashboard |
| React Hook Form | 7.54.2 | Form state | Established form pattern |
| Zod | 3.24.2 | Schema validation | Paired with @hookform/resolvers |
| Zustand | 5.0.3 | Client state | Auth store pattern |
| shadcn/ui | (project) | Component library | All UI components |
| Tabler Icons | 3.37.1 | Icon library | Project standard |
| date-fns | 4.1.0 | Date utilities | Project standard |

### No New Dependencies Needed

This phase uses exclusively existing project dependencies. No new packages required.

## Architecture Patterns

### Backend: Entity Modifications Required

#### 1. Appointment Entity (Scheduling.Domain)

Current `Appointment.cs` needs these additions:

```csharp
// New fields for receptionist workflow
public Guid? PatientId { get; private set; }          // Make NULLABLE (was required Guid)
public string? GuestName { get; private set; }         // D-11: phone booking without patient
public string? GuestPhone { get; private set; }        // D-11: phone booking without patient
public string? GuestReason { get; private set; }       // D-11: reason for guest booking
public DateTime? CheckedInAt { get; private set; }     // Check-in timestamp
public AppointmentSource Source { get; private set; }  // Phone/Web/Staff
public DateTime? NoShowAt { get; private set; }        // No-show timestamp
public Guid? NoShowBy { get; private set; }            // Who marked no-show
public string? NoShowNotes { get; private set; }       // Optional no-show note
public Guid? CancelledBy { get; private set; }         // Who cancelled

// New domain methods
public void CheckIn() { ... }
public void MarkNoShow(Guid userId, string? notes) { ... }
```

**AppointmentStatus enum** needs `NoShow = 4` value added.

**New enum needed:** `AppointmentSource { Staff = 0, Phone = 1, Web = 2 }`

**Critical:** PatientId becoming nullable is a breaking schema change. The `Appointment.Create()` factory needs an overload or modification to support guest bookings (D-11). The validator must be updated to allow null PatientId when GuestName/GuestPhone are provided.

#### 2. Visit Entity (Clinical.Domain)

Current `Visit.cs` needs additions:

```csharp
public VisitSource Source { get; private set; }        // Walk-in vs Appointment
public string? Reason { get; private set; }            // Chief complaint from intake
public string? CancelledReason { get; private set; }   // Why visit was cancelled
public Guid? CancelledBy { get; private set; }         // Who cancelled

// Updated Cancel method (currently takes no params)
public void Cancel(string reason, Guid cancelledBy) { ... }
```

**New enum needed:** `VisitSource { Appointment = 0, WalkIn = 1 }`

**Critical:** The existing `Cancel()` method has no parameters. Adding required reason+userId is a breaking change -- existing callers (CancelVisitHandler, tests) must be updated. Consider adding a new overload rather than changing the signature, then deprecating the old one.

#### 3. Patient Entity (Patient.Domain)

The intake form (SCR-003) collects fields NOT currently on the Patient entity:

```csharp
// Missing fields that need adding
public string? Email { get; private set; }
public string? Occupation { get; private set; }
public string? OcularHistory { get; private set; }     // Tien su benh mat
public string? SystemicHistory { get; private set; }   // Tien su benh toan than
public string? CurrentMedications { get; private set; } // Thuoc dang dung
public decimal? ScreenTimeHours { get; private set; }  // Gio/ngay
public WorkEnvironment? WorkEnvironment { get; private set; }
public ContactLensUsage? ContactLensUsage { get; private set; }
public string? LifestyleNotes { get; private set; }
```

**Note:** Allergy is already a separate entity (`Allergy`) with collection on Patient. The intake form's "Di ung" field maps to adding Allergy entities, not a string field.

**New enums needed:** `WorkEnvironment { Office, Outdoor, Factory, Other }`, `ContactLensUsage { None, Daily, Occasional }`

#### 4. Receptionist Role Seeding

No "Receptionist" role exists in the seeder (8 roles currently: Admin, Doctor, Technician, Nurse, Cashier, OpticalStaff, Manager, Accountant). Must add as role #9 with permissions:

```
Patient.View, Patient.Create, Patient.Update
Scheduling.View, Scheduling.Create, Scheduling.Update
Clinical.View (limited -- for creating visits/check-in only)
```

### Backend: New Handlers Required

| Handler | Module | Purpose |
|---------|--------|---------|
| `GetReceptionistDashboard` | Scheduling | Aggregated query: appointments + visits for today, mapped to 4 statuses |
| `GetReceptionistKpiStats` | Scheduling | KPI card counts (today appointments, waiting, examining, completed) |
| `CheckInAppointment` | Scheduling | Mark appointment checked-in, create visit |
| `CreateWalkInVisit` | Clinical | Create visit for walk-in existing patient |
| `MarkAppointmentNoShow` | Scheduling | Mark no-show with optional notes |
| `BookGuestAppointment` | Scheduling | Book appointment with GuestName/Phone (no PatientId) |
| `GetAvailableSlots` | Scheduling | Get available 30-min slots for a date + optional doctor |
| `CancelVisitWithReason` | Clinical | Extended cancel with reason + cancelled-by |
| `RegisterPatientFromIntake` | Patient | Extended patient registration with all intake fields |
| `UpdatePatientFromIntake` | Patient | Update existing patient with intake fields (for incomplete check-in) |

### Frontend: Component Architecture

```
frontend/src/features/receptionist/
  api/
    receptionist-api.ts          # API client + TanStack Query hooks
  components/
    ReceptionistDashboard.tsx     # Main dashboard container
    KpiCards.tsx                  # 4 KPI stat cards
    PatientQueueTable.tsx         # TanStack Table for patient queue
    StatusBadge.tsx               # Status badge with 4 color variants
    SourceBadge.tsx               # Appointment/Walk-in source badge
    StatusFilterPills.tsx         # Filter toggle group
    RowActionMenu.tsx             # Three-dot dropdown menu per row
    CheckInDialog.tsx             # Check-in popup (complete patient)
    CheckInIncompleteDialog.tsx   # Check-in popup (incomplete patient)
    WalkInVisitDialog.tsx         # Walk-in existing patient popup
    RescheduleDialog.tsx          # Reschedule appointment popup
    CancelAppointmentDialog.tsx   # Cancel appointment popup
    NoShowDialog.tsx              # Mark no-show popup
    CancelVisitDialog.tsx         # Cancel visit popup
  hooks/
    useReceptionistPolling.ts     # Polling logic with manual refresh
  types/
    receptionist.types.ts         # DTO types for receptionist API
  schemas/
    intake-form.schema.ts         # Zod schema for patient intake form
    booking.schema.ts             # Zod schema for appointment booking

frontend/src/features/receptionist/components/booking/
  NewAppointmentPage.tsx          # Full booking page
  TimeSlotGrid.tsx                # Slot selection grid
  ConfirmationBar.tsx             # Bottom confirmation summary

frontend/src/features/receptionist/components/intake/
  PatientIntakeForm.tsx           # 4-section intake form
  PersonalInfoSection.tsx         # Section 1: personal info fields
  ExamInfoSection.tsx             # Section 2: examination reason
  MedicalHistorySection.tsx       # Section 3: medical history
  LifestyleSection.tsx            # Section 4: lifestyle factors
```

### Frontend: Route Changes

```
# Modified route (role branching)
frontend/src/app/routes/_authenticated/dashboard.tsx
  -> Detect user role, render ReceptionistDashboard or existing dashboard

# New route
frontend/src/app/routes/_authenticated/appointments/new.tsx
  -> NewAppointmentPage component at /appointment/new
```

### Frontend: Polling Strategy (D-04)

```typescript
// KPI cards: 30s polling
useQuery({
  queryKey: ['receptionist', 'kpi'],
  queryFn: fetchKpiStats,
  refetchInterval: 30_000,
})

// Patient table: 15s polling
useQuery({
  queryKey: ['receptionist', 'queue', filters],
  queryFn: fetchPatientQueue,
  refetchInterval: 15_000,
})

// Manual refresh button: invalidate both queries
const handleRefresh = () => {
  queryClient.invalidateQueries({ queryKey: ['receptionist'] })
}
```

### Receptionist Status Mapping (4 -> 11 stages)

This mapping must happen at the query level (backend GetReceptionistDashboard handler):

| Receptionist Status | Condition |
|---------------------|-----------|
| Chua den (Not Arrived) | Appointment exists for today, no visit created, status = Confirmed |
| Cho kham (Waiting) | Visit exists, CurrentStage = Reception |
| Dang kham (Examining) | Visit exists, CurrentStage >= PreExam AND Status != Signed/Cancelled |
| Hoan thanh (Completed) | Visit exists, Status = Signed OR CurrentStage = Done |

The query joins Appointments (today) + Visits (today) and maps each row to one of 4 statuses. This is a read-model concern -- no domain changes needed for the mapping itself.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Table with sorting/filtering | Custom table logic | TanStack Table | Already used, handles column defs, sorting, filtering |
| Form validation | Manual validation | React Hook Form + Zod | Established pattern, handles field-level errors |
| Date/time slot grid | Custom date math | date-fns + ClinicSchedule API | Timezone handling, operating hours already solved |
| Patient search autocomplete | Custom search dropdown | usePatientSearch hook + Command (cmdk) | Hook exists, Command component installed |
| Dialog modals | Custom modal | shadcn Dialog | Already installed and used throughout app |
| Dropdown menus | Custom context menu | shadcn DropdownMenu | Already installed |
| Badge variants | Custom badge styles | shadcn Badge + CSS variables | Per UI-SPEC: use className overrides, not new variants |
| Permission guard | Custom auth check | requirePermission utility | Already exists at `shared/utils/permission-guard.ts` |

## Common Pitfalls

### Pitfall 1: Nullable PatientId Breaking Existing Flows
**What goes wrong:** Making PatientId nullable on Appointment breaks all existing booking flows that assume non-null PatientId.
**Why it happens:** Guest bookings (D-11) need nullable PatientId, but existing BookAppointment handler validates `PatientId.NotEmpty()`.
**How to avoid:** Add a separate `BookGuestAppointment` handler/command rather than modifying the existing one. Keep `BookAppointment` for patient-linked bookings, add new handler for guest bookings. This preserves backward compatibility.
**Warning signs:** Existing appointment tests start failing after schema change.

### Pitfall 2: EF Core Migration Ordering
**What goes wrong:** Multiple entity changes (Appointment, Visit, Patient) across 3 modules create migration conflicts if not ordered carefully.
**Why it happens:** Each module has its own DbContext and migration history. Changes must be applied in dependency order.
**How to avoid:** Create one migration per module, test each independently. Order: Patient first (no dependencies), then Scheduling (Appointment), then Clinical (Visit).
**Warning signs:** Migration apply fails with FK constraint errors.

### Pitfall 3: Cross-Module Query for Dashboard
**What goes wrong:** The receptionist dashboard needs data from both Scheduling (Appointments) and Clinical (Visits) modules, but modules should not directly query each other's DbContext.
**Why it happens:** Modular monolith pattern -- each module owns its data.
**How to avoid:** Create a dedicated read-model query in the Scheduling module that uses raw SQL or a read-only cross-module view. Alternatively, use the Wolverine message bus to query the Clinical module for visit data and join in-memory. The existing `GetActiveVisits` handler in Clinical returns visit data that can be cross-referenced.
**Warning signs:** Circular dependency between module projects.

### Pitfall 4: Visit.Cancel() Signature Change Breaking Existing Tests
**What goes wrong:** Adding required parameters to `Visit.Cancel()` breaks the existing CancelVisitHandler and its tests.
**Why it happens:** CancelVisit currently calls `visit.Cancel()` with no args. Adding reason/cancelledBy breaks the contract.
**How to avoid:** Option A: Add new method `CancelWithReason(string reason, Guid cancelledBy)` and keep old `Cancel()` for backward compat. Option B: Update `Cancel()` to accept optional parameters with defaults. Option A is cleaner.
**Warning signs:** Clinical.Unit.Tests compilation errors.

### Pitfall 5: Timezone Handling in Slot Grid
**What goes wrong:** Appointment times are stored in UTC but the slot grid must display Vietnam local time.
**Why it happens:** The existing BookAppointment handler already converts UTC <-> Vietnam time (see line 59-64 in BookAppointment.cs). Must be consistent.
**How to avoid:** Follow the established pattern: frontend sends UTC, backend validates against Vietnam local time. The slot grid on frontend must convert ClinicSchedule open/close times (stored as TimeSpan, presumably local) to display correctly.
**Warning signs:** Slots appear at wrong times, off by 7 hours.

### Pitfall 6: Intake Form Field Mapping to Existing Patient Entity
**What goes wrong:** The intake form has fields (occupation, medical history, lifestyle) that don't exist on the Patient entity yet.
**Why it happens:** Original Patient entity was minimal (name, phone, DOB, gender, address, CCCD).
**How to avoid:** Add all new fields to Patient entity BEFORE building the frontend form. Ensure the migration runs clean. The RegisterPatientCommand and UpdatePatientCommand must be extended.
**Warning signs:** 400 errors when submitting intake form because backend doesn't recognize new fields.

### Pitfall 7: Role Detection for Dashboard Branching
**What goes wrong:** Dashboard renders wrong view because role detection is unreliable.
**Why it happens:** AuthUser has `permissions: string[]` but no explicit `roles` array. Must detect receptionist by checking for specific permission combinations, or add role names to the JWT claims.
**How to avoid:** Check if the auth token / AuthUser already includes role names. Current AuthUser has `permissions` array only. Either: (a) add a `roles` field to the JWT claims and AuthUser interface, or (b) detect receptionist by checking for `Scheduling.Create` + absence of `Clinical.Create` (receptionists don't create clinical records beyond visits). Option (a) is cleaner and more maintainable.
**Warning signs:** Admin users see receptionist dashboard, or receptionists see clinical dashboard.

## Code Examples

### Pattern: Wolverine Handler (from existing BookAppointment.cs)
```csharp
// Source: backend/src/Modules/Scheduling/Scheduling.Application/Features/BookAppointment.cs
public static class BookAppointmentHandler
{
    public static async Task<Result<Guid>> Handle(
        BookAppointmentCommand command,
        IAppointmentRepository appointmentRepository,
        IClinicScheduleRepository clinicScheduleRepository,
        IUnitOfWork unitOfWork,
        IValidator<BookAppointmentCommand> validator,
        ILogger<BookAppointmentCommand> logger,
        CancellationToken ct)
    {
        // 1. Validate
        // 2. Load related entities
        // 3. Business logic checks
        // 4. Create domain entity via factory
        // 5. Persist + SaveChanges
        // 6. Return Result<T>
    }
}
```

### Pattern: API Client Hook (from existing scheduling-api.ts)
```typescript
// Source: frontend/src/features/scheduling/api/scheduling-api.ts
export function useAppointmentsByDoctor(doctorId, dateFrom, dateTo) {
  return useQuery({
    queryKey: schedulingKeys.appointmentsByDoctor(doctorId, dateFrom, dateTo),
    queryFn: async () => {
      const { data, error } = await api.GET("/api/..." as never, { params: { ... } } as never)
      if (error) throw new Error("...")
      return (data as Type[]) ?? []
    },
    enabled: !!doctorId,
    staleTime: 30_000,
  })
}
```

### Pattern: Permission Guard (from existing routes)
```typescript
// Source: frontend/src/app/routes/_authenticated/clinical/index.tsx
export const Route = createFileRoute("/_authenticated/clinical/")({
  beforeLoad: () => requirePermission("Clinical.View"),
  component: ClinicalWorkflowPage,
})
```

### Pattern: Badge with CSS Variables (from 14-UI-SPEC.md)
```tsx
// Source: .planning/phases/14-implement-receptionist-role-flow/14-UI-SPEC.md
<Badge className="border-transparent bg-[var(--status-not-arrived-bg)] text-[var(--status-not-arrived-text)]">
  Chua den
</Badge>
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Separate dashboard routes per role | Single /dashboard with role-based rendering | D-01 decision | Simpler routing, role detection needed |
| WebSocket for real-time | Polling at 15-30s intervals | D-04 decision | Simpler infra, slight data lag acceptable |
| Full patient record for booking | Guest fields on Appointment | D-11 decision | Patient record created only at check-in |

## Open Questions

1. **Cross-Module Dashboard Query**
   - What we know: Receptionist dashboard needs both Appointment and Visit data joined.
   - What's unclear: Whether to use raw SQL cross-module query, in-memory join via two bus calls, or a shared read model.
   - Recommendation: Use two separate queries (one to Scheduling, one to Clinical) and join in the API handler. This respects module boundaries. The receptionist dashboard handler can be in a new `Receptionist` module or in Scheduling with a dependency on Clinical.Contracts for the query.

2. **Role Detection Mechanism**
   - What we know: AuthUser has `permissions: string[]` but no `roles` field.
   - What's unclear: Whether to add role names to JWT claims or detect by permission pattern.
   - Recommendation: Add `roles: string[]` to JWT claims and AuthUser. This is a small change in the auth token generation and makes role-based UI branching reliable and maintainable. Permission-based detection is fragile.

3. **Intake Form Data Model**
   - What we know: Patient entity lacks ~9 fields needed for the intake form (email, occupation, medical history, lifestyle, etc.).
   - What's unclear: Whether to add all fields directly to Patient or create a separate PatientIntake/PatientProfile child entity.
   - Recommendation: Add directly to Patient entity. The data is inherently part of the patient profile (reused across visits). A separate entity adds query complexity for no real benefit at this scale. The existing `Patient.Update()` method and `RegisterPatientCommand` just need extending.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit + FluentAssertions (backend), Vitest (frontend -- if configured) |
| Config file | Backend: each test project has .csproj; Frontend: check vitest.config |
| Quick run command | `dotnet test backend/tests/Scheduling.Unit.Tests/ --no-build -v q` |
| Full suite command | `dotnet test backend/tests/ -v q` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| SCR-002a | Receptionist dashboard renders with KPI + table | unit | `dotnet test --filter "ReceptionistDashboard"` | Wave 0 |
| SCR-003 | Patient intake creates patient with all fields | unit | `dotnet test --filter "RegisterPatientFromIntake"` | Wave 0 |
| SCR-004 | Guest appointment booking with nullable PatientId | unit | `dotnet test --filter "BookGuestAppointment"` | Wave 0 |
| SCR-005 | Check-in marks appointment + creates visit | unit | `dotnet test --filter "CheckInAppointment"` | Wave 0 |
| SCR-006 | No-show/Cancel actions with reasons | unit | `dotnet test --filter "MarkNoShow OR CancelVisitWithReason"` | Wave 0 |
| D-11 | Appointment allows null PatientId with guest fields | unit | `dotnet test --filter "GuestAppointment"` | Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test backend/tests/Scheduling.Unit.Tests/ --no-build -v q`
- **Per wave merge:** `dotnet test backend/tests/ -v q`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `backend/tests/Scheduling.Unit.Tests/Features/CheckInAppointmentTests.cs`
- [ ] `backend/tests/Scheduling.Unit.Tests/Features/BookGuestAppointmentTests.cs`
- [ ] `backend/tests/Scheduling.Unit.Tests/Features/MarkNoShowTests.cs`
- [ ] `backend/tests/Scheduling.Unit.Tests/Features/GetReceptionistDashboardTests.cs`
- [ ] `backend/tests/Clinical.Unit.Tests/Features/CancelVisitWithReasonTests.cs`
- [ ] `backend/tests/Patient.Unit.Tests/Features/RegisterPatientFromIntakeTests.cs`
- [ ] Patient entity tests for new fields
- [ ] Appointment entity tests for NoShow, CheckIn, Guest booking methods

## Sources

### Primary (HIGH confidence)
- Codebase analysis: `backend/src/Modules/Scheduling/Scheduling.Domain/Entities/Appointment.cs` -- current entity structure, 148 lines
- Codebase analysis: `backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs` -- current entity structure, 515 lines
- Codebase analysis: `backend/src/Modules/Patient/Patient.Domain/Entities/Patient.cs` -- current entity structure, 180 lines
- Codebase analysis: `backend/src/Modules/Auth/Auth.Infrastructure/Seeding/AuthDataSeeder.cs` -- 8 existing roles, no Receptionist
- Codebase analysis: `frontend/src/features/scheduling/api/scheduling-api.ts` -- established API client pattern
- Codebase analysis: `frontend/src/app/routes/_authenticated/dashboard.tsx` -- current dashboard (needs role branching)
- Phase context: `14-CONTEXT.md` -- all locked decisions D-01 through D-19
- UI spec: `14-UI-SPEC.md` -- component inventory, color system, typography, copy
- Spec docs: `docs/dev/receiptionist/` -- 5 screen specifications + 11 HTML mockups

### Secondary (MEDIUM confidence)
- Codebase analysis: `backend/src/Shared/Shared.Domain/Permissions.cs` -- permission constants (10 modules x 6 actions)
- Codebase analysis: `frontend/src/shared/stores/authStore.ts` -- AuthUser interface (no roles field)
- Codebase analysis: handler patterns from BookAppointment, CreateVisit, CancelVisit

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - all libraries already in project, no new dependencies
- Architecture: HIGH - patterns thoroughly analyzed from existing codebase
- Pitfalls: HIGH - identified from actual code analysis (nullable PatientId, Cancel() signature, cross-module query)
- Entity changes: HIGH - exact field gaps identified by comparing specs to entities

**Research date:** 2026-03-28
**Valid until:** 2026-04-28 (stable -- no external dependency changes expected)

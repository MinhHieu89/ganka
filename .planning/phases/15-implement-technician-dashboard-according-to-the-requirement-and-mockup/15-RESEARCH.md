# Phase 15: Implement Technician Dashboard - Research

**Researched:** 2026-03-29
**Domain:** Full-stack role-based dashboard (C# modular monolith backend + React/TypeScript frontend)
**Confidence:** HIGH

## Summary

Phase 15 builds the Technician (Pre-Exam) dashboard: a new `TechnicianOrder` entity in the Clinical module, a `WorkflowStage.RefractionVA` to `PreExam` rename (breaking migration), dashboard query/KPI endpoints, and a full frontend dashboard with KPI cards, filter pills, patient queue table, action menus, in-progress banner, and a slide-over panel for viewing patient results. This follows the exact same architectural patterns established by the Phase 14 Receptionist dashboard.

The codebase already has all infrastructure needed: shadcn Sheet component for slide-over, TanStack Table for queue tables, TanStack Query with polling for real-time updates, Wolverine vertical-slice handlers, and EF Core migrations. The primary complexity is the `RefractionVA` to `PreExam` rename which touches ~30 files across backend and frontend, and the new `TechnicianOrder` entity with its status derivation logic.

**Primary recommendation:** Build in this order: (1) enum rename + migration, (2) TechnicianOrder entity + auto-creation hook, (3) backend dashboard/KPI/action endpoints with TDD, (4) frontend dashboard UI following receptionist patterns, (5) slide-over panel, (6) stub Pre-Exam page.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** All pre-exam tracking lives in a `TechnicianOrder` child entity on Visit -- NO pre-exam fields directly on Visit. Handles both initial pre-exam and future doctor-ordered additional exams via `OrderType` enum.
- **D-02:** `TechnicianOrderType` enum: `PreExam` (auto-created when visit enters PreExam stage) and `AdditionalExam` (future).
- **D-03:** Enum stored as **string** in DB (`HasConversion<string>()`), not int.
- **D-04:** Auto-create `TechnicianOrder(OrderType=PreExam)` when receptionist advances Visit to PreExam stage.
- **D-05:** TechnicianOrder fields: Id, VisitId, OrderType, TechnicianId (nullable), TechnicianName, StartedAt, CompletedAt, IsRedFlag, RedFlagReason, RedFlaggedAt, OrderedByDoctorId (nullable), OrderedByDoctorName, Instructions (nullable), OrderedAt.
- **D-06:** Rename `WorkflowStage.RefractionVA = 1` to `WorkflowStage.PreExam = 1`. Breaking rename requiring migration + update all backend/frontend references.
- **D-07:** Dashboard queries `TechnicianOrder` table (single source). Join with Visit and Patient for display fields.
- **D-08:** Technician display statuses derived from TechnicianOrder fields:
  - Cho kham = `TechnicianId IS NULL AND CompletedAt IS NULL`
  - Dang do = `TechnicianId = currentUser AND CompletedAt IS NULL`
  - Red flag = `IsRedFlag = true`
  - Hoan tat = `CompletedAt IS NOT NULL` (today)
- **D-09:** KPI counts query the same TechnicianOrder table.
- **D-10:** "Loai" column: `PreExam` + no prior visits = "Moi", `PreExam` + has prior visits = "Tai kham", `AdditionalExam` = "Do bo sung" badge.
- **D-11:** Dashboard + stub navigation. "Nhan BN" and "Tiep tuc do" navigate to placeholder Pre-Exam page.
- **D-12:** "Do bo sung" flow deferred -- `TechnicianOrder` entity supports it but no UI to create them yet.
- **D-13:** "Xem ket qua" opens a slide-over side panel (drawer from right) using shadcn Sheet. Read-only: personal info, visit history, pre-exam data.
- **D-14:** All technician dashboard endpoints live in the Clinical module.
- **D-15:** 1 patient per technician at a time -- optimistic validation. Backend validates on accept; if already assigned, return error. Frontend shows toast and refreshes.
- **D-16:** Same `/dashboard` route with role-based rendering. Add Technician role check -> render `<TechnicianDashboard />`.
- **D-17:** Polling-based real-time updates. Dashboard rows: 15s, KPI cards: 30s, wait time: 60s client-side calculation.

### Claude's Discretion
- Internal component structure within `features/technician/`
- API endpoint naming and DTO shapes (follow Clinical module patterns)
- Query invalidation strategy for polling + manual refresh
- Slide-over panel component implementation (shadcn Sheet)
- Stub Pre-Exam page design (minimal placeholder)
- i18n key structure for technician namespace

### Deferred Ideas (OUT OF SCOPE)
- Pre-Exam measurement form (Step 1 & Step 2) -- separate phase
- "Do bo sung" flow (Section 9) -- needs doctor-side EMR integration
- Keyboard shortcuts -- polish phase
- Notification sounds -- polish phase
- Auto-save on Pre-Exam form -- needs draft storage infrastructure
</user_constraints>

## Project Constraints (from CLAUDE.md)

- **TDD strictly:** Write failing tests first, then implement (red-green-refactor). At least 80% code coverage.
- **shadcn/ui components:** Use where applicable, Mantine as fallback with wrapper.
- **Migrations required:** When models change, create and run migrations.
- **Free libraries only.**
- **Backend port 5255, frontend port 3000** for verification.
- **Backend logs:** `backend/src/Bootstrapper/logs/ganka-YYYYMMDD.log`.
- **Lock file issues:** Stop backend then continue.

## Standard Stack

### Core (Already Installed)
| Library | Purpose | Where Used |
|---------|---------|------------|
| Wolverine | CQRS command/query bus | All backend handlers (vertical slice pattern) |
| EF Core 9.0 | ORM + migrations | `ClinicalDbContext`, entity configurations |
| FluentValidation | Request validation | Command validators |
| xUnit + NSubstitute | Unit testing | `Clinical.Unit.Tests` project |
| TanStack Query v5 | Server state, polling | `refetchInterval: 15_000` / `30_000` |
| TanStack Table | Data table with sorting | Queue table (receptionist pattern) |
| shadcn/ui | UI components | Card, Badge, Button, Sheet, DropdownMenu, etc. |
| Tabler Icons React | Icon library | All dashboard icons |
| react-i18next | Internationalization | Vietnamese/English UI text |
| React Hook Form + Zod | Form validation | Red flag dialog |
| OpenAPI Fetch | Type-safe API client | `api.GET/POST/PUT` pattern |

### No New Libraries Needed
All required components and patterns exist in the codebase. Sheet (slide-over), DropdownMenu (action menus), ToggleGroup (filter pills), Badge (status badges), Card (KPI cards), Table, and Sonner (toast notifications) are all installed.

## Architecture Patterns

### Backend: Vertical Slice in Clinical Module

```
backend/src/Modules/Clinical/
  Clinical.Domain/
    Entities/TechnicianOrder.cs          # NEW - child entity of Visit
    Enums/WorkflowStage.cs               # MODIFY - rename RefractionVA -> PreExam
    Enums/TechnicianOrderType.cs         # NEW - PreExam | AdditionalExam
  Clinical.Application/
    Features/
      GetTechnicianDashboard.cs          # NEW - query handler
      GetTechnicianKpiStats.cs           # NEW - KPI query handler
      AcceptTechnicianOrder.cs           # NEW - "Nhan BN"
      CompleteTechnicianOrder.cs         # NEW - "Hoan tat chuyen BS"
      ReturnToQueue.cs                   # NEW - "Tra lai hang doi"
      RedFlagTechnicianOrder.cs          # NEW - "Chuyen BS ngay"
      AdvanceWorkflowStage.cs            # MODIFY - add auto-create hook
    Interfaces/
      ITechnicianOrderRepository.cs      # NEW
  Clinical.Contracts/
    Dtos/TechnicianDashboardDto.cs       # NEW
    Queries/GetTechnicianDashboardQuery.cs # NEW
    Commands/AcceptTechnicianOrderCommand.cs # NEW (etc.)
  Clinical.Infrastructure/
    Configurations/TechnicianOrderConfiguration.cs # NEW
    Repositories/TechnicianOrderRepository.cs      # NEW
    Migrations/YYYYMMDD_AddTechnicianOrder.cs      # NEW
    Migrations/YYYYMMDD_RenameRefractionVAToPreExam.cs # NEW (data migration)
```

### Frontend: Feature Module Pattern

```
frontend/src/features/technician/
  api/technician-api.ts                  # Query keys, hooks, mutations
  types/technician.types.ts              # DTO types, status enum
  components/
    TechnicianDashboard.tsx              # Main dashboard container
    TechnicianKpiCards.tsx               # 4 KPI cards
    TechnicianBanner.tsx                 # "Dang thuc hien" banner
    TechnicianToolbar.tsx                # Filter pills + search
    TechnicianQueueTable.tsx             # Patient queue table
    TechnicianActionMenu.tsx             # Status-based dropdown menu
    PatientResultsPanel.tsx              # Slide-over (Sheet) for "Xem ket qua"
    RedFlagDialog.tsx                    # Red flag confirmation dialog
    ReturnToQueueDialog.tsx              # Return confirmation dialog
frontend/src/app/routes/_authenticated/
  pre-exam.tsx                           # NEW - stub Pre-Exam page
```

### Pattern 1: Entity with String Enum Conversion (per D-03)
**What:** TechnicianOrderType stored as string, not int.
**Example from existing codebase:** Most enums use `HasConversion<int>()`. This entity specifically uses `HasConversion<string>()` per decision D-03.
```csharp
// TechnicianOrderConfiguration.cs
builder.Property(to => to.OrderType)
    .IsRequired()
    .HasConversion<string>()
    .HasMaxLength(20);
```

### Pattern 2: Auto-Creation Hook in AdvanceWorkflowStage
**What:** When visit advances to PreExam stage, auto-create a TechnicianOrder.
**Where:** Modify `AdvanceWorkflowStageHandler.Handle()` to detect `newStage == WorkflowStage.PreExam` and create the TechnicianOrder.
**Key consideration:** The handler currently only calls `visit.AdvanceStage()` + save. Need to inject `ITechnicianOrderRepository` (or add TechnicianOrders as a navigation collection on Visit and use domain method).

**Recommended approach:** Add `TechnicianOrders` navigation collection to Visit entity (like other child entities), add `Visit.CreatePreExamOrder()` domain method, call it from `AdvanceWorkflowStageHandler` when `newStage == PreExam`.

### Pattern 3: Dashboard Query (Following Receptionist Pattern)
**What:** Single handler that loads TechnicianOrders + joins Visit/Patient data, derives display status, applies filters/pagination.
**Reference:** `GetReceptionistDashboardHandler` in Scheduling module. Technician version is simpler -- single source table (TechnicianOrder) vs receptionist's appointment+visit union.

### Pattern 4: Role-Based Dashboard Rendering
**What:** `/dashboard` route checks user role and renders appropriate component.
**Existing code in `dashboard.tsx`:**
```typescript
const isReceptionist = user?.roles?.includes("Receptionist") ?? false
if (isReceptionist) return <ReceptionistDashboard />
```
**Add:**
```typescript
const isTechnician = user?.roles?.includes("Technician") ?? false
if (isTechnician) return <TechnicianDashboard />
```
**Order matters:** Technician check before default dashboard.

### Pattern 5: Polling + Manual Refresh
**What:** TanStack Query refetchInterval for automatic polling, plus manual refresh button.
**Reference:** Receptionist uses `refetchInterval: 15_000` for dashboard, `30_000` for KPI.
**Technician:** Same intervals per D-17. Wait time calculated client-side every 60s via `setInterval`.

### Anti-Patterns to Avoid
- **Do NOT put pre-exam fields on Visit directly** -- D-01 explicitly requires TechnicianOrder child entity
- **Do NOT use WebSocket** -- D-17 specifies polling only
- **Do NOT build pessimistic locking** -- D-15 specifies optimistic validation
- **Do NOT create UI for AdditionalExam creation** -- D-12 defers this
- **Do NOT build the Pre-Exam measurement form** -- only a stub page

## Runtime State Inventory

> This phase includes a rename (RefractionVA -> PreExam) so runtime state matters.

| Category | Items Found | Action Required |
|----------|-------------|------------------|
| Stored data | Visit.CurrentStage stored as int (value=1) in `clinical.Visits` table -- the integer value stays the same, only C# enum name changes | No data migration needed -- enum value 1 is unchanged |
| Stored data | `StageSkip.Stage` references `WorkflowStage.RefractionVA` as int in DB | No data migration -- int value unchanged |
| Stored data | `AllowedReversals` dictionary in Visit.cs uses `WorkflowStage.RefractionVA` key | Code-only rename |
| Live service config | None -- no external services reference "RefractionVA" string | None |
| OS-registered state | None -- no OS registrations reference the enum name | None |
| Secrets/env vars | None -- enum name not in secrets or env vars | None |
| Build artifacts | None -- C# compilation replaces enum names with int values at compile time | None |

**Key insight:** Because `WorkflowStage` uses `HasConversion<int>()` and `RefractionVA = 1` keeps the same integer value, the rename is purely a code-level change. No data migration is needed for this specific rename. The DB stores `1`, not `"RefractionVA"`. The migration only needs to create the new `TechnicianOrders` table.

**Frontend references to "RefractionVA":** The frontend uses the integer value (`1`) when calling `advanceStage` API. However, there may be string references in display logic or type definitions. All ~30 files from the grep must be checked and updated.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Slide-over panel | Custom drawer animation | shadcn Sheet (already installed, side="right") | Handles focus trap, overlay, animation, accessibility |
| Action dropdown menus | Custom popover with click-outside | shadcn DropdownMenu | Handles keyboard nav, submenus, positioning |
| Toast notifications | Custom notification system | Sonner (already installed via shadcn) | "BN da duoc nhan boi {ten}" toast |
| Status badges | Custom colored spans | shadcn Badge with custom variant classes | Consistent theming |
| Data table | Custom table with manual sorting | TanStack Table (already used in receptionist) | Column definitions, sorting, row model |
| Confirmation dialogs | Custom modal | shadcn AlertDialog | Red flag + return to queue confirmations |
| Form validation | Manual validation | React Hook Form + Zod | Red flag reason form |
| Time ago calculation | Custom interval | Simple `Date.now() - checkinTime` with 60s setInterval | Lightweight, no library needed |

## Common Pitfalls

### Pitfall 1: Enum Rename Breaking Tests
**What goes wrong:** Renaming `RefractionVA` to `PreExam` in the enum breaks ~15 test files that reference the old name.
**Why it happens:** Tests use `WorkflowStage.RefractionVA` directly.
**How to avoid:** Do the rename as the very first task. Run `dotnet build` and fix all compilation errors across the entire solution before proceeding.
**Warning signs:** Build failures in test projects.

### Pitfall 2: AdvanceWorkflowStage Auto-Creation Race Condition
**What goes wrong:** If two requests to advance to PreExam arrive simultaneously, two TechnicianOrders could be created.
**Why it happens:** No unique constraint on (VisitId, OrderType) for PreExam.
**How to avoid:** Add a unique index on `(VisitId, OrderType)` where `OrderType = 'PreExam'` (filtered index). Or check in the domain method: `if TechnicianOrders.Any(o => o.OrderType == PreExam) return`.
**Warning signs:** Duplicate TechnicianOrder rows.

### Pitfall 3: Optimistic Concurrency on Accept
**What goes wrong:** Two technicians click "Nhan BN" at the same time for the same patient.
**Why it happens:** No lock between read and write.
**How to avoid:** Per D-15, use optimistic validation. The `AcceptTechnicianOrder` handler checks if `TechnicianId` is already set. If so, return a structured error with the assigned technician's name. Frontend catches this, shows toast, and refreshes.
**Warning signs:** Silent overwrite of TechnicianId.

### Pitfall 4: Wait Time Calculation Timezone Issues
**What goes wrong:** Wait time shows wrong value because server UTC time vs local display time.
**Why it happens:** `checkinTime` stored as UTC, client calculates diff.
**How to avoid:** Ensure frontend converts UTC to local before calculating diff, or use raw UTC diff (which is timezone-invariant for duration calculations).

### Pitfall 5: Filter + Search + Polling Interaction
**What goes wrong:** Polling response overwrites filtered view, or search resets on poll refresh.
**Why it happens:** TanStack Query refetch uses stale query key without current filter state.
**How to avoid:** Include filter/search params in query key (already the pattern: `technicianKeys.dashboard(filters)`). Ensure search state is debounced but not cleared on refetch.

### Pitfall 6: Missing Technician Role in Seed Data
**What goes wrong:** Cannot test the dashboard because no user has Technician role.
**Why it happens:** Auth seed data may not include a Technician user.
**How to avoid:** Check if Technician role exists in seed data. If not, add a test user with Technician role or assign role to existing test account.

## Code Examples

### TechnicianOrder Entity
```csharp
// Source: Pattern from Visit.cs child entities
public class TechnicianOrder : Entity, IAuditable
{
    public Guid VisitId { get; private set; }
    public TechnicianOrderType OrderType { get; private set; }
    public Guid? TechnicianId { get; private set; }
    public string? TechnicianName { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public bool IsRedFlag { get; private set; }
    public string? RedFlagReason { get; private set; }
    public DateTime? RedFlaggedAt { get; private set; }
    public Guid? OrderedByDoctorId { get; private set; }
    public string? OrderedByDoctorName { get; private set; }
    public string? Instructions { get; private set; }
    public DateTime OrderedAt { get; private set; }

    private TechnicianOrder() { }

    public static TechnicianOrder CreatePreExam(Guid visitId)
    {
        return new TechnicianOrder
        {
            VisitId = visitId,
            OrderType = TechnicianOrderType.PreExam,
            OrderedAt = DateTime.UtcNow
        };
    }

    public void Accept(Guid technicianId, string technicianName)
    {
        if (TechnicianId.HasValue)
            throw new InvalidOperationException(
                $"Order already accepted by {TechnicianName}");
        TechnicianId = technicianId;
        TechnicianName = technicianName;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (!TechnicianId.HasValue)
            throw new InvalidOperationException("Order must be accepted first");
        CompletedAt = DateTime.UtcNow;
    }

    public void ReturnToQueue()
    {
        TechnicianId = null;
        TechnicianName = null;
        StartedAt = null;
    }

    public void MarkRedFlag(string reason)
    {
        IsRedFlag = true;
        RedFlagReason = reason;
        RedFlaggedAt = DateTime.UtcNow;
        CompletedAt = DateTime.UtcNow; // Red flag also completes the order
    }
}
```

### Dashboard Query Status Derivation (per D-08)
```csharp
private static string DeriveStatus(TechnicianOrder order, Guid currentTechnicianId)
{
    if (order.IsRedFlag) return "red_flag";
    if (order.CompletedAt.HasValue) return "completed";
    if (order.TechnicianId == currentTechnicianId && !order.CompletedAt.HasValue)
        return "in_progress";
    if (!order.TechnicianId.HasValue && !order.CompletedAt.HasValue)
        return "waiting";
    // Assigned to different technician -- still show as waiting to others
    return "waiting";
}
```

### Frontend Query Key Factory
```typescript
// Source: receptionist-api.ts pattern
export const technicianKeys = {
  all: ["technician"] as const,
  dashboard: (filters: TechnicianDashboardFilters) =>
    [...technicianKeys.all, "dashboard", filters] as const,
  kpi: () => [...technicianKeys.all, "kpi"] as const,
}
```

### EF Migration Command
```bash
# Add TechnicianOrder table
dotnet ef migrations add AddTechnicianOrder \
  -p backend/src/Modules/Clinical/Clinical.Infrastructure \
  -s backend/src/Bootstrapper \
  --context ClinicalDbContext

# Apply
dotnet ef database update \
  -p backend/src/Modules/Clinical/Clinical.Infrastructure \
  -s backend/src/Bootstrapper \
  --context ClinicalDbContext
```

### Color Constants (from Spec)
```typescript
// Status badge colors
export const technicianStatusColors = {
  waiting:    { bg: "#FAEEDA", text: "#854F0B" },  // Cho kham (amber)
  in_progress:{ bg: "#E6F1FB", text: "#0C447C" },  // Dang do (blue)
  red_flag:   { bg: "#FCEBEB", text: "#791F1F" },  // Red flag (red)
  completed:  { bg: "#E1F5EE", text: "#085041" },  // Hoan tat (teal)
} as const

// KPI card accent colors
export const technicianKpiColors = {
  waiting:    "#BA7517",  // Amber
  inProgress: "#185FA5",  // Blue
  completed:  "#0F6E56",  // Teal
  redFlag:    "#A32D2D",  // Red
} as const
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Fields on Visit entity | Separate TechnicianOrder child entity | D-01 decision | Cleaner separation, supports future AdditionalExam |
| `RefractionVA` stage name | `PreExam` stage name | D-06 decision | Better semantics, breaking rename |
| WebSocket for real-time | Polling (15s/30s/60s) | D-17 decision | Simpler infra, sufficient for clinic scale |

## Open Questions

1. **Technician Role in Seed Data**
   - What we know: Auth module has role management, test account is Admin
   - What's unclear: Whether a Technician user exists for testing
   - Recommendation: Check seed data early; if missing, add a test Technician user as part of setup

2. **"Dang do" Visibility Across Technicians**
   - What we know: D-08 says "Dang do" = TechnicianId matches current user. Spec Section 10 says Technician cannot see patients assigned to other technicians.
   - What's unclear: Should rows assigned to OTHER technicians be hidden entirely or shown as "Cho kham"?
   - Recommendation: Show as "Cho kham" in the "Tat ca" filter (they appear as waiting from this technician's perspective). In the "Dang do" filter pill, only show own patients.

3. **Visit AdvanceStage Hook Pattern**
   - What we know: Current handler is simple (visit.AdvanceStage + save). Need to add TechnicianOrder creation.
   - What's unclear: Whether to add TechnicianOrders as navigation collection on Visit or create via separate repository.
   - Recommendation: Add as Visit navigation collection (consistent with all other child entities: Refractions, ImagingRequests, etc.). Add `Visit.CreatePreExamOrder()` domain method.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.x + NSubstitute + FluentAssertions |
| Config file | `backend/tests/Clinical.Unit.Tests/Clinical.Unit.Tests.csproj` |
| Quick run command | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~Technician" --no-restore -v q` |
| Full suite command | `dotnet test backend/tests/Clinical.Unit.Tests --no-restore` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| D-01 | TechnicianOrder entity creation | unit | `dotnet test --filter "TechnicianOrder"` | Wave 0 |
| D-04 | Auto-create on PreExam advance | unit | `dotnet test --filter "AdvanceWorkflowStage"` | Exists (modify) |
| D-06 | RefractionVA -> PreExam rename compiles | unit | `dotnet build` | Existing tests |
| D-08 | Status derivation logic | unit | `dotnet test --filter "TechnicianDashboard"` | Wave 0 |
| D-09 | KPI counts | unit | `dotnet test --filter "TechnicianKpi"` | Wave 0 |
| D-15 | Concurrent accept rejection | unit | `dotnet test --filter "AcceptTechnicianOrder"` | Wave 0 |
| Actions | Accept/Complete/Return/RedFlag | unit | `dotnet test --filter "TechnicianOrder"` | Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~Technician" --no-restore -v q`
- **Per wave merge:** `dotnet test backend/tests/Clinical.Unit.Tests --no-restore`
- **Phase gate:** Full suite green + manual dashboard verification at localhost:3000

### Wave 0 Gaps
- [ ] `backend/tests/Clinical.Unit.Tests/Domain/TechnicianOrderTests.cs` -- entity domain logic
- [ ] `backend/tests/Clinical.Unit.Tests/Features/GetTechnicianDashboardTests.cs` -- dashboard query
- [ ] `backend/tests/Clinical.Unit.Tests/Features/GetTechnicianKpiStatsTests.cs` -- KPI query
- [ ] `backend/tests/Clinical.Unit.Tests/Features/AcceptTechnicianOrderTests.cs` -- accept + concurrency
- [ ] `backend/tests/Clinical.Unit.Tests/Features/CompleteTechnicianOrderTests.cs` -- complete
- [ ] `backend/tests/Clinical.Unit.Tests/Features/ReturnToQueueTests.cs` -- return to queue
- [ ] `backend/tests/Clinical.Unit.Tests/Features/RedFlagTechnicianOrderTests.cs` -- red flag

## Sources

### Primary (HIGH confidence)
- Direct codebase analysis: Visit.cs, WorkflowStage.cs, AdvanceWorkflowStage.cs, VisitConfiguration.cs, ClinicalDbContext.cs
- Direct codebase analysis: ReceptionistDashboard.tsx, receptionist-api.ts, KpiCards.tsx, receptionist.types.ts
- Direct codebase analysis: GetReceptionistDashboardHandler.cs, GetReceptionistKpiStatsHandler.cs
- Spec document: `docs/dev/technician/technician-dashboard.md`

### Secondary (MEDIUM confidence)
- CONTEXT.md decisions D-01 through D-17 (user-validated architecture decisions)

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - all libraries already installed and patterns proven in Phase 14
- Architecture: HIGH - follows exact same patterns as receptionist dashboard, just different module
- Pitfalls: HIGH - based on direct code analysis and understanding of EF Core/DDD patterns
- Rename scope: HIGH - grep found all 30 files referencing RefractionVA

**Research date:** 2026-03-29
**Valid until:** 2026-04-28 (stable codebase, no external dependency changes expected)

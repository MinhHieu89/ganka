# Phase 15: Implement Technician Dashboard - Context

**Gathered:** 2026-03-29
**Status:** Ready for planning

<domain>
## Phase Boundary

Implement the Technician (Pre-Exam) dashboard: role-based view on `/dashboard` showing the patient queue for pre-exam screening. Includes KPI cards, filter pills, search, patient table with status-based action menus, "Đang thực hiện" banner, and a slide-over panel for viewing patient results. Backend includes the `TechnicianOrder` entity, dashboard query/KPI endpoints, and all status transition commands (accept, complete, return to queue, red flag).

**In scope:**
- Full dashboard UI per spec (KPI cards, banner, toolbar, table, action menus)
- `TechnicianOrder` entity with `OrderType` enum (`PreExam` / `AdditionalExam`)
- Rename `WorkflowStage.RefractionVA` → `PreExam` (migration + all references)
- Auto-create `TechnicianOrder(PreExam)` when Visit advances to PreExam stage
- All technician actions: Nhận BN, Tiếp tục đo, Hoàn tất chuyển BS, Trả lại hàng đợi, Chuyển BS ngay, Xem kết quả
- Stub Pre-Exam screen (placeholder navigation target)
- Slide-over side panel for "Xem kết quả"

**Out of scope:**
- Pre-Exam measurement form (separate phase)
- "Đo bổ sung" flow / `AdditionalExam` creation (needs doctor UI — data model is ready)
- Section 9 of the spec (doctor-ordered additional exams)

</domain>

<decisions>
## Implementation Decisions

### Data Model: TechnicianOrder Entity
- **D-01:** All pre-exam tracking lives in a `TechnicianOrder` child entity on Visit — NO pre-exam fields directly on Visit. This entity handles both initial pre-exam and future doctor-ordered additional exams via `OrderType` enum.
- **D-02:** `TechnicianOrderType` enum has two values: `PreExam` (auto-created when visit enters PreExam stage) and `AdditionalExam` (doctor-ordered, built in future phase).
- **D-03:** Enum stored as **string** in DB (`HasConversion<string>()`), not int. Makes DB human-readable (`"PreExam"` instead of `0`).
- **D-04:** Auto-create `TechnicianOrder(OrderType=PreExam)` when receptionist advances Visit to PreExam stage.
- **D-05:** `TechnicianOrder` fields: Id, VisitId, OrderType, TechnicianId (nullable), TechnicianName, StartedAt, CompletedAt, IsRedFlag, RedFlagReason, RedFlaggedAt, OrderedByDoctorId (nullable), OrderedByDoctorName, Instructions (nullable), OrderedAt.

### WorkflowStage Rename
- **D-06:** Rename `WorkflowStage.RefractionVA = 1` → `WorkflowStage.PreExam = 1`. Breaking rename requiring migration + update all backend/frontend references.

### Dashboard Query & Status Derivation
- **D-07:** Dashboard queries `TechnicianOrder` table (single source, no union needed for Phase 15). Join with Visit and Patient for display fields.
- **D-08:** Technician display statuses derived from TechnicianOrder fields:
  - Chờ khám = `TechnicianId IS NULL AND CompletedAt IS NULL`
  - Đang đo = `TechnicianId = currentUser AND CompletedAt IS NULL`
  - Red flag = `IsRedFlag = true`
  - Hoàn tất = `CompletedAt IS NOT NULL` (today)
- **D-09:** KPI counts query the same TechnicianOrder table with appropriate filters.
- **D-10:** "Loại" column: `PreExam` + no prior visits = "Mới", `PreExam` + has prior visits = "Tái khám", `AdditionalExam` = "Đo bổ sung" badge.

### Scope & Navigation
- **D-11:** Dashboard + stub navigation. "Nhận BN" and "Tiếp tục đo" navigate to a placeholder Pre-Exam page. All status transitions and backend logic work fully. Pre-Exam measurement form is a separate phase.
- **D-12:** "Đo bổ sung" flow deferred — requires doctor-side EMR integration. The `TechnicianOrder` entity supports it via `OrderType=AdditionalExam` but no UI to create them yet.

### View Results Panel
- **D-13:** "Xem kết quả" opens a **slide-over side panel** (drawer from right). New UI pattern — keeps the queue table partially visible. Shows read-only: personal info, visit history, pre-exam data collected (if any).

### Backend Architecture
- **D-14:** All technician dashboard endpoints live in the **Clinical module**. Pre-exam is a clinical activity — keeps Scheduling focused on appointments.

### Concurrency & Assignment
- **D-15:** 1 patient per technician at a time — **optimistic validation**. Backend validates on accept: if TechnicianOrder already has a technician assigned, return error. Frontend shows toast "BN đã được nhận bởi {tên}" and refreshes the list. No pessimistic locking or WebSocket needed.

### Route & Role Rendering
- **D-16:** Same `/dashboard` route with role-based rendering (consistent with Phase 14 receptionist pattern). Add Technician role check → render `<TechnicianDashboard />`.
- **D-17:** Polling-based real-time updates (no WebSocket). Dashboard rows: 15s, KPI cards: 30s, wait time: 60s client-side calculation.

### Claude's Discretion
- Internal component structure within `features/technician/`
- API endpoint naming and DTO shapes (follow Clinical module patterns)
- Query invalidation strategy for polling + manual refresh
- Slide-over panel component implementation (shadcn Sheet or custom)
- Stub Pre-Exam page design (minimal placeholder)
- i18n key structure for technician namespace

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Technician Spec & Mockup
- `docs/dev/technician/technician-dashboard.md` — Full spec: layout, KPI cards, banner, toolbar, table columns, action menus, status badges, edge cases, permissions, realtime requirements
- `docs/dev/technician/technician_dashboard.html` — Visual mockup with exact styling, colors, component structure, interactive filter pills and dropdown menus

### Backend Domain (Must Read Before Modifying)
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs` — Visit aggregate, current fields, AdvanceStage/ReverseStage methods
- `backend/src/Modules/Clinical/Clinical.Domain/Enums/WorkflowStage.cs` — Current enum (RefractionVA → PreExam rename target)
- `backend/src/Modules/Clinical/Clinical.Domain/Enums/VisitStatus.cs` — Visit immutability status enum
- `backend/src/Modules/Clinical/Clinical.Application/Features/AdvanceWorkflowStage.cs` — Stage transition logic (auto-create TechnicianOrder hook point)

### Receptionist Reference (Pattern to Follow)
- `frontend/src/features/receptionist/components/ReceptionistDashboard.tsx` — Dashboard component structure reference
- `frontend/src/features/receptionist/components/KpiCards.tsx` — KPI card pattern
- `frontend/src/features/receptionist/components/StatusFilterPills.tsx` — Filter pill pattern
- `frontend/src/features/receptionist/components/PatientQueueTable.tsx` — Queue table pattern
- `frontend/src/features/receptionist/api/receptionist-api.ts` — API hooks, query keys, polling pattern
- `frontend/src/features/receptionist/types/receptionist.types.ts` — DTO types pattern
- `backend/src/Modules/Scheduling/Scheduling.Application/Features/GetReceptionistDashboard.cs` — Dashboard query + status mapping reference
- `backend/src/Modules/Scheduling/Scheduling.Application/Features/GetReceptionistKpiStats.cs` — KPI query reference

### Frontend Infrastructure
- `frontend/src/app/routes/_authenticated/dashboard.tsx` — Dashboard route (add Technician role check)
- `frontend/src/shared/stores/authStore.ts` — Auth store, role checking
- `frontend/src/shared/lib/api-client.ts` — API client with auth middleware

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- **shadcn/ui components**: Card, Badge, Button, Input, DropdownMenu, ToggleGroup, Sheet (for slide-over), Skeleton — all installed
- **TanStack Table**: Used in receptionist queue table — reuse for technician table
- **usePatientSearch hook**: Debounced search with autocomplete — reuse for dashboard search
- **Tabler Icons React**: Icon library already in use
- **React Hook Form + Zod**: Established form pattern for red flag dialog validation

### Established Patterns
- **API client**: OpenAPI Fetch with typed endpoints in `features/{module}/api/{module}-api.ts`
- **Query keys factory**: `receptionistKeys.dashboard(filters)` pattern — replicate as `technicianKeys`
- **Polling**: `refetchInterval: 15_000` on dashboard query, `30_000` on KPI
- **Role-based dashboard**: `user?.roles?.includes("Receptionist")` check in dashboard.tsx
- **Modular monolith backend**: Vertical slice features (handler + validator + request/response), Wolverine bus
- **Entity configuration**: `HasConversion<int>()` for most enums, but TechnicianOrderType uses `HasConversion<string>()`

### Integration Points
- `/dashboard` route — Add `user?.roles?.includes("Technician")` check
- Clinical module — New `TechnicianOrder` entity, new feature handlers
- `AdvanceWorkflowStage` handler — Hook to auto-create `TechnicianOrder(PreExam)` when advancing to PreExam
- Visit entity — Add `TechnicianOrders` navigation collection
- Sidebar — Verify Technician role has Clinical.View permission for dashboard access

</code_context>

<specifics>
## Specific Ideas

### Color System from Spec
- Chờ khám badge: bg `#FAEEDA`, text `#854F0B` (amber)
- Đang đo badge: bg `#E6F1FB`, text `#0C447C` (blue)
- Red flag badge: bg `#FCEBEB`, text `#791F1F` (red)
- Hoàn tất badge: bg `#E1F5EE`, text `#085041` (teal)
- KPI colors: Amber `#BA7517`, Blue `#185FA5`, Teal `#0F6E56`, Red `#A32D2D`
- Banner: bg `#E6F1FB`, border `#B5D4F4`
- Primary action button: `#534AB7` (purple)
- Wait time: amber default, red `#A32D2D` if >= 25 minutes

### Row Styling
- "Đang đo" row pinned to top of table
- "Hoàn tất" rows dimmed at opacity 0.55
- Red flag patient name and reason in red text `#A32D2D`

</specifics>

<deferred>
## Deferred Ideas

- Pre-Exam measurement form (Step 1 & Step 2) — separate phase
- "Đo bổ sung" flow (Section 9) — needs doctor-side EMR integration, data model ready via `TechnicianOrder(AdditionalExam)`
- Keyboard shortcuts — polish phase
- Notification sounds — polish phase
- Auto-save on Pre-Exam form — needs draft storage infrastructure

</deferred>

---

*Phase: 15-implement-technician-dashboard-according-to-the-requirement-and-mockup*
*Context gathered: 2026-03-29*

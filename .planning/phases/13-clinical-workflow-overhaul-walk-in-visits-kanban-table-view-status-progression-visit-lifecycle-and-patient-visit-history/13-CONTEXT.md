# Phase 13: Clinical Workflow Overhaul - Context

**Gathered:** 2026-03-25
**Status:** Ready for planning

<domain>
## Phase Boundary

Fix and enhance the clinical workflow system to support a complete visit lifecycle. This includes: fixing the kanban board to have 1 column per workflow stage (8 columns instead of 5), adding a table view alternative, enabling bidirectional stage transitions for specific stages, auto-advancing workflow on doctor sign-off, defining visit completion rules, and adding a patient visit history tab with timeline + detail panel layout.

Also folds in 4 pending todos: patient name link to detail page, OSDI answer viewing, auto-expand optical prescription section, and realtime OSDI score updates.

This phase does NOT include: new clinical data entry features, new workflow stages, billing changes, or appointment system changes.

</domain>

<decisions>
## Implementation Decisions

### Kanban Columns & Status Mapping
- **D-01:** Each of the 8 workflow stages gets its own kanban column (1:1 mapping). No more grouping stages into combined columns.
- **D-02:** Column order: Reception, Refraction/VA, Doctor Exam, Diagnostics, Doctor Reads, Rx, Cashier, Pharmacy/Optical.
- **D-03:** Horizontal scroll for overflow — all 8 columns at comfortable width, scroll to see off-screen columns (Trello/Jira pattern).
- **D-04:** A 9th "Done" column shows completed visits for the current day, then they disappear.

### View Toggle (Kanban/Table)
- **D-05:** Toggle button with grid/list icons in the dashboard toolbar header. Remember last selection per user via localStorage.
- **D-06:** Table view shows a full patient list with sortable columns: Patient, Doctor, Stage, Wait Time, Visit Time, Status. Sortable and filterable.

### Status Reversal & Progression Rules
- **D-07:** Specific stages allow backward movement (not all stages). Claude's discretion on which backward transitions make clinical sense based on real-world ophthalmology clinic workflow best practices.
- **D-08:** Claude's discretion on role-based restrictions for who can reverse stages.
- **D-09:** Stage reversal requires a mandatory reason (audit trail), similar to how amendments require a reason.

### Visit Completion Lifecycle
- **D-10:** A visit is considered "done" when it reaches the last stage (PharmacyOptical = 7). Reaching this stage triggers removal from active kanban (but shows in Done column for current day).
- **D-11:** Doctor sign-off automatically advances the visit to the next workflow stage. No manual stage advancement needed after sign-off.
- **D-12:** Staff can skip stages that aren't needed (e.g., jump from DoctorExam directly to Rx, skipping Diagnostics and DoctorReads).

### Patient Visit History Tab
- **D-13:** New "Visit History" tab added to the patient profile page (alongside Overview, Appointments, Allergies, Dry Eye tabs).
- **D-14:** 2-column layout: vertical timeline on the left, visit details panel on the right. Clicking a visit in the timeline updates the right panel.
- **D-15:** Timeline cards show compact summary: date, doctor name, primary diagnosis, visit status badge (Draft/Signed/Amended).
- **D-16:** Detail panel shows the same sections as the existing VisitDetailPage (refraction, dry eye, diagnosis, prescriptions, images, notes, sign-off info) in read-only mode.

### Folded Todos
- **D-17:** Patient name in visit kanban cards and visit detail page links to patient profile page.
- **D-18:** OSDI questionnaire answers visible in the visit detail page (currently only shows scores).
- **D-19:** Optical Prescription section auto-expands when data exists (currently collapsed by default).
- **D-20:** Realtime OSDI score update on visit detail page when questionnaire is completed.

### Claude's Discretion
- Which specific backward stage transitions are allowed (based on clinical best practices research)
- Role-based restrictions for stage reversal
- Walk-in patient creation improvements (if any UX issues found during implementation)
- Table view filtering options and column widths
- Timeline card styling and spacing
- How to handle the Done column filtering (time-based or day-based cutoff)
- Loading states and error handling for all new features

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Clinical Workflow (Frontend)
- `frontend/src/features/clinical/components/WorkflowDashboard.tsx` — Current kanban dashboard implementation with 5-column grouping (lines 43-49 define the column mapping that needs to change)
- `frontend/src/features/clinical/components/KanbanColumn.tsx` — Individual column container component
- `frontend/src/features/clinical/components/PatientCard.tsx` — Draggable patient card in kanban
- `frontend/src/features/clinical/components/NewVisitDialog.tsx` — Walk-in visit creation dialog
- `frontend/src/features/clinical/components/SignOffSection.tsx` — Doctor sign-off component (needs auto-advance integration)
- `frontend/src/features/clinical/api/clinical-api.ts` — React Query hooks for clinical endpoints

### Clinical Workflow (Backend)
- `backend/src/Modules/Clinical/Clinical.Domain/Enums/WorkflowStage.cs` — 8 workflow stage enum (Reception=0 through PharmacyOptical=7)
- `backend/src/Modules/Clinical/Clinical.Domain/Enums/VisitStatus.cs` — 4 visit status enum (Draft, Signed, Amended, Cancelled)
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs` — Visit aggregate root with AdvanceStage method (currently forward-only)

### Patient Profile (Frontend)
- `frontend/src/app/routes/_authenticated/patients/$patientId.tsx` — Patient profile page with existing tabs
- `frontend/src/features/clinical/components/VisitDetailPage.tsx` — Existing visit detail page (sections to reuse in history detail panel)

### Visit Detail Components (to reuse in history panel)
- `frontend/src/features/clinical/components/RefractionSection.tsx` — Refraction data display
- `frontend/src/features/clinical/components/DryEyeSection.tsx` — Dry eye assessment display
- `frontend/src/features/clinical/components/DiagnosisSection.tsx` — ICD-10 diagnosis display
- `frontend/src/features/clinical/components/DrugPrescriptionSection.tsx` — Drug prescription display
- `frontend/src/features/clinical/components/OpticalPrescriptionSection.tsx` — Optical prescription display
- `frontend/src/features/clinical/components/OsdiAnswersSection.tsx` — OSDI answers display

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `WorkflowDashboard.tsx` — Existing kanban with @dnd-kit/core for drag-and-drop. Column config is a simple array that can be updated from 5 to 8+ entries.
- `PatientCard.tsx` — Draggable card component with patient info, allergy warning, wait time badge. Can be reused as-is for 8 columns.
- `VisitDetailPage.tsx` — Full visit detail with all clinical sections. Sections can be extracted/reused for the history detail panel.
- `clinical-api.ts` — React Query hooks (useActiveVisits, useAdvanceStage, useSignOffVisit) — need extension for new features.
- shadcn/ui components — Table, Tabs, Badge, Button, Dialog components already in use.

### Established Patterns
- Kanban columns defined as array of objects with `id`, `title`, `stages[]`, `accentColor`
- Drag-and-drop uses @dnd-kit/core with DndContext/useDroppable/useDraggable
- API follows CQRS pattern: Commands for mutations, Queries for reads
- Visit entity uses domain events for state changes
- Frontend uses TanStack Router file-based routing
- Patient profile uses tab-based layout

### Integration Points
- Route: `/clinical` — WorkflowDashboard mounts here (add table view)
- Route: `/patients/$patientId` — PatientProfilePage (add Visit History tab)
- Route: `/visits/$visitId` — VisitDetailPage (add auto-advance on sign-off)
- Backend: `AdvanceWorkflowStageHandler` — Needs backward transition support
- Backend: `SignOffVisitHandler` — Needs auto-advance logic
- Backend: `GetActiveVisitsAsync` — Needs "done" status filtering
- Backend: New endpoint needed for patient visit history query

</code_context>

<specifics>
## Specific Ideas

- The 5-column to 8-column change is in `WorkflowDashboard.tsx` lines 43-49 where `WORKFLOW_COLUMNS` array groups stages
- Sign-off auto-advance should be a backend domain event — when visit is signed off, automatically call AdvanceStage
- Done column should filter by `VisitDate` being today (not by a separate timer)
- Patient visit history tab should use the same scrollable medical chart layout as the existing VisitDetailPage but in read-only mode
- Timeline on the left should be narrow (~300px) with the detail panel taking remaining space
- Walk-in visit creation already works via NewVisitDialog — verify it handles patients without appointments correctly

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

### Reviewed Todos (not folded)
- "Chart view for dry eye metrics across all visits" — Already addressed by dry eye chart feature in prior phases
- "Print all pharmacy labels at once" — Out of scope, belongs in pharmacy module improvements
- "Auto focus search field when opening Add Drug form" — Minor UX fix, not related to workflow overhaul

</deferred>

---

*Phase: 13-clinical-workflow-overhaul*
*Context gathered: 2026-03-25*

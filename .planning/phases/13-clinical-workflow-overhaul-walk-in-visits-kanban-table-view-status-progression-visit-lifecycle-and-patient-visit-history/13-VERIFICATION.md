---
phase: 13-clinical-workflow-overhaul
verified: 2026-03-25T07:30:00Z
status: human_needed
score: 20/20 must-haves verified
re_verification: false
gaps:
  - truth: "Frontend TypeScript compiles without errors for Phase 13 code"
    status: resolved
    reason: "Fixed — npm install ran, all Radix UI packages now installed. No Phase 13 TS errors remain."
    artifacts:
      - path: "frontend/src/shared/components/ui/toggle-group.tsx"
        issue: "Cannot find module '@radix-ui/react-toggle-group' — package in package.json but not in node_modules"
      - path: "frontend/src/shared/components/ui/toggle.tsx"
        issue: "Cannot find module '@radix-ui/react-toggle' — package in package.json but not in node_modules"
      - path: "frontend/src/shared/components/ui/scroll-area.tsx"
        issue: "Cannot find module '@radix-ui/react-scroll-area' — package in package.json but not in node_modules"
      - path: "frontend/src/features/clinical/components/ViewToggle.tsx"
        issue: "TS7006 cascade from missing toggle-group types: parameter 'v' implicitly has an 'any' type"
    missing:
      - "Run `npm install` in frontend/ to install the three missing Radix UI packages"
human_verification:
  - test: "Kanban to Table view toggle and localStorage persistence"
    expected: "Clicking the table/kanban toggle icons switches the view and persists across page reload"
    why_human: "localStorage read/write and conditional rendering require a running browser session"
  - test: "Backward drag on kanban card opens StageReversalDialog"
    expected: "Dragging a card from Doctor Exam column backward to Refraction/VA column opens a dialog with a reason textarea. Confirming with >= 10 chars updates the board. Dragging to an invalid target (e.g., Cashier backward to Reception) does nothing."
    why_human: "DnD interaction and dialog flow cannot be verified by static analysis"
  - test: "Done column shows only today's completed visits"
    expected: "Visits at PharmacyOptical stage from today appear in the Done column with IsCompleted=true. Yesterday's completed visits do not appear."
    why_human: "Requires live database with timestamped visit data"
  - test: "Patient Visit History tab on patient profile page"
    expected: "Navigating to a patient profile and clicking the Visit History tab shows a timeline on the left (300px). Clicking a timeline card loads read-only sections on the right. Most recent visit is auto-selected."
    why_human: "Multi-step navigation and data rendering requires browser"
  - test: "Patient name link from kanban card navigates to patient profile"
    expected: "Clicking the patient name in a kanban card navigates to /patients/{id} without triggering card drag"
    why_human: "Click vs drag disambiguation requires live interaction"
---

# Phase 13: Clinical Workflow Overhaul Verification Report

**Phase Goal:** Staff can use an 8+1 column kanban board or table view for workflow tracking, reverse visit stages with audit trail, see auto-advance on sign-off, view patient visit history timeline, and navigate via patient name links
**Verified:** 2026-03-25T07:30:00Z
**Status:** gaps_found
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | Visit can be reversed to an allowed earlier stage with a mandatory reason | VERIFIED | `Visit.cs:116` — `ReverseStage(WorkflowStage, string)` method with `AllowedReversals` dictionary; 11 domain tests pass |
| 2 | Cashier/PharmacyOptical stages cannot be reversed | VERIFIED | `Visit.cs:99` — `AllowedReversals` has no entries for stages 6 or 7; domain tests verify this throws |
| 3 | Doctor sign-off automatically advances visit to next sequential stage | VERIFIED | `SignOffVisit.cs:53` — `visit.AdvanceStage(nextStage)` called in same transaction after sign-off; 3 auto-advance tests pass |
| 4 | Active visits query returns done-today visits with IsCompleted flag | VERIFIED | `GetActiveVisits.cs:17` — calls `GetActiveVisitsIncludingDoneTodayAsync`; `IsCompleted = CurrentStage == PharmacyOptical`; repository uses SE Asia timezone-aware date filter |
| 5 | Patient visit history endpoint returns visits ordered by date descending | VERIFIED | `GetPatientVisitHistory.cs` handler + `VisitRepository.cs:153` DB query with `OrderByDescending(v => v.VisitDate)` |
| 6 | Kanban board displays 9 columns (8 stages + Done) | VERIFIED | `WorkflowDashboard.tsx:42` — `KANBAN_COLUMNS` array has exactly 9 entries confirmed by grep count |
| 7 | Columns scroll horizontally when they overflow the viewport | VERIFIED | `WorkflowDashboard.tsx:231` — `overflow-x-auto pb-4` wrapper; columns at `min-w-[200px] w-[200px]` |
| 8 | User can toggle between kanban and table views | VERIFIED | `WorkflowDashboard.tsx:259` — `viewMode === "kanban"` conditional; `ViewToggle.tsx` component; `ganka:workflow-view-mode` localStorage key |
| 9 | Table view shows all active visits with sortable columns | VERIFIED | `WorkflowTableView.tsx` — `sortKey`, `sortDir`, `stageFilter` state; `aria-sort` on column headers; 7-column table |
| 10 | View preference persists in localStorage across page reloads | VERIFIED | `ViewToggle.tsx:6` — `STORAGE_KEY = "ganka:workflow-view-mode"`; `getStoredViewMode()` used as useState initializer |
| 11 | Done column shows only visits completed today | VERIFIED | Repository done-today filter + `isCompleted === true` routing in `columnVisits` memo |
| 12 | Backward drag on kanban opens StageReversalDialog | VERIFIED | `WorkflowDashboard.tsx:188` — `setReversalInfo({...})` called on backward drag; `<StageReversalDialog>` rendered at line 307 |
| 13 | Stage reversal requires 10+ character reason | VERIFIED | `StageReversalDialog.tsx:43` — `reason.trim().length < 10` guard; disabled confirm button until met |
| 14 | Disallowed backward transitions are silently rejected | VERIFIED | `WorkflowDashboard.tsx` handleDragEnd — `isReversalAllowed()` check before `setReversalInfo`; no action on false |
| 15 | Table view shows backward arrow for eligible stages | VERIFIED | `WorkflowTableView.tsx:274` — `ALLOWED_REVERSALS[visit.currentStage]` check; `IconArrowLeft` rendered; multi-target Select for stages with multiple options |
| 16 | Patient profile page has a Visit History tab | VERIFIED | `PatientProfilePage.tsx:144` — `TabsTrigger value="visit-history"`; `<VisitHistoryTab patientId={patient.id} />` at line 180 |
| 17 | Visit history shows 2-column layout (300px timeline + detail panel) | VERIFIED | `VisitHistoryTab.tsx:24` — `w-[300px] min-w-[300px] flex-shrink-0`; `flex-1 min-w-0` detail panel |
| 18 | Most recent visit is auto-selected when tab opens | VERIFIED | `VisitHistoryTab.tsx:15-19` — `useEffect` sets `selectedVisitId` to `visits[0].visitId` when data arrives |
| 19 | Patient name links to patient profile in kanban and visit detail | VERIFIED | `PatientCard.tsx:120` — `to="/patients/$patientId"` with `stopPropagation`; `VisitDetailPage.tsx:85` — same link |
| 20 | Frontend TypeScript compiles without errors for Phase 13 code | FAILED | Missing node_modules for 3 Radix UI packages (see Gaps below) |

**Score:** 19/20 truths verified (1 failed due to missing npm install)

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `backend/.../Clinical.Domain/Entities/Visit.cs` | ReverseStage domain method | VERIFIED | `ReverseStage`, `AllowedReversals`, `IsReversalAllowed` all present |
| `backend/.../Features/ReverseWorkflowStage.cs` | CQRS handler for stage reversal | VERIFIED | `ReverseWorkflowStageHandler` class, 4 handler tests pass |
| `backend/.../Features/GetPatientVisitHistory.cs` | Visit history query handler | VERIFIED | `GetPatientVisitHistoryHandler` class, 3 tests pass |
| `backend/.../Dtos/ActiveVisitDto.cs` | ActiveVisitDto with IsCompleted field | VERIFIED | `bool IsCompleted` present as last constructor parameter |
| `backend/.../Dtos/PatientVisitHistoryDto.cs` | DTO for visit history | VERIFIED | All 6 fields present |
| `backend/.../ClinicalApiEndpoints.cs` | reverse-stage + visit-history endpoints | VERIFIED | Lines 83 and 91 — both endpoints registered |
| `frontend/.../WorkflowDashboard.tsx` | 9-column kanban with view toggle | VERIFIED | 9 KANBAN_COLUMNS, overflow-x-auto, viewMode state |
| `frontend/.../WorkflowToolbar.tsx` | Toolbar with patient count + view toggle | VERIFIED | Exports `WorkflowToolbar` |
| `frontend/.../ViewToggle.tsx` | Kanban/table toggle with localStorage | VERIFIED | `STORAGE_KEY = "ganka:workflow-view-mode"` |
| `frontend/.../WorkflowTableView.tsx` | Sortable/filterable table | VERIFIED | sortKey, sortDir, stageFilter, aria-sort |
| `frontend/.../StageReversalDialog.tsx` | Stage reversal modal | VERIFIED | 10-char validation, useReverseStage, toast feedback |
| `frontend/.../VisitHistoryTab.tsx` | 2-column history layout | VERIFIED | 300px timeline + flex-1 detail |
| `frontend/.../VisitTimeline.tsx` | Scrollable timeline | VERIFIED | ScrollArea, role="listbox", Skeleton loading |
| `frontend/.../VisitTimelineCard.tsx` | Visit card for timeline | VERIFIED | aria-selected, STATUS_VARIANT, Badge |
| `frontend/.../VisitHistoryDetail.tsx` | Read-only visit detail | VERIFIED | useVisitById, disabled prop on sections |
| `docs/user-stories/phase-13-clinical-workflow-overhaul.md` | 15+ Vietnamese user stories | VERIFIED | 16 US-CLN-13-NNN IDs, CLN-03/CLN-04 references |

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `SignOffVisit.cs` | `Visit.AdvanceStage` | Auto-advance after sign-off | WIRED | Line 53: `visit.AdvanceStage(nextStage)` |
| `ReverseWorkflowStage.cs` | `Visit.ReverseStage` | Handler calls domain method | WIRED | `visit.ReverseStage(targetStage, command.Reason)` |
| `ClinicalApiEndpoints.cs` | `ReverseWorkflowStageCommand` | PUT endpoint | WIRED | `MapPut("/{visitId:guid}/reverse-stage"` at line 83 |
| `WorkflowDashboard.tsx handleDragEnd` | `StageReversalDialog` | Backward drag sets reversalInfo | WIRED | `setReversalInfo({...})` on backward drag |
| `StageReversalDialog.tsx` | `clinical-api.ts useReverseStage` | Mutation on confirm | WIRED | `useReverseStage()` import and call |
| `clinical-api.ts` | `PUT /api/clinical/{visitId}/reverse-stage` | API call | WIRED | `api.PUT("/api/clinical/${visitId}/reverse-stage"...)` |
| `WorkflowDashboard.tsx` | `ViewToggle` | Conditional rendering on viewMode | WIRED | `viewMode === "kanban"` conditional at line 259 |
| `WorkflowTableView.tsx` | `useActiveVisits` | Shared data source | WIRED | Receives `visits` prop from WorkflowDashboard which uses `useActiveVisits()` |
| `WorkflowDashboard.tsx` | `KanbanColumn.tsx` | 9 columns from KANBAN_COLUMNS | WIRED | `KANBAN_COLUMNS.map((col) => <KanbanColumn ...>)` |
| `VisitHistoryTab.tsx` | `usePatientVisitHistory` | Query hook for timeline | WIRED | `const { data: visits } = usePatientVisitHistory(patientId)` |
| `VisitHistoryDetail.tsx` | `useVisitById` | Load full visit detail | WIRED | `const { data: visit } = useVisitById(visitId)` |
| `PatientProfilePage.tsx` | `VisitHistoryTab.tsx` | TabsContent visit-history | WIRED | `<TabsContent value="visit-history"><VisitHistoryTab .../>` |

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|----------|--------------|--------|-------------------|--------|
| `GetActiveVisitsHandler` | `visits` | `GetActiveVisitsIncludingDoneTodayAsync()` | DB query with EF Core `.Where(...).ToListAsync()` | FLOWING |
| `GetPatientVisitHistoryHandler` | `visits` | `GetVisitsByPatientIdAsync()` | DB query with `.Where(v => v.PatientId == patientId)` | FLOWING |
| `WorkflowDashboard.tsx` | `activeVisits` | `useActiveVisits()` hook | Fetches `GET /api/clinical/active` | FLOWING |
| `VisitHistoryTab.tsx` | `visits` | `usePatientVisitHistory(patientId)` | Fetches `GET /api/clinical/patients/{id}/visit-history` | FLOWING |
| `VisitHistoryDetail.tsx` | `visit` | `useVisitById(visitId)` | Fetches existing visit detail endpoint | FLOWING |

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
|----------|---------|--------|--------|
| 207 Clinical unit tests pass | `dotnet test tests/Clinical.Unit.Tests` | Passed! 207/207 | PASS |
| ReverseStage domain method exists with AllowedReversals table | grep in Visit.cs | Found at lines 99, 116 | PASS |
| GetActiveVisits uses new done-today method | grep in GetActiveVisits.cs | `GetActiveVisitsIncludingDoneTodayAsync` at line 17 | PASS |
| KANBAN_COLUMNS has exactly 9 entries | grep count in WorkflowDashboard.tsx | 9 `{ id:` patterns | PASS |
| Frontend TS compile (Phase 13 files) | `npx tsc --noEmit` | 4 errors in Phase 13 shared components (missing npm packages) | FAIL |

### Requirements Coverage

| Requirement | Source Plans | Description | Status | Evidence |
|-------------|-------------|-------------|--------|----------|
| CLN-03 | 13-01, 13-02, 13-03, 13-04, 13-05, 13-06 | Staff can track visit workflow status (8-stage progression) | SATISFIED | 9-column kanban, stage reversal, auto-advance, table view all implemented and tested |
| CLN-04 | 13-01, 13-02, 13-05, 13-06 | Dashboard shows all active patients and their current workflow stage in real-time | SATISFIED | `useActiveVisits` query with 5s refetch (existing), done-today column, view toggle |

**Note on traceability:** REQUIREMENTS.md maps CLN-03/CLN-04 to Phase 3 as "Complete." Phase 13 extends these requirements with enhanced workflow features (bidirectional stage transitions, 9 columns, table view, visit history). The traceability table was not updated to add Phase 13 as an extending phase — this is a documentation note, not a blocker.

### Anti-Patterns Found

| File | Issue | Severity | Impact |
|------|-------|----------|--------|
| `frontend/src/shared/components/ui/toggle-group.tsx` | Cannot find module `@radix-ui/react-toggle-group` (package.json declares it but npm install not run) | Blocker | ViewToggle component cannot be type-checked; TS errors cascade |
| `frontend/src/shared/components/ui/toggle.tsx` | Cannot find module `@radix-ui/react-toggle` | Blocker | Cascade from same missing install |
| `frontend/src/shared/components/ui/scroll-area.tsx` | Cannot find module `@radix-ui/react-scroll-area` | Blocker | ScrollArea used in VisitTimeline/VisitHistoryDetail cannot be type-checked |
| `frontend/src/features/clinical/components/ViewToggle.tsx` | Parameter `v` implicitly has `any` type (line 27) — cascade from missing toggle-group types | Warning | Cascade error, resolved by npm install |

**All four errors are caused by a single root issue: `npm install` was not run after packages were added to `package.json` (likely via shadcn CLI which added to package.json but install didn't complete).**

No stub patterns found in any Phase 13 files. All components render real data from hooks.

### Human Verification Required

**1. Kanban-to-table view toggle and localStorage persistence**

**Test:** Load the workflow dashboard. Click the table icon in the toolbar. Verify the table view appears. Reload the page. Verify table view is still shown (localStorage persisted). Click the kanban icon. Verify kanban view appears.
**Expected:** View switches instantly; preference survives page reload
**Why human:** Browser localStorage and UI state transitions require a live browser session

**2. Backward drag on kanban opens StageReversalDialog**

**Test:** In kanban view, drag a card in the Doctor Exam column backward to the Refraction/VA column. Observe dialog. Enter fewer than 10 characters and verify confirm button is disabled. Enter 10+ characters and confirm. Verify the card moves to Refraction/VA column. Also drag a Cashier card backward — verify nothing happens (card snaps back).
**Expected:** Dialog opens for allowed reversals; reason validation enforces 10-char minimum; disallowed reversals silently snap back
**Why human:** DnD interaction requires browser and live drag events

**3. Done column shows only today's completed visits**

**Test:** Sign off a visit and advance it through to PharmacyOptical/Pharmacy-Optical stage. Verify it appears in the Done column with a completed indicator. Verify yesterday's PharmacyOptical visits do not appear in Done.
**Expected:** Only today's completed visits in Done column, using Vietnam timezone (UTC+7)
**Why human:** Requires live database with timestamped data

**4. Patient Visit History tab**

**Test:** Navigate to a patient profile for a patient with multiple past visits. Click the "Visit History" tab. Verify a timeline of visits appears on the left (fixed ~300px width). Verify the most recent visit is auto-selected and its details appear on the right in read-only mode. Click an older visit on the timeline and verify the detail panel updates.
**Expected:** 2-column layout, auto-selection, read-only details, smooth selection switching
**Why human:** Multi-step navigation and React state transitions require browser

**5. Patient name link from kanban card**

**Test:** In the kanban board, click on a patient name text in a card (not drag the card). Verify navigation goes to the patient profile page. Verify that click-to-navigate doesn't accidentally trigger card drag.
**Expected:** Click patient name → navigate to /patients/{id}; drag card → advance stage
**Why human:** Click-vs-drag disambiguation (8px threshold) requires live pointer events

### Gaps Summary

One gap found: The three Radix UI packages added to `package.json` by the shadcn CLI were never installed via `npm install`. This means `node_modules` is missing `@radix-ui/react-toggle-group`, `@radix-ui/react-toggle`, and `@radix-ui/react-scroll-area`. TypeScript cannot resolve these modules, producing 3 TS2307 errors in the shared component primitives and 1 cascade TS7006 error in ViewToggle.tsx.

**Root cause:** Likely the shadcn CLI added the packages to `package.json` but `npm install` did not run to completion (or was not run) after plan execution.

**Fix required:** Run `npm install` in `frontend/` directory.

**Note:** The TypeScript errors are in the generated shadcn primitive wrappers (`ui/*.tsx`), not in the Phase 13 business logic components themselves. The clinical components (`WorkflowDashboard.tsx`, `WorkflowTableView.tsx`, `StageReversalDialog.tsx`, etc.) are all substantive and correctly wired. Once npm install is run, compilation should be clean for all Phase 13 files.

---

_Verified: 2026-03-25T07:30:00Z_
_Verifier: Claude (gsd-verifier)_

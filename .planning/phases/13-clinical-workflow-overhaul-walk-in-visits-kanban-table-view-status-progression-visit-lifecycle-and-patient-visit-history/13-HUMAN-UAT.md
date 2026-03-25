---
status: diagnosed
phase: 13-clinical-workflow-overhaul
source: [13-VERIFICATION.md]
started: 2026-03-25T07:35:00Z
updated: 2026-03-25T08:15:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Kanban to Table view toggle and localStorage persistence
expected: Clicking the table/kanban toggle icons switches the view and persists across page reload
result: pass

### 2. Backward drag on kanban card opens StageReversalDialog
expected: Dragging a card from Doctor Exam column backward to Refraction/VA column opens a dialog with a reason textarea. Confirming with >= 10 chars updates the board. Dragging to an invalid target (e.g., Cashier backward to Reception) does nothing.
result: pass

### 3. Done column shows only today's completed visits
expected: Visits at PharmacyOptical stage from today appear in the Done column with IsCompleted=true. Yesterday's completed visits do not appear.
result: issue
reported: "when drag into PharmacyOptical, it should stay instead of auto Done"
severity: major

### 4. Patient Visit History tab on patient profile page
expected: Navigating to a patient profile and clicking the Visit History tab shows a timeline on the left (300px). Clicking a timeline card loads read-only sections on the right. Most recent visit is auto-selected.
result: issue
reported: "the design too ugly. Need to use frontend design to redo this view"
severity: cosmetic

### 5. Patient name link from kanban card navigates to patient profile
expected: Clicking the patient name in a kanban card navigates to /patients/{id} without triggering card drag
result: pass

## Summary

total: 5
passed: 3
issues: 2
pending: 0
skipped: 0
blocked: 0

## Gaps

- truth: "Visits dragged to PharmacyOptical stage should remain in PharmacyOptical column, not auto-advance to Done"
  status: failed
  reason: "User reported: when drag into PharmacyOptical, it should stay instead of auto Done"
  severity: major
  test: 3
  root_cause: "GetActiveVisits.cs sets IsCompleted=true when CurrentStage==PharmacyOptical (stage 7, the last stage). Frontend then sorts any isCompleted visit into Done column. Fix: add WorkflowStage.Done=8 enum value, only set IsCompleted when stage==Done."
  artifacts:
    - "backend/src/Modules/Clinical/Clinical.Application/Features/GetActiveVisits.cs:28-29"
    - "frontend/src/features/clinical/components/WorkflowDashboard.tsx:116-117"
  missing: []

- truth: "Patient Visit History tab should have polished, well-designed UI"
  status: failed
  reason: "User reported: the design too ugly. Need to use frontend design to redo this view"
  severity: cosmetic
  test: 4
  root_cause: "Visit History tab UI needs redesign using frontend-design skill for polished layout"
  artifacts:
    - "frontend/src/features/clinical/components/PatientVisitHistory (or similar)"
  missing: []

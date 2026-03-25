---
status: partial
phase: 13-clinical-workflow-overhaul
source: [13-VERIFICATION.md]
started: 2026-03-25T07:35:00Z
updated: 2026-03-25T07:35:00Z
---

## Current Test

[awaiting human testing]

## Tests

### 1. Kanban to Table view toggle and localStorage persistence
expected: Clicking the table/kanban toggle icons switches the view and persists across page reload
result: [pending]

### 2. Backward drag on kanban card opens StageReversalDialog
expected: Dragging a card from Doctor Exam column backward to Refraction/VA column opens a dialog with a reason textarea. Confirming with >= 10 chars updates the board. Dragging to an invalid target (e.g., Cashier backward to Reception) does nothing.
result: [pending]

### 3. Done column shows only today's completed visits
expected: Visits at PharmacyOptical stage from today appear in the Done column with IsCompleted=true. Yesterday's completed visits do not appear.
result: [pending]

### 4. Patient Visit History tab on patient profile page
expected: Navigating to a patient profile and clicking the Visit History tab shows a timeline on the left (300px). Clicking a timeline card loads read-only sections on the right. Most recent visit is auto-selected.
result: [pending]

### 5. Patient name link from kanban card navigates to patient profile
expected: Clicking the patient name in a kanban card navigates to /patients/{id} without triggering card drag
result: [pending]

## Summary

total: 5
passed: 0
issues: 0
pending: 5
skipped: 0
blocked: 0

## Gaps

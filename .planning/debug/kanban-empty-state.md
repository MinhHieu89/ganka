---
status: diagnosed
trigger: "Kanban Dashboard shows empty state instead of 5 columns at /clinical"
created: 2026-03-09T00:00:00Z
updated: 2026-03-09T00:00:00Z
---

## Current Focus

hypothesis: CONFIRMED - Mutually exclusive conditional rendering hides Kanban columns when totalPatients is 0
test: Read WorkflowDashboard.tsx lines 217-250
expecting: Found the exact condition
next_action: Report root cause

## Symptoms

expected: Navigate to /clinical. Five columns render with Vietnamese labels (Tiep nhan, Kham nghiem, Bac si, Xu ly, Hoan tat). Sidebar shows "Clinical" nav item as active.
actual: Shows "Khong co benh nhan dang trong quy trinh" (empty state message) instead of the 5-column layout.
errors: None reported
reproduction: Navigate to /clinical when no active visits exist
started: Unknown

## Eliminated

## Evidence

- timestamp: 2026-03-09T00:01:00Z
  checked: WorkflowDashboard.tsx lines 217-250
  found: Lines 218-222 render empty state when totalPatients === 0. Lines 225-250 render Kanban board ONLY when totalPatients > 0. These are mutually exclusive conditions.
  implication: When there are zero active visits, the empty state message replaces the entire Kanban board including all 5 columns.

- timestamp: 2026-03-09T00:01:30Z
  checked: KanbanColumn.tsx
  found: KanbanColumn component correctly handles empty visits array (renders column structure with 0 count badge and empty droppable area). The column itself renders fine with no cards.
  implication: The columns CAN display in empty state - the problem is purely that the parent never renders them.

- timestamp: 2026-03-09T00:02:00Z
  checked: clinical.json translation key "workflow.noActivePatients"
  found: Translates to "Khong co benh nhan dang trong quy trinh" - matches the user-reported empty state message exactly.
  implication: Confirms the empty state branch at line 218-222 is what the user sees.

## Resolution

root_cause: In WorkflowDashboard.tsx lines 217-250, the Kanban board and the empty state message use mutually exclusive conditional rendering. When totalPatients === 0 (line 218), only the empty state message renders. The Kanban columns with DndContext (line 225) only render when totalPatients > 0. This means the 5-column layout is completely hidden whenever there are no active visits, instead of showing 5 empty columns.
fix:
verification:
files_changed: []

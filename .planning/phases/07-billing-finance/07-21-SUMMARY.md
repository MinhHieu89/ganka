---
phase: 07-billing-finance
plan: 21
subsystem: ui
tags: [react, tanstack-router, tanstack-query, shift-management, cash-reconciliation, shadcn-ui, i18n]

# Dependency graph
requires:
  - phase: 07-billing-finance
    provides: shift-api.ts hooks (useCurrentShift, useOpenShift, useCloseShift, useShiftReport, useShiftTemplates), billing-api.ts, format-vnd.ts
provides:
  - ShiftOpenDialog component with template selector and opening balance
  - ShiftCloseDialog component with live cash discrepancy calculation
  - ShiftReportView component with revenue by method and cash reconciliation
  - Shifts route page with current shift card and history table
  - useShiftHistory hook for paginated shift history
affects: [07-billing-finance, billing-ui, shift-management]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Expandable DataTable row pattern for inline detail view"
    - "Live form calculation (discrepancy = actual - expected)"
    - "Confirmation AlertDialog for destructive actions with discrepancy"

key-files:
  created:
    - frontend/src/features/billing/components/ShiftReportView.tsx
    - frontend/src/app/routes/_authenticated/billing/shifts.tsx
  modified:
    - frontend/src/features/billing/api/shift-api.ts
    - frontend/public/locales/en/billing.json
    - frontend/public/locales/vi/billing.json
    - frontend/src/shared/components/AppSidebar.tsx
    - frontend/src/app/routeTree.gen.ts

key-decisions:
  - "Used expandable DataTable rows for shift history instead of separate detail page for faster cashier workflow"
  - "Inline ShiftReportView within expandable row avoids navigation and keeps context"
  - "AlertDialog confirmation only shown when discrepancy is non-zero (streamlined UX)"

patterns-established:
  - "Expandable DataTable row: click to toggle, renderSubRow renders inline detail component"
  - "Cash reconciliation display: color-coded discrepancy (green=match, red=deficit, blue=surplus)"

requirements-completed: [FIN-10]

# Metrics
duration: 9min
completed: 2026-03-06
---

# Phase 07 Plan 21: Shift Management UI Summary

**Shift management page with open/close dialogs, live cash discrepancy, expandable shift history with inline report view**

## Performance

- **Duration:** 9 min
- **Started:** 2026-03-06T13:59:41Z
- **Completed:** 2026-03-06T14:08:24Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments
- ShiftOpenDialog with shift template selector and VND opening balance input
- ShiftCloseDialog with live discrepancy calculation (surplus/deficit/match), AlertDialog confirmation, and manager note highlighting
- ShiftReportView with revenue-by-method table, cash reconciliation card, and PDF print button
- Shifts route page showing current shift status card and paginated history DataTable with expandable rows for inline report

## Task Commits

Each task was committed atomically:

1. **Task 1: Create ShiftOpenDialog and ShiftCloseDialog** - `b6a7aa7` (feat, pre-existing from 07-20 batch execution)
2. **Task 2: Create ShiftReportView and shifts route page** - `2cf202f` (feat)

**Plan metadata:** pending (docs: complete plan)

## Files Created/Modified
- `frontend/src/features/billing/components/ShiftOpenDialog.tsx` - Dialog to open shift with template selection and opening balance
- `frontend/src/features/billing/components/ShiftCloseDialog.tsx` - Dialog to close shift with live cash discrepancy and AlertDialog confirmation
- `frontend/src/features/billing/components/ShiftReportView.tsx` - Shift report with revenue by method table and cash reconciliation card
- `frontend/src/app/routes/_authenticated/billing/shifts.tsx` - Shifts management page with current shift card and history DataTable
- `frontend/src/features/billing/api/shift-api.ts` - Added useShiftHistory hook and getShiftHistory API function
- `frontend/public/locales/en/billing.json` - EN translations for shift management columns
- `frontend/public/locales/vi/billing.json` - VI translations for shift management columns
- `frontend/src/shared/components/AppSidebar.tsx` - Enabled billing sidebar with shifts navigation link
- `frontend/src/app/routeTree.gen.ts` - Auto-generated route tree with billing/shifts route

## Decisions Made
- Used expandable DataTable rows (click row to expand inline ShiftReportView) instead of navigating to a separate detail page, for faster cashier workflow
- AlertDialog confirmation only appears when cash discrepancy is non-zero, streamlining the common case where cash matches
- Manager note field gets yellow highlight when discrepancy exists, recommending but not requiring explanation

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added useShiftHistory hook and getShiftHistory API function**
- **Found during:** Task 2 (shifts route page)
- **Issue:** shift-api.ts had no history list endpoint or hook, needed for the shift history DataTable
- **Fix:** Added ShiftHistoryResult type, shiftKeys.list() key, getShiftHistory API function, and useShiftHistory hook
- **Files modified:** frontend/src/features/billing/api/shift-api.ts
- **Verification:** TypeScript compilation passes
- **Committed in:** 2cf202f (Task 2 commit)

**2. [Rule 3 - Blocking] Added i18n translation keys for shift history table**
- **Found during:** Task 2 (shifts route page)
- **Issue:** Missing translation keys for shift table columns (closedAt, date, shiftStatus, noShiftHistory, viewReport, shiftsSubtitle)
- **Fix:** Added keys to both EN and VI billing.json locale files
- **Files modified:** frontend/public/locales/en/billing.json, frontend/public/locales/vi/billing.json
- **Verification:** Translation keys referenced correctly in components
- **Committed in:** 2cf202f (Task 2 commit)

**3. [Rule 3 - Blocking] Enabled billing sidebar navigation**
- **Found during:** Task 2 (shifts route page)
- **Issue:** AppSidebar had billing link disabled and no children navigation items
- **Fix:** Enabled billing section with billingDashboard and billingShifts child links
- **Files modified:** frontend/src/shared/components/AppSidebar.tsx
- **Verification:** Sidebar renders billing section with sub-links
- **Committed in:** 2cf202f (Task 2 commit)

---

**Total deviations:** 3 auto-fixed (3 blocking)
**Impact on plan:** All auto-fixes were necessary prerequisites for the shift management UI to function. No scope creep.

## Issues Encountered
- Task 1 files (ShiftOpenDialog, ShiftCloseDialog) were already committed from a previous batch execution (07-20). Verified they match plan requirements and used existing commit b6a7aa7.
- Used getSortedRowModel (correct TanStack Table v8 API) instead of getSortingRowModel initially written.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Shift management UI complete with all CRUD operations
- Ready for billing dashboard integration (plan 07-18 routes) and checkout flow
- PDF shift report print requires backend endpoint to be operational

## Self-Check: PASSED

All 5 created/modified files verified on disk. Both commit hashes (b6a7aa7, 2cf202f) verified in git history.

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*

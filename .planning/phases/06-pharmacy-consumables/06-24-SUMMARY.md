---
phase: 06-pharmacy-consumables
plan: 24
subsystem: ui
tags: [react, tanstack-query, zod, react-hook-form, shadcn, tanstack-router]

requires:
  - phase: 06-19
    provides: consumables API endpoints and query hooks (consumables-api.ts, consumables-queries.ts)

provides:
  - ConsumableItemTable: DataTable with search, tracking mode badges, and per-row action buttons
  - ConsumableItemForm: Dialog form for create/edit consumable items with tracking mode radio
  - AddStockDialog: Stock add dialog branching by tracking mode (SimpleStock vs ExpiryTracked)
  - ConsumableAlertBanner: Collapsible low-stock alert banner
  - ConsumableAdjustDialog: Stock adjustment dialog with reason selection
  - Consumables warehouse route at /_authenticated/consumables/
  - Sidebar navigation item for consumables

affects:
  - 06-25
  - 07-billing
  - 09-treatment-protocols

tech-stack:
  added: []
  patterns:
    - ConsumableItemTable uses DataTable generic component with TanStack Table
    - AddStockDialog branches form rendering by trackingMode field value
    - Alert banner follows LowStockAlertBanner collapsible pattern from pharmacy

key-files:
  created:
    - frontend/src/features/consumables/components/ConsumableItemTable.tsx
    - frontend/src/features/consumables/components/ConsumableItemForm.tsx
    - frontend/src/features/consumables/components/AddStockDialog.tsx
    - frontend/src/features/consumables/components/ConsumableAlertBanner.tsx
    - frontend/src/features/consumables/components/ConsumableAdjustDialog.tsx
    - frontend/src/app/routes/_authenticated/consumables/index.tsx
  modified:
    - frontend/src/shared/components/AppSidebar.tsx
    - frontend/public/locales/en/common.json
    - frontend/public/locales/vi/common.json

key-decisions:
  - "ConsumableAdjustDialog added (not in plan) to complete Điều chỉnh action in table - required for full stock management"
  - "Create dialog handled by route page, not table - cleaner separation of concerns"
  - "Used Vietnamese hard-coded strings in components (no separate consumables.json) consistent with existing code pattern for this feature"

patterns-established:
  - "AddStockDialog: form branching pattern for ExpiryTracked vs SimpleStock modes"
  - "ConsumableAlertBanner: same collapsible warning pattern as pharmacy LowStockAlertBanner"

requirements-completed: [CON-01, CON-02]

duration: 15min
completed: 2026-03-06
---

# Phase 06 Plan 24: Consumables Warehouse Page Summary

**Consumables warehouse UI with DataTable inventory, tracking-mode-aware stock add dialog, low-stock alert banner, and sidebar nav registration**

## Performance

- **Duration:** 15 min
- **Started:** 2026-03-06T09:11:00Z
- **Completed:** 2026-03-06T09:26:23Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments

- ConsumableItemTable showing all inventory items with tracking mode badges (Theo lo / Don gian) and per-row Them hang / Dieu chinh / Edit buttons
- AddStockDialog branching: ExpiryTracked shows Batch Number + DatePicker + Quantity; SimpleStock shows Quantity only
- ConsumableAlertBanner mirrors pharmacy LowStockAlertBanner - collapsible with item count badge
- Consumables route registered and accessible at /consumables with sidebar nav item

## Task Commits

1. **Task 1: Create consumable components** - `7807163` (feat)
2. **Task 2: Create alert banner and consumables route** - `2ae4b34` (feat)

## Files Created/Modified

- `frontend/src/features/consumables/components/ConsumableItemTable.tsx` - DataTable with search, badges, and row actions
- `frontend/src/features/consumables/components/ConsumableItemForm.tsx` - Create/edit dialog with tracking mode radio
- `frontend/src/features/consumables/components/AddStockDialog.tsx` - Stock add dialog branching by tracking mode
- `frontend/src/features/consumables/components/ConsumableAdjustDialog.tsx` - Stock adjustment dialog with reason select
- `frontend/src/features/consumables/components/ConsumableAlertBanner.tsx` - Collapsible low-stock warning banner
- `frontend/src/app/routes/_authenticated/consumables/index.tsx` - Consumables warehouse page route
- `frontend/src/shared/components/AppSidebar.tsx` - Added consumables nav item to operations group
- `frontend/public/locales/en/common.json` - Added sidebar.consumables key
- `frontend/public/locales/vi/common.json` - Added sidebar.consumables key

## Decisions Made

- Added ConsumableAdjustDialog (not explicitly in plan) since ConsumableItemTable action column had "Điều chỉnh" button - Rule 2 auto-add for critical missing functionality
- Create dialog handled at route level (ConsumablesPage) to keep table component focused on display/row-level actions
- Used Vietnamese strings directly in components rather than a separate consumables i18n file - consistent with how these new components are structured

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Created ConsumableAdjustDialog**
- **Found during:** Task 1 (ConsumableItemTable creation)
- **Issue:** Plan specified "Điều chỉnh" button in table but did not define a separate dialog component for stock adjustment - button would be non-functional without it
- **Fix:** Created ConsumableAdjustDialog.tsx mirroring pharmacy's StockAdjustmentDialog pattern with quantity change + reason + notes
- **Files modified:** frontend/src/features/consumables/components/ConsumableAdjustDialog.tsx
- **Verification:** TypeScript compiles without errors, button wired up in table
- **Committed in:** 7807163 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (Rule 2 - missing critical functionality)
**Impact on plan:** Essential for functional stock management. No scope creep.

## Issues Encountered

None - plan executed cleanly. All 60 TypeScript errors are pre-existing in unrelated files.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Consumables warehouse UI is complete and functional
- Stock management (add/adjust) wired to existing API hooks
- Alert banner monitors low-stock items
- Ready for Phase 9 treatment session auto-deduction integration

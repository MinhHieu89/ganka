---
phase: 06-pharmacy-consumables
plan: 20
subsystem: pharmacy-frontend
tags: [frontend, pharmacy, inventory, alerts, components]
dependency_graph:
  requires: [06-19]
  provides: [DrugInventoryTable, DrugBatchTable, ExpiryAlertBanner, LowStockAlertBanner, pharmacy-inventory-route]
  affects: [pharmacy-ui]
tech_stack:
  added: []
  patterns: [DataTable-expandable-rows, collapsible-alert-banners, FEFO-display, TanStack-Table-getExpandedRowModel]
key_files:
  created:
    - frontend/src/features/pharmacy/components/DrugInventoryTable.tsx
    - frontend/src/features/pharmacy/components/DrugBatchTable.tsx
    - frontend/src/features/pharmacy/components/ExpiryAlertBanner.tsx
    - frontend/src/features/pharmacy/components/LowStockAlertBanner.tsx
  modified:
    - frontend/src/app/routes/_authenticated/pharmacy/index.tsx
    - frontend/public/locales/en/pharmacy.json
    - frontend/public/locales/vi/pharmacy.json
decisions:
  - DrugInventoryTable uses inline EditPricingDialog rather than separate file for cohesion
  - Action buttons (suppliers, stock-import) rendered as disabled placeholders since routes don't exist yet
  - FEFO ordering enforced by default sort (ExpiryDate ASC) in DrugBatchTable
  - Alert banners use Collapsible pattern matching project's shadcn wrapper
metrics:
  duration: 4min
  completed: "2026-03-06T08:54:31Z"
  tasks: 2
  files: 7
---

# Phase 6 Plan 20: Pharmacy Drug Inventory Page Summary

Pharmacy drug inventory page with tables, batch details, and alert banners for primary pharmacy staff view.

## What Was Built

DrugInventoryTable with expandable rows using TanStack Table's getExpandedRowModel, DrugBatchTable with FEFO default sort, and collapsible ExpiryAlertBanner/LowStockAlertBanner components. Route updated to compose all components.

## Tasks Completed

| Task | Description | Commit | Files |
|------|-------------|--------|-------|
| 1 | DrugInventoryTable + DrugBatchTable | 4f7cc02 | 4 files (2 new components, 2 i18n files) |
| 2 | ExpiryAlertBanner + LowStockAlertBanner + route | c8973d6 | 3 files |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] TanStack Router type error for non-existent sub-routes**
- **Found during:** Task 2
- **Issue:** `<Link to="/pharmacy/suppliers">` and `<Link to="/pharmacy/stock-import">` caused TypeScript errors because those routes don't exist yet
- **Fix:** Rendered action buttons as disabled `<Button disabled>` placeholders instead of route links; future plans will enable them when routes are created
- **Files modified:** `frontend/src/app/routes/_authenticated/pharmacy/index.tsx`
- **Commit:** c8973d6

## Self-Check

Checking created files exist:
- DrugInventoryTable.tsx: FOUND
- DrugBatchTable.tsx: FOUND
- ExpiryAlertBanner.tsx: FOUND
- LowStockAlertBanner.tsx: FOUND
- pharmacy/index.tsx: FOUND (updated)

Checking commits:
- 4f7cc02: FOUND (feat(06-20): add DrugInventoryTable and DrugBatchTable components)
- c8973d6: FOUND (feat(06-20): add ExpiryAlertBanner, LowStockAlertBanner, pharmacy inventory page route)

TypeScript: 60 errors in 9 pre-existing files, 0 errors in new pharmacy files.

## Self-Check: PASSED

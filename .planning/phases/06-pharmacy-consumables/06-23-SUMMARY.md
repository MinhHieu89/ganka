---
phase: 06-pharmacy-consumables
plan: 23
subsystem: pharmacy-frontend
tags: [otc-sales, stock-adjustment, react, tanstack-query, react-hook-form]
dependency_graph:
  requires:
    - 06-19
  provides:
    - OtcSaleForm component
    - StockAdjustmentDialog component
    - /pharmacy/otc-sales route
  affects:
    - pharmacy frontend feature
tech_stack:
  added: []
  patterns:
    - React Hook Form with useFieldArray for dynamic line items
    - useWatch for reactive total calculation
    - Popover + Command combobox for drug search with price auto-fill
    - Dialog with controlled form and reset-on-open pattern
key_files:
  created:
    - frontend/src/features/pharmacy/components/OtcSaleForm.tsx
    - frontend/src/features/pharmacy/components/StockAdjustmentDialog.tsx
    - frontend/src/app/routes/_authenticated/pharmacy/otc-sales.tsx
  modified:
    - frontend/public/locales/en/pharmacy.json
    - frontend/public/locales/vi/pharmacy.json
decisions:
  - OTC sale form uses isAnonymous toggle (walk-in vs named customer) rather than patient search combobox, matching the quick-sale intent and deferring patient lookup to Phase 7
  - StockAdjustmentDialog takes drugCatalogItemId (not batchId) to match backend AdjustStockInput interface
  - OTC sales page uses xl:grid-cols-[480px_1fr] layout to show form and history side-by-side on wide screens
metrics:
  duration_minutes: 15
  completed_date: "2026-03-06"
  tasks_completed: 2
  tasks_total: 2
  files_created: 3
  files_modified: 2
---

# Phase 06 Plan 23: OTC Sales Page and Stock Adjustment Dialog Summary

**One-liner:** Walk-in OTC sale form with anonymous/named customer toggle and auto-fill drug prices, plus manual stock adjustment dialog with reason selection and live quantity preview.

## Tasks Completed

| # | Name | Commit | Files |
|---|------|--------|-------|
| 1 | Create OTC sale form and stock adjustment dialog | a2650c6 | OtcSaleForm.tsx, StockAdjustmentDialog.tsx, pharmacy.json (EN/VI) |
| 2 | Create OTC sales route | 00a3a87 | otc-sales.tsx |

## What Was Built

### OtcSaleForm (`frontend/src/features/pharmacy/components/OtcSaleForm.tsx`)
- Quick walk-in sale form using React Hook Form with useFieldArray for dynamic line items
- Customer section: toggle between "Khach vang lai" (anonymous) and named customer with name input
- Drug search combobox (Popover + Command) with live filtering by Vietnamese/English name and generic name
- Auto-fill selling price from drug catalog when drug is selected
- Reactive total calculation via useWatch (no server round-trip needed)
- Notes field, submit via useCreateOtcSale mutation with success toast
- No payment collection (deferred to Phase 7 per spec)

### StockAdjustmentDialog (`frontend/src/features/pharmacy/components/StockAdjustmentDialog.tsx`)
- Dialog for manual stock adjustment on a specific drug (by drugCatalogItemId)
- Props: batch (BatchInfo with drugCatalogItemId, drugName, currentQuantity)
- Live "Current -> New" preview with color indicator (red if below zero, primary if changed)
- Quantity change field (positive to add, negative to subtract)
- Reason select: Correction (0), WriteOff (1), Damage (2), Expired (3), Other (4)
- Validation: rejects changes that would bring stock below 0
- Submit via useAdjustStock mutation, form resets on dialog open

### OTC Sales Route (`frontend/src/app/routes/_authenticated/pharmacy/otc-sales.tsx`)
- Two-section layout: OtcSaleForm card + history DataTable card
- Responsive: stacks on small screens, side-by-side (480px + 1fr) on xl+
- History columns: Customer (with "Khach vang lai" badge for anonymous), Date, Items, Total Amount
- Loading skeleton while fetching sales history

## i18n Keys Added
- `otcSale.*` - 26 keys in EN and VI
- `stockAdjust.*` - 12 keys in EN and VI

## Deviations from Plan

None - plan executed exactly as written.

The plan mentioned `drugBatchId` as a prop for StockAdjustmentDialog but the backend `AdjustStockInput` uses `drugCatalogItemId`. The interface was designed to match the existing API contract rather than a non-existent batchId field.

## Self-Check

- [x] OtcSaleForm.tsx created at correct path
- [x] StockAdjustmentDialog.tsx created at correct path
- [x] otc-sales.tsx route created at correct path
- [x] TypeScript check: no errors in new files
- [x] Task 1 commit: a2650c6
- [x] Task 2 commit: 00a3a87

## Self-Check: PASSED

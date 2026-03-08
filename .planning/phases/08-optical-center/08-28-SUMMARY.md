---
phase: 08-optical-center
plan: 28
subsystem: optical-frontend
tags: [frontend, lens-catalog, data-table, form-dialog, tanstack-router]
dependency_graph:
  requires: [08-25]
  provides: [lens-catalog-ui]
  affects: [optical-navigation]
tech_stack:
  added: []
  patterns:
    - TanStack Table with getExpandedRowModel for expandable stock entries
    - Dual-mode dialog (catalog item vs stock adjustment) via mode prop
    - Flags bitfield encode/decode for lens coatings (decodeCoatings/encodeCoatings)
    - LowLensStockAlert collapsible banner mirroring pharmacy LowStockAlertBanner
key_files:
  created:
    - frontend/src/features/optical/components/LensCatalogTable.tsx
    - frontend/src/features/optical/components/LensFormDialog.tsx
    - frontend/src/features/optical/components/LensCatalogPage.tsx
    - frontend/src/app/routes/_authenticated/optical/lenses.tsx
  modified:
    - frontend/src/features/optical/api/optical-api.ts
decisions:
  - "Used TanStack Table built-in getExpandedRowModel for stock entry sub-rows instead of custom Set state; DataTable renderSubRow prop already checks row.getIsExpanded()"
  - "LensFormDialog uses three modes (create/edit/stock) via single mode prop to keep dialog management simple in parent"
  - "Coatings encoded as bitfield flags matching backend LensCoating [Flags] enum; decodeCoatings/encodeCoatings helpers added to optical-api.ts"
  - "Updated LensCatalogItemDto to match backend contract: brand, lensType, availableCoatings, sellingPrice, costPrice fields"
metrics:
  duration_minutes: 11
  completed_date: "2026-03-08"
  tasks_completed: 2
  tasks_total: 2
  files_created: 4
  files_modified: 1
---

# Phase 8 Plan 28: Lens Catalog Frontend Summary

**One-liner:** Lens catalog UI with expandable per-power stock table, dual-mode form dialog (catalog + stock adjustment), and low-stock alert banner.

## Tasks Completed

| Task | Description | Commit | Status |
|------|-------------|--------|--------|
| 1 | LensCatalogTable and LensFormDialog | d149252 | Done |
| 2 | LensCatalogPage and lenses route | a8f2dca | Done |

## What Was Built

### LensCatalogTable.tsx
- DataTable using TanStack Table with `getExpandedRowModel` for expandable sub-rows
- Columns: expand toggle, Brand, Name, Lens Type, Material, Coatings (badges), Selling Price, Cost Price, Total Stock, Status, Actions
- Expandable rows show per-power stock entries (SPH / CYL / ADD / Qty / Min / Status)
- Low stock entries highlighted in yellow, out-of-stock in red/destructive
- Per-row action buttons: + (add stock) and edit pencil

### LensFormDialog.tsx
- Three modes via `mode` prop: `"create"` | `"edit"` | `"stock"`
- Catalog Item form: Brand, Name, Lens Type (select), Material (select), Available Coatings (checkboxes mapped from flags bitfield), Selling Price, Cost Price
- Stock Adjustment form: SPH (step 0.25, -20 to +20), CYL (step 0.25, -10 to 0), ADD (optional, 0 to +4), Quantity Change (positive = add, negative = remove), Min Stock Level
- React Hook Form + Zod validation; mutations wired to useCreateLensCatalogItem, useUpdateLensCatalogItem, useAdjustLensStock

### LensCatalogPage.tsx
- Page header with "Lens Catalog" title and "Add Lens" button
- LowLensStockAlert component: collapsible banner with per-power alert list (brand, name, SPH, CYL, qty, min)
- Loading state via Skeleton components; uses useLensCatalog and useLowLensStockAlerts hooks

### lenses.tsx route
- `createFileRoute('/_authenticated/optical/lenses')` pointing to LensCatalogPage
- Auto-registered in routeTree.gen.ts

### optical-api.ts updates (deviation)
Updated DTO interfaces to match backend Optical.Contracts.Dtos:
- `LensCatalogItemDto`: added brand, lensType, availableCoatings, sellingPrice, costPrice, preferredSupplierId, supplierName, createdAt
- `LensStockEntryDto`: added lensCatalogItemId, minStockLevel
- `LowLensStockAlertDto`: added brand, name, quantity, minStockLevel fields
- `CreateLensCatalogItemInput` / `UpdateLensCatalogItemInput`: updated to match backend command fields
- Added LENS_MATERIAL_MAP, LENS_COATING_MAP, LENS_COATING_BITS, LENS_TYPE_OPTIONS, decodeCoatings(), encodeCoatings()

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Updated LensCatalogItemDto to match backend contract**
- **Found during:** Task 1 implementation
- **Issue:** Existing optical-api.ts LensCatalogItemDto used fields (name, material, coatings, basePrice) that didn't match backend Optical.Contracts.Dtos LensCatalogItemDto (Brand, Name, LensType, Material, AvailableCoatings, SellingPrice, CostPrice, PreferredSupplierId, SupplierName, StockEntries, CreatedAt)
- **Fix:** Updated DTO and input types to match backend; added lens enum maps and utility functions
- **Files modified:** frontend/src/features/optical/api/optical-api.ts
- **Commit:** d149252

## Verification

- TypeScript: 0 errors in optical files (`npx tsc --noEmit` shows 0 optical errors)
- Route accessible at `/optical/lenses` (auto-registered in routeTree.gen.ts)
- All 4 required files created and committed

## Self-Check: PASSED

Files verified:
- [x] frontend/src/features/optical/components/LensCatalogTable.tsx (FOUND)
- [x] frontend/src/features/optical/components/LensFormDialog.tsx (FOUND)
- [x] frontend/src/features/optical/components/LensCatalogPage.tsx (FOUND)
- [x] frontend/src/app/routes/_authenticated/optical/lenses.tsx (FOUND)

Commits verified:
- [x] d149252 - feat(08-28): create LensCatalogTable and LensFormDialog components (FOUND)
- [x] a8f2dca - docs(08-25): complete optical frontend API layer plan - LensCatalogPage and lenses route (FOUND)

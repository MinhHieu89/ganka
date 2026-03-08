---
phase: 08-optical-center
plan: 27
subsystem: optical-frontend
tags: [frontend, optical, react, tanstack-table, shadcn-ui, barcode]
dependency_graph:
  requires: [08-25, 08-26]
  provides: [optical-frame-catalog-ui]
  affects: [optical-navigation]
tech_stack:
  added: []
  patterns: [tanstack-table, react-hook-form, zod-validation, shadcn-ui-dialog]
key_files:
  created:
    - frontend/src/features/optical/components/FrameCatalogTable.tsx
    - frontend/src/features/optical/components/FrameFormDialog.tsx
    - frontend/src/features/optical/components/FrameCatalogPage.tsx
    - frontend/src/app/routes/_authenticated/optical/frames.tsx
  modified:
    - frontend/src/features/optical/api/optical-api.ts
    - frontend/src/features/optical/components/StocktakingScanner.tsx
decisions:
  - "Used client-side filtering in FrameCatalogTable rather than separate server-side filter calls to avoid multiple API round trips for the same data set"
  - "useSearchFrames hook used for all frame data fetching since it supports all filter params"
  - "Generate Barcode button only shown for frames without existing barcode"
metrics:
  duration_seconds: 768
  completed_date: "2026-03-08"
  tasks_completed: 2
  files_created: 4
  files_modified: 2
---

# Phase 8 Plan 27: Frame Catalog Frontend Summary

**One-liner:** Frame catalog page with DataTable (search/filter/pagination), FrameFormDialog (React Hook Form + Zod validation with optical size ranges), and TanStack Router route at /optical/frames.

## What Was Built

### Task 1: FrameCatalogTable and FrameFormDialog

**FrameCatalogTable.tsx** - TanStack Table with:
- Client-side text search (brand, model, color, barcode)
- Dropdown filters for Material (Metal/Plastic/Titanium), Frame Type (FullRim/SemiRimless/Rimless), Gender (M/F/Unisex)
- Size displayed as standard optical format: LensWidth-BridgeWidth-TempleLength (monospace font)
- Selling price formatted as Vietnamese Dong (VND) using Intl.NumberFormat
- Stock badge: destructive when qty = 0, secondary otherwise
- BarcodeDisplay component inline for frames with EAN-13 barcodes
- Edit and Generate Barcode action buttons per row
- Pagination (20 per page) with prev/next controls

**FrameFormDialog.tsx** - shadcn/ui Dialog with:
- React Hook Form + Zod validation (zodResolver)
- Brand, Model, Color text fields
- Size triplet fields: Lens Width (40-65), Bridge Width (12-24), Temple Length (120-155)
- Material, Frame Type, Gender Select dropdowns matching backend enums
- Selling Price and Cost Price number inputs (VND)
- Optional Barcode field (validates 13-digit pattern)
- Stock Quantity field
- Create and Edit modes, pre-populates from frame DTO on edit
- Closes dialog on successful mutation

### Task 2: FrameCatalogPage and Route

**FrameCatalogPage.tsx** - Page component:
- Header with "Frame Catalog" title and "Add Frame" button
- BarcodeScannerInput for quick barcode lookup (sets search term)
- Loading skeleton while data fetches
- Integrates FrameCatalogTable with edit and generate-barcode handlers
- useSearchFrames hook for data fetching (pageSize=100)
- Generate barcode shows toast with generated barcode value

**frames.tsx** - TanStack Router route file at `/_authenticated/optical/frames`

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed StocktakingScanner wrong import path**
- **Found during:** Task 1 verification
- **Issue:** `import { cn } from "@/lib/utils"` - wrong path, should be `@/shared/lib/utils`
- **Fix:** Updated import to correct path
- **Files modified:** `frontend/src/features/optical/components/StocktakingScanner.tsx`
- **Commit:** 0f1fd00

**2. [Rule 2 - Missing] Added DeliveredOrderSummaryDto and getDeliveredGlassesOrders**
- **Found during:** Task 1 verification (WarrantyClaimForm.tsx blocked compilation)
- **Issue:** WarrantyClaimForm.tsx imports `useDeliveredOrders` and `DeliveredOrderSummaryDto` which were missing
- **Fix:** Added `DeliveredOrderSummaryDto` interface, `getDeliveredOrders` and `getDeliveredGlassesOrders` alias to optical-api.ts
- **Files modified:** `frontend/src/features/optical/api/optical-api.ts`
- **Commit:** 0f1fd00

**3. [Rule 1 - Bug] Fixed generateBarcode endpoint URL**
- **Found during:** Task 2 (reviewing backend endpoints)
- **Issue:** Frontend called `/api/optical/frames/${frameId}/barcode` but backend exposes `/api/optical/frames/{id}/generate-barcode`
- **Fix:** Updated URL to `/generate-barcode` suffix
- **Files modified:** `frontend/src/features/optical/api/optical-api.ts`
- **Commit:** Task 2 commit

## Self-Check: PASSED

| Check | Status |
|-------|--------|
| `frontend/src/features/optical/components/FrameCatalogTable.tsx` | FOUND |
| `frontend/src/features/optical/components/FrameFormDialog.tsx` | FOUND |
| `frontend/src/features/optical/components/FrameCatalogPage.tsx` | FOUND |
| `frontend/src/app/routes/_authenticated/optical/frames.tsx` | FOUND |
| `.planning/phases/08-optical-center/08-27-SUMMARY.md` | FOUND |
| Commit 0f1fd00 (Task 1) | FOUND |
| Commit 4925cb4 (Task 2, part of 08-30 commit) | FOUND |

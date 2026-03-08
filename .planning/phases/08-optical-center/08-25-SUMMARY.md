---
phase: 08-optical-center
plan: 25
subsystem: optical-frontend-api
tags: [frontend, tanstack-query, api-client, barcode, typescript]
dependency_graph:
  requires: [08-24]
  provides: [optical-api-layer]
  affects: [08-26, 08-27, 08-28, 08-29, 08-30]
tech_stack:
  added: [jsbarcode@3.11.6, html5-qrcode@2.3.8, "@types/jsbarcode"]
  patterns: [tanstack-query-key-factory, api-client-pattern, pharmacy-api-mirror]
key_files:
  created:
    - frontend/src/features/optical/api/optical-api.ts
    - frontend/src/features/optical/api/optical-queries.ts
  modified:
    - frontend/package.json
    - frontend/package-lock.json
decisions:
  - "LensCatalogItemDto includes both basePrice (for component compat) and sellingPrice/costPrice (for full pricing info)"
  - "DeliveredOrderSummaryDto added to support WarrantyClaimForm component from plan 08-26"
  - "getDeliveredGlassesOrders and useDeliveredOrders added to support warranty claim form"
metrics:
  duration: "~15 minutes"
  completed_date: "2026-03-08"
  tasks_completed: 2
  tasks_total: 2
  files_created: 2
  files_modified: 2
---

# Phase 8 Plan 25: Frontend Barcode Dependencies and Optical API Layer Summary

**One-liner:** JsBarcode + html5-qrcode installed with 32-function optical API client and 31-hook TanStack Query layer covering all optical module endpoints.

## What Was Built

Established the complete frontend data layer for the optical center module:

1. **Barcode npm dependencies** - Installed `jsbarcode` (EAN-13 rendering to SVG/Canvas) and `html5-qrcode` (camera-based barcode scanning) with TypeScript types.

2. **optical-api.ts** - API client with 32 async functions covering:
   - Frame endpoints: getFrames, searchFrames, createFrame, updateFrame, generateBarcode
   - Lens endpoints: getLensCatalog, createLensCatalogItem, updateLensCatalogItem, adjustLensStock, getLowLensStockAlerts
   - Order endpoints: getGlassesOrders, getGlassesOrderById, getOverdueOrders, createGlassesOrder, updateOrderStatus
   - Combo endpoints: getComboPackages, createComboPackage, updateComboPackage
   - Warranty endpoints: getWarrantyClaims, createWarrantyClaim, approveWarrantyClaim, uploadWarrantyDocument, getDeliveredGlassesOrders
   - Prescription endpoints: getPatientPrescriptionHistory, getPrescriptionComparison
   - Stocktaking endpoints: getStocktakingSessions, getStocktakingSession, getDiscrepancyReport, startStocktakingSession, recordStocktakingItem, completeStocktaking
   - Full TypeScript DTOs for all request/response types
   - File upload via native fetch (warranty documents)

3. **optical-queries.ts** - 31 TanStack Query hooks with `opticalKeys` factory:
   - useQuery hooks for all GET endpoints
   - useMutation hooks for all POST/PUT endpoints
   - All mutations have `onError: (error) => toast.error(error.message)` pattern
   - All mutations invalidate relevant query keys on success
   - Hierarchical key factory following pharmacy-queries.ts pattern

## Commits

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Install barcode npm dependencies | b0800e9 | frontend/package.json, frontend/package-lock.json |
| 2 | Create optical API client and TanStack Query hooks | fe8da85 | frontend/src/features/optical/api/optical-api.ts, frontend/src/features/optical/api/optical-queries.ts |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Added basePrice to LensCatalogItemDto for component compatibility**
- **Found during:** TypeScript verification after Task 2
- **Issue:** Pre-existing component files (CreateGlassesOrderForm.tsx, ComboPackageForm.tsx from plan 08-26) reference `LensCatalogItemDto.basePrice` which was not in the initial DTO definition
- **Fix:** Added `basePrice: number` to `LensCatalogItemDto` alongside `sellingPrice` and `costPrice` to maintain backward compatibility with existing components
- **Files modified:** frontend/src/features/optical/api/optical-api.ts
- **Commit:** fe8da85 (included in Task 2 commit as linter applied the fix)

**2. [Rule 2 - Missing Functionality] Added getDeliveredGlassesOrders and useDeliveredOrders**
- **Found during:** TypeScript verification after Task 2
- **Issue:** WarrantyClaimForm.tsx (from plan 08-26) imports `useDeliveredOrders` from optical-queries and `DeliveredOrderSummaryDto` from optical-api - both were missing
- **Fix:** Added `DeliveredOrderSummaryDto` interface, `getDeliveredGlassesOrders` async function, and `useDeliveredOrders` query hook
- **Files modified:** frontend/src/features/optical/api/optical-api.ts, frontend/src/features/optical/api/optical-queries.ts
- **Commit:** fe8da85 (included in Task 2 commit as linter applied the fix)

## Self-Check: PASSED

| Item | Status |
|------|--------|
| frontend/src/features/optical/api/optical-api.ts | FOUND |
| frontend/src/features/optical/api/optical-queries.ts | FOUND |
| .planning/phases/08-optical-center/08-25-SUMMARY.md | FOUND |
| Commit b0800e9 (chore: install barcode deps) | FOUND |
| Commit fe8da85 (feat: optical API client) | FOUND |
| No TypeScript errors in optical/api files | PASSED |

---
phase: 08-optical-center
plan: 32
subsystem: ui
tags: [react, tanstack-query, tanstack-router, react-hook-form, zod, shadcn-ui, warranty, optical]

# Dependency graph
requires:
  - phase: 08-optical-center
    provides: optical-api.ts and optical-queries.ts patterns established in earlier plans

provides:
  - WarrantyClaimsPage with filterable claims list and manager approve/reject workflow
  - WarrantyClaimForm dialog for filing new warranty claims with warranty period validation
  - WarrantyDocumentUpload component for attaching evidence (images/PDFs) via Azure Blob
  - TanStack Router route at /optical/warranty

affects: [08-optical-center, frontend-routes]

# Tech tracking
tech-stack:
  added: [date-fns (format/differenceInDays for warranty date display)]
  patterns:
    - DataTable with expandable sub-rows for claim details
    - AlertDialog for approve/reject confirmation flow
    - Drag-and-drop file upload with image preview thumbnails
    - Warranty period validation gating form submission

key-files:
  created:
    - frontend/src/features/optical/components/WarrantyClaimsPage.tsx
    - frontend/src/features/optical/components/WarrantyClaimForm.tsx
    - frontend/src/features/optical/components/WarrantyDocumentUpload.tsx
    - frontend/src/app/routes/_authenticated/optical/warranty.tsx
  modified:
    - frontend/src/features/optical/api/optical-api.ts
    - frontend/src/features/optical/api/optical-queries.ts

key-decisions:
  - "Used number enum (0/1/2) for resolutionType and approvalStatus matching backend C# enums"
  - "Warranty info panel shown inline in form when order selected, disables submit for expired orders"
  - "WarrantyDocumentUpload supports both upload mode and readonly mode (for ClaimDetailsSubRow)"
  - "Upload done sequentially (one at a time) to match useUploadWarrantyDocument mutation pattern"

patterns-established:
  - "Pattern: Inline warranty info panel using DeliveredOrderSummaryDto for real-time warranty status display"
  - "Pattern: Expandable sub-row in DataTable for claim details without separate page navigation"

requirements-completed: [OPT-07]

# Metrics
duration: 35min
completed: 2026-03-08
---

# Phase 08 Plan 32: Warranty Claims Frontend Summary

**Warranty Claims UI with approval workflow, document upload, and period validation using TanStack Query, RHF/Zod, and shadcn/ui components**

## Performance

- **Duration:** 35 min
- **Started:** 2026-03-08T08:00:00Z
- **Completed:** 2026-03-08T08:35:00Z
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments

- WarrantyClaimsPage with filter tabs (All/Pending Approval/Approved/Rejected) and DataTable with approval status color-coded badges
- Approve/Reject actions for Replace-resolution claims behind manager confirmation dialog
- WarrantyClaimForm with searchable order selection, warranty info panel (delivery date, expiry, days remaining), warranty-expired guard on submit
- Manager approval notice shown for Replace resolution type
- WarrantyDocumentUpload with drag-and-drop, image previews, sequential file upload, readonly mode for viewing existing documents
- Route at /optical/warranty registered in TanStack Router routeTree

## Task Commits

1. **Task 1: WarrantyClaimsPage and WarrantyClaimForm** - `9838c61` (feat)
2. **Task 2: WarrantyDocumentUpload and route** - `0211016` (feat)

## Files Created/Modified

- `frontend/src/features/optical/components/WarrantyClaimsPage.tsx` - Claims list with filter tabs, DataTable, expand sub-row, approve/reject dialog
- `frontend/src/features/optical/components/WarrantyClaimForm.tsx` - New claim dialog with order selector, warranty info panel, resolution radio, discount amount, notes
- `frontend/src/features/optical/components/WarrantyDocumentUpload.tsx` - Drag-and-drop upload with image preview, sequential upload, existing docs list with links
- `frontend/src/app/routes/_authenticated/optical/warranty.tsx` - TanStack Router route wrapping WarrantyClaimsPage
- `frontend/src/features/optical/api/optical-api.ts` - Added DeliveredOrderSummaryDto and getDeliveredGlassesOrders (alias of getDeliveredOrders)
- `frontend/src/features/optical/api/optical-queries.ts` - Added useDeliveredOrders hook, imported getDeliveredGlassesOrders

## Decisions Made

- Used number-based enums (0/1/2) for resolutionType and approvalStatus to match the existing backend C# enum values already in optical-api.ts
- Warranty period validation done client-side via `isUnderWarranty` flag from DeliveredOrderSummaryDto (server re-validates on submit)
- Used `readonly` prop on WarrantyDocumentUpload to render existing documents without the upload UI in the claim details sub-row
- Chose sequential file upload (one at a time) to show meaningful progress indicators and avoid race conditions

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added optical-queries.ts export for useDeliveredOrders**
- **Found during:** Task 1 (WarrantyClaimForm implementation)
- **Issue:** optical-queries.ts had no hook for fetching delivered orders needed by the warranty form
- **Fix:** Added `useDeliveredOrders` export and imported `getDeliveredGlassesOrders` from optical-api.ts
- **Files modified:** frontend/src/features/optical/api/optical-queries.ts, frontend/src/features/optical/api/optical-api.ts
- **Verification:** TypeScript compiles cleanly, no errors in optical files
- **Committed in:** 9838c61 (Task 1 commit)

**2. [Rule 3 - Blocking] Fixed duplicate getDeliveredGlassesOrders in optical-api.ts**
- **Found during:** Task 1 verification (TypeScript compilation)
- **Issue:** I added a new `getDeliveredGlassesOrders` async function but the file already had `getDeliveredOrders` + a const alias `getDeliveredGlassesOrders = getDeliveredOrders` — duplicate identifier error
- **Fix:** Removed the new standalone function, kept the alias pattern
- **Files modified:** frontend/src/features/optical/api/optical-api.ts
- **Verification:** TypeScript compilation shows no optical-api.ts errors
- **Committed in:** 9838c61 (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (both Rule 3 - blocking)
**Impact on plan:** Both fixes were necessary to resolve blocking TypeScript compilation errors. No scope creep.

## Issues Encountered

- `optical-api.ts` already had a `DeliveredOrderSummaryDto` interface (from prior plan work) and an alias pattern for `getDeliveredGlassesOrders`. My additions created duplicates that needed cleanup.

## Self-Check

- [x] WarrantyClaimsPage.tsx - created at frontend/src/features/optical/components/WarrantyClaimsPage.tsx
- [x] WarrantyClaimForm.tsx - created at frontend/src/features/optical/components/WarrantyClaimForm.tsx
- [x] WarrantyDocumentUpload.tsx - created at frontend/src/features/optical/components/WarrantyDocumentUpload.tsx
- [x] warranty.tsx route - created at frontend/src/app/routes/_authenticated/optical/warranty.tsx
- [x] Task 1 commit: 9838c61
- [x] Task 2 commit: 0211016
- [x] TypeScript compiles with no errors in new optical files (60 pre-existing errors in unrelated files)

## Self-Check: PASSED

## Next Phase Readiness

- Warranty Claims UI ready for integration with live backend warranty API endpoints
- WarrantyDocumentUpload assumes `documentUrl` field in upload response — backend needs to return this field
- The route at /optical/warranty is accessible once the optical sidebar link is enabled
- Prior-plan optical sidebar item is currently disabled (`disabled: true`) in AppSidebar.tsx

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*

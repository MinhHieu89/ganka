---
phase: 10-address-all-pending-todos
plan: 09
subsystem: ui
tags: [react, react-hook-form, signalr, collapsible, pagination, mime-validation]

# Dependency graph
requires:
  - phase: 10-address-all-pending-todos
    provides: code review findings from plans 10-01, 10-05, 10-06
provides:
  - Fixed OTC sale notes data loss
  - Fixed OpticalPrescriptionSection collapse behavior
  - Fixed AutoResizeTextarea integration in BookingForm and ExaminationNotes
  - Fixed DrugCatalogImportDialog row number display for non-contiguous rows
  - Fixed StockImportForm drug selection validation
  - Fixed SignalR useOsdiHub cleanup with isMounted guard
  - Added client-side MIME validation for logo upload
  - Fixed DrugCatalogPage pagination race condition
  - Simplified OsdiAnswersSection from Collapsible misuse to conditional render
affects: [pharmacy, clinical, booking, admin]

# Tech tracking
tech-stack:
  added: []
  patterns: [isMounted-ref-pattern-for-signalr-cleanup, controller-pattern-for-autoresize-textarea]

key-files:
  created: []
  modified:
    - frontend/src/features/pharmacy/components/OtcSaleForm.tsx
    - frontend/src/features/pharmacy/api/pharmacy-api.ts
    - frontend/src/features/clinical/components/OpticalPrescriptionSection.tsx
    - frontend/src/features/clinical/components/ExaminationNotesSection.tsx
    - frontend/src/features/booking/components/BookingForm.tsx
    - frontend/src/features/pharmacy/components/DrugCatalogImportDialog.tsx
    - frontend/src/features/pharmacy/components/StockImportForm.tsx
    - frontend/src/features/clinical/hooks/use-osdi-hub.ts
    - frontend/src/features/admin/components/ClinicSettingsPage.tsx
    - frontend/src/features/pharmacy/components/DrugCatalogPage.tsx
    - frontend/src/features/clinical/components/OsdiAnswersSection.tsx

key-decisions:
  - "Simplified OsdiAnswersSection from Collapsible primitive to conditional render for cleaner toggle behavior"
  - "Used isMounted ref pattern for SignalR cleanup instead of AbortController for compatibility"
  - "DrugCatalogImport valid rows use sequential index display since backend does not return original Excel row numbers"

patterns-established:
  - "isMounted ref pattern: use { current: true } object ref for unmount guards in async hooks"
  - "Controller pattern: always use Controller with AutoResizeTextarea to maintain RHF value sync"

requirements-completed: []

# Metrics
duration: 4min
completed: 2026-03-14
---

# Phase 10 Plan 09: Frontend Bug Fixes Summary

**Fixed 12 frontend bugs: OTC notes data loss, OpticalRx collapse, AutoResizeTextarea integration, SignalR cleanup, MIME validation, pagination race condition, and Collapsible misuse**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-14T08:12:25Z
- **Completed:** 2026-03-14T08:16:22Z
- **Tasks:** 2
- **Files modified:** 11

## Accomplishments
- Fixed OTC sale notes field being dropped from submission payload
- Fixed OpticalPrescriptionSection not being collapsible after initial render
- Fixed SignalR useOsdiHub hook with proper cleanup guards preventing stale state updates
- Fixed DrugCatalogPage pagination race condition between search input and debounced query
- Added client-side MIME type validation for clinic logo upload
- Removed dead code (useHasStockExceeded) and Collapsible primitive misuse

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix data loss and critical component bugs** - `c64413f` (fix)
2. **Task 2: Fix SignalR cleanup and UX issues** - `5a1678a` (fix)

## Files Created/Modified
- `frontend/src/features/pharmacy/components/OtcSaleForm.tsx` - Added notes to submit payload, removed dead useHasStockExceeded
- `frontend/src/features/pharmacy/api/pharmacy-api.ts` - Added notes to CreateOtcSaleInput, TODO for branchId
- `frontend/src/features/clinical/components/OpticalPrescriptionSection.tsx` - Fixed controlled open/onOpenChange props
- `frontend/src/features/clinical/components/ExaminationNotesSection.tsx` - Changed resize-y to resize-none
- `frontend/src/features/booking/components/BookingForm.tsx` - Controller pattern for notes AutoResizeTextarea
- `frontend/src/features/pharmacy/components/DrugCatalogImportDialog.tsx` - Graceful non-contiguous row handling
- `frontend/src/features/pharmacy/components/StockImportForm.tsx` - shouldValidate on drug selection
- `frontend/src/features/clinical/hooks/use-osdi-hub.ts` - isMounted ref, nullify connectionRef, state checks
- `frontend/src/features/admin/components/ClinicSettingsPage.tsx` - MIME type validation before upload
- `frontend/src/features/pharmacy/components/DrugCatalogPage.tsx` - useEffect pageIndex reset on debouncedSearch
- `frontend/src/features/clinical/components/OsdiAnswersSection.tsx` - Simplified to conditional render

## Decisions Made
- Simplified OsdiAnswersSection from Collapsible primitive to conditional render for cleaner toggle behavior
- Used isMounted ref pattern for SignalR cleanup instead of AbortController for compatibility
- DrugCatalogImport valid rows use sequential index display since backend does not return original Excel row numbers

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 12 frontend issues from code review are resolved
- Ready for subsequent plans in phase 10

---
*Phase: 10-address-all-pending-todos*
*Completed: 2026-03-14*

---
phase: 06-pharmacy-consumables
plan: 30
subsystem: api
tags: [wolverine, cqrs, toggle, supplier, patch-endpoint, tdd]

requires:
  - phase: 06-pharmacy-consumables
    provides: Supplier domain entity with Activate()/Deactivate() methods
provides:
  - PATCH /api/pharmacy/suppliers/{id}/toggle-active endpoint
  - useToggleSupplierActive frontend mutation hook
  - GetAllAsync repository method returning both active and inactive suppliers
affects: [06-pharmacy-consumables]

tech-stack:
  added: []
  patterns: [dedicated PATCH toggle endpoint instead of overloading PUT update]

key-files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Suppliers/ToggleSupplierActive.cs
  modified:
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/ISupplierRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/SupplierRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Suppliers/GetSuppliers.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Presentation/PharmacyApiEndpoints.cs
    - backend/tests/Pharmacy.Unit.Tests/Features/SupplierHandlerTests.cs
    - frontend/src/features/pharmacy/api/pharmacy-api.ts
    - frontend/src/features/pharmacy/api/pharmacy-queries.ts
    - frontend/src/app/routes/_authenticated/pharmacy/suppliers.tsx

key-decisions:
  - "Dedicated PATCH toggle endpoint rather than adding IsActive to UpdateSupplierCommand"
  - "GetSuppliers now returns all suppliers (active+inactive) so toggle UI is visible for both states"

patterns-established:
  - "Toggle pattern: PATCH /{resource}/{id}/toggle-active with domain Activate()/Deactivate() methods"

requirements-completed: []

duration: 4min
completed: 2026-03-11
---

# Phase 06 Plan 30: Supplier Toggle Fix Summary

**Dedicated PATCH /toggle-active endpoint and frontend wiring to fix supplier active/inactive toggle not persisting (UAT Test 2)**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-11T10:59:40Z
- **Completed:** 2026-03-11T11:03:29Z
- **Tasks:** 2
- **Files modified:** 10

## Accomplishments
- Created ToggleSupplierActiveCommand + handler with domain Activate()/Deactivate() calls
- Added PATCH /api/pharmacy/suppliers/{id}/toggle-active endpoint
- Updated GetSuppliersHandler to return all suppliers (not just active) so toggle button is visible
- Wired frontend to use dedicated toggle endpoint instead of broken PUT update
- 8 unit tests pass including 3 new TDD toggle tests

## Task Commits

Each task was committed atomically:

1. **Task 1 (RED): Add failing TDD tests** - `fd9ff3d` (test)
2. **Task 1 (GREEN): Implement handler, endpoint, GetAllAsync** - `ae9808f` (feat)
3. **Task 2: Wire frontend toggle to PATCH endpoint** - `8ffbe8b` (feat)

_Note: TDD task has separate RED and GREEN commits._

## Files Created/Modified
- `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Suppliers/ToggleSupplierActive.cs` - Command + handler for toggling supplier active status
- `backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/ISupplierRepository.cs` - Added GetAllAsync method
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/SupplierRepository.cs` - Implemented GetAllAsync
- `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Suppliers/GetSuppliers.cs` - Changed from GetAllActiveAsync to GetAllAsync
- `backend/src/Modules/Pharmacy/Pharmacy.Presentation/PharmacyApiEndpoints.cs` - Added PATCH toggle-active endpoint
- `backend/tests/Pharmacy.Unit.Tests/Features/SupplierHandlerTests.cs` - 3 new toggle tests + updated GetSuppliers test
- `backend/tests/Pharmacy.Unit.Tests/Features/OtcSaleAndInventoryHandlerTests.cs` - Fixed pre-existing build error
- `frontend/src/features/pharmacy/api/pharmacy-api.ts` - Added toggleSupplierActive function, removed unused isActive from UpdateSupplierInput
- `frontend/src/features/pharmacy/api/pharmacy-queries.ts` - Added useToggleSupplierActive mutation hook
- `frontend/src/app/routes/_authenticated/pharmacy/suppliers.tsx` - Switched to useToggleSupplierActive

## Decisions Made
- Used dedicated PATCH toggle endpoint rather than adding IsActive to UpdateSupplierCommand -- cleaner separation of concerns and follows REST best practices
- Changed GetSuppliersHandler to return all suppliers (active + inactive) so inactive suppliers remain visible with toggle button

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed pre-existing OtcSaleAndInventoryHandlerTests build error**
- **Found during:** Task 1 (TDD GREEN phase)
- **Issue:** GetDrugBatchesHandler.Handle signature had been updated to include ISupplierRepository parameter but OtcSaleAndInventoryHandlerTests was not updated, preventing project build
- **Fix:** Added ISupplierRepository mock field and passed it to handler calls
- **Files modified:** backend/tests/Pharmacy.Unit.Tests/Features/OtcSaleAndInventoryHandlerTests.cs
- **Verification:** Full test project builds and all tests pass
- **Committed in:** ae9808f (Task 1 GREEN commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Pre-existing build error fix necessary to compile tests. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Supplier toggle is fully functional end-to-end
- Both active and inactive suppliers visible in management page
- Ready for UAT re-verification of Test 2

---
*Phase: 06-pharmacy-consumables*
*Completed: 2026-03-11*

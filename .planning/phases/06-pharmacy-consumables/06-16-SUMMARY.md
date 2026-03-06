---
phase: 06-pharmacy-consumables
plan: 16
subsystem: api
tags: [pharmacy, consumables, stock-management, tdd, csharp, cqrs]

# Dependency graph
requires:
  - phase: 06-09
    provides: ConsumableItem and ConsumableBatch domain entities
  - phase: 06-10
    provides: IConsumableRepository with GetAlertsAsync, AddBatch, AddStockAdjustment methods

provides:
  - AddConsumableStock handler: SimpleStock increments CurrentStock, ExpiryTracked creates ConsumableBatch
  - AdjustConsumableStock handler: manual corrections with StockAdjustment audit for ExpiryTracked
  - GetConsumableItems handler: all active items with computed stock (batch sum for ExpiryTracked)
  - GetConsumableAlerts handler: items below MinStockLevel via repository delegation
  - ConsumableBatch.AddStock domain method (added for positive adjustment symmetry with DrugBatch)

affects:
  - 06-pharmacy-consumables (consumable stock endpoint wiring)
  - phase-09 (auto-deduction will reuse these handler patterns)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - TrackingMode branching in handlers (SimpleStock vs ExpiryTracked)
    - StockAdjustment audit only for ExpiryTracked items (domain constraint: requires non-null batch FK)
    - GetConsumableItems computes ExpiryTracked stock by batches sum (avoids N+1 via GetBatchesAsync per item)
    - ConsumableBatch mirrors DrugBatch with AddStock/Deduct pair for symmetric stock operations

key-files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/AddConsumableStock.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/AdjustConsumableStock.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/GetConsumableItems.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/GetConsumableAlerts.cs
    - backend/tests/Pharmacy.Unit.Tests/Features/ConsumableStockHandlerTests.cs
  modified:
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/ConsumableBatch.cs

key-decisions:
  - "StockAdjustment not created for SimpleStock adjustments: domain constraint requires exactly one non-null FK (DrugBatchId or ConsumableBatchId); SimpleStock has no batch"
  - "ConsumableBatch.AddStock added to mirror DrugBatch.AddStock for positive batch corrections"
  - "GetConsumableItems uses per-item GetBatchesAsync call for ExpiryTracked stock sum (acceptable for Phase 6 manual-only management)"
  - "GetConsumableAlerts delegates entirely to repository (same pattern as GetLowStockAlertsHandler)"

patterns-established:
  - "TrackingMode branching: all consumable stock handlers branch on item.TrackingMode for SimpleStock vs ExpiryTracked logic"
  - "Entity Id reflection pattern in tests: set item.Id via reflection when batch FK assertion requires matching ID"

requirements-completed: [CON-02, CON-03]

# Metrics
duration: 5min
completed: 2026-03-06
---

# Phase 06 Plan 16: Consumable Stock Management Summary

**Consumable stock management with TrackingMode branching: SimpleStock via CurrentStock, ExpiryTracked via ConsumableBatch, with StockAdjustment audit and low-stock alerts**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-06T08:12:18Z
- **Completed:** 2026-03-06T08:17:30Z
- **Tasks:** 1 (TDD: RED + GREEN)
- **Files modified:** 6

## Accomplishments
- AddConsumableStock supports both SimpleStock (direct increment) and ExpiryTracked (new batch creation)
- AdjustConsumableStock handles both modes with StockAdjustment audit for ExpiryTracked items
- GetConsumableItems returns all active items with computed stock levels and IsLowStock flag
- GetConsumableAlerts returns low-stock consumables via repository delegation
- 9/9 unit tests passing in ConsumableStockHandlerTests

## Task Commits

Each task was committed atomically:

1. **Task 1: Write tests (RED) and implement consumable stock handlers (GREEN)** - `05a18c8` (feat)

**Plan metadata:** [pending docs commit]

_Note: TDD task combined RED+GREEN in single commit per plan design (single tdd="true" task)_

## Files Created/Modified
- `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/AddConsumableStock.cs` - Command+Validator+Handler for adding stock to SimpleStock or ExpiryTracked items
- `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/AdjustConsumableStock.cs` - Command+Validator+Handler for manual stock corrections with audit
- `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/GetConsumableItems.cs` - Query+Handler returning active items with computed stock and IsLowStock flag
- `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/GetConsumableAlerts.cs` - Query+Handler returning items below MinStockLevel
- `backend/tests/Pharmacy.Unit.Tests/Features/ConsumableStockHandlerTests.cs` - 9 TDD tests covering all behaviors
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/ConsumableBatch.cs` - Added AddStock method to mirror DrugBatch

## Decisions Made

- **StockAdjustment for SimpleStock**: The `StockAdjustment` domain entity enforces exactly one non-null FK (DrugBatchId or ConsumableBatchId). SimpleStock items have no batch, so no StockAdjustment is created. The IAuditable audit interceptor on ConsumableItem tracks the change timestamp instead.
- **ConsumableBatch.AddStock**: Added to achieve symmetry with DrugBatch.AddStock. Required for positive adjustments in AdjustConsumableStock without reflection hacks.
- **GetConsumableItems stock computation**: Uses a per-item GetBatchesAsync call for ExpiryTracked items. This is N+1 but acceptable for Phase 6 consumables (small catalog, manual-only operations).

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added AddStock to ConsumableBatch domain entity**
- **Found during:** Task 1 (AdjustConsumableStock implementation)
- **Issue:** ConsumableBatch only had Deduct(), no AddStock(). Positive adjustments would require reflection hacks. DrugBatch has AddStock() for symmetry.
- **Fix:** Added ConsumableBatch.AddStock(int qty) mirroring DrugBatch.AddStock exactly
- **Files modified:** backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/ConsumableBatch.cs
- **Verification:** All 9 tests pass, domain method used without reflection
- **Committed in:** 05a18c8 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (Rule 2 - missing critical domain method)
**Impact on plan:** Required for correct stock management without reflection in production code. No scope creep.

## Issues Encountered
- Test `AddConsumableStock_ExpiryTracked_CreatesBatch` initially failed because `ConsumableItem.Id` is auto-generated and didn't match `DefaultItemId`. Fixed by setting item Id via reflection in test setup (established pattern from OtcSaleAndInventoryHandlerTests).
- `StockAdjustment.Create` domain constraint (exactly-one-non-null FK) prevents creating audit records for SimpleStock adjustments. Documented as design decision; no test verification of audit for SimpleStock.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Consumable stock management handlers ready for endpoint wiring (Pharmacy.Presentation)
- GetConsumableItems and GetConsumableAlerts ready for frontend consumables warehouse page
- Phase 9 auto-deduction can reuse AdjustConsumableStock patterns with ConsumableBatchId for ExpiryTracked

---
*Phase: 06-pharmacy-consumables*
*Completed: 2026-03-06*

---
phase: 06-pharmacy-consumables
plan: 04
subsystem: domain
tags: [pharmacy, consumables, otc-sale, domain-entities, fefo, batch-tracking]

# Dependency graph
requires:
  - phase: 06-pharmacy-consumables
    provides: BatchDeduction entity with dual nullable FK pattern (DispensingLineId/OtcSaleLineId) from Plan 03

provides:
  - OtcSale AggregateRoot with optional PatientId/CustomerName for walk-in OTC sales
  - OtcSaleLine Entity with DrugCatalogItemId, DrugName, Quantity, UnitPrice (price snapshot)
  - ConsumableItem AggregateRoot with dual tracking modes (ExpiryTracked/SimpleStock)
  - ConsumableBatch Entity for expiry-tracked consumables with FEFO support
  - ConsumableTrackingMode enum (ExpiryTracked=0, SimpleStock=1)

affects:
  - 06-pharmacy-consumables plans 05+ (infrastructure, application, persistence)
  - Phase 9 treatment protocols (auto-deduction from consumables)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "OtcSale uses same aggregate/line/batch-deduction pattern as DispensingRecord"
    - "ConsumableBatch mirrors DrugBatch pattern for ExpiryTracked items"
    - "ConsumableItem.AddStock/RemoveStock guard against negative stock for SimpleStock mode"
    - "Internal factory pattern: OtcSaleLine.Create() is internal, always via OtcSale.AddLine()"

key-files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/OtcSale.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/OtcSaleLine.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/ConsumableItem.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/ConsumableBatch.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Enums/ConsumableTrackingMode.cs
  modified: []

key-decisions:
  - "OtcSaleLine.Create() is internal -- lines always created through OtcSale.AddLine() to enforce aggregate ownership"
  - "ConsumableItem.CurrentStock only used for SimpleStock mode -- ExpiryTracked stock computed from ConsumableBatch.CurrentQuantity sum"
  - "ConsumableBatch.Deduct() throws InvalidOperationException on insufficient stock (domain invariant, same as DrugBatch)"
  - "ConsumableItem.AddStock/RemoveStock throw InvalidOperationException if called on ExpiryTracked item"
  - "OtcSale allows both PatientId and CustomerName to be null for fully anonymous sales"

patterns-established:
  - "Dual-mode aggregate: ConsumableItem supports both ExpiryTracked (batch records) and SimpleStock (direct counter) in single entity"
  - "Price snapshot: OtcSaleLine.UnitPrice stores selling price at time of sale, immutable for audit"

requirements-completed: [PHR-06, CON-01, CON-02]

# Metrics
duration: 2min
completed: 2026-03-06
---

# Phase 06 Plan 04: OTC Sale and Consumables Domain Entities Summary

**OtcSale aggregate for walk-in pharmacy sales and ConsumableItem/ConsumableBatch for dual-mode consumables warehouse tracking**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-06T06:51:35Z
- **Completed:** 2026-03-06T06:53:30Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- OtcSale aggregate root with optional patient linkage and OtcSaleLine children using BatchDeduction.CreateForOtcSale factory (connects to Plan 03 shared entity)
- ConsumableItem aggregate with dual tracking: ExpiryTracked (batch-level FEFO) and SimpleStock (direct counter with negative-stock guard)
- ConsumableBatch entity mirroring DrugBatch pattern with Deduct(), IsExpired, IsNearExpiry(), and RowVersion for optimistic concurrency

## Task Commits

Each task was committed atomically:

1. **Task 1: Create OtcSale and OtcSaleLine entities** - `d0d88ce` (feat)
2. **Task 2: Create ConsumableItem, ConsumableBatch entities and ConsumableTrackingMode enum** - `a52ea06` (feat)

**Plan metadata:** (docs commit follows)

## Files Created/Modified
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/OtcSale.cs` - AggregateRoot with optional PatientId/CustomerName, AddLine() factory method
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/OtcSaleLine.cs` - Entity with price snapshot, AddBatchDeduction() using CreateForOtcSale
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/ConsumableItem.cs` - AggregateRoot with dual tracking modes, AddStock/RemoveStock guards
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/ConsumableBatch.cs` - Entity with FEFO support, RowVersion optimistic concurrency
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Enums/ConsumableTrackingMode.cs` - ExpiryTracked=0, SimpleStock=1

## Decisions Made
- OtcSaleLine.Create() is internal -- lines always created through OtcSale.AddLine() to enforce aggregate ownership (same pattern as DispensingLine)
- ConsumableItem.CurrentStock is only meaningful for SimpleStock mode; ExpiryTracked stock is computed from ConsumableBatch records (not duplicated on entity)
- AddStock/RemoveStock throw InvalidOperationException if called on ExpiryTracked item to prevent incorrect usage at runtime
- OtcSale allows fully anonymous sales (both PatientId and CustomerName can be null) per context decision

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 5 domain entity files ready for Plan 05 (EF Core configuration and DbContext mappings)
- ConsumableBatch/ConsumableItem require infrastructure mappings and migration
- OtcSale/OtcSaleLine require infrastructure mappings and migration
- BatchDeduction already covers OtcSaleLine FK from Plan 03 (dual nullable FK pattern)

---
*Phase: 06-pharmacy-consumables*
*Completed: 2026-03-06*

## Self-Check: PASSED
- FOUND: OtcSale.cs
- FOUND: OtcSaleLine.cs
- FOUND: ConsumableItem.cs
- FOUND: ConsumableBatch.cs
- FOUND: ConsumableTrackingMode.cs
- FOUND commit: d0d88ce (Task 1)
- FOUND commit: a52ea06 (Task 2)

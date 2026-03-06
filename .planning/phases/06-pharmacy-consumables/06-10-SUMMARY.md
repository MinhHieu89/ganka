---
phase: 06-pharmacy-consumables
plan: 10
subsystem: pharmacy-domain
tags: [tdd, fefo, domain-service, unit-tests, batch-allocation]
dependency_graph:
  requires: [06-07, 06-06]
  provides: [FEFOAllocator domain service, Pharmacy.Unit.Tests Domain suite]
  affects: [dispensing handlers, OTC sale handlers]
tech_stack:
  added: []
  patterns: [TDD red-green, static domain service, sealed record BatchAllocation, all-or-nothing allocation]
key_files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Services/FEFOAllocator.cs
    - backend/tests/Pharmacy.Unit.Tests/Domain/FEFOAllocatorTests.cs
    - backend/tests/Pharmacy.Unit.Tests/Domain/DrugBatchTests.cs
  modified: []
decisions:
  - FEFOAllocator as static class (not instance service) -- pure function with no state, no DI needed; testable directly
  - BatchAllocation as sealed record in same file as FEFOAllocator for cohesion
  - All-or-nothing semantics: returns empty list on insufficient stock (not partial allocation) per research recommendation
  - ExpiryDate == Today is treated as expired (IsExpired uses <=) consistent with DrugBatch entity
  - Tests use DrugBatch.Create (public factory) directly -- no reflection needed since constructor is accessible via factory
metrics:
  duration: 2min
  completed_date: "2026-03-06"
  tasks_completed: 2
  files_changed: 3
---

# Phase 06 Plan 10: FEFO Allocator Domain Service Summary

**One-liner:** Static FEFOAllocator with BatchAllocation record implementing earliest-expiry-first batch selection with multi-batch spanning and all-or-nothing semantics.

## What Was Built

Created the `FEFOAllocator` domain service in `Pharmacy.Domain.Services` namespace with:

- `BatchAllocation` sealed record carrying BatchId, BatchNumber, Quantity, ExpiryDate
- `FEFOAllocator.Allocate(IReadOnlyList<DrugBatch>, int)` static method
- Algorithm: filter expired + zero-quantity, order by ExpiryDate ASC, iterate taking Math.Min(remaining, batch.CurrentQuantity), return empty on insufficient stock

Also created comprehensive unit tests:

- **FEFOAllocatorTests** (8 tests): single batch, multi-batch FEFO ordering, multi-batch spanning, expired skip, zero-quantity skip, insufficient stock, exact match, empty batch list, today-as-expired edge case
- **DrugBatchTests** (13 tests): Deduct valid/exact/overflow/zero/negative/multi, IsExpired future/today/past, IsNearExpiry within/after/already-expired, AddStock valid/zero

## Test Results

```
All tests passed: 23/23 FEFO+DrugBatch, 34/34 total Pharmacy.Unit.Tests
```

## TDD Execution

| Phase | State | Commit |
|-------|-------|--------|
| RED | Build error: FEFOAllocator namespace does not exist | eca5ca2 |
| GREEN | All 23 tests pass | 34d7221 |
| REFACTOR | Not needed -- implementation is clean and minimal | - |

## Deviations from Plan

None -- plan executed exactly as written.

## Self-Check

- [x] FEFOAllocator.cs exists at `backend/src/Modules/Pharmacy/Pharmacy.Domain/Services/FEFOAllocator.cs`
- [x] FEFOAllocatorTests.cs exists at `backend/tests/Pharmacy.Unit.Tests/Domain/FEFOAllocatorTests.cs`
- [x] DrugBatchTests.cs exists at `backend/tests/Pharmacy.Unit.Tests/Domain/DrugBatchTests.cs`
- [x] RED commit: eca5ca2
- [x] GREEN commit: 34d7221
- [x] All 23 FEFO+DrugBatch tests pass
- [x] Total Pharmacy.Unit.Tests: 34/34 pass

## Self-Check: PASSED

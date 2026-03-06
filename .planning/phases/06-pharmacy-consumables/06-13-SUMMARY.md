---
phase: 06-pharmacy-consumables
plan: 13
subsystem: pharmacy-dispensing
tags: [tdd, dispensing, fefo, pharmacy, csharp, backend]
dependency_graph:
  requires: [06-08, 06-10]
  provides: [dispensing-handlers, cross-module-pending-rx-query]
  affects: [pharmacy-presentation, clinical-contracts]
tech_stack:
  added: []
  patterns: [vertical-slice-handler, fefo-allocation, tdd-red-green, cross-module-query, error-custom-codes]
key_files:
  created:
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/PendingPrescriptionQuery.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Dispensing/DispenseDrugs.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Dispensing/GetPendingPrescriptions.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Dispensing/GetDispensingHistory.cs
    - backend/tests/Pharmacy.Unit.Tests/Features/DispensingHandlerTests.cs
  modified:
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IDispensingRepository.cs
decisions:
  - "Error.Custom used for domain-specific error codes (Prescription.AlreadyDispensed, Prescription.Expired, DispensingLine.InsufficientStock) -- Error.Validation/Conflict only provide generic codes"
  - "GetPendingPrescriptions handler delegates entirely to IDispensingRepository.GetPendingPrescriptionsAsync -- cross-module Clinical query handled in Infrastructure implementation via IMessageBus"
  - "BatchOverride.ManualBatches is List<BatchOverride>? on command -- null means auto-FEFO, non-null means manual override per line"
  - "Off-catalog lines dispensed with Guid.Empty DrugCatalogItemId -- no batch deduction, just audit record"
metrics:
  duration: 7min
  completed_date: "2026-03-06"
  tasks: 2
  files: 6
---

# Phase 06 Plan 13: Drug Dispensing Handlers Summary

**One-liner:** FEFO dispensing handlers with 7-day prescription validity, manual batch override, and cross-module pending prescriptions query in Clinical.Contracts.

## What Was Built

### Core Handlers (3 files)

**DispenseDrugs.cs** -- Primary pharmacy workflow handler:
- `DispenseDrugsCommand` with `DispenseLineInput` and `BatchOverride` input types
- 7-day prescription validity enforcement: expired prescriptions blocked unless `OverrideReason` provided (PHR-07)
- Duplicate dispensing prevention via `GetByPrescriptionIdAsync` check
- Per-line processing: Skip (no stock deduction), IsOffCatalog (audit-only), Catalog drug (FEFO or manual)
- FEFO auto-allocation via `FEFOAllocator.Allocate` with fallback to `ManualBatches` override
- All-or-nothing per drug line: empty allocation list returns error, no partial dispensing
- `DrugBatch.Deduct()` called per allocated batch for stock deduction

**GetPendingPrescriptions.cs** -- Pharmacy dispensing queue:
- `GetPendingPrescriptionsQuery(Guid? PatientId)` with optional patient filter
- Delegates to `IDispensingRepository.GetPendingPrescriptionsAsync` (cross-module resolution in Infrastructure)

**GetDispensingHistory.cs** -- Paginated dispensing history:
- `GetDispensingHistoryQuery(Page, PageSize, PatientId?)` with optional patient filter
- Returns `DispensingHistoryDto(List<DispensingRecordDto>, TotalCount)` for frontend pagination

### Cross-Module Query (1 file)

**Clinical.Contracts/Dtos/PendingPrescriptionQuery.cs**:
- `GetPendingPrescriptionsQuery(Guid? PatientId)` record in Clinical.Contracts
- Handler lives in Clinical.Application (cross-module boundary respected)
- Pharmacy.Presentation invokes via `IMessageBus.InvokeAsync<List<PendingPrescriptionDto>>(query)`

### Interface Extension (1 file)

**IDispensingRepository.cs** -- Added `GetPendingPrescriptionsAsync`:
- Returns `List<PendingPrescriptionDto>` for pending prescriptions without a DispensingRecord
- Infrastructure implementation will cross-query Clinical via IMessageBus

### Tests (1 file, 9 tests)

**DispensingHandlerTests.cs** -- Full TDD coverage:
1. `DispenseDrugs_ValidPrescription_CreatesRecordWithFEFO` -- happy path with FEFO
2. `DispenseDrugs_ExpiredRx_WithoutOverride_ReturnsError` -- 7-day validity enforcement
3. `DispenseDrugs_ExpiredRx_WithOverrideReason_Succeeds` -- expired with reason allowed
4. `DispenseDrugs_AlreadyDispensed_ReturnsError` -- duplicate dispensing blocked
5. `DispenseDrugs_InsufficientStock_ReturnsError` -- all-or-nothing enforcement
6. `DispenseDrugs_SkippedLine_RecordsSkipStatus` -- skip with no batch deduction
7. `DispenseDrugs_BatchDeductionApplied_AcrossMultipleBatches` -- multi-batch FEFO allocation
8. `GetPendingPrescriptions_ReturnsUndispensed` -- queue handler delegates to repository
9. `GetDispensingHistory_ReturnsPaginated` -- history pagination

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Error.Validation does not accept 2 arguments**
- **Found during:** Task 2 (GREEN phase compilation)
- **Issue:** `Error.Validation(code, description)` overload doesn't exist; only single-arg `Error.Validation(description)` exists
- **Fix:** Used `Error.Custom(code, description)` for all domain-specific error codes
- **Files modified:** DispenseDrugs.cs
- **Commit:** b05a87a

**2. [Rule 1 - Bug] Test used result.Error.Message (non-existent property)**
- **Found during:** Task 2 (GREEN phase test review)
- **Issue:** `Error` has `Description` not `Message` property
- **Fix:** Changed test assertion from `.Error.Message` to `.Error.Description`
- **Files modified:** DispensingHandlerTests.cs
- **Commit:** b05a87a

## TDD Cycle

| Phase | Commit | Result |
|-------|--------|--------|
| RED | a6bae09 | PendingPrescriptionQuery + tests written, build fails (handlers missing) |
| GREEN | b05a87a | All 3 handlers implemented, all 9 tests pass |
| REFACTOR | N/A | No refactoring needed -- code clean on first pass |

## Self-Check: PASSED

All 5 key files present. Both commits (a6bae09, b05a87a) confirmed in git history. All 9 dispensing tests pass.

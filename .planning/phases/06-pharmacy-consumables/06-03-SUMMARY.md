---
phase: 06-pharmacy-consumables
plan: "03"
subsystem: pharmacy-domain
tags: [domain-entities, dispensing, pharmacy, fefo, batch-deduction]
dependency_graph:
  requires: [06-01, 06-02]
  provides: [dispensing-domain-model]
  affects: [pharmacy-application, pharmacy-infrastructure]
tech_stack:
  added: []
  patterns: [aggregate-root-with-children, dual-nullable-fk, backing-field-collection, internal-factory]
key_files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Enums/DispensingStatus.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/BatchDeduction.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/DispensingLine.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/DispensingRecord.cs
  modified: []
decisions:
  - "DispensingLine.Create() is internal (not public) — lines are always created through DispensingRecord.AddLine() to enforce aggregate ownership"
  - "BatchDeduction has dual nullable FK (DispensingLineId, OtcSaleLineId) with separate factory methods for each parent type, enabling shared entity reuse in Plan 04 OTC sales"
  - "DispensingLine.AddBatchDeduction() delegates to BatchDeduction.CreateForDispensing() passing the line's own Id — encapsulates parent FK wiring inside the child entity"
metrics:
  duration: 1min
  completed_date: "2026-03-06"
  tasks_completed: 2
  files_changed: 4
---

# Phase 06 Plan 03: Dispensing Domain Entities Summary

Dispensing domain model with FEFO multi-batch deduction: DispensingRecord aggregate -> DispensingLine children -> BatchDeduction grandchildren, with shared BatchDeduction supporting both dispensing and OTC sale parent FKs.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Create DispensingStatus enum and BatchDeduction entity | 097309b | DispensingStatus.cs, BatchDeduction.cs |
| 2 | Create DispensingRecord and DispensingLine entities | 346a3d2 | DispensingRecord.cs, DispensingLine.cs |

## What Was Built

### DispensingStatus Enum
- `Dispensed = 0`: line was dispensed, batch deductions recorded
- `Skipped = 1`: line intentionally skipped (out of stock, patient refusal, etc.)
- All-or-nothing per drug line enforced by this status

### BatchDeduction Entity
- Dual nullable FK: `DispensingLineId` and `OtcSaleLineId` (exactly one non-null per instance)
- `CreateForDispensing(dispensingLineId, drugBatchId, batchNumber, quantity)` factory
- `CreateForOtcSale(otcSaleLineId, drugBatchId, batchNumber, quantity)` factory
- `BatchNumber` denormalized for audit records without cross-module joins
- Domain guard: quantity must be positive, batch number required

### DispensingLine Entity
- `DispensingRecordId` FK back to aggregate root
- `PrescriptionItemId` FK to Clinical PrescriptionItem (cross-module reference)
- `DrugCatalogItemId` FK to Pharmacy DrugCatalogItem
- `DrugName` denormalized for audit
- `Status: DispensingStatus` — Dispensed or Skipped
- Backing field `_batchDeductions` with `IReadOnlyCollection<BatchDeduction>` public surface
- `AddBatchDeduction(drugBatchId, batchNumber, quantity)` creates BatchDeduction via `CreateForDispensing`
- Internal `Create()` factory — created only through `DispensingRecord.AddLine()`

### DispensingRecord Aggregate
- `AggregateRoot` + `IAuditable` — full audit tracking
- `PrescriptionId`: Clinical DrugPrescription.Id (cross-module reference)
- `VisitId`, `PatientId`, `PatientName` (denormalized), `DispensedById`, `DispensedAt`
- `OverrideReason` nullable string for expired prescription override per PHR-07
- Backing field `_lines` with `IReadOnlyCollection<DispensingLine>` public surface
- `Create(prescriptionId, visitId, patientId, patientName, dispensedById, overrideReason, branchId)` factory
- `AddLine(prescriptionItemId, drugCatalogItemId, drugName, quantity, status)` returns created line for batch deductions

## Verification

`dotnet build backend/src/Modules/Pharmacy/Pharmacy.Domain/Pharmacy.Domain.csproj --no-restore -v q`
Result: Build succeeded. 0 Warning(s). 0 Error(s).

## Deviations from Plan

None - plan executed exactly as written.

## Self-Check: PASSED

Files verified:
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Domain/Enums/DispensingStatus.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/BatchDeduction.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/DispensingLine.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/DispensingRecord.cs

Commits verified:
- FOUND: 097309b (Task 1)
- FOUND: 346a3d2 (Task 2)

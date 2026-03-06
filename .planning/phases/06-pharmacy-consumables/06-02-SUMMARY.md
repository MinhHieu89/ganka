---
phase: 06-pharmacy-consumables
plan: "02"
subsystem: pharmacy-domain
tags: [domain-entities, stock-import, stock-adjustment, inventory, pharmacy]
dependency_graph:
  requires: [06-01]
  provides: [StockImport, StockImportLine, StockAdjustment, StockAdjustmentReason]
  affects: [Pharmacy.Infrastructure, Pharmacy.Application]
tech_stack:
  added: []
  patterns: [AggregateRoot-with-backing-field, dual-FK-nullable-pattern, domain-guards, factory-method]
key_files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/StockImport.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/StockImportLine.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/StockAdjustment.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Enums/StockAdjustmentReason.cs
  modified: []
decisions:
  - "StockAdjustment uses dual nullable FK (DrugBatchId, ConsumableBatchId) with exactly-one-non-null domain guard for shared use across pharmacy and consumables warehouse"
  - "StockImportLine domain guard requires expiry date in the future at creation time"
  - "IAuditable is a marker interface only -- no properties to add, Entity base already has CreatedAt/UpdatedAt"
metrics:
  duration: "2min"
  completed: "2026-03-06"
  tasks_completed: 2
  files_created: 4
  files_modified: 0
requirements_addressed: [PHR-02]
---

# Phase 06 Plan 02: Stock Import and Adjustment Domain Entities Summary

**One-liner:** StockImport AggregateRoot with backing-field Lines collection and StockAdjustment Entity with dual-FK (DrugBatchId/ConsumableBatchId) for shared pharmacy+consumables adjustment tracking.

## What Was Built

### Task 1: StockImport and StockImportLine Entities

**StockImport** (`Pharmacy.Domain/Entities/StockImport.cs`):
- AggregateRoot implementing IAuditable
- Tracks a single stock import event (supplier invoice or Excel bulk upload)
- Properties: SupplierId, SupplierName (denormalized), ImportSource enum, InvoiceNumber?, ImportedById, ImportedAt, Notes?
- Backing field `_lines` (List<StockImportLine>) with IReadOnlyCollection<StockImportLine> Lines property
- Factory: `Create(supplierId, supplierName, importSource, invoiceNumber, importedById, notes, branchId)`
- Method: `AddLine(drugCatalogItemId, drugName, batchNumber, expiryDate, quantity, purchasePrice)` — creates and adds StockImportLine, returns the created line

**StockImportLine** (`Pharmacy.Domain/Entities/StockImportLine.cs`):
- Entity child of StockImport (FK: StockImportId)
- Properties: StockImportId, DrugCatalogItemId, DrugName (denormalized), BatchNumber, ExpiryDate, Quantity, PurchasePrice
- Factory: `Create(stockImportId, drugCatalogItemId, drugName, batchNumber, expiryDate, quantity, purchasePrice)`
- Domain guards: quantity must be positive, expiryDate must be in the future, purchasePrice non-negative, drugName and batchNumber required

### Task 2: StockAdjustment Entity and StockAdjustmentReason Enum

**StockAdjustmentReason** (`Pharmacy.Domain/Enums/StockAdjustmentReason.cs`):
- Enum values: Correction=0, WriteOff=1, Damage=2, Expired=3, Other=4

**StockAdjustment** (`Pharmacy.Domain/Entities/StockAdjustment.cs`):
- Entity implementing IAuditable (marker interface; Entity base provides CreatedAt/UpdatedAt)
- Dual nullable FK design: DrugBatchId (Guid?), ConsumableBatchId (Guid?)
- QuantityChange (int): positive for stock additions, negative for removals
- Reason (StockAdjustmentReason), Notes?, AdjustedById, AdjustedAt
- Factory: `Create(drugBatchId, consumableBatchId, quantityChange, reason, notes, adjustedById)`
- Domain guards: exactly one FK must be non-null (both null or both non-null throw), quantityChange must not be zero

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Removed duplicate IAuditable properties from StockAdjustment**
- **Found during:** Task 2 verification (build warnings)
- **Issue:** Initial implementation added CreatedAt, UpdatedAt, CreatedById, UpdatedById as explicit properties, but IAuditable is a marker interface only and Entity base already provides CreatedAt and UpdatedAt with private setters
- **Fix:** Removed duplicate property declarations; StockAdjustment relies on Entity base for audit timestamps
- **Files modified:** `Pharmacy.Domain/Entities/StockAdjustment.cs`
- **Commit:** 31624a7

## Verification

- `dotnet build Pharmacy.Domain.csproj --no-restore -v q` — Build succeeded, 0 warnings, 0 errors

## Self-Check: PASSED

All 4 created files exist on disk. Both task commits (a28042a, 31624a7) verified in git log.

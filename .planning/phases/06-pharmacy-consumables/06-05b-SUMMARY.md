---
phase: 06-pharmacy-consumables
plan: 05b
subsystem: pharmacy-infrastructure
tags: [ef-core, configuration, dispensing, otc-sales, consumables, stock-adjustment]
dependency_graph:
  requires: [06-03, 06-04]
  provides: [dispensing-ef-config, otc-sale-ef-config, consumable-ef-config, stock-adjustment-ef-config]
  affects: [pharmacy-db-context, pharmacy-migrations]
tech_stack:
  added: []
  patterns: [ef-core-configuration, backing-field-access, optimistic-concurrency, cascade-delete]
key_files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DispensingRecordConfiguration.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/OtcSaleConfiguration.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/ConsumableItemConfiguration.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/StockAdjustmentConfiguration.cs
  modified:
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/PharmacyDbContext.cs
decisions:
  - "BatchDeductionConfiguration placed in DispensingRecordConfiguration.cs since BatchDeduction is the shared child entity first introduced by the dispensing hierarchy"
  - "ConsumableBatch uses HasOne<ConsumableItem>().WithMany() (not HasMany on ConsumableItem) since ConsumableItem has no navigation property to batches"
  - "PharmacyDbContext updated inline with 9 new DbSets covering all phase 6 entities"
metrics:
  duration: 2min
  completed: 2026-03-06
  tasks_completed: 2
  files_changed: 5
---

# Phase 6 Plan 05b: EF Core Configurations for Dispensing, OTC, Consumables, StockAdjustment Summary

EF Core configurations for DispensingRecord/Line/BatchDeduction, OtcSale/Line, ConsumableItem/Batch, and StockAdjustment with backing field access and RowVersion concurrency on ConsumableBatch.

## What Was Built

4 configuration files covering 8 entities total:

1. **DispensingRecordConfiguration.cs** — 3 entity configurations:
   - `DispensingRecord`: "DispensingRecords" table with BranchId conversion, _lines backing field access, cascading HasMany to DispensingLines
   - `DispensingLine`: "DispensingLines" table with DispensingStatus HasConversion<int>(), _batchDeductions backing field access, cascading HasMany to BatchDeductions
   - `BatchDeduction`: "BatchDeductions" table with dual nullable FKs (DispensingLineId/OtcSaleLineId), required DrugBatchId, MaxLength(100) on BatchNumber

2. **OtcSaleConfiguration.cs** — 2 entity configurations:
   - `OtcSale`: "OtcSales" table with nullable PatientId, BranchId conversion, _lines backing field access, cascading HasMany to OtcSaleLines
   - `OtcSaleLine`: "OtcSaleLines" table with HasPrecision(18,2) on UnitPrice, _batchDeductions backing field access, cascading HasMany to BatchDeductions via OtcSaleLineId

3. **ConsumableItemConfiguration.cs** — 2 entity configurations:
   - `ConsumableItem`: "ConsumableItems" table with Vietnamese_CI_AI collation on Name/NameVi, ConsumableTrackingMode HasConversion<int>(), HasDefaultValue(0) on CurrentStock/MinStockLevel, HasDefaultValue(true) on IsActive, BranchId conversion, Name index
   - `ConsumableBatch`: "ConsumableBatches" table with IsRowVersion() on RowVersion, DateOnly ExpiryDate, HasOne<ConsumableItem>().WithMany() relationship, FEFO composite index on (ConsumableItemId, ExpiryDate)

4. **StockAdjustmentConfiguration.cs** — 1 entity configuration:
   - `StockAdjustment`: "StockAdjustments" table with dual nullable FKs (DrugBatchId/ConsumableBatchId), StockAdjustmentReason HasConversion<int>(), MaxLength(1000) on Notes

**PharmacyDbContext** updated with 9 new DbSets: DispensingRecords, DispensingLines, BatchDeductions, OtcSales, OtcSaleLines, ConsumableItems, ConsumableBatches, StockAdjustments.

## Verification

- `dotnet build Pharmacy.Infrastructure` passes with 0 warnings, 0 errors
- 4 configuration files created covering 8 entities
- Backing field access configured on all 4 collection navigations (_lines x2, _batchDeductions x2)
- RowVersion on ConsumableBatch confirmed
- BranchId conversion consistent with existing DrugCatalogItem pattern

## Deviations from Plan

None - plan executed exactly as written.

## Self-Check: PASSED

Files created:
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DispensingRecordConfiguration.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/OtcSaleConfiguration.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/ConsumableItemConfiguration.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/StockAdjustmentConfiguration.cs

Commits:
- FOUND: 4c2837d (Task 1 - DispensingRecord and OtcSale configurations)
- FOUND: 30a8538 (Task 2 - ConsumableItem and StockAdjustment configurations)

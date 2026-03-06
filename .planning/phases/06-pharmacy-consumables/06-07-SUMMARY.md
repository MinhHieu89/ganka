---
phase: 06-pharmacy-consumables
plan: 07
subsystem: pharmacy-application-interfaces
tags: [repository-interfaces, pharmacy, FEFO, dispensing, consumables]
dependency_graph:
  requires: [06-01, 06-02, 06-03, 06-04]
  provides: [ISupplierRepository, IDrugBatchRepository, IStockImportRepository, IDispensingRepository, IOtcSaleRepository, IConsumableRepository]
  affects: [Pharmacy.Application, Pharmacy.Infrastructure]
tech_stack:
  added: []
  patterns: [repository-per-aggregate, interface-abstraction, paginated-tuple-return]
key_files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/ISupplierRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IDrugBatchRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IStockImportRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IDispensingRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IOtcSaleRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IConsumableRepository.cs
  modified: []
decisions:
  - "[06-07]: IDrugBatchRepository.GetAvailableBatchesFEFOAsync filters CurrentQuantity > 0 and not expired, ordered by ExpiryDate ASC -- batch selection order enforced at interface contract level"
  - "[06-07]: IStockImportRepository and IOtcSaleRepository use (List<T>, int TotalCount) tuple return for paginated queries -- consistent with established pattern"
  - "[06-07]: IDispensingRepository.GetByPrescriptionIdAsync returns nullable to support duplicate-dispensing-check without separate Exists() call"
  - "[06-07]: IConsumableRepository.AddStockAdjustment accepts StockAdjustment (same entity as IDrugBatchRepository) -- shared StockAdjustment entity used across both warehouse types"
metrics:
  duration: 2min
  completed_date: "2026-03-06"
  tasks_completed: 2
  files_created: 6
  files_modified: 0
---

# Phase 06 Plan 07: Pharmacy Repository Interfaces Summary

**One-liner:** Six Application-layer repository interfaces for all pharmacy aggregate roots including FEFO batch selection and paginated history queries.

## What Was Built

Created 6 repository interfaces in the Pharmacy.Application layer following the existing IDrugCatalogItemRepository pattern:

### Task 1: ISupplierRepository, IDrugBatchRepository, IStockImportRepository

**ISupplierRepository** - Supplier CRUD plus SupplierDrugPrice management:
- `GetByIdAsync`, `GetAllActiveAsync` for supplier entity access
- `GetSupplierDrugPricesAsync` for supplier price list queries
- `GetDefaultPriceAsync` for single drug price lookup (returns decimal?)
- `Add`, `AddSupplierDrugPrice`, `UpdateSupplierDrugPrice` for persistence

**IDrugBatchRepository** - Critical FEFO interface for dispensing:
- `GetBatchesForDrugAsync` - all batches including empty (for audit)
- `GetAvailableBatchesFEFOAsync` - CurrentQuantity > 0, not expired, ordered ExpiryDate ASC
- `GetTotalStockAsync` - sum of available quantities
- `GetExpiryAlertsAsync(int daysThreshold)` - returns ExpiryAlertDto list
- `GetLowStockAlertsAsync()` - returns LowStockAlertDto for under-minimum drugs
- `Add`, `AddStockAdjustment` for persistence

**IStockImportRepository** - Stock import record management:
- `GetByIdAsync`, paginated `GetAllAsync`, `Add`

### Task 2: IDispensingRepository, IOtcSaleRepository, IConsumableRepository

**IDispensingRepository** - Prescription dispensing records:
- `GetByPrescriptionIdAsync` - duplicate-dispensing check (returns nullable)
- Paginated `GetHistoryAsync(page, pageSize, Guid? patientId)` with optional patient filter
- `GetByIdAsync`, `Add`

**IOtcSaleRepository** - Walk-in OTC sales:
- `GetByIdAsync`, paginated `GetAllAsync`, `Add`

**IConsumableRepository** - Consumable items and batches (both tracking modes):
- `GetBatchesAsync` / `GetBatchByIdAsync` for ExpiryTracked batch access
- `GetAlertsAsync` - items where IsLowStock is true
- `Add`, `Update`, `AddBatch`, `AddStockAdjustment`

## Success Criteria Verification

- [x] 6 repository interfaces defined in Application layer (5 new + 1 existing IDrugCatalogItemRepository)
- [x] IDrugBatchRepository has FEFO method (`GetAvailableBatchesFEFOAsync`) and alert query methods
- [x] `dotnet build Pharmacy.Application` succeeds (0 warnings, 0 errors)

## Deviations from Plan

None - plan executed exactly as written.

## Self-Check: PASSED

Files verified:
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/ISupplierRepository.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IDrugBatchRepository.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IStockImportRepository.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IDispensingRepository.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IOtcSaleRepository.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IConsumableRepository.cs

Commits verified:
- 922c7ff: feat(06-07): add ISupplierRepository, IDrugBatchRepository, IStockImportRepository
- 0552f02: feat(06-07): add IDispensingRepository, IOtcSaleRepository, IConsumableRepository

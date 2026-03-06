---
phase: 06-pharmacy-consumables
plan: 06
subsystem: pharmacy-contracts
tags: [contracts, dtos, dbcontext, pharmacy, consumables]
dependency_graph:
  requires: [06-01, 06-02, 06-03, 06-04]
  provides: [Pharmacy.Contracts.Dtos.SupplierDto, Pharmacy.Contracts.Dtos.DrugBatchDto, Pharmacy.Contracts.Dtos.DispensingDto, Pharmacy.Contracts.Dtos.ConsumableDto]
  affects: [Pharmacy.Application, Pharmacy.Infrastructure, Clinical module consumers]
tech_stack:
  added: []
  patterns: [sealed-record-dtos, optional-param-backward-compat, int-serialized-enums]
key_files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/SupplierDto.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/DrugBatchDto.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/DispensingDto.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/ConsumableDto.cs
  modified:
    - backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/DrugCatalogItemDto.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/GetAllActiveDrugs.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DrugCatalogItemRepository.cs
decisions:
  - "DrugCatalogItemDto extended with SellingPrice and MinStockLevel as optional C# record parameters (default null/0) for backward compatibility with all existing Clinical module consumers"
  - "PendingPrescriptionDto placed in DispensingDto.cs (not Clinical.Contracts) since it is constructed by Pharmacy query handlers, not Clinical handlers"
  - "All enum fields in DTOs use int to avoid cross-module Domain enum dependency per established Contracts pattern"
metrics:
  duration: 3min
  completed_date: "2026-03-06"
  tasks_completed: 2
  files_changed: 7
---

# Phase 06 Plan 06: Pharmacy DbContext + Contracts DTOs Summary

**One-liner:** 4 new Contracts DTO files (14 record types) for supplier, batch, dispensing, and consumables cross-module data transfer, plus DrugCatalogItemDto extended with pricing fields.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Update PharmacyDbContext with all entity DbSets | (already complete from prior plans) | PharmacyDbContext.cs |
| 2 | Create all Contracts DTOs | b3b42b6 | SupplierDto.cs, DrugBatchDto.cs, DispensingDto.cs, ConsumableDto.cs, DrugCatalogItemDto.cs |

## What Was Built

### Task 1: PharmacyDbContext DbSets (Pre-existing)

The PharmacyDbContext already had all 14 required DbSets from prior plan executions (plans 01-05b). No changes were needed:
- DrugCatalogItems, Suppliers, SupplierDrugPrices, DrugBatches
- StockImports, StockImportLines, StockAdjustments
- DispensingRecords, DispensingLines, BatchDeductions
- OtcSales, OtcSaleLines
- ConsumableItems, ConsumableBatches

### Task 2: Contracts DTOs

**SupplierDto.cs** (2 records):
- `SupplierDto(Guid Id, string Name, string? ContactInfo, string? Phone, string? Email, bool IsActive)`
- `SupplierDrugPriceDto(Guid Id, Guid SupplierId, string SupplierName, Guid DrugCatalogItemId, string DrugName, decimal DefaultPurchasePrice)`

**DrugBatchDto.cs** (7 records):
- `DrugBatchDto` - batch with expiry, quantity, IsExpired/IsNearExpiry computed fields
- `DrugInventoryDto` - drug inventory summary aggregating batch data with catalog info
- `StockImportDto` / `StockImportLineDto` - stock import event and line items
- `StockAdjustmentDto` - manual stock adjustment with dual nullable FK (drug or consumable)
- `ExpiryAlertDto` / `LowStockAlertDto` - alert DTOs for pharmacy inventory warnings

**DispensingDto.cs** (7 records):
- `DispensingRecordDto` / `DispensingLineDto` / `BatchDeductionDto` - dispensing hierarchy
- `OtcSaleDto` / `OtcSaleLineDto` - OTC walk-in sale records
- `PendingPrescriptionDto` / `PendingPrescriptionItemDto` - pharmacy queue for pending Rx

**ConsumableDto.cs** (2 records):
- `ConsumableItemDto` - consumable with tracking mode (ExpiryTracked/SimpleStock)
- `ConsumableBatchDto` - consumable batch for expiry-tracked items

**DrugCatalogItemDto extended:**
- Added `decimal? SellingPrice = null` and `int MinStockLevel = 0` as optional parameters
- Backward-compatible: all existing Clinical module consumers unaffected (positional records only need updating at construction sites)
- Updated 2 construction sites: `SearchAsync` in Repository, `GetAllActiveDrugs` handler

## Deviations from Plan

### Auto-fixed Issues

None.

### Scope Notes

- Task 1 was already complete from prior plans (01-05b executed the entity and configuration work that triggered DbSet additions). Verified build passes with all 14 DbSets.
- `DrugCatalogItemDto` used optional parameters (default values) rather than creating a separate `ExtendedDrugCatalogItemDto` since all construction sites are within Pharmacy module and backward compatibility is maintained through C# optional parameters.

## Verification

Both build targets pass:
- `dotnet build Pharmacy.Infrastructure` -- Build succeeded, 0 errors
- `dotnet build Pharmacy.Contracts` -- Build succeeded, 0 errors

## Self-Check: PASSED

Files created:
- backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/SupplierDto.cs -- FOUND
- backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/DrugBatchDto.cs -- FOUND
- backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/DispensingDto.cs -- FOUND
- backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/ConsumableDto.cs -- FOUND

Commit verified: b3b42b6

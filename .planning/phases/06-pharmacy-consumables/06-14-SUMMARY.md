---
phase: 06-pharmacy-consumables
plan: 14
subsystem: pharmacy-application
tags: [tdd, otc-sales, inventory, fefo, stock-adjustment, drug-batches]
dependency_graph:
  requires:
    - 06-08  # IDrugBatchRepository.GetBatchesForDrugAsync (Plan 07 via 08)
    - 06-10  # IDrugCatalogItemRepository.GetAllWithInventoryAsync
  provides:
    - CreateOtcSaleHandler with FEFO batch deduction
    - AdjustStockHandler with StockAdjustment audit record
    - GetDrugInventoryHandler with computed stock levels
    - GetDrugBatchesHandler returning batches per drug via IDrugBatchRepository
  affects:
    - Plan 17 (GET /inventory/{drugId}/batches endpoint consumes GetDrugBatchesHandler)
tech_stack:
  added: []
  patterns:
    - Wolverine static handler pattern (same as dispensing)
    - FEFOAllocator reuse for OTC sales (no separate logic)
    - TDD red-green cycle with NSubstitute mocks
key_files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/OtcSales/CreateOtcSale.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/OtcSales/GetOtcSales.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Inventory/AdjustStock.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Inventory/GetDrugInventory.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Inventory/GetDrugBatches.cs
    - backend/tests/Pharmacy.Unit.Tests/Features/OtcSaleAndInventoryHandlerTests.cs
  modified: []
decisions:
  - "OTC sale handler reuses FEFOAllocator.Allocate() identical to dispensing - no duplicate batch selection logic"
  - "GetDrugBatchesHandler returns all batches (active/expired/empty) ordered by ExpiryDate for full pharmacist history view"
  - "AdjustStockHandler catches InvalidOperationException from DrugBatch.Deduct() and converts to Result.Failure per established domain boundary pattern"
  - "GetDrugBatchesHandler maps DrugBatch entities to DrugBatchDto with SupplierName empty - Infrastructure layer can enrich if needed"
metrics:
  duration: 3min
  completed_date: "2026-03-06"
  tasks_completed: 1
  files_created: 6
---

# Phase 06 Plan 14: OTC Sales and Inventory Management Summary

OTC sale handler with FEFO batch deduction reusing FEFOAllocator; AdjustStock with StockAdjustment audit record; GetDrugInventory returning computed stock levels; GetDrugBatches querying IDrugBatchRepository.GetBatchesForDrugAsync for FEFO-ordered batch history.

## Tasks Completed

| Task | Description | Commit | Status |
|------|-------------|--------|--------|
| 1 (RED) | Write 10 failing tests for OTC sale and inventory handlers | d1eb6ee | DONE |
| 1 (GREEN) | Implement 5 handler files to pass all tests | a4e9c1a | DONE |

## What Was Built

### CreateOtcSale.cs
Command/handler for walk-in OTC sales without prescription (PHR-06):
- `CreateOtcSaleCommand(Guid? PatientId, string? CustomerName, string? Notes, List<OtcSaleLineInput> Lines)`
- `OtcSaleLineInput(DrugCatalogItemId, DrugName, Quantity, UnitPrice)` per plan
- Handler uses `FEFOAllocator.Allocate()` identically to `DispenseDrugsHandler` - zero code duplication
- Customer linkage optional: `PatientId` and `CustomerName` are both nullable for anonymous sales
- Each line gets `OtcSaleLine` aggregate and `BatchDeduction` records via `AddBatchDeduction()`

### GetOtcSales.cs
Paginated query for OTC sale history:
- `GetOtcSalesQuery(Page, PageSize)` with safe clamping
- Returns `OtcSalesPagedResult(Items, TotalCount)` wrapper

### AdjustStock.cs
Manual stock correction with full audit trail (PHR-01):
- `AdjustStockCommand(DrugBatchId, QuantityChange, StockAdjustmentReason, Notes?)`
- Positive `QuantityChange` → `batch.AddStock(qty)` domain method
- Negative `QuantityChange` → `batch.Deduct(abs(qty))` with domain invariant guard
- Creates `StockAdjustment.Create(drugBatchId, null, quantityChange, reason, notes, userId)` for audit
- `InvalidOperationException` from `Deduct()` caught and converted to `Error.Custom("StockAdjustment.InsufficientStock")`

### GetDrugInventory.cs
Drug inventory list with computed stock levels and alert flags:
- `GetDrugInventoryQuery(ExpiryAlertDays = 30)`
- Delegates entirely to `IDrugCatalogItemRepository.GetAllWithInventoryAsync(expiryAlertDays, ct)`
- Returns `List<DrugInventoryDto>` with `TotalStock`, `BatchCount`, `IsLowStock`, `HasExpiryAlert`

### GetDrugBatches.cs
Batch list for a specific drug (bridging Plan 07 repository to Plan 17 endpoint):
- `GetDrugBatchesQuery(Guid DrugCatalogItemId)` record
- Delegates to `IDrugBatchRepository.GetBatchesForDrugAsync(drugCatalogItemId, ct)`
- Maps `DrugBatch` domain entities to `DrugBatchDto` with FEFO ordering (ExpiryDate ASC)
- Returns all batches (active, expired, empty) for full pharmacist history view

## Tests

10 tests in `OtcSaleAndInventoryHandlerTests.cs`, all GREEN:

| Test | Scenario | Result |
|------|----------|--------|
| CreateOtcSale_ValidWithCustomer_CreatesSaleAndDeductsStock | Valid sale linked to patient | PASS |
| CreateOtcSale_Anonymous_CreatesSale | No PatientId, no CustomerName | PASS |
| CreateOtcSale_InsufficientStock_ReturnsError | Stock insufficient for line qty | PASS |
| AdjustStock_PositiveAdjustment_IncreasesQuantity | +10 on batch with 20 → 30 | PASS |
| AdjustStock_NegativeAdjustment_DecreasesQuantity | -5 on batch with 20 → 15 | PASS |
| AdjustStock_ExcessiveNegative_ReturnsError | -100 on batch with 5 units | PASS |
| AdjustStock_CreatesAuditRecord | StockAdjustment saved via AddStockAdjustment | PASS |
| GetDrugInventory_ReturnsComputedStockLevels | Returns DrugInventoryDto list | PASS |
| GetDrugBatches_ValidDrug_ReturnsBatchesFEFOOrdered | Two batches ordered by ExpiryDate | PASS |
| GetDrugBatches_NoMatchingDrug_ReturnsEmptyList | Empty list for unknown drugId | PASS |

Full suite: 66/66 Pharmacy.Unit.Tests pass.

## Deviations from Plan

None - plan executed exactly as written.

## Self-Check: PASSED

- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Features/OtcSales/CreateOtcSale.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Features/OtcSales/GetOtcSales.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Inventory/AdjustStock.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Inventory/GetDrugInventory.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Inventory/GetDrugBatches.cs
- FOUND: backend/tests/Pharmacy.Unit.Tests/Features/OtcSaleAndInventoryHandlerTests.cs
- FOUND: d1eb6ee (test commit)
- FOUND: a4e9c1a (feat commit)
- Tests: 10/10 OtcSaleAndInventoryHandlerTests pass; 66/66 Pharmacy.Unit.Tests pass

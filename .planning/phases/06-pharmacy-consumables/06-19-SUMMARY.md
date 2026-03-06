---
phase: 06-pharmacy-consumables
plan: 19
subsystem: frontend-api
tags: [pharmacy, consumables, tanstack-query, typescript, api-layer]
dependency_graph:
  requires: [06-17, 06-18]
  provides: [pharmacy-api-layer, consumables-api-layer]
  affects: [pharmacy-pages, consumables-pages, sidebar-badge]
tech_stack:
  added: []
  patterns: [openapi-fetch-as-never, tanstack-query-key-factory, mutation-onerror-toast]
key_files:
  created:
    - frontend/src/features/pharmacy/api/pharmacy-queries.ts
    - frontend/src/features/consumables/api/consumables-api.ts
    - frontend/src/features/consumables/api/consumables-queries.ts
  modified:
    - frontend/src/features/pharmacy/api/pharmacy-api.ts
    - frontend/src/features/pharmacy/components/DrugCatalogPage.tsx
    - frontend/src/features/pharmacy/components/DrugFormDialog.tsx
decisions:
  - Hooks separated into pharmacy-queries.ts while types/functions remain in pharmacy-api.ts
  - Existing components updated to import hooks from pharmacy-queries.ts to avoid duplication
  - consumables feature directory created at frontend/src/features/consumables/
metrics:
  duration: 3min
  completed: 2026-03-06
  tasks: 2
  files: 6
---

# Phase 06 Plan 19: Frontend API Layer for Pharmacy and Consumables Summary

**One-liner:** Pharmacy and consumables API layer with typed DTOs matching backend Contracts, TanStack Query hooks with key factories, 30s polling for pending prescription count, and all mutations with onError toast.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Create pharmacy API functions and TanStack Query hooks | 2ddfe90 | pharmacy-api.ts, pharmacy-queries.ts, DrugCatalogPage.tsx, DrugFormDialog.tsx |
| 2 | Create consumables API functions and TanStack Query hooks | f9b39af | consumables-api.ts, consumables-queries.ts |

## What Was Built

### Task 1: Pharmacy API Layer

**pharmacy-api.ts** (extended from existing drug catalog functions):
- TypeScript interfaces: SupplierDto, SupplierDrugPriceDto, DrugBatchDto, DrugInventoryDto, StockImportDto, StockImportLineDto, ExpiryAlertDto, LowStockAlertDto, PendingPrescriptionDto, DispensingRecordDto, OtcSaleDto, PendingCountDto
- Request types: CreateSupplierInput, UpdateSupplierInput, UpdateDrugPricingInput, AdjustStockInput, CreateStockImportInput, DispenseDrugsInput, DispenseLineInput, BatchAllocationInput, CreateOtcSaleInput, OtcSaleLineInput, ExcelImportPreviewDto
- API functions for all pharmacy endpoints including Excel import using native fetch + FormData pattern
- Preserved existing drug catalog functions with backward compatibility exports

**pharmacy-queries.ts** (new):
- pharmacyKeys factory with all key patterns: suppliers, inventory (with batches), stockImports, alerts (expiry/lowStock), dispensing (pending/pendingCount/history), otcSales
- Query hooks: useSuppliers, useDrugInventory, useDrugBatches, useStockImports, useExpiryAlerts, useLowStockAlerts, usePendingPrescriptions, usePendingCount (30s polling), useDispensingHistory, useOtcSales
- Drug catalog hooks migrated: useDrugCatalogList, useDrugCatalogSearch
- Mutation hooks: useCreateSupplier, useUpdateSupplier, useUpdateDrugPricing, useAdjustStock, useCreateStockImport, useDispenseDrugs, useCreateOtcSale, useCreateDrugCatalogItem, useUpdateDrugCatalogItem
- All mutations have onError with toast.error

### Task 2: Consumables API Layer

**consumables-api.ts** (new):
- TypeScript interfaces: ConsumableItemDto, ConsumableBatchDto, ConsumableAlertDto
- Request types: CreateConsumableItemInput, UpdateConsumableItemInput, AddConsumableStockInput, AdjustConsumableStockInput
- API functions: getConsumableItems, createConsumableItem, updateConsumableItem, addConsumableStock, adjustConsumableStock, getConsumableAlerts

**consumables-queries.ts** (new):
- consumableKeys factory: items.all, alerts.all
- Query hooks: useConsumableItems, useConsumableAlerts
- Mutation hooks: useCreateConsumableItem, useUpdateConsumableItem, useAddConsumableStock, useAdjustConsumableStock
- All mutations have onError with toast.error

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Backward Compatibility] Updated existing components to import from pharmacy-queries.ts**
- **Found during:** Task 1
- **Issue:** DrugCatalogPage.tsx and DrugFormDialog.tsx imported hooks (useDrugCatalogList, useCreateDrugCatalogItem, useUpdateDrugCatalogItem) directly from pharmacy-api.ts, which would duplicate hook definitions after moving them to pharmacy-queries.ts
- **Fix:** Updated both components to import hooks from pharmacy-queries.ts while keeping type/enum imports from pharmacy-api.ts
- **Files modified:** DrugCatalogPage.tsx, DrugFormDialog.tsx
- **Commit:** 2ddfe90

## Self-Check

### Created files exist:
- `frontend/src/features/pharmacy/api/pharmacy-queries.ts` - FOUND
- `frontend/src/features/consumables/api/consumables-api.ts` - FOUND
- `frontend/src/features/consumables/api/consumables-queries.ts` - FOUND

### Commits exist:
- 2ddfe90 - FOUND (feat(06-19): create pharmacy API functions and TanStack Query hooks)
- f9b39af - FOUND (feat(06-19): create consumables API functions and TanStack Query hooks)

### TypeScript: No errors in pharmacy or consumables files (60 pre-existing unrelated errors in other modules unchanged)

## Self-Check: PASSED

---
phase: 06-pharmacy-consumables
plan: 18
subsystem: pharmacy-api
tags: [api-endpoints, dispensing, consumables, otc-sales, bootstrapper, migration]
dependency_graph:
  requires: [06-09, 06-13, 06-14, 06-15, 06-16, 06-17]
  provides: [dispensing-api, consumables-api, otc-sales-api, pharmacy-inventory-schema]
  affects: [bootstrapper, pharmacy-presentation, clinical-application]
tech_stack:
  added: []
  patterns: [minimal-api-extension-methods, route-group-per-domain, ef-core-migration]
key_files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Presentation/DispensingApiEndpoints.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Presentation/ConsumablesApiEndpoints.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Migrations/20260306084141_PharmacyInventory.cs
  modified:
    - backend/src/Bootstrapper/Program.cs
decisions:
  - "[06-18]: MapDispensingApiEndpoints and MapConsumablesApiEndpoints kept as separate extension methods (not merged into MapPharmacyApiEndpoints) for clear separation between pharmacy drug operations and consumables warehouse"
  - "[06-18]: Dispensing/OTC endpoints under /api/pharmacy route group (consistent with existing pharmacy routes); consumables under separate /api/consumables route group as specified"
  - "[06-18]: /dispensing/pending/count endpoint computes non-expired count from same query result (no separate DB query)"
  - "[06-18]: GetPendingPrescriptions handler in Clinical.Application already existed from Plan 17 - no re-implementation needed"
metrics:
  duration: 8min
  completed_date: "2026-03-06"
  tasks_completed: 2
  files_changed: 4
---

# Phase 06 Plan 18: Dispensing and Consumables API Endpoints Summary

**One-liner:** HTTP API surface for prescription dispensing (PHR-05/07), OTC sales (PHR-06), and consumables warehouse (CON-01/02) wired into Bootstrapper with PharmacyInventory EF migration.

## What Was Built

### Task 1: DispensingApiEndpoints.cs + ConsumablesApiEndpoints.cs

**DispensingApiEndpoints** (`/api/pharmacy`, RequireAuthorization):
- `GET /dispensing/pending?patientId=` - pending prescription queue (GetPendingPrescriptionsQuery)
- `GET /dispensing/pending/count` - sidebar badge count (non-expired prescriptions only)
- `POST /dispensing` - dispense drugs against prescription (DispenseDrugsCommand)
- `GET /dispensing/history?page=&pageSize=&patientId=` - paginated dispensing history (GetDispensingHistoryQuery)
- `POST /otc-sales` - process walk-in OTC sale (CreateOtcSaleCommand)
- `GET /otc-sales?page=&pageSize=` - paginated OTC sale history (GetOtcSalesQuery)

**ConsumablesApiEndpoints** (`/api/consumables`, RequireAuthorization):
- `GET /` - all active consumable items with computed stock (GetConsumableItemsQuery)
- `POST /` - create consumable item (CreateConsumableItemCommand)
- `PUT /{id}` - update consumable item metadata (UpdateConsumableItemCommand)
- `POST /{id}/stock` - add stock (SimpleStock increment or ExpiryTracked batch creation)
- `POST /{id}/adjust` - manual stock adjustment with reason and audit record
- `GET /alerts` - consumables below minimum stock level

### Task 2: Bootstrapper + Migration

**Program.cs updates:**
- Added `app.MapDispensingApiEndpoints()` call
- Added `app.MapConsumablesApiEndpoints()` call
- Note: `AddPharmacyInfrastructure()` (which registers ConsumableCatalogSeeder) and `MapPharmacyApiEndpoints()` were already present from prior plans

**GetPendingPrescriptions in Clinical.Application:**
- Already implemented in Plan 17 (GetPendingPrescriptionsHandler returning ClinicalPendingPrescriptionDto)
- No changes needed

**PharmacyInventory EF Migration (20260306084141):**
- Creates: DispensingRecords, DispensingLines, BatchDeductions (dispensing hierarchy)
- Creates: OtcSales, OtcSaleLines (OTC sale hierarchy)
- Creates: ConsumableItems, ConsumableBatches (consumables warehouse)
- Creates: StockAdjustments (shared audit table for drug + consumable batch adjustments)
- Indexes: FEFO composite on ConsumableBatches (ConsumableItemId, ExpiryDate), names on ConsumableItems

## Deviations from Plan

None - plan executed exactly as written. The two pre-existing items noted in the plan (GetPendingPrescriptions handler, ConsumableCatalogSeeder registration) were both confirmed present and correct from Plans 16-17.

## Self-Check: PASSED

- FOUND: DispensingApiEndpoints.cs
- FOUND: ConsumablesApiEndpoints.cs
- FOUND: PharmacyInventory.cs migration
- FOUND: commit 2faba08 (task 1 - dispensing and consumables endpoints)
- FOUND: commit bf1b2bf (task 2 - bootstrapper wiring and migration)

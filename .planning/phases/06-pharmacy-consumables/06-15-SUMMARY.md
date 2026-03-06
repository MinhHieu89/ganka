---
phase: 06-pharmacy-consumables
plan: 15
subsystem: pharmacy-backend
tags: [alerts, consumables, tdd, handlers, pharmacy]
dependency_graph:
  requires: [06-08, 06-09, 06-10]
  provides: [expiry-alert-handler, low-stock-alert-handler, consumable-create-handler, consumable-update-handler]
  affects: [pharmacy-api-endpoints, consumables-api-endpoints]
tech_stack:
  added: []
  patterns: [wolverine-static-handler, fluent-validation, repository-unit-of-work, tdd-red-green]
key_files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Alerts/GetExpiryAlerts.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Alerts/GetLowStockAlerts.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/CreateConsumableItem.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/UpdateConsumableItem.cs
    - backend/tests/Pharmacy.Unit.Tests/Features/AlertAndConsumableHandlerTests.cs
  modified: []
decisions:
  - "[06-15]: GetExpiryAlerts handler delegates threshold directly to repository — no application-layer date filtering needed"
  - "[06-15]: GetLowStockAlerts excludes zero-MinStockLevel drugs at repository layer — handler is a pure delegation passthrough"
  - "[06-15]: CreateConsumableItem uses int TrackingMode (not enum type) matching Contracts DTO normalization pattern from DrugCatalogItemDto"
  - "[06-15]: UpdateConsumableItem returns Result (not Result<Guid>) matching UpdateSupplierHandler pattern — ID already known by caller"
metrics:
  duration: 2min
  completed_date: "2026-03-06"
  tasks_completed: 1
  files_changed: 5
---

# Phase 06 Plan 15: Alert Handlers and Consumable CRUD Summary

Alert query handlers (expiry + low stock) and consumable item CRUD handlers via TDD with 9 tests covering configurable thresholds, validation, and not-found error paths.

## What Was Built

Four Wolverine static handlers in the Pharmacy.Application module:

1. **GetExpiryAlerts** (`Features/Alerts/GetExpiryAlerts.cs`) — `GetExpiryAlertsQuery(int DaysThreshold = 90)` delegates to `IDrugBatchRepository.GetExpiryAlertsAsync(daysThreshold)`. Supports configurable thresholds of 30, 60, or 90 days per PHR-03.

2. **GetLowStockAlerts** (`Features/Alerts/GetLowStockAlerts.cs`) — `GetLowStockAlertsQuery` delegates to `IDrugBatchRepository.GetLowStockAlertsAsync()`. Returns drugs where totalStock < minStockLevel, excluding zero-MinStockLevel drugs (filtered in repository per PHR-04).

3. **CreateConsumableItem** (`Features/Consumables/CreateConsumableItem.cs`) — `CreateConsumableItemCommand(Name, NameVi, Unit, TrackingMode, MinStockLevel)` with FluentValidation (Name/NameVi max 200, Unit max 50, TrackingMode enum check, MinStockLevel >= 0). Creates `ConsumableItem` via factory method, saves via IConsumableRepository + IUnitOfWork.

4. **UpdateConsumableItem** (`Features/Consumables/UpdateConsumableItem.cs`) — `UpdateConsumableItemCommand(Id, Name, NameVi, Unit, TrackingMode, MinStockLevel)` loads item by ID, returns `Error.NotFound` if missing, calls `item.Update()`, saves.

## Test Results

9 tests written and passing (TDD red-green):
- `GetExpiryAlerts_ReturnsNearExpiryBatches` - PASS
- `GetExpiryAlerts_ExcludesExpiredAndEmpty` - PASS
- `GetExpiryAlerts_ConfigurableThreshold` - PASS (30d returns 1, 60d returns 2)
- `GetLowStockAlerts_ReturnsBelowMinimum` - PASS
- `GetLowStockAlerts_ExcludesZeroMinLevel` - PASS
- `CreateConsumableItem_Valid_CreatesItem` - PASS
- `CreateConsumableItem_EmptyName_ValidationError` - PASS
- `UpdateConsumableItem_Existing_UpdatesFields` - PASS
- `UpdateConsumableItem_NotFound_ReturnsError` - PASS

## Deviations from Plan

None — plan executed exactly as written.

## Commits

- `768b732`: `test(06-15): add failing tests for alert and consumable handlers` (RED phase)
- `c31e85d`: `feat(06-15): implement alert and consumable CRUD handlers` (GREEN phase)

## Self-Check: PASSED

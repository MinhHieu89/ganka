---
phase: 06-pharmacy-consumables
plan: "09"
subsystem: pharmacy-infrastructure
tags: [repository, seeder, ioc, consumables, inventory]
dependency_graph:
  requires: [06-05b, 06-06, 06-07, 06-08]
  provides: [ConsumableRepository, ConsumableCatalogSeeder, IoC-complete, DrugInventoryQuery]
  affects: [Pharmacy.Infrastructure, Pharmacy.Application]
tech_stack:
  added: []
  patterns: [IHostedService-seeder, two-step-aggregation-query, repository-per-aggregate]
key_files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/ConsumableRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Seeding/ConsumableCatalogSeeder.cs
  modified:
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/IoC.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DrugCatalogItemRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IDrugCatalogItemRepository.cs
decisions:
  - "ConsumableRepository.GetAlertsAsync uses two-step query: load all active items with threshold, then batch-aggregate ExpiryTracked stock separately to avoid N+1 and EF Core GroupBy translation issues"
  - "ConsumableCatalogSeeder seeds 12 items (MinStockLevel=10, CurrentStock=0) following DrugCatalogSeeder IHostedService pattern"
  - "DrugCatalogItemRepository.GetAllWithInventoryAsync uses two-step aggregation (catalog load + batch GroupBy) consistent with DrugBatchRepository.GetLowStockAlertsAsync pattern"
  - "IoC.cs registers IConsumableRepository and ConsumableCatalogSeeder alongside existing drug repositories"
  - "IDrugCatalogItemRepository interface extended with GetAllWithInventoryAsync returning DrugInventoryDto (existing record in Contracts)"
metrics:
  duration: 5min
  completed_date: "2026-03-06T07:29:19Z"
  tasks_completed: 2
  files_changed: 5
---

# Phase 06 Plan 09: Consumable Repository, Seeder, and IoC Summary

**One-liner:** ConsumableRepository with dual-mode alert queries, 12-item IPL/LLLT seeder, complete IoC registration, and DrugInventoryDto inventory query on DrugCatalogItemRepository.

## Tasks Completed

| Task | Description | Commit | Status |
|------|-------------|--------|--------|
| 1 | ConsumableRepository + ConsumableCatalogSeeder | 7ed2f55 | Done |
| 2 | IoC update + DrugCatalogItemRepository inventory query | 67d858d | Done |

## What Was Built

### ConsumableRepository (new)

Implements `IConsumableRepository` with:
- `GetAllActiveAsync`: WHERE IsActive, ordered by Name
- `GetBatchesAsync`: WHERE ConsumableItemId, ordered by ExpiryDate ASC
- `GetBatchByIdAsync`: FirstOrDefault by batch ID
- `GetAlertsAsync`: Dual-mode alert query
  - SimpleStock items: CurrentStock < MinStockLevel
  - ExpiryTracked items: sum of ConsumableBatch.CurrentQuantity < MinStockLevel (single batch aggregation query for all ExpiryTracked items)
- `Add`, `Update`, `AddBatch`, `AddStockAdjustment`: change tracker operations

### ConsumableCatalogSeeder (new)

IHostedService seeding 12 core IPL/LLLT/treatment supplies with Vietnamese diacritics:

**SimpleStock items:**
1. IPL Gel / Gel IPL (Tuýp)
2. Eye Shields / Kính chắn mắt (Cái)
3. LLLT Disposable Tips / Đầu LLLT dùng một lần (Cái)
4. Lid Care Pads / Miếng vệ sinh mi mắt (Cái)
5. Sterile Wipes / Khăn lau vô trùng (Cái)
6. Cotton Applicators / Que bông tăm (Túi)
7. Disposable Gloves / Găng tay dùng một lần (Hộp)
8. MGD Expression Forceps Tips / Đầu kẹp nặn tuyến Meibomian (Cái)
9. Thermal Pulsation Eyecups / Cốc mắt nhiệt xung (Cái)

**ExpiryTracked items:**
10. Anesthetic Eye Drops / Thuốc nhỏ tê (Chai)
11. Fluorescein Strips / Giấy thử fluorescein (Hộp)
12. Saline Solution / Nước muối sinh lý (Chai)

All items: MinStockLevel=10, CurrentStock=0 (clinic imports actual stock after seeding).

### IoC.cs (updated)

Added two registrations:
- `services.AddScoped<IConsumableRepository, ConsumableRepository>()`
- `services.AddHostedService<ConsumableCatalogSeeder>()`

### DrugCatalogItemRepository (updated)

Added `GetAllWithInventoryAsync(int expiryAlertDays, CancellationToken ct)` returning `List<DrugInventoryDto>`:
- Two-step query: load catalog items, then group batch data by drug in separate query
- Computes TotalStock (non-expired, positive-quantity batches only), BatchCount, IsLowStock, HasExpiryAlert
- Consistent with `DrugBatchRepository.GetLowStockAlertsAsync` two-step pattern

### IDrugCatalogItemRepository (updated)

Added `GetAllWithInventoryAsync` method signature to the interface.

## Verification

`dotnet build Pharmacy.Infrastructure.csproj` -- succeeded (0 warnings, 0 errors)
`dotnet build Pharmacy.Application.csproj` -- succeeded (0 warnings, 0 errors)

## Deviations from Plan

### Auto-added Missing Interface Method

**1. [Rule 2 - Missing Critical Functionality] Added GetAllWithInventoryAsync to IDrugCatalogItemRepository interface**
- **Found during:** Task 2
- **Issue:** Plan called for adding `GetAllWithInventoryAsync` to the repository implementation but the interface didn't have the method signature — implementing without the interface contract would fail at DI registration
- **Fix:** Added method to both `IDrugCatalogItemRepository.cs` interface and `DrugCatalogItemRepository.cs` implementation
- **Files modified:** `Pharmacy.Application/Interfaces/IDrugCatalogItemRepository.cs`
- **Commit:** 67d858d

## Self-Check: PASSED

Files verified:
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/ConsumableRepository.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Seeding/ConsumableCatalogSeeder.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/IoC.cs (updated)
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DrugCatalogItemRepository.cs (updated)
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IDrugCatalogItemRepository.cs (updated)

Commits verified:
- 7ed2f55: feat(06-09): implement ConsumableRepository and ConsumableCatalogSeeder
- 67d858d: feat(06-09): update IoC and DrugCatalogItemRepository with inventory queries

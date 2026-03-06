---
phase: 06-pharmacy-consumables
plan: 08
subsystem: database
tags: [ef-core, repository, fefo, pharmacy, csharp, dotnet]

requires:
  - phase: 06-07
    provides: "Repository interfaces: ISupplierRepository, IDrugBatchRepository, IStockImportRepository, IDispensingRepository, IOtcSaleRepository"
  - phase: 06-05a
    provides: "DrugBatch, Supplier, StockImport domain entities"
  - phase: 06-05b
    provides: "DispensingRecord, OtcSale, ConsumableItem domain entities"
  - phase: 06-06
    provides: "PharmacyDbContext with all DbSets"
provides:
  - "SupplierRepository: CRUD with SupplierDrugPrice join queries for DrugName"
  - "DrugBatchRepository: FEFO ordering, GetAvailableBatchesFEFOAsync, expiry alerts, low stock alerts with zero-stock inclusion"
  - "StockImportRepository: paginated list with eager-loaded Lines"
  - "DispensingRepository: Include Lines ThenInclude BatchDeductions, GetByPrescriptionIdAsync for duplicate-dispensing prevention"
  - "OtcSaleRepository: paginated list with eager-loaded Lines ordered by SoldAt DESC"
  - "All 5 repositories registered in IoC.cs"
affects:
  - "06-09 (handler implementations will inject these repositories)"

tech-stack:
  added: []
  patterns:
    - "Primary constructor injection: public sealed class XxxRepository(PharmacyDbContext context) : IXxxRepository"
    - "Vietnam timezone DateOnly: cross-platform SE Asia Standard Time / Asia/Ho_Chi_Minh via OperatingSystem.IsWindows()"
    - "FEFO query: WHERE CurrentQuantity > 0 AND ExpiryDate > today ORDER BY ExpiryDate ASC"
    - "Low stock alert with zero-stock inclusion: query active drugs with MinStockLevel > 0, join with stock lookup dictionary"
    - "Eager loading for aggregate navigation: Include + ThenInclude for Lines and BatchDeductions"

key-files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/SupplierRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DrugBatchRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/StockImportRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DispensingRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/OtcSaleRepository.cs
  modified:
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/IoC.cs

key-decisions:
  - "Vietnam timezone via OperatingSystem.IsWindows() for DateOnly FEFO comparisons (SE Asia Standard Time on Windows, Asia/Ho_Chi_Minh on Linux) -- matches project cross-platform pattern"
  - "Low stock alert includes zero-stock drugs (no batches at all) by querying all active drugs with MinStockLevel > 0, not just those with batches"
  - "GetLowStockAlertsAsync uses two queries (batch totals + catalog) instead of single JOIN to avoid EF Core GroupBy translation issues with Sum"
  - "SupplierRepository.GetSupplierDrugPricesAsync sets SupplierName to empty string in DTO -- caller already has supplier context, join not needed"

requirements-completed: [PHR-01, PHR-03, PHR-04, PHR-05]

duration: 6min
completed: 2026-03-06
---

# Phase 06 Plan 08: Repository Implementations Summary

**5 EF Core repository implementations for pharmacy inventory: FEFO batch queries with Vietnam timezone, expiry/low-stock alerts, eager-loaded dispensing history with BatchDeductions**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-06T07:14:41Z
- **Completed:** 2026-03-06T07:21:17Z
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments
- DrugBatchRepository implements FEFO ordering (ExpiryDate ASC), Vietnam timezone DateOnly comparison, expiry alerts, and low-stock alerts that include zero-stock drugs
- DispensingRepository with GetByPrescriptionIdAsync for duplicate-dispensing prevention and Include/ThenInclude for full aggregate eager loading
- All 5 new repositories registered in IoC.cs alongside existing DrugCatalogItemRepository

## Task Commits

Each task was committed atomically:

1. **Task 1: Implement SupplierRepository, DrugBatchRepository, StockImportRepository** - `d47e6c6` (feat)
2. **Task 2: Implement DispensingRepository and OtcSaleRepository** - `5f6490a` (feat, includes IoC.cs update)

## Files Created/Modified
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/SupplierRepository.cs` - CRUD with SupplierDrugPrice join for DrugName
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DrugBatchRepository.cs` - FEFO queries, expiry alerts, low stock alerts
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/StockImportRepository.cs` - Paginated list with Include Lines
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DispensingRepository.cs` - Include Lines ThenInclude BatchDeductions, prescription ID lookup
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/OtcSaleRepository.cs` - Paginated list with Include Lines
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/IoC.cs` - Registered all 5 new repositories

## Decisions Made
- Used Vietnam timezone (SE Asia Standard Time / Asia/Ho_Chi_Minh) for DateOnly comparison in FEFO queries, consistent with project cross-platform timezone pattern established in Phase 02
- Low stock alert query uses two-step approach (batch aggregation + catalog join as in-memory dictionary lookup) to avoid EF Core GroupBy translation limitations with Sum; includes drugs with zero stock (no batches) by querying all active drugs with MinStockLevel > 0
- SupplierRepository.GetSupplierDrugPricesAsync omits SupplierName from DTO (empty string) since callers always have supplier context from parent queries

## Deviations from Plan

None - plan executed exactly as written. IoC.cs registration was added as part of Task 2 as a natural extension (repositories are useless without registration).

## Issues Encountered

None - all builds passed on first attempt with 0 warnings and 0 errors.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All 5 repository implementations ready for handler consumption in plan 09+
- DrugBatchRepository FEFO query is the critical path for dispensing handler (plan 09/10)
- IoC.cs fully updated — no registration work needed in future plans

---
*Phase: 06-pharmacy-consumables*
*Completed: 2026-03-06*

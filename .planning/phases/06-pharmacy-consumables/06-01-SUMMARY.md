---
phase: 06-pharmacy-consumables
plan: 01
subsystem: database
tags: [domain-entities, ddd, pharmacy, inventory, batch-tracking, fefo]

# Dependency graph
requires:
  - phase: 05-pharmacy-catalog
    provides: DrugCatalogItem AggregateRoot entity (Phase 5 plan 01)

provides:
  - Supplier AggregateRoot with name, contact info, phone, email and lifecycle methods
  - DrugBatch Entity with FEFO support via ExpiryDate, Deduct/AddStock domain methods
  - SupplierDrugPrice junction Entity for default purchase pricing per supplier-drug pair
  - ImportSource enum (SupplierInvoice, ExcelBulk)
  - DrugCatalogItem extended with SellingPrice and MinStockLevel for inventory management

affects:
  - 06-02 (stock import infrastructure - uses Supplier and DrugBatch)
  - 06-03 (dispensing - uses DrugBatch for FEFO deduction)
  - 06-04 (OTC sales - uses DrugBatch for stock deduction)
  - 06-05 (consumables - similar batch/pricing model)
  - All pharmacy database migrations (entities map to pharmacy schema tables)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "DrugBatch.Deduct() throws InvalidOperationException when qty > CurrentQuantity (domain guard)"
    - "IsExpired and IsNearExpiry(int daysThreshold) as computed properties on DrugBatch for FEFO sorting"
    - "RowVersion byte[] on DrugBatch for optimistic concurrency during concurrent dispensing"
    - "UpdatePricing() as separate domain method on DrugCatalogItem (pricing management separate from catalog management)"
    - "DateOnly used for ExpiryDate on DrugBatch (no time component needed for expiry)"

key-files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/Supplier.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/SupplierDrugPrice.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/DrugBatch.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Enums/ImportSource.cs
  modified:
    - backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/DrugCatalogItem.cs

key-decisions:
  - "DrugBatch.Deduct() throws InvalidOperationException (not Result<T>) -- domain invariant violation is exceptional, not a recoverable error"
  - "RowVersion on DrugBatch for optimistic concurrency -- concurrent dispensing of same batch must not double-deduct"
  - "DateOnly for ExpiryDate -- date-only precision is correct for pharmaceutical expiry (no time zone concerns)"
  - "SupplierDrugPrice as separate Entity (not Value Object) -- needs independent Id for EF Core relationship navigation"
  - "UpdatePricing() separate from Update() on DrugCatalogItem -- keeps catalog management and pricing as distinct bounded operations, backward compatible with Phase 5 handlers"

patterns-established:
  - "FEFO pattern: DrugBatch.IsExpired computed from DateOnly.FromDateTime(DateTime.UtcNow); IsNearExpiry(int daysThreshold) for configurable alert thresholds"
  - "Batch domain guards: Create() validates positive quantity and non-negative price before entity construction"
  - "Supplier lifecycle: IsActive soft-delete with Activate/Deactivate following DrugCatalogItem pattern"

requirements-completed: [PHR-01]

# Metrics
duration: 2min
completed: 2026-03-06
---

# Phase 06 Plan 01: Pharmacy Stock Domain Entities Summary

**Supplier, DrugBatch with FEFO/optimistic-concurrency support, SupplierDrugPrice junction, and DrugCatalogItem extended with SellingPrice and MinStockLevel -- all Pharmacy.Domain inventory entities established**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-06T06:38:37Z
- **Completed:** 2026-03-06T06:40:37Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments

- Created Supplier AggregateRoot with Create/Update/Activate/Deactivate factory methods and IAuditable interface
- Created DrugBatch Entity with FEFO batch tracking: Deduct/AddStock domain methods, IsExpired/IsNearExpiry computed properties, RowVersion for optimistic concurrency
- Created SupplierDrugPrice junction Entity linking Supplier to DrugCatalogItem with default purchase price and UpdatePrice method
- Created ImportSource enum (SupplierInvoice=0, ExcelBulk=1)
- Extended DrugCatalogItem with SellingPrice (decimal?) and MinStockLevel (int) properties plus UpdatePricing() domain method
- Full solution build passes with 0 errors

## Task Commits

Each task was committed atomically:

1. **Task 1: Create Supplier, SupplierDrugPrice, DrugBatch entities and ImportSource enum** - `d359265` (feat)
2. **Task 2: Extend DrugCatalogItem with SellingPrice and MinStockLevel** - `f2c4b84` (feat)

**Plan metadata:** (docs commit — see final commit)

## Files Created/Modified

- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/Supplier.cs` - Supplier AggregateRoot with branch isolation and lifecycle management
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/SupplierDrugPrice.cs` - Junction entity for supplier-drug default pricing
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/DrugBatch.cs` - Batch entity with FEFO support, Deduct/AddStock domain methods, optimistic concurrency
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Enums/ImportSource.cs` - Import source enum for stock import audit trail
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/DrugCatalogItem.cs` - Extended with SellingPrice, MinStockLevel, UpdatePricing()

## Decisions Made

- DrugBatch.Deduct() throws InvalidOperationException (not Result<T>) -- domain invariant violation is exceptional; application layer catches and converts to Result at service boundary
- RowVersion byte[] on DrugBatch for optimistic concurrency -- concurrent dispensing of same batch must not double-deduct stock
- DateOnly for ExpiryDate -- date-only precision is correct for pharmaceutical expiry, avoids time zone complications
- SupplierDrugPrice created as Entity (not Value Object) -- needs EF Core-navigable Id for the junction table relationship
- UpdatePricing() kept separate from Update() on DrugCatalogItem -- maintains backward compatibility with all Phase 5 handlers that call Create/Update

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required. Database migrations for these entities will be created in a subsequent plan.

## Next Phase Readiness

- All pharmacy domain inventory entities are in place for Plan 02 (infrastructure: EF Core config, migrations, repositories)
- DrugBatch is ready for FEFO dispensing logic in Plan 03
- SupplierDrugPrice is ready for price auto-fill during stock import in Plan 02
- DrugCatalogItem SellingPrice/MinStockLevel ready for low-stock alert queries

---
*Phase: 06-pharmacy-consumables*
*Completed: 2026-03-06*

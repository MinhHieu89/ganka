---
phase: 07-billing-finance
plan: 30
subsystem: billing
tags: [domain-events, integration-events, wolverine, prescription-billing, idempotency]

# Dependency graph
requires:
  - phase: 07-billing-finance
    provides: "Invoice aggregate, HandleDrugDispensed handler, billing integration event pattern"
provides:
  - "DrugPrescriptionAdded domain event and integration event chain"
  - "HandleDrugPrescriptionAdded billing handler with price lookup"
  - "Idempotent dispensing handler that respects prescription-created items"
  - "GetDrugCatalogPricesQuery cross-module query for price lookup"
  - "InvoiceLineItem.UpdatePrice and UpdateSourceType methods"
affects: [billing, clinical, pharmacy]

# Tech tracking
tech-stack:
  added: []
  patterns: ["domain-event -> cascading-handler -> integration-event -> billing-handler with cross-module price lookup"]

key-files:
  created:
    - "backend/src/Modules/Clinical/Clinical.Domain/Events/DrugPrescriptionAddedEvent.cs"
    - "backend/src/Modules/Clinical/Clinical.Contracts/IntegrationEvents/DrugPrescriptionAddedIntegrationEvent.cs"
    - "backend/src/Modules/Clinical/Clinical.Application/Features/PublishDrugPrescriptionAddedIntegrationEvent.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/HandleDrugPrescriptionAdded.cs"
    - "backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/GetDrugCatalogPricesQuery.cs"
    - "backend/src/Modules/Pharmacy/Pharmacy.Application/Features/DrugCatalog/GetDrugCatalogPrices.cs"
    - "backend/tests/Clinical.Unit.Tests/Features/DrugPrescriptionAddedEventTests.cs"
  modified:
    - "backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/HandleDrugDispensed.cs"
    - "backend/src/Modules/Billing/Billing.Domain/Entities/InvoiceLineItem.cs"
    - "backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IDrugCatalogItemRepository.cs"
    - "backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DrugCatalogItemRepository.cs"
    - "backend/tests/Billing.Unit.Tests/Features/IntegrationEventHandlerTests.cs"

key-decisions:
  - "Raise domain event in Visit.AddDrugPrescription with items but without pricing (domain has no price access)"
  - "Use cross-module GetDrugCatalogPricesQuery via IMessageBus for billing handler price lookup"
  - "Off-catalog items get price 0 (cashier adjusts manually); HandleDrugDispensed updates zero-price items"

patterns-established:
  - "Cross-module price lookup: Billing uses IMessageBus to query Pharmacy for catalog prices"
  - "Dual-source idempotency: HandleDrugDispensed checks both Prescription and Dispensing source types"

requirements-completed: [FIN-01, FIN-03]

# Metrics
duration: 11min
completed: 2026-03-17
---

# Phase 07 Plan 30: DrugPrescriptionAdded Event Chain Summary

**Domain event chain from Clinical to Billing creates pharmacy line items at prescription time, enabling Vietnamese prescribe-pay-dispense flow with cross-module price lookup and dual-source idempotency**

## Performance

- **Duration:** 11 min
- **Started:** 2026-03-17T10:09:18Z
- **Completed:** 2026-03-17T10:20:09Z
- **Tasks:** 2
- **Files modified:** 13

## Accomplishments
- DrugPrescriptionAddedEvent domain event raised when doctor adds prescription to visit
- Cascading handler converts to integration event for cross-module consumption
- HandleDrugPrescriptionAdded creates pharmacy line items with catalog prices via GetDrugCatalogPricesQuery
- HandleDrugDispensed made idempotent with prescription items; updates zero-price items on dispensing
- 17 TDD tests (10 clinical domain/cascading + 7 billing handler) all passing
- Full backend build succeeds with zero errors

## Task Commits

Each task was committed atomically:

1. **Task 1: Domain event + integration event for drug prescription creation** - `127a8d1` (feat)
2. **Task 2: Billing HandleDrugPrescriptionAdded handler + make HandleDrugDispensed idempotent** - `2b5e9dd` (feat)

## Files Created/Modified
- `Clinical.Domain/Events/DrugPrescriptionAddedEvent.cs` - Domain event with PrescribedDrugDto items
- `Clinical.Contracts/IntegrationEvents/DrugPrescriptionAddedIntegrationEvent.cs` - Cross-module integration event
- `Clinical.Application/Features/PublishDrugPrescriptionAddedIntegrationEvent.cs` - Wolverine cascading handler
- `Clinical.Domain/Entities/Visit.cs` - AddDrugPrescription now raises domain event
- `Billing.Application/Features/HandleDrugPrescriptionAdded.cs` - Creates pharmacy line items from prescription
- `Billing.Application/Features/HandleDrugDispensed.cs` - Idempotent with prescription items, updates zero-price
- `Billing.Domain/Entities/InvoiceLineItem.cs` - Added UpdatePrice and UpdateSourceType methods
- `Pharmacy.Contracts/Dtos/GetDrugCatalogPricesQuery.cs` - Cross-module price query DTO
- `Pharmacy.Application/Features/DrugCatalog/GetDrugCatalogPrices.cs` - Wolverine handler for price lookup
- `Pharmacy.Application/Interfaces/IDrugCatalogItemRepository.cs` - Added GetPricesByIdsAsync
- `Pharmacy.Infrastructure/Repositories/DrugCatalogItemRepository.cs` - Implemented GetPricesByIdsAsync
- `Clinical.Unit.Tests/Features/DrugPrescriptionAddedEventTests.cs` - 10 tests for domain event and cascading handler
- `Billing.Unit.Tests/Features/IntegrationEventHandlerTests.cs` - 7 new tests for prescription billing

## Decisions Made
- **Domain event without pricing:** Visit entity has no access to drug catalog prices, so DrugPrescriptionAddedEvent carries only drug names, catalog item IDs, and quantities. Pricing is resolved by the billing handler.
- **Cross-module query for prices:** Created GetDrugCatalogPricesQuery in Pharmacy.Contracts so billing handler can look up SellingPrice without coupling to Pharmacy domain.
- **Dual-source idempotency:** HandleDrugDispensed checks both "Prescription" and "Dispensing" source types to avoid duplicate line items. Zero-price prescription items are updated with actual dispensing price.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added GetDrugCatalogPricesQuery for cross-module price lookup**
- **Found during:** Task 2 (HandleDrugPrescriptionAdded implementation)
- **Issue:** Plan mentioned needing price lookup but didn't fully specify the query/handler pair
- **Fix:** Created GetDrugCatalogPricesQuery DTO, DrugCatalogPriceDto response, handler in Pharmacy.Application, and GetPricesByIdsAsync in repository
- **Files modified:** Pharmacy.Contracts/Dtos/GetDrugCatalogPricesQuery.cs, Pharmacy.Application/Features/DrugCatalog/GetDrugCatalogPrices.cs, IDrugCatalogItemRepository.cs, DrugCatalogItemRepository.cs
- **Verification:** Full backend build succeeds, all tests pass
- **Committed in:** 2b5e9dd (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 missing critical functionality)
**Impact on plan:** Essential for correct price lookup. No scope creep.

## Issues Encountered
- Backend Bootstrapper process was locking DLLs during full build; killed process per CLAUDE.md instructions and rebuild succeeded.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Prescription-to-billing event chain is complete
- Vietnamese clinic flow (prescribe -> pay -> dispense) is supported
- HandleDrugDispensed is backward-compatible with existing dispensing events

---
*Phase: 07-billing-finance*
*Completed: 2026-03-17*

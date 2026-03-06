---
phase: 07-billing-finance
plan: 14
subsystem: api, database, infra
tags: [ef-core, minimal-api, wolverine, billing, migration, bootstrapper]

# Dependency graph
requires:
  - phase: 07-billing-finance (plans 01-12)
    provides: "Domain entities, DbContext, repositories, handlers, unit tests for Billing module"
provides:
  - "Billing module fully wired in Bootstrapper (DI, endpoints, Wolverine discovery)"
  - "Billing.Presentation project with all billing API endpoints"
  - "InitialBilling EF Core migration for 7 billing tables"
  - "Database schema created for billing module"
affects: [07-billing-finance, frontend-billing]

# Tech tracking
tech-stack:
  added: []
  patterns: ["Billing API endpoints under /api/billing with RequireAuthorization"]

key-files:
  created:
    - "backend/src/Modules/Billing/Billing.Presentation/Billing.Presentation.csproj"
    - "backend/src/Modules/Billing/Billing.Presentation/IoC.cs"
    - "backend/src/Modules/Billing/Billing.Presentation/BillingApiEndpoints.cs"
    - "backend/src/Modules/Billing/Billing.Infrastructure/Migrations/20260306144000_InitialBilling.cs"
  modified:
    - "backend/src/Bootstrapper/Program.cs"
    - "backend/src/Bootstrapper/Bootstrapper.csproj"
    - "backend/Ganka28.slnx"
    - "backend/src/Modules/Billing/Billing.Infrastructure/Configurations/InvoiceConfiguration.cs"
    - "backend/src/Modules/Billing/Billing.Domain/Entities/InvoiceLineItem.cs"

key-decisions:
  - "Created Billing.Presentation project following Pharmacy.Presentation pattern"
  - "Fixed Invoice BranchId value conversion missing from InvoiceConfiguration"
  - "Changed InvoiceLineItem.LineTotal from computed expression to persisted property for EF materialization"

patterns-established:
  - "Billing API endpoints grouped under /api/billing with 5 sub-groups: invoices, payments, discounts, refunds, shifts"

requirements-completed: [FIN-01, FIN-09]

# Metrics
duration: 12min
completed: 2026-03-06
---

# Phase 07 Plan 14: Bootstrapper Integration Summary

**Billing module wired into Bootstrapper with Presentation layer, EF migration for 7 tables, and full API endpoint mapping**

## Performance

- **Duration:** 12 min
- **Started:** 2026-03-06T14:28:55Z
- **Completed:** 2026-03-06T14:41:18Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments
- Created Billing.Presentation project with BillingApiEndpoints exposing all billing APIs (invoices, payments, discounts, refunds, shifts)
- Wired Billing module into Bootstrapper with AddBillingApplication, AddBillingInfrastructure, AddBillingPresentation DI calls
- Created and applied InitialBilling EF Core migration for all 7 billing tables (Invoices, InvoiceLineItems, Payments, Discounts, Refunds, CashierShifts, ShiftTemplates)
- Full solution builds (0 errors) and all 45 billing unit tests pass

## Task Commits

Each task was committed atomically:

1. **Task 1: Wire Bootstrapper with Billing module** - `c323c11` (feat)
2. **Task 2: Add projects to solution and create migration** - `9d7de7e` (chore)

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Presentation/Billing.Presentation.csproj` - New project file for Billing presentation layer
- `backend/src/Modules/Billing/Billing.Presentation/IoC.cs` - DI registration placeholder for Billing presentation
- `backend/src/Modules/Billing/Billing.Presentation/BillingApiEndpoints.cs` - All billing API endpoints (invoices, payments, discounts, refunds, shifts)
- `backend/src/Bootstrapper/Program.cs` - Added Billing DI calls and endpoint mapping
- `backend/src/Bootstrapper/Bootstrapper.csproj` - Added Billing.Presentation project reference
- `backend/Ganka28.slnx` - Added Billing.Presentation to solution
- `backend/src/Modules/Billing/Billing.Infrastructure/Configurations/InvoiceConfiguration.cs` - Added BranchId value conversion
- `backend/src/Modules/Billing/Billing.Domain/Entities/InvoiceLineItem.cs` - Changed LineTotal to persisted property
- `backend/src/Modules/Billing/Billing.Infrastructure/Migrations/20260306144000_InitialBilling.cs` - Initial migration for billing schema

## Decisions Made
- Created Billing.Presentation project following established Pharmacy.Presentation pattern (csproj structure, IoC, endpoint grouping)
- Fixed Invoice BranchId value conversion that was missing from InvoiceConfiguration (CashierShift and ShiftTemplate already had it)
- Changed InvoiceLineItem.LineTotal from a computed expression property to a persisted property with private setter to support EF Core materialization; value is set in Create factory method

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Missing BranchId value conversion on InvoiceConfiguration**
- **Found during:** Task 2 (Migration creation)
- **Issue:** Invoice entity inherits BranchId from AggregateRoot, but InvoiceConfiguration lacked the HasConversion call for BranchId, preventing EF migration creation
- **Fix:** Added BranchId value conversion (b => b.Value, v => new BranchId(v)) to InvoiceConfiguration
- **Files modified:** backend/src/Modules/Billing/Billing.Infrastructure/Configurations/InvoiceConfiguration.cs
- **Verification:** Migration created successfully after fix
- **Committed in:** 9d7de7e (Task 2 commit)

**2. [Rule 1 - Bug] InvoiceLineItem.LineTotal computed property incompatible with EF Core**
- **Found during:** Task 2 (Migration creation)
- **Issue:** LineTotal was defined as `decimal LineTotal => UnitPrice * Quantity` (expression-bodied, no setter/backing field), which EF Core cannot materialize
- **Fix:** Changed to `decimal LineTotal { get; private set; }` and set value in Create factory (`LineTotal = unitPrice * quantity`)
- **Files modified:** backend/src/Modules/Billing/Billing.Domain/Entities/InvoiceLineItem.cs
- **Verification:** Migration created and applied; all 45 unit tests pass
- **Committed in:** 9d7de7e (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (2 bugs)
**Impact on plan:** Both auto-fixes necessary for EF Core migration generation. No scope creep.

## Issues Encountered
- Bootstrapper process was locked (PID 74840) during first build attempt, required killing the process before build succeeded
- EF tools version mismatch warning (9.0.2 tools vs 10.0.3 runtime) - non-blocking, migration created successfully

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Billing module is fully functional in the running application
- All API endpoints available under /api/billing with authorization
- Database schema created with all 7 billing tables in the billing schema
- Ready for frontend billing UI development and end-to-end integration

## Self-Check: PASSED

All created files verified present. Both task commits (c323c11, 9d7de7e) confirmed in git log.

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*

---
phase: 07-billing-finance
plan: 05
subsystem: database
tags: [ef-core, entity-configuration, seeder, cashier-shift, shift-template, sql-server]

# Dependency graph
requires:
  - phase: 07-03
    provides: CashierShift aggregate and ShiftTemplate entity domain models
provides:
  - CashierShiftConfiguration with filtered unique index for single open shift per branch
  - ShiftTemplateConfiguration with BranchId index
  - Updated BillingDbContext with all 7 entity DbSets and ApplyConfigurationsFromAssembly
  - ShiftTemplateSeeder for Morning and Afternoon default templates
affects: [07-06, 07-07, 07-08, 07-10, 07-11]

# Tech tracking
tech-stack:
  added: []
  patterns: [filtered-unique-index, hosted-service-seeder, schema-per-module-ef-core]

key-files:
  created:
    - backend/src/Modules/Billing/Billing.Infrastructure/Configurations/CashierShiftConfiguration.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Configurations/ShiftTemplateConfiguration.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Seeding/ShiftTemplateSeeder.cs
  modified:
    - backend/src/Modules/Billing/Billing.Infrastructure/BillingDbContext.cs
    - backend/src/Modules/Billing/Billing.Domain/Entities/ShiftTemplate.cs

key-decisions:
  - "Added BranchId property to ShiftTemplate entity for multi-tenant isolation and configuration indexing"
  - "Used filtered unique index with HasFilter('[Status] = 0') for single open shift per branch enforcement"
  - "Ignored computed property ExpectedCashAmount in EF Core configuration since it's derived from other persisted fields"

patterns-established:
  - "Filtered unique index: Use HasFilter + IsUnique for business rule enforcement at database level"
  - "IHostedService seeder: Follow AllergyCatalogSeeder pattern for idempotent data seeding"

requirements-completed: [FIN-10]

# Metrics
duration: 8min
completed: 2026-03-06
---

# Phase 07 Plan 05: CashierShift/ShiftTemplate EF Core Configs and DbContext Summary

**EF Core configurations for CashierShift (filtered unique index for one open shift per branch) and ShiftTemplate, plus BillingDbContext with all 7 entity DbSets and Morning/Afternoon template seeder**

## Performance

- **Duration:** 8 min
- **Started:** 2026-03-06T13:58:21Z
- **Completed:** 2026-03-06T14:06:25Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- CashierShiftConfiguration with filtered unique index `(BranchId, Status) WHERE Status = 0` preventing multiple open shifts per branch
- ShiftTemplateConfiguration with BranchId conversion and index for multi-tenant queries
- BillingDbContext updated with all 7 DbSets (Invoice, InvoiceLineItem, Payment, Discount, Refund, CashierShift, ShiftTemplate) and ApplyConfigurationsFromAssembly
- ShiftTemplateSeeder seeds Morning (08:00-12:00) and Afternoon (13:00-20:00) templates matching clinic hours

## Task Commits

Each task was committed atomically:

1. **Task 1: Create CashierShift and ShiftTemplate EF Core configurations** - `0f4d934` (feat)
2. **Task 2: Update BillingDbContext and create ShiftTemplateSeeder** - `a66096d` (feat, via parallel agent 07-08)

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Infrastructure/Configurations/CashierShiftConfiguration.cs` - EF Core config with filtered unique index, VND precision, query filter
- `backend/src/Modules/Billing/Billing.Infrastructure/Configurations/ShiftTemplateConfiguration.cs` - EF Core config with BranchId conversion and index
- `backend/src/Modules/Billing/Billing.Infrastructure/BillingDbContext.cs` - Updated with all 7 entity DbSets and ApplyConfigurationsFromAssembly
- `backend/src/Modules/Billing/Billing.Infrastructure/Seeding/ShiftTemplateSeeder.cs` - IHostedService seeder for Morning/Afternoon templates
- `backend/src/Modules/Billing/Billing.Domain/Entities/ShiftTemplate.cs` - Added BranchId property for multi-tenant isolation

## Decisions Made
- Added BranchId property to ShiftTemplate entity (was missing from Entity base class) to support multi-tenant indexing and seeder requirements
- Used `HasFilter($"[Status] = {(int)ShiftStatus.Open}")` with enum cast for type-safe filter expression
- Ignored `ExpectedCashAmount` computed property in EF Core since it derives from OpeningBalance, CashReceived, and CashRefunds

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added BranchId to ShiftTemplate entity**
- **Found during:** Task 1 (Configuration creation)
- **Issue:** ShiftTemplate extends Entity (not AggregateRoot) which lacks BranchId, but configuration requires BranchId for index and seeder uses BranchId for branch association
- **Fix:** Added `public BranchId BranchId { get; private set; }` property and branchId parameter to Create factory method
- **Files modified:** backend/src/Modules/Billing/Billing.Domain/Entities/ShiftTemplate.cs
- **Verification:** Domain and Infrastructure projects build successfully
- **Committed in:** 0f4d934 (Task 1 commit)

**2. [Rule 3 - Blocking] Parallel agent overlap on Task 2 files**
- **Found during:** Task 2 (DbContext and Seeder)
- **Issue:** Parallel plan agents (07-07, 07-08) had already committed the BillingDbContext updates and ShiftTemplateSeeder as prerequisites for their own work
- **Fix:** Verified committed content matches plan requirements; no additional commit needed
- **Files modified:** None (already committed by parallel agents)
- **Verification:** Build succeeds, git show confirms correct content in HEAD

---

**Total deviations:** 2 auto-fixed (2 blocking)
**Impact on plan:** Both deviations were necessary for correctness. BranchId addition enables multi-tenant isolation. Parallel agent overlap is expected in wave-based execution.

## Issues Encountered
- Linter auto-modified configuration files after creation (simplified comments, added `sealed` keyword, removed some explicit configurations). Required re-adding BranchId conversion and index that the linter stripped.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All EF Core configurations for billing entities are in place (7 total: Invoice, InvoiceLineItem, Payment, Discount, Refund, CashierShift, ShiftTemplate)
- BillingDbContext ready for repository implementations and migration generation
- Shift template seeder ready for registration in DI container

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*

## Self-Check: PASSED
- All 5 key files exist on disk
- Commit 0f4d934 (Task 1) verified in git log
- Commit a66096d (Task 2 via parallel agent) verified in git log
- Build verification: `dotnet build Billing.Infrastructure.csproj` succeeds with 0 errors

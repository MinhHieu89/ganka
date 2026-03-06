---
phase: 07-billing-finance
plan: 28
subsystem: api, domain
tags: [invoice, finalize-guard, pending-invoices, wolverine, billing]

# Dependency graph
requires:
  - phase: 07-billing-finance
    provides: Invoice domain entity, IInvoiceRepository, BillingApiEndpoints
provides:
  - Invoice.Finalize() validation guards for empty invoices and zero total
  - GetPendingInvoicesHandler for cashier dashboard pending panel
  - IInvoiceRepository.GetPendingAsync for draft invoice queries
affects: [billing-frontend, cashier-dashboard]

# Tech tracking
tech-stack:
  added: []
  patterns: [domain-guard-validation, wolverine-query-handler]

key-files:
  created:
    - backend/src/Modules/Billing/Billing.Application/Features/GetPendingInvoices.cs
  modified:
    - backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs
    - backend/src/Modules/Billing/Billing.Application/Interfaces/IInvoiceRepository.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Repositories/InvoiceRepository.cs
    - backend/tests/Billing.Unit.Tests/Features/InvoiceCrudHandlerTests.cs
    - backend/tests/Billing.Unit.Tests/Features/DiscountHandlerTests.cs
    - backend/tests/Billing.Unit.Tests/Features/RefundHandlerTests.cs

key-decisions:
  - "Guard order: line items first, then total amount, then IsFullyPaid for clearest error messages"
  - "Reuse CreateInvoiceHandler.MapToDto for GetPendingInvoices to avoid DTO mapping duplication"

patterns-established:
  - "Domain guards in Finalize: validate business invariants before state transition"

requirements-completed: [FIN-01, FIN-02]

# Metrics
duration: 8min
completed: 2026-03-06
---

# Phase 07 Plan 28: Finalize Guards & GetPendingInvoices Summary

**Invoice.Finalize() validation guards for empty/zero-total invoices + GetPendingInvoicesHandler for cashier dashboard draft invoice list**

## Performance

- **Duration:** 8 min
- **Started:** 2026-03-06T16:30:08Z
- **Completed:** 2026-03-06T16:38:17Z
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments
- Invoice.Finalize() now rejects invoices with zero line items or zero/negative total amount
- Created GetPendingInvoicesHandler returning draft invoices mapped to InvoiceDtos
- Added GetPendingAsync to IInvoiceRepository and InvoiceRepository with optional cashierShiftId filter
- All 50 billing unit tests pass (5 new: 3 finalize guards + 2 pending invoices)
- Fixed pre-existing compilation errors in DiscountHandlerTests and RefundHandlerTests

## Task Commits

Each task was committed atomically:

1. **Task 1: Add finalize validation guards + create GetPendingInvoices handler** - `de58ffc` (feat)
2. **Task 2: Add unit tests for pending invoices handler** - `e76582b` (test)

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs` - Added line items count and total amount guards to Finalize()
- `backend/src/Modules/Billing/Billing.Application/Features/GetPendingInvoices.cs` - New Wolverine handler for GetPendingInvoicesQuery
- `backend/src/Modules/Billing/Billing.Application/Interfaces/IInvoiceRepository.cs` - Added GetPendingAsync method
- `backend/src/Modules/Billing/Billing.Infrastructure/Repositories/InvoiceRepository.cs` - Implemented GetPendingAsync with EF Core
- `backend/tests/Billing.Unit.Tests/Features/InvoiceCrudHandlerTests.cs` - 5 new tests for finalize guards and pending invoices
- `backend/tests/Billing.Unit.Tests/Features/DiscountHandlerTests.cs` - Fixed constructor args and added ICurrentUser
- `backend/tests/Billing.Unit.Tests/Features/RefundHandlerTests.cs` - Fixed constructor args and added ICurrentUser

## Decisions Made
- Guard order in Finalize(): line items check first (clearest error for the most common mistake), then total amount, then IsFullyPaid
- Reused CreateInvoiceHandler.MapToDto in GetPendingInvoicesHandler to avoid duplicating DTO mapping logic

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed pre-existing DiscountHandlerTests compilation errors**
- **Found during:** Task 1 (RED phase - test compilation)
- **Issue:** ApplyDiscountCommand constructor had 6 arguments instead of 5 (extra Guid.NewGuid()), and Handle calls were missing ICurrentUser parameter
- **Fix:** Corrected constructor calls to 5 args, added ICurrentUser mock field and parameter to Handle calls, added using Shared.Application
- **Files modified:** backend/tests/Billing.Unit.Tests/Features/DiscountHandlerTests.cs
- **Verification:** Full test project compiles, all discount tests pass
- **Committed in:** de58ffc (Task 1 commit)

**2. [Rule 3 - Blocking] Fixed pre-existing RefundHandlerTests compilation errors**
- **Found during:** Task 1 (RED phase - test compilation)
- **Issue:** RequestRefundCommand constructor had 5 arguments instead of 4 (extra Guid.NewGuid()), and Handle calls were missing ICurrentUser parameter
- **Fix:** Corrected constructor calls to 4 args, added _currentUser parameter to Handle calls
- **Files modified:** backend/tests/Billing.Unit.Tests/Features/RefundHandlerTests.cs
- **Verification:** Full test project compiles, all refund tests pass
- **Committed in:** de58ffc (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (2 blocking)
**Impact on plan:** Both fixes were necessary to unblock test compilation. No scope creep -- pre-existing test constructor mismatches from earlier plan implementations.

## Issues Encountered
None beyond the pre-existing test compilation errors documented above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Invoice finalize guards prevent data integrity issues from empty finalized invoices
- GET /api/billing/invoices/pending endpoint is now functional for the cashier dashboard
- All billing unit tests pass (50 total)

## Self-Check: PASSED

All files verified present. All commit hashes verified in git log.

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*

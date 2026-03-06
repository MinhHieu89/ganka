---
phase: 07-billing-finance
plan: 11
subsystem: billing
tags: [discount, refund, approval-workflow, manager-pin, wolverine, tdd]

# Dependency graph
requires:
  - phase: 07-07
    provides: Invoice domain model with line items and payment tracking
  - phase: 07-08
    provides: Billing domain entities (Discount, Refund, Payment)
provides:
  - ApplyDiscountCommand handler with percentage/fixed-amount support
  - ApproveDiscountCommand handler with manager PIN verification via IMessageBus
  - RequestRefundCommand handler with finalized invoice validation
  - VerifyManagerPinQuery/Response cross-module contract
  - Billing.Unit.Tests project with 9 discount/refund tests
affects: [07-26, 07-20]

# Tech tracking
tech-stack:
  added: []
  patterns: [cross-module-pin-verification, discount-approval-workflow, refund-request-pattern]

key-files:
  created:
    - backend/tests/Billing.Unit.Tests/Features/DiscountHandlerTests.cs
    - backend/tests/Billing.Unit.Tests/Features/RefundHandlerTests.cs
    - backend/src/Modules/Billing/Billing.Application/Features/ApplyDiscount.cs
    - backend/src/Modules/Billing/Billing.Application/Features/ApproveDiscount.cs
    - backend/src/Modules/Billing/Billing.Application/Features/RequestRefund.cs
  modified:
    - backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs
    - backend/Ganka28.slnx

key-decisions:
  - "Manager PIN verification via IMessageBus cross-module query (VerifyManagerPinQuery) keeps approval logic in Billing handler"
  - "Discount starts as Pending, requires explicit ApproveDiscount to activate and recalculate invoice totals"
  - "Added Invoice.RecalculateAfterDiscountApproval() and Invoice.AddRefund() domain methods to support handler workflows"

patterns-established:
  - "Cross-module PIN verification: handler sends VerifyManagerPinQuery via IMessageBus, receives VerifyManagerPinResponse"
  - "Discount approval workflow: Apply (Pending) -> Approve (with PIN) -> recalculate totals"
  - "Refund request pattern: only on Finalized invoices, amount <= TotalAmount, starts as Requested status"

requirements-completed: [FIN-07, FIN-08]

# Metrics
duration: 6min
completed: 2026-03-06
---

# Phase 07 Plan 11: Discount and Refund Handlers Summary

**TDD discount/refund handlers with manager PIN approval workflow, percentage/fixed-amount discounts, and refund request validation on finalized invoices**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-06T14:18:22Z
- **Completed:** 2026-03-06T14:24:11Z
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments
- Percentage and fixed-amount discounts both calculate correctly on invoice subtotal or specific line items
- Manager PIN approval gates discount application via IMessageBus cross-module query
- Refund requests validate finalized invoice status and amount constraints
- All 9 tests pass (6 discount + 3 refund) following strict TDD RED-GREEN cycle

## Task Commits

Each task was committed atomically:

1. **Task 1: Write failing discount and refund tests** - `dfc333c` (test)
2. **Task 2: Implement core discount and refund handlers** - `377d000` (feat)

## Files Created/Modified
- `backend/tests/Billing.Unit.Tests/Billing.Unit.Tests.csproj` - New test project for Billing module
- `backend/tests/Billing.Unit.Tests/Features/DiscountHandlerTests.cs` - 6 tests for ApplyDiscount and ApproveDiscount handlers
- `backend/tests/Billing.Unit.Tests/Features/RefundHandlerTests.cs` - 3 tests for RequestRefund handler
- `backend/src/Modules/Billing/Billing.Application/Features/ApplyDiscount.cs` - ApplyDiscountCommand, validator, and handler
- `backend/src/Modules/Billing/Billing.Application/Features/ApproveDiscount.cs` - ApproveDiscountCommand, VerifyManagerPinQuery/Response, and handler
- `backend/src/Modules/Billing/Billing.Application/Features/RequestRefund.cs` - RequestRefundCommand, validator, and handler
- `backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs` - Added RecalculateAfterDiscountApproval() and AddRefund() methods
- `backend/Ganka28.slnx` - Added Billing.Unit.Tests project to solution

## Decisions Made
- Manager PIN verification via IMessageBus cross-module query keeps approval logic in Billing handler without coupling to Auth internals
- Discount CalculateAmount uses Math.Round with 0 decimal places for VND (no fractional amounts)
- Added Invoice.RecalculateAfterDiscountApproval() as a public method since RecalculateTotals() is private and approval happens outside the ApplyDiscount flow

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added Invoice.RecalculateAfterDiscountApproval() domain method**
- **Found during:** Task 2 (Implement handlers)
- **Issue:** Invoice.RecalculateTotals() is private; after approving a discount, the handler needs to trigger recalculation of DiscountTotal and TotalAmount
- **Fix:** Added public RecalculateAfterDiscountApproval() method that delegates to private RecalculateTotals()
- **Files modified:** backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs
- **Verification:** ApproveDiscount_ValidPin test passes, invoice totals recalculated correctly
- **Committed in:** 377d000 (Task 2 commit)

**2. [Rule 3 - Blocking] Added Invoice.AddRefund() domain method**
- **Found during:** Task 2 (Implement handlers)
- **Issue:** Invoice had no method to add refunds to its collection; only Draft-restricted ApplyDiscount existed
- **Fix:** Added AddRefund() method that validates invoice is Finalized before accepting refund
- **Files modified:** backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs
- **Verification:** RequestRefund_FinalizedInvoice test passes, refund added to invoice
- **Committed in:** 377d000 (Task 2 commit)

**3. [Rule 1 - Bug] Fixed Department enum reference in tests**
- **Found during:** Task 1 (Write tests)
- **Issue:** Tests used Department.Clinical which doesn't exist; correct value is Department.Medical
- **Fix:** Changed all references from Department.Clinical to Department.Medical
- **Files modified:** DiscountHandlerTests.cs, RefundHandlerTests.cs
- **Verification:** Tests compile and run correctly
- **Committed in:** dfc333c (Task 1 commit)

---

**Total deviations:** 3 auto-fixed (1 bug, 2 blocking)
**Impact on plan:** All auto-fixes necessary for correctness. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Discount and refund core handlers ready for Plan 26 supplementary handlers (RejectDiscount, ApproveRefund, ProcessRefund)
- VerifyManagerPinQuery contract ready for Auth module handler implementation
- Billing.Unit.Tests project established for future billing test plans

## Self-Check: PASSED

All 6 created files verified on disk. Both task commits (dfc333c, 377d000) verified in git log.

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*

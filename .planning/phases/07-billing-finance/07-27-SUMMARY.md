---
phase: 07-billing-finance
plan: 27
subsystem: api
tags: [ef-core, concurrency, wolverine, billing, handlers]

# Dependency graph
requires:
  - phase: 07-billing-finance
    provides: "Billing handlers (07-08 through 07-26)"
provides:
  - "Concurrency-safe billing handlers (no redundant Update() calls)"
  - "Server-side RequestedById auto-population via ICurrentUser for discounts and refunds"
affects: [07-billing-finance, frontend-billing]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "EF Core tracked entity pattern: entities loaded via GetByIdAsync are tracked, no Update() needed"
    - "Wolverine DI parameter injection for ICurrentUser in handler signatures"

key-files:
  created: []
  modified:
    - "backend/src/Modules/Billing/Billing.Application/Features/RecordPayment.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/ApplyDiscount.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/ApproveDiscount.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/RejectDiscount.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/RequestRefund.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/ApproveRefund.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/ProcessRefund.cs"
    - "backend/tests/Billing.Unit.Tests/Features/DiscountHandlerTests.cs"
    - "backend/tests/Billing.Unit.Tests/Features/RefundHandlerTests.cs"
    - "backend/tests/Billing.Unit.Tests/Features/PaymentHandlerTests.cs"

key-decisions:
  - "Removed all invoiceRepository.Update(invoice) calls since EF Core change tracker handles tracked entities automatically"
  - "Removed paymentRepository.Update(payment) from ProcessRefund since payments are loaded via Invoice Include and tracked"
  - "Kept cashierShiftRepository.Update(shift) because CashierShift has no RowVersion column"
  - "ICurrentUser injected via Wolverine DI parameter injection pattern (not constructor injection)"

patterns-established:
  - "EF Core tracked entities: never call Update() on entities loaded via GetByIdAsync -- change tracker detects modifications on SaveChangesAsync"
  - "Server-side user context: always use ICurrentUser for RequestedById/CreatedById instead of client-supplied values"

requirements-completed: [FIN-04, FIN-05, FIN-08]

# Metrics
duration: 7min
completed: 2026-03-06
---

# Phase 07 Plan 27: Billing Handler Concurrency & RequestedById Fixes Summary

**Removed redundant EF Core Update() calls from 7 billing handlers to fix DbUpdateConcurrencyException, and auto-populated RequestedById from ICurrentUser in discount/refund handlers**

## Performance

- **Duration:** 7 min
- **Started:** 2026-03-06T16:30:05Z
- **Completed:** 2026-03-06T16:37:29Z
- **Tasks:** 2
- **Files modified:** 10

## Accomplishments
- Eliminated DbUpdateConcurrencyException across all 7 billing handlers by removing redundant invoiceRepository.Update() calls
- Removed RequestedById from ApplyDiscountCommand (now 5 params) and RequestRefundCommand (now 4 params)
- Added ICurrentUser injection to ApplyDiscountHandler and RequestRefundHandler for server-side user identity
- Removed paymentRepository.Update() from ProcessRefund (payments tracked via Invoice Include)
- All 50 billing unit tests pass with updated handler signatures

## Task Commits

Each task was committed atomically:

1. **Task 1: Remove redundant Update() calls from all 7 handlers + fix RequestedById auto-population** - `8874a67` (fix)
2. **Task 2: Update unit tests for changed handler signatures** - `9afdaba` (test)

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Application/Features/RecordPayment.cs` - Removed invoiceRepository.Update(invoice)
- `backend/src/Modules/Billing/Billing.Application/Features/ApplyDiscount.cs` - Removed RequestedById from command, added ICurrentUser, removed Update()
- `backend/src/Modules/Billing/Billing.Application/Features/ApproveDiscount.cs` - Removed invoiceRepository.Update(invoice)
- `backend/src/Modules/Billing/Billing.Application/Features/RejectDiscount.cs` - Removed invoiceRepository.Update(invoice)
- `backend/src/Modules/Billing/Billing.Application/Features/RequestRefund.cs` - Removed RequestedById from command, added ICurrentUser, removed Update()
- `backend/src/Modules/Billing/Billing.Application/Features/ApproveRefund.cs` - Removed invoiceRepository.Update(invoice)
- `backend/src/Modules/Billing/Billing.Application/Features/ProcessRefund.cs` - Removed invoiceRepository.Update(invoice) and paymentRepository.Update(payment)
- `backend/tests/Billing.Unit.Tests/Features/DiscountHandlerTests.cs` - Added ICurrentUser mock, updated command/Handle signatures
- `backend/tests/Billing.Unit.Tests/Features/RefundHandlerTests.cs` - Updated command/Handle signatures with ICurrentUser
- `backend/tests/Billing.Unit.Tests/Features/PaymentHandlerTests.cs` - Removed invoiceRepository.Update assertion

## Decisions Made
- Removed all `invoiceRepository.Update(invoice)` calls since EF Core's change tracker detects modifications on tracked entities automatically via `SaveChangesAsync()`
- Removed `paymentRepository.Update(payment)` from ProcessRefund because payments loaded via `Invoice.Include(i => i.Payments)` are already tracked
- Kept `cashierShiftRepository.Update(shift)` since CashierShift has no RowVersion column and the Update() is harmless
- Used Wolverine DI parameter injection pattern (`ICurrentUser currentUser` in Handle() signature) rather than constructor injection

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All 7 billing handlers are now concurrency-safe and will not throw DbUpdateConcurrencyException
- Frontend discount/refund forms no longer need to send RequestedById (server auto-populates from auth context)
- Ready for plan 07-28 (remaining gap closure items)

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*

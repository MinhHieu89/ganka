---
phase: 07-billing-finance
plan: 01
subsystem: billing
tags: [domain-entities, aggregate-root, ddd, invoice, csharp]

# Dependency graph
requires:
  - phase: shared-domain
    provides: AggregateRoot, Entity, IAuditable, IDomainEvent, BranchId base classes
provides:
  - Invoice aggregate root with progressive line item accumulation
  - InvoiceLineItem child entity with department-based revenue allocation
  - InvoiceStatus and Department enums
  - InvoiceFinalizedEvent domain event
affects: [07-02, 07-04, 07-07, 07-10, billing-infrastructure, billing-application]

# Tech tracking
tech-stack:
  added: []
  patterns: [aggregate-root-with-backing-fields, domain-event-sealed-record, factory-method-pattern]

key-files:
  created:
    - backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs
    - backend/src/Modules/Billing/Billing.Domain/Enums/InvoiceStatus.cs
    - backend/src/Modules/Billing/Billing.Domain/Enums/Department.cs
    - backend/src/Modules/Billing/Billing.Domain/Events/InvoiceFinalizedEvent.cs
  modified:
    - backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs

key-decisions:
  - "Invoice.RecordPayment does not enforce Draft status to allow payments on pending invoices before finalization"
  - "RecalculateTotals only sums approved discounts (ApprovalStatus.Approved) for correctness"
  - "Added _refunds backing field since Refund entity already exists from Plan 07-02"
  - "Used string.Empty for default string properties matching Visit.cs pattern"

patterns-established:
  - "Billing aggregate root pattern: AggregateRoot + IAuditable, private backing fields, factory Create method"
  - "Department enum for revenue allocation on all billable line items"
  - "InvoiceFinalizedEvent as sealed record with primary constructor parameters"

requirements-completed: [FIN-01, FIN-02, FIN-09]

# Metrics
duration: 6min
completed: 2026-03-06
---

# Phase 07 Plan 01: Invoice Domain Entities Summary

**Invoice aggregate root with department-based line items, payment/discount backing fields, and IAuditable for price change audit logging**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-06T13:56:55Z
- **Completed:** 2026-03-06T14:03:00Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Created Invoice aggregate root with AddLineItem, RemoveLineItem, RecordPayment, ApplyDiscount, Finalize, Void domain methods
- Created InvoiceLineItem child entity with Department enum for revenue allocation (FIN-02)
- Established backing fields (_lineItems, _payments, _discounts, _refunds) for EF Core configuration in Plan 04
- Added InvoiceStatus (Draft/Finalized/Voided), Department (Medical/Pharmacy/Optical/Treatment) enums and InvoiceFinalizedEvent

## Task Commits

Each task was committed atomically:

1. **Task 1: Create Invoice enums and domain event** - `4136a99` (feat)
2. **Task 2: Create Invoice aggregate root and InvoiceLineItem entity** - `9b6f4d2` (feat)

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Domain/Enums/InvoiceStatus.cs` - Invoice lifecycle status enum (Draft, Finalized, Voided)
- `backend/src/Modules/Billing/Billing.Domain/Enums/Department.cs` - Department categories for revenue allocation
- `backend/src/Modules/Billing/Billing.Domain/Events/InvoiceFinalizedEvent.cs` - Domain event raised on invoice finalization
- `backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs` - Invoice aggregate root with full domain logic

## Decisions Made
- Invoice.RecordPayment does not enforce Draft status -- payments need to be recorded before finalization check
- RecalculateTotals filters discounts by ApprovalStatus.Approved to only count manager-approved discounts
- Included _refunds backing field since Refund entity was already created by concurrent Plan 07-02
- Used string.Empty initialization (not default!) matching the Visit.cs codebase pattern

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added Void() idempotency guard**
- **Found during:** Task 2 (Invoice aggregate root)
- **Issue:** Plan didn't specify what happens when Void() is called on an already-voided invoice
- **Fix:** Added guard: `if (Status == InvoiceStatus.Voided) throw`
- **Files modified:** Invoice.cs
- **Verification:** Build succeeds
- **Committed in:** 9b6f4d2 (Task 2 commit)

**2. [Rule 3 - Blocking] Leveraged existing Plan 02 entities instead of creating stubs**
- **Found during:** Task 2 (Invoice aggregate root)
- **Issue:** Plan specified creating stub Payment.cs/Discount.cs, but Plan 07-02 had already committed full implementations
- **Fix:** Used existing full Payment/Discount/Refund entities; added _refunds backing field
- **Files modified:** Invoice.cs
- **Verification:** Build succeeds with full entity references
- **Committed in:** 9b6f4d2 (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (1 missing critical, 1 blocking)
**Impact on plan:** Both deviations improve correctness. No scope creep.

## Issues Encountered
- File lock on Shared.Domain.dll required killing stale dotnet processes before build
- Concurrent agent execution (Plans 02, 03, 06, 07) created entities in working tree that this plan references

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Invoice and InvoiceLineItem entities ready for EF Core configuration (Plan 04)
- Backing fields (_payments, _discounts, _refunds) declared for HasMany/PropertyAccessMode.Field setup
- Domain methods ready for TDD testing (Plan 10)
- Department enum ready for revenue allocation queries

## Self-Check: PASSED

- All 4 created files verified on disk
- Commit 4136a99 (Task 1) verified in git history
- Commit 9b6f4d2 (Task 2) verified in git history

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*

---
phase: 07-billing-finance
plan: 09
subsystem: api
tags: [wolverine, tdd, invoice, billing, fluent-validation, vertical-slice]

# Dependency graph
requires:
  - phase: 07-billing-finance (plans 07-08)
    provides: "Repository interfaces, UnitOfWork, domain entities, IoC registration"
provides:
  - "CreateInvoiceHandler with HD-YYYY-NNNNN invoice number generation and FluentValidation"
  - "AddInvoiceLineItemHandler with department-based revenue allocation"
  - "FinalizeInvoiceHandler with full payment validation and cashier shift recording"
  - "InvoiceDto mapping from Invoice aggregate (shared MapToDto method)"
  - "6 unit tests covering all invoice lifecycle behaviors"
affects: [07-10, 07-11, 07-12, 07-15, 07-25]

# Tech tracking
tech-stack:
  added: []
  patterns: [wolverine-static-handler, tdd-red-green, vertical-slice-command-handler-validator]

key-files:
  created:
    - "backend/src/Modules/Billing/Billing.Application/Features/CreateInvoice.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/AddInvoiceLineItem.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/FinalizeInvoice.cs"
    - "backend/tests/Billing.Unit.Tests/Billing.Unit.Tests.csproj"
    - "backend/tests/Billing.Unit.Tests/Features/InvoiceCrudHandlerTests.cs"
  modified: []

key-decisions:
  - "Reused MapToDto as internal static method on CreateInvoiceHandler for DRY invoice-to-DTO mapping across all handlers"
  - "Used Error.Custom for domain InvalidOperationException wrapping instead of Error.Validation to distinguish domain rule violations from input validation"

patterns-established:
  - "Billing handler pattern: Wolverine static Handle method, repository load, domain method call, save, return mapped DTO"
  - "Invoice mapping pattern: CreateInvoiceHandler.MapToDto used by all invoice-returning handlers"

requirements-completed: [FIN-01, FIN-02, FIN-09]

# Metrics
duration: 7min
completed: 2026-03-06
---

# Phase 07 Plan 09: Invoice CRUD Handlers Summary

**TDD implementation of CreateInvoice, AddInvoiceLineItem, FinalizeInvoice handlers with HD-YYYY-NNNNN number generation, department revenue allocation, and full payment validation**

## Performance

- **Duration:** 7 min
- **Started:** 2026-03-06T14:17:59Z
- **Completed:** 2026-03-06T14:25:15Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- Created Billing.Unit.Tests project with xUnit, FluentAssertions, NSubstitute references
- Wrote 6 comprehensive tests covering CreateInvoice (valid + validation error), AddInvoiceLineItem (draft + finalized), FinalizeInvoice (fully paid + outstanding balance)
- Implemented CreateInvoiceHandler with FluentValidation, auto-generated invoice number via repository, and BranchId from ICurrentUser
- Implemented AddInvoiceLineItemHandler with department-based line items and total recalculation for FIN-02 revenue allocation
- Implemented FinalizeInvoiceHandler with full payment validation, cashier shift recording, and InvoiceFinalizedEvent
- All 33 Billing.Unit.Tests pass including tests from other plans (no regressions)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create test project and write failing tests (RED)** - `b46682b` (test) - Tests and stub handlers committed by parallel plan 07-10
2. **Task 2: Implement invoice CRUD handlers (GREEN)** - `518eae4` (feat) - Full handler implementations replacing stubs

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Application/Features/CreateInvoice.cs` - CreateInvoiceCommand, CreateInvoiceValidator, CreateInvoiceHandler with MapToDto
- `backend/src/Modules/Billing/Billing.Application/Features/AddInvoiceLineItem.cs` - AddInvoiceLineItemCommand, AddInvoiceLineItemHandler with department allocation
- `backend/src/Modules/Billing/Billing.Application/Features/FinalizeInvoice.cs` - FinalizeInvoiceCommand, FinalizeInvoiceHandler with payment validation
- `backend/tests/Billing.Unit.Tests/Billing.Unit.Tests.csproj` - Test project with xUnit, FluentAssertions, NSubstitute, Billing refs
- `backend/tests/Billing.Unit.Tests/Features/InvoiceCrudHandlerTests.cs` - 6 unit tests for invoice CRUD lifecycle

## Decisions Made
- Reused MapToDto as `internal static` method on CreateInvoiceHandler for DRY invoice-to-DTO mapping -- all invoice-returning handlers can call `CreateInvoiceHandler.MapToDto(invoice)` instead of duplicating mapping logic
- Used `Error.Custom("Error.InvalidOperation", ex.Message)` to wrap domain `InvalidOperationException` from EnsureDraft and Finalize -- distinguishes domain rule violations from input validation errors

## Deviations from Plan

None - plan executed exactly as written. Task 1 RED phase stubs were already committed by parallel plan 07-10 which needed them as prerequisites for payment handler tests.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Invoice CRUD handlers complete, ready for Plan 10 (payment handlers) and Plan 11 (discount/refund handlers)
- MapToDto method available for reuse by Plan 25 (supplementary invoice handlers: RemoveInvoiceLineItem, GetInvoiceById, GetInvoicesByVisit)
- InvoiceFinalizedEvent raised on finalization, ready for event handlers in later plans

## Self-Check: PASSED

All 5 key files verified present. Both task commits (b46682b, 518eae4) found in git log.

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*

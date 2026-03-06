---
phase: 07-billing-finance
plan: 25
subsystem: api
tags: [wolverine-handlers, vertical-slice, invoice-crud, shift-templates, query-handlers]

# Dependency graph
requires:
  - phase: 07-billing-finance (plans 07-09)
    provides: "CreateInvoiceHandler with MapToDto, Invoice aggregate with RemoveLineItem, IInvoiceRepository"
  - phase: 07-billing-finance (plans 07-12)
    provides: "ICashierShiftRepository, CashierShiftRepository, CashierShift entity, ShiftTemplate entity"
provides:
  - "RemoveInvoiceLineItem command handler for removing line items from draft invoices"
  - "GetInvoiceById query handler returning full InvoiceDto with all child collections"
  - "GetInvoicesByVisit query handler returning lightweight InvoiceSummaryDto list"
  - "GetShiftTemplates query handler returning active shift templates via ICashierShiftRepository"
  - "GetAllByVisitIdAsync on IInvoiceRepository for multi-invoice visit queries"
  - "GetActiveShiftTemplatesAsync on ICashierShiftRepository for branch-scoped template queries"
affects: [07-13, 07-21]

# Tech tracking
tech-stack:
  added: []
  patterns: [wolverine-static-handler, vertical-slice-query-handler, result-pattern]

key-files:
  created:
    - "backend/src/Modules/Billing/Billing.Application/Features/RemoveInvoiceLineItem.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/GetInvoiceById.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/GetInvoicesByVisit.cs"
    - "backend/src/Modules/Billing/Billing.Application/Features/GetShiftTemplates.cs"
  modified:
    - "backend/src/Modules/Billing/Billing.Application/Interfaces/IInvoiceRepository.cs"
    - "backend/src/Modules/Billing/Billing.Application/Interfaces/ICashierShiftRepository.cs"
    - "backend/src/Modules/Billing/Billing.Infrastructure/Repositories/InvoiceRepository.cs"
    - "backend/src/Modules/Billing/Billing.Infrastructure/Repositories/CashierShiftRepository.cs"

key-decisions:
  - "Reused CreateInvoiceHandler.MapToDto for all invoice-returning handlers (RemoveInvoiceLineItem, GetInvoiceById) for DRY consistency"
  - "Added GetAllByVisitIdAsync to IInvoiceRepository (returns List<Invoice>) separate from existing GetByVisitIdAsync (returns single Invoice?) to support multi-invoice visit queries"
  - "Extended ICashierShiftRepository with GetActiveShiftTemplatesAsync rather than creating separate IShiftTemplateRepository, keeping shift-related data access centralized"
  - "Used TimeOnly.ToString(HH:mm) format for ShiftTemplateDto time fields for clean API serialization"

patterns-established:
  - "Invoice query pattern: lightweight summary DTOs (InvoiceSummaryDto) for list views, full DTOs (InvoiceDto) for detail views"
  - "Repository extension pattern: add query methods to existing domain repository rather than creating new repository for related entities"

requirements-completed: [FIN-01, FIN-10]

# Metrics
duration: 3min
completed: 2026-03-06
---

# Phase 07 Plan 25: Supplementary Invoice & Shift Handlers Summary

**RemoveInvoiceLineItem, GetInvoiceById, GetInvoicesByVisit, and GetShiftTemplates handlers completing the billing CRUD surface for API endpoints**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-06T14:28:54Z
- **Completed:** 2026-03-06T14:32:09Z
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments
- Created RemoveInvoiceLineItem command handler with FluentValidation and domain method call (invoice.RemoveLineItem) for draft invoice line item removal
- Created GetInvoiceById query handler returning full InvoiceDto with all child collections (line items, payments, discounts, refunds) via MapToDto
- Created GetInvoicesByVisit query handler returning lightweight InvoiceSummaryDto list for visit-scoped invoice lookups
- Created GetShiftTemplates query handler with ICashierShiftRepository extension for branch-scoped active template queries
- Extended both IInvoiceRepository and ICashierShiftRepository interfaces with new query methods and their EF Core implementations

## Task Commits

Each task was committed atomically:

1. **Task 1: Create supplementary invoice handlers** - `d10bfe6` (feat) - RemoveInvoiceLineItem, GetInvoiceById, GetInvoicesByVisit + repository extension
2. **Task 2: Create GetShiftTemplates handler** - `a50c209` (feat) - GetShiftTemplates handler + ICashierShiftRepository extension

**Plan metadata:** (pending)

## Files Created/Modified
- `Billing.Application/Features/RemoveInvoiceLineItem.cs` - RemoveInvoiceLineItemCommand, validator, handler with draft guard and recalculation
- `Billing.Application/Features/GetInvoiceById.cs` - GetInvoiceByIdQuery, handler returning full InvoiceDto
- `Billing.Application/Features/GetInvoicesByVisit.cs` - GetInvoicesByVisitQuery, handler returning InvoiceSummaryDto list
- `Billing.Application/Features/GetShiftTemplates.cs` - GetShiftTemplatesQuery, handler returning active ShiftTemplateDto list
- `Billing.Application/Interfaces/IInvoiceRepository.cs` - Added GetAllByVisitIdAsync method
- `Billing.Application/Interfaces/ICashierShiftRepository.cs` - Added GetActiveShiftTemplatesAsync method
- `Billing.Infrastructure/Repositories/InvoiceRepository.cs` - Implemented GetAllByVisitIdAsync with EF Core
- `Billing.Infrastructure/Repositories/CashierShiftRepository.cs` - Implemented GetActiveShiftTemplatesAsync with EF Core

## Decisions Made
- Reused `CreateInvoiceHandler.MapToDto` for RemoveInvoiceLineItem and GetInvoiceById handlers to maintain DRY consistency across all invoice-returning handlers
- Added `GetAllByVisitIdAsync` (returns `List<Invoice>`) to IInvoiceRepository separately from existing `GetByVisitIdAsync` (returns single `Invoice?`) -- the existing method supports progressive invoice lookup while the new one supports visit summary listing
- Extended ICashierShiftRepository with GetActiveShiftTemplatesAsync rather than creating a separate IShiftTemplateRepository, keeping all shift-related data access in one repository as per plan guidance
- Used `TimeOnly.ToString("HH:mm")` for ShiftTemplateDto time serialization for clean, readable API output

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added GetAllByVisitIdAsync to IInvoiceRepository**
- **Found during:** Task 1 (GetInvoicesByVisit handler)
- **Issue:** Existing GetByVisitIdAsync returns single Invoice?, but GetInvoicesByVisit needs to return a list of all invoices for a visit
- **Fix:** Added GetAllByVisitIdAsync method to IInvoiceRepository interface and InvoiceRepository implementation
- **Files modified:** IInvoiceRepository.cs, InvoiceRepository.cs
- **Verification:** Build succeeds with 0 warnings, 0 errors
- **Committed in:** d10bfe6 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Required for handler to function. No scope creep.

## Issues Encountered
- Pre-existing test failures in RefundHandlerTests.cs (ApproveRefundCommand, ProcessRefundCommand not yet implemented) and DiscountHandlerTests.cs (RejectDiscountCommand signature change) -- these are from other plans and out of scope

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All supplementary invoice handlers complete, ready for Plan 13 (API endpoints) which references all these handlers
- GetShiftTemplates handler ready for /api/billing/shifts/templates endpoint
- Invoice CRUD surface complete: Create, AddLineItem, RemoveLineItem, GetById, GetByVisit, Finalize

## Self-Check: PASSED

All 4 created files verified present. Both task commits (d10bfe6, a50c209) found in git log.

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*

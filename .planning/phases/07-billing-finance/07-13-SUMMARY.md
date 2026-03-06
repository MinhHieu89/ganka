---
phase: 07-billing-finance
plan: 13
subsystem: api
tags: [minimal-api, wolverine, billing-endpoints, authorization, invoice, payment, discount, refund, shift]

# Dependency graph
requires:
  - phase: 07-09
    provides: "CreateInvoice, AddInvoiceLineItem, FinalizeInvoice command handlers"
  - phase: 07-10
    provides: "RecordPayment, GetPaymentsByInvoice handlers, PaymentDto"
  - phase: 07-11
    provides: "ApplyDiscount, ApproveDiscount, RequestRefund handlers with PIN verification"
  - phase: 07-12
    provides: "OpenShift, CloseShift, GetCurrentShift, GetShiftReport handlers"
  - phase: 07-25
    provides: "RemoveInvoiceLineItem, GetInvoiceById, GetInvoicesByVisit, GetShiftTemplates handlers"
  - phase: 07-26
    provides: "RejectDiscount, ApproveRefund, ProcessRefund handlers"
provides:
  - "21 billing API endpoints under /api/billing with RequireAuthorization"
  - "Full CRUD for invoices: create, get by ID, get by visit, add/remove line items, finalize"
  - "Payment endpoints: record payment, get payments by invoice"
  - "Discount lifecycle endpoints: apply, approve, reject"
  - "Refund lifecycle endpoints: request, approve, process"
  - "Shift management endpoints: open, close, current, report, templates"
affects: [07-20, 07-21, frontend-billing]

# Tech tracking
tech-stack:
  added: []
  patterns: [minimal-api-endpoint-grouping, wolverine-bus-dispatch, route-parameter-enrichment]

key-files:
  created: []
  modified:
    - "backend/src/Modules/Billing/Billing.Presentation/BillingApiEndpoints.cs"

key-decisions:
  - "Kept existing /invoices/visit/{visitId} endpoint for cross-module GetVisitInvoiceQuery alongside new /invoices/by-visit/{visitId} for GetInvoicesByVisit summary list -- separate routes for different use cases"
  - "Used route parameter enrichment pattern for all endpoints with path IDs (discountId, refundId, invoiceId) -- extracting from route and combining with request body into enriched command"
  - "Kept /invoices/pending endpoint from Plan 14 in addition to plan-specified endpoints for cashier dashboard functionality"

patterns-established:
  - "Billing endpoint groups: invoices (8 endpoints), payments (2), discounts (3), refunds (3), shifts (5)"
  - "DELETE verb used for RemoveInvoiceLineItem following REST conventions (only DELETE endpoint in billing)"

requirements-completed: [FIN-01, FIN-03, FIN-07, FIN-08, FIN-10]

# Metrics
duration: 2min
completed: 2026-03-06
---

# Phase 07 Plan 13: Billing API Endpoints Summary

**21 Minimal API endpoints under /api/billing covering invoices, payments, discounts, refunds, and shifts with RequireAuthorization and Wolverine bus dispatch**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-06T14:43:52Z
- **Completed:** 2026-03-06T14:46:40Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- Added 7 missing endpoints from Plan 25/26 handlers: GetInvoiceById, GetInvoicesByVisit, RemoveInvoiceLineItem, RejectDiscount, ApproveRefund, ProcessRefund, GetShiftTemplates
- Total of 21 billing endpoints with proper authorization, HTTP methods, and route patterns
- All endpoints use bus.InvokeAsync for Wolverine command/query dispatch with Result pattern
- MapGroup used for /api/billing prefix consolidation with group-level RequireAuthorization

## Task Commits

Each task was committed atomically:

1. **Task 1: Create Billing.Presentation project** - already existed from Plan 07-14 (no changes needed)
2. **Task 2: Create all billing API endpoints** - `d899e60` (feat) - 7 new endpoints added to complete the billing API surface

**Plan metadata:** (pending)

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Presentation/BillingApiEndpoints.cs` - All 21 billing API endpoints with 7 new additions for Plan 25/26 handlers

## Decisions Made
- Kept existing /invoices/visit/{visitId} cross-module endpoint alongside new /invoices/by-visit/{visitId} for distinct query purposes (single invoice vs summary list)
- Used route parameter enrichment for all endpoints with path IDs to prevent client-supplied ID mismatch
- Kept /invoices/pending endpoint from Plan 14 as it provides cashier dashboard functionality beyond the plan specification

## Deviations from Plan

None - plan executed as written. Task 1 deliverables (csproj, IoC.cs) already existed from Plan 07-14 which created the Billing.Presentation project as part of bootstrapper integration. Task 2 added the 7 missing endpoints that were not available when Plan 14 was executed (Plan 25 and 26 handlers did not exist yet).

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All billing API endpoints are live and ready for frontend integration
- Full invoice lifecycle API: create -> add items -> remove items -> record payment -> finalize
- Full discount lifecycle API: apply -> approve/reject (with PIN verification)
- Full refund lifecycle API: request -> approve (with PIN) -> process (with shift tracking)
- Full shift management API: open -> transact -> close -> report + templates
- Ready for Plan 20 (E-Invoice) and Plan 21 (Financial Reporting) frontend integration

## Self-Check: PASSED

(verified below)

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*

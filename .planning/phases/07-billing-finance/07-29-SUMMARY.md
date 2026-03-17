---
phase: 07-billing-finance
plan: 29
subsystem: billing
tags: [invoice-history, pagination, datatable, search, status-filter, tanstack-query]

requires:
  - phase: 07-billing-finance
    provides: Invoice CRUD, InvoiceSummaryDto, billing API endpoints
provides:
  - GET /api/billing/invoices paginated endpoint with status filter and search
  - Invoice History page at /billing/invoices with DataTable, tabs, search
  - Navigation from billing dashboard to invoice history
affects: [07-billing-finance]

tech-stack:
  added: []
  patterns: [paginated-query-result, debounced-search-with-tabs]

key-files:
  created:
    - backend/src/Modules/Billing/Billing.Contracts/Queries/GetAllInvoicesQuery.cs
    - backend/src/Modules/Billing/Billing.Application/Features/GetAllInvoices.cs
    - frontend/src/features/billing/components/InvoiceHistoryPage.tsx
    - frontend/src/app/routes/_authenticated/billing/invoices.index.tsx
  modified:
    - backend/src/Modules/Billing/Billing.Application/Interfaces/IInvoiceRepository.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Repositories/InvoiceRepository.cs
    - backend/src/Modules/Billing/Billing.Presentation/BillingApiEndpoints.cs
    - backend/tests/Billing.Unit.Tests/Features/InvoiceCrudHandlerTests.cs
    - frontend/src/features/billing/api/billing-api.ts
    - frontend/src/app/routes/_authenticated/billing/index.tsx
    - frontend/public/locales/en/billing.json
    - frontend/public/locales/vi/billing.json

key-decisions:
  - "Used PaginatedInvoicesResult wrapper for consistent pagination response"
  - "Placed GET /invoices before /invoices/{id} to avoid route conflicts"

patterns-established:
  - "Paginated list query pattern: Query with status/search/page/pageSize -> PaginatedResult<SummaryDto>"

requirements-completed: [FIN-01, FIN-02]

duration: 6min
completed: 2026-03-17
---

# Phase 07 Plan 29: All Invoices Page Summary

**Paginated invoice history endpoint and DataTable page with status tabs, search, and dashboard navigation closing UAT Gap 1**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-17T10:09:24Z
- **Completed:** 2026-03-17T10:15:26Z
- **Tasks:** 2
- **Files modified:** 12

## Accomplishments
- Backend GET /api/billing/invoices endpoint with pagination, status filter, and search (5 unit tests)
- Frontend Invoice History page at /billing/invoices with shadcn Table, Tabs, debounced search, pagination
- "View All" navigation link on billing dashboard
- Full i18n support (en/vi) for all new UI strings

## Task Commits

Each task was committed atomically:

1. **Task 1 RED: Failing tests for GetAllInvoices** - `40feea7` (test)
2. **Task 1 GREEN: GetAllInvoices handler implementation** - `0dd5835` (feat)
3. **Task 2: Frontend Invoice History page** - `056ba92` (feat)

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Contracts/Queries/GetAllInvoicesQuery.cs` - Query and PaginatedInvoicesResult DTOs
- `backend/src/Modules/Billing/Billing.Application/Features/GetAllInvoices.cs` - Wolverine handler mapping Invoice to InvoiceSummaryDto
- `backend/src/Modules/Billing/Billing.Application/Interfaces/IInvoiceRepository.cs` - Added GetAllAsync method
- `backend/src/Modules/Billing/Billing.Infrastructure/Repositories/InvoiceRepository.cs` - EF Core implementation with Where/Skip/Take
- `backend/src/Modules/Billing/Billing.Presentation/BillingApiEndpoints.cs` - GET /api/billing/invoices endpoint
- `backend/tests/Billing.Unit.Tests/Features/InvoiceCrudHandlerTests.cs` - 5 new test methods
- `frontend/src/features/billing/api/billing-api.ts` - getAllInvoices, useAllInvoices, PaginatedInvoicesResult
- `frontend/src/features/billing/components/InvoiceHistoryPage.tsx` - DataTable with status tabs, search, pagination
- `frontend/src/app/routes/_authenticated/billing/invoices.index.tsx` - Route with breadcrumb
- `frontend/src/app/routes/_authenticated/billing/index.tsx` - Added "View All" link
- `frontend/public/locales/en/billing.json` - Invoice history i18n keys
- `frontend/public/locales/vi/billing.json` - Vietnamese translations

## Decisions Made
- Used PaginatedInvoicesResult wrapper for consistent pagination response (matches existing ShiftHistoryResult pattern)
- Placed GET /invoices route before /invoices/{invoiceId:guid} to avoid ASP.NET route matching conflicts
- Used case-insensitive Contains for search (ToLower pattern for SQL Server compatibility)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Invoice history page is complete and accessible from billing dashboard
- UAT Gap 1 (Test 8) is closed: finalized invoices are now browsable
- Status filter tabs and search provide full invoice discovery

---
*Phase: 07-billing-finance*
*Completed: 2026-03-17*

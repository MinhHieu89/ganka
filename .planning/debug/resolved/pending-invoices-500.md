---
status: resolved
trigger: "GET /api/billing/invoices/pending returns 500 error. GetPendingInvoicesQuery has no Wolverine handler."
created: 2026-03-06T00:00:00Z
updated: 2026-03-06T18:00:00Z
---

## Current Focus

hypothesis: GetPendingInvoicesQuery defined in Contracts but no handler exists in Application
test: Search entire backend/src for any handler class referencing GetPendingInvoicesQuery
expecting: Zero matches confirms missing handler
next_action: Return diagnosis

## Symptoms

expected: GET /api/billing/invoices/pending returns list of pending/draft invoices
actual: Returns 500 Internal Server Error
errors: Wolverine cannot find handler for GetPendingInvoicesQuery message type
reproduction: Call GET /api/billing/invoices/pending (with or without ?cashierShiftId=)
started: Handler was never implemented

## Eliminated

(none -- root cause confirmed on first hypothesis)

## Evidence

- timestamp: 2026-03-06T00:01:00Z
  checked: Billing.Contracts/Queries/GetVisitInvoiceQuery.cs
  found: GetPendingInvoicesQuery(Guid? CashierShiftId = null) defined at line 14
  implication: Query contract exists, Wolverine needs a matching handler

- timestamp: 2026-03-06T00:02:00Z
  checked: All .cs files in Billing.Application/Features/ directory
  found: No file matches *PendingInvoice*. Existing handlers: GetInvoiceById, GetInvoicesByVisit, CreateInvoice, FinalizeInvoice, AddInvoiceLineItem, RemoveInvoiceLineItem, etc.
  implication: Handler was never created

- timestamp: 2026-03-06T00:03:00Z
  checked: Full-text search for "GetPendingInvoicesQuery" across entire backend/src
  found: Only 2 references: (1) Contracts definition, (2) Presentation endpoint dispatching it via bus.InvokeAsync
  implication: Confirmed -- zero handler implementations exist anywhere in the codebase

- timestamp: 2026-03-06T00:04:00Z
  checked: BillingApiEndpoints.cs lines 72-78
  found: Endpoint dispatches GetPendingInvoicesQuery via IMessageBus.InvokeAsync<Result<List<InvoiceDto>>>
  implication: Wolverine receives the message but has no handler to route it to, causing 500

- timestamp: 2026-03-06T00:05:00Z
  checked: IInvoiceRepository interface
  found: No method for getting pending/draft invoices globally. Has GetPendingByPatientIdAsync(patientId) but no general GetPendingAsync() or GetDraftAsync()
  implication: Repository also needs a new method to support the handler

- timestamp: 2026-03-06T00:06:00Z
  checked: GetVisitInvoiceQuery handler
  found: Also missing -- no handler in Application for this Contracts query either
  implication: Secondary issue -- but separate from the pending invoices bug

## Resolution

root_cause: GetPendingInvoicesQuery is defined in Billing.Contracts and dispatched by the API endpoint at GET /api/billing/invoices/pending, but no Wolverine handler exists in Billing.Application to process it. Wolverine throws a runtime exception when it cannot find a handler for the message type, resulting in 500.
fix: (not applied -- diagnosis only)
verification: (not applied)
files_changed: []

---
phase: 07-billing-finance
plan: 33
subsystem: billing
tags: [domain-events, signalr, wolverine, prescription, invoice, tdd]

requires:
  - phase: 07-billing-finance
    provides: Invoice entity with line items, HandleDrugPrescriptionAdded event chain, SignalR billing hub

provides:
  - Prescription line item removal guard (domain-level protection)
  - DrugPrescriptionRemoved event chain (domain event -> integration event -> billing handler)
  - RemoveLineItemsBySource method for bulk source-based removal
  - LineItemRemoved SignalR notification
  - Frontend prescription line item lock icon

affects: [07-billing-finance]

tech-stack:
  added: []
  patterns: [domain-event-to-integration-event cascade for removal, source-based line item guard]

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Domain/Events/DrugPrescriptionRemovedEvent.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/IntegrationEvents/DrugPrescriptionRemovedIntegrationEvent.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/PublishDrugPrescriptionRemovedIntegrationEvent.cs
    - backend/src/Modules/Billing/Billing.Application/Features/HandleDrugPrescriptionRemoved.cs
  modified:
    - backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs
    - backend/src/Modules/Billing/Billing.Application/Interfaces/IBillingNotificationService.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Services/BillingNotificationService.cs
    - frontend/src/features/billing/components/InvoiceLineItemsTable.tsx
    - frontend/src/features/billing/hooks/use-billing-hub.ts

key-decisions:
  - "RemoveLineItemsBySource method allows event handlers to remove items without hitting the prescription guard"
  - "DrugPrescriptionRemovedEvent carries drug names for logging only, no pricing needed for removal"

patterns-established:
  - "Source-based line item guard: prescription items protected at domain level, only removable via RemoveLineItemsBySource"

requirements-completed: [FIN-01, FIN-03, FIN-04]

duration: 9min
completed: 2026-03-17
---

# Phase 07 Plan 33: Prescription Line Item Guard and Removal Event Chain Summary

**Domain-level prescription line item protection with DrugPrescriptionRemoved event chain for auto-sync between clinical prescriptions and billing invoices**

## Performance

- **Duration:** 9 min
- **Started:** 2026-03-17T15:11:18Z
- **Completed:** 2026-03-17T15:20:33Z
- **Tasks:** 2
- **Files modified:** 14

## Accomplishments
- Invoice.RemoveLineItem rejects prescription-sourced items at domain level with clear error message
- Full DrugPrescriptionRemoved event chain: Visit raises domain event -> cascading handler publishes integration event -> Billing handler removes line items and sends SignalR notification
- Frontend hides delete button for prescription items, shows lock icon with tooltip
- 10 new tests all pass with TDD RED->GREEN verified

## Task Commits

Each task was committed atomically:

1. **Task 1: Backend -- Protect prescription line items + event chain (RED)** - `eb668ce` (test)
2. **Task 1: Backend -- Protect prescription line items + event chain (GREEN)** - `42030af` (feat)
3. **Task 2: Frontend -- Hide delete button + LineItemRemoved SignalR** - `d0bc60a` (feat)

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Domain/Events/DrugPrescriptionRemovedEvent.cs` - Domain event for prescription removal
- `backend/src/Modules/Clinical/Clinical.Contracts/IntegrationEvents/DrugPrescriptionRemovedIntegrationEvent.cs` - Cross-module integration event
- `backend/src/Modules/Clinical/Clinical.Application/Features/PublishDrugPrescriptionRemovedIntegrationEvent.cs` - Cascading handler
- `backend/src/Modules/Billing/Billing.Application/Features/HandleDrugPrescriptionRemoved.cs` - Billing handler removes line items
- `backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs` - RemoveLineItem guard + RemoveLineItemsBySource method
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs` - Raises DrugPrescriptionRemovedEvent
- `backend/src/Modules/Billing/Billing.Application/Interfaces/IBillingNotificationService.cs` - NotifyLineItemRemovedAsync
- `backend/src/Modules/Billing/Billing.Infrastructure/Services/BillingNotificationService.cs` - LineItemRemoved SignalR notification
- `frontend/src/features/billing/components/InvoiceLineItemsTable.tsx` - Lock icon for prescription items
- `frontend/src/features/billing/hooks/use-billing-hub.ts` - LineItemRemoved event handler

## Decisions Made
- RemoveLineItemsBySource method allows event handlers to bypass the prescription guard, since removal is system-initiated (not cashier-initiated)
- DrugPrescriptionRemovedEvent carries only drug names (no pricing), since removal doesn't need price info
- HandleDrugPrescriptionRemoved only calls SaveChangesAsync when items were actually removed (optimization)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Bootstrapper build failed due to locked DLL files from running backend process. Resolved by killing dotnet processes per CLAUDE.md instructions.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- UAT retest gaps 3 and 4 are now closed
- Prescription integrity protected end-to-end (backend domain guard + frontend UI guard)
- Real-time sync between clinical and billing modules via event chain

---
*Phase: 07-billing-finance*
*Completed: 2026-03-17*

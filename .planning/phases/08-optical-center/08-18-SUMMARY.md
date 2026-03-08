---
phase: 08-optical-center
plan: 18
subsystem: api
tags: [optical, glasses-orders, payment-gate, tdd, wolverine, cross-module]

# Dependency graph
requires:
  - phase: 08-optical-center
    plan: 14
    provides: "GlassesOrder domain entity with TransitionTo/ConfirmPayment behaviours and state machine"
  - phase: 08-optical-center
    plan: 15
    provides: "IGlassesOrderRepository interface and GlassesOrderRepository EF Core implementation"
provides:
  - "CreateGlassesOrderHandler: creates orders with Ordered status, validates fields, adds line items"
  - "UpdateOrderStatusHandler: OPT-04 payment gate blocks Ordered->Processing without full invoice payment via IMessageBus"
  - "GetGlassesOrdersHandler: paginated list with optional status filter"
  - "GetGlassesOrderByIdHandler: single order detail with items and computed IsOverdue/IsUnderWarranty"
  - "GetOverdueOrdersHandler: alert list from repository.GetOverdueOrdersAsync"
affects: [08-optical-center, OPT-03, OPT-04, glasses-order-api, optical-presentation]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "OPT-04 payment gate: server-side IMessageBus.InvokeAsync<InvoiceDto?>(GetVisitInvoiceQuery) in UpdateOrderStatusHandler -- not a frontend UX check"
    - "Inline validation in handlers (no FluentValidation DI dependency) for testability"
    - "Result<T> return type for all query handlers; Result for command handlers"

key-files:
  created:
    - backend/src/Modules/Optical/Optical.Application/Features/Orders/CreateGlassesOrder.cs
    - backend/src/Modules/Optical/Optical.Application/Features/Orders/UpdateOrderStatus.cs
    - backend/src/Modules/Optical/Optical.Application/Features/Orders/GetGlassesOrders.cs
    - backend/src/Modules/Optical/Optical.Application/Features/Orders/GetGlassesOrderById.cs
    - backend/src/Modules/Optical/Optical.Application/Features/Orders/GetOverdueOrders.cs
    - backend/tests/Optical.Unit.Tests/Features/OrderHandlerTests.cs
  modified:
    - backend/tests/Optical.Unit.Tests/Optical.Unit.Tests.csproj

key-decisions:
  - "Payment gate uses IMessageBus.InvokeAsync<InvoiceDto?> cross-module query per OPT-04 pitfall 1 -- server-side enforcement, not stale frontend check"
  - "ConfirmPayment() called on entity before TransitionTo(Processing) to persist payment confirmation state"
  - "Inline validation in handlers rather than FluentValidation DI to keep static handlers directly testable without IoC setup"
  - "GetGlassesOrderByIdHandler sets FrameName/LensName to null -- denormalized names not stored on order item, Description field used for display"

patterns-established:
  - "IMessageBus cross-module query pattern: bus.InvokeAsync<TResult>(query, ct) - same as Billing module ApproveDiscount handler"
  - "OrderHandler test pattern: use helper CreateTestOrder() with initialStatus parameter to advance state machine"

requirements-completed: [OPT-03, OPT-04]

# Metrics
duration: 7min
completed: 2026-03-08
---

# Phase 08 Plan 18: GlassesOrder Lifecycle Handlers Summary

**Five Wolverine static handlers for glasses order lifecycle with OPT-04 payment gate: UpdateOrderStatus blocks Ordered->Processing via IMessageBus cross-module billing check**

## Performance

- **Duration:** 7 min
- **Started:** 2026-03-08T03:18:02Z
- **Completed:** 2026-03-08T03:25:20Z
- **Tasks:** 2 (TDD RED + GREEN for all 5 handlers)
- **Files modified:** 7

## Accomplishments

- OPT-04 payment gate implemented server-side: UpdateOrderStatusHandler queries Billing module via IMessageBus.InvokeAsync<InvoiceDto?>(GetVisitInvoiceQuery) before allowing Ordered->Processing transition
- All 5 handlers implemented with TDD (19 tests, all passing): CreateGlassesOrder, UpdateOrderStatus, GetGlassesOrders, GetGlassesOrderById, GetOverdueOrders
- Payment gate calls order.ConfirmPayment() and order.TransitionTo(Processing) atomically on successful billing verification
- Overdue detection delegated to repository.GetOverdueOrdersAsync which performs date comparison server-side

## Task Commits

Each task was committed atomically:

1. **RED: Test file** - `4b42255` (test): 19 failing tests covering all handlers and payment gate scenarios
2. **GREEN Task 1: CreateGlassesOrder + UpdateOrderStatus** - `d6e760d` (feat): payment gate enforcement via IMessageBus
3. **GREEN Task 2: Query handlers** - `5b45fab` (feat): GetGlassesOrders, GetGlassesOrderById, GetOverdueOrders

_Note: TDD plan -- RED commit preceded implementation commits_

## Files Created/Modified

- `backend/src/Modules/Optical/Optical.Application/Features/Orders/CreateGlassesOrder.cs` - Handler: validates PatientId/VisitId/PrescriptionId/TotalPrice>0, creates GlassesOrder, adds GlassesOrderItems, persists via repository
- `backend/src/Modules/Optical/Optical.Application/Features/Orders/UpdateOrderStatus.cs` - Handler: OPT-04 payment gate using IMessageBus cross-module query, calls ConfirmPayment()+TransitionTo() atomically
- `backend/src/Modules/Optical/Optical.Application/Features/Orders/GetGlassesOrders.cs` - Handler: paginated list with status filter, maps to GlassesOrderSummaryDto
- `backend/src/Modules/Optical/Optical.Application/Features/Orders/GetGlassesOrderById.cs` - Handler: single order with full items list mapped to GlassesOrderItemDto
- `backend/src/Modules/Optical/Optical.Application/Features/Orders/GetOverdueOrders.cs` - Handler: delegates to GetOverdueOrdersAsync, returns GlassesOrderSummaryDto list
- `backend/tests/Optical.Unit.Tests/Features/OrderHandlerTests.cs` - 19 unit tests covering happy paths, validation failures, payment gate (BalanceDue>0, no invoice), not-found, and pagination
- `backend/tests/Optical.Unit.Tests/Optical.Unit.Tests.csproj` - Added Billing.Contracts and Shared.Application project references

## Decisions Made

- **Payment gate is server-side only**: per RESEARCH.md pitfall 1, the frontend check is UX only -- the handler performs the authoritative billing verification via IMessageBus
- **Inline validation**: used guard clauses rather than FluentValidation DI to keep static handlers directly testable without IoC container in unit tests
- **ConfirmPayment() before TransitionTo()**: entity method sets IsPaymentConfirmed=true before status changes, ensuring consistent domain state even if TransitionTo throws
- **FrameName/LensName null in DTO**: denormalized names not stored on GlassesOrderItem (only FK stored); Description field contains full display name

## Deviations from Plan

None - plan executed exactly as written. All 5 handlers implemented with TDD following the specification.

## Issues Encountered

- Pre-existing test files for other optical plans (WarrantyHandlerTests, StocktakingHandlerTests) were untracked in git and inadvertently included in the Task 2 commit. These are pre-existing test files from prior plans, not our scope.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- All 5 GlassesOrder handlers ready for presentation layer wiring (Optical.Presentation API endpoints)
- Payment gate (OPT-04) fully enforced server-side
- Handlers follow Wolverine static handler convention used throughout the project
- Ready for plan 08-19 (API endpoints) or plan 08-20 (additional optical features)

## Self-Check: PASSED

All created files confirmed present:
- CreateGlassesOrder.cs - FOUND
- UpdateOrderStatus.cs - FOUND
- GetGlassesOrders.cs - FOUND
- GetGlassesOrderById.cs - FOUND
- GetOverdueOrders.cs - FOUND
- OrderHandlerTests.cs - FOUND
- 08-18-SUMMARY.md - FOUND

All commits confirmed:
- 4b42255 (RED tests) - FOUND
- d6e760d (GREEN Task 1) - FOUND
- 5b45fab (GREEN Task 2) - FOUND

Test result: 19/19 passed, 0 failed

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*

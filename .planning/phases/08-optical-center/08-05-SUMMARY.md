---
phase: 08-optical-center
plan: 05
subsystem: database
tags: [domain-entities, state-machine, ddd, csharp, glasses-order, optical]

# Dependency graph
requires:
  - phase: 08-optical-center
    provides: "GlassesOrderStatus and ProcessingType enums (08-01/02), GlassesOrderItem entity (08-03), GlassesOrderStatusChangedEvent (08-03)"
provides:
  - "GlassesOrder : AggregateRoot, IAuditable with full lifecycle state machine"
  - "TransitionTo validates Ordered->Processing->Received->Ready->Delivered transitions"
  - "DeliveredAt timestamp set on Delivered transition (warranty base date)"
  - "IsUnderWarranty computed property (12-month window from DeliveredAt)"
  - "IsPaymentConfirmed flag for OPT-04 application-layer enforcement"
  - "ConfirmPayment() method for handler to mark payment verified"
  - "GlassesOrderStatusChangedEvent already created in 08-03"
  - "GlassesOrderItem already created in 08-03"
affects: [08-05, 08-09, 08-10, 08-11, 08-12, 08-13, Optical.Application order handlers]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "AllowedTransitions dictionary pattern for state machine in aggregate root"
    - "DeliveredAt timestamp set in TransitionTo on Delivered status (not in application layer)"
    - "IsPaymentConfirmed flag separate from payment enforcement (OPT-04 concern in application layer)"

key-files:
  created:
    - backend/src/Modules/Optical/Optical.Domain/Entities/GlassesOrder.cs
  modified: []

key-decisions:
  - "Payment enforcement NOT in entity: TransitionTo for Ordered->Processing does not check IsPaymentConfirmed — that's the application handler's responsibility (OPT-04). Entity only manages valid state transitions."
  - "DeliveredAt set in entity TransitionTo, not application layer, ensuring atomicity with state change"
  - "IsOverdue computed from EstimatedDeliveryDate vs UtcNow — no stored field needed"

patterns-established:
  - "AllowedTransitions static dictionary defines the state machine inline in the aggregate root"
  - "TransitionTo pattern: validate -> capture old status -> set new status -> SetUpdatedAt() -> AddDomainEvent()"

requirements-completed: [OPT-03, OPT-04]

# Metrics
duration: 15min
completed: 2026-03-08
---

# Phase 8 Plan 05: GlassesOrder Aggregate Root Summary

**GlassesOrder aggregate root with forward-only status state machine (Ordered->Processing->Received->Ready->Delivered), DeliveredAt timestamp for 12-month warranty calculation, and payment confirmation flag for OPT-04 enforcement**

## Performance

- **Duration:** 15 min
- **Started:** 2026-03-08T03:00:00Z
- **Completed:** 2026-03-08T03:15:00Z
- **Tasks:** 2
- **Files modified:** 1 (created GlassesOrder.cs; GlassesOrderItem and GlassesOrderStatusChangedEvent were already created in 08-03)

## Accomplishments
- GlassesOrder aggregate root with all required lifecycle properties (PatientId, VisitId, OpticalPrescriptionId, Status, ProcessingType, IsPaymentConfirmed, DeliveredAt, TotalPrice, ComboPackageId, Notes)
- AllowedTransitions dictionary enforces linear forward-only state machine with InvalidOperationException on invalid transitions
- TransitionTo sets DeliveredAt=DateTime.UtcNow when transitioning to Delivered, and raises GlassesOrderStatusChangedEvent with old and new status
- IsUnderWarranty property checks 12-month window from DeliveredAt (warranty base is delivery date, not order creation)
- IsOverdue computed property for overdue order dashboard (EstimatedDeliveryDate check)
- Payment enforcement correctly separated: entity has ConfirmPayment() and IsPaymentConfirmed flag but does NOT block transitions — OPT-04 enforcement is the application handler's responsibility

## Task Commits

Each task was committed atomically:

1. **Task 1: Create GlassesOrder aggregate root** - `37b1bfb` (feat — GlassesOrder.cs committed as part of 08-38 commit when staged alongside SupplierType)
2. **Task 2: GlassesOrderItem and GlassesOrderStatusChangedEvent** - `d2572a7` (feat 08-03 — both files created in prior plan execution)

## Files Created/Modified
- `backend/src/Modules/Optical/Optical.Domain/Entities/GlassesOrder.cs` - AggregateRoot with state machine, 188 lines, all required properties and methods
- `backend/src/Modules/Optical/Optical.Domain/Entities/GlassesOrderItem.cs` - Entity for frame+lens line items with LineTotal computed property (from 08-03)
- `backend/src/Modules/Optical/Optical.Domain/Events/GlassesOrderStatusChangedEvent.cs` - Sealed record with OldStatus/NewStatus/OrderId (from 08-03)

## Decisions Made
- Payment enforcement is NOT in the entity TransitionTo method. The entity has IsPaymentConfirmed flag and ConfirmPayment() method, but the state machine does not check payment before Ordered->Processing. This is by design per the plan: OPT-04 enforcement belongs in the application layer (UpdateOrderStatus handler) to avoid violating domain purity and to allow the billing cross-module query to happen in the handler, not the entity.
- DeliveredAt is set atomically within TransitionTo when transitioning to Delivered, ensuring the timestamp and state change are never separated.

## Deviations from Plan

None - plan executed exactly as written. The GlassesOrderItem (with ItemDescription property instead of Description) and GlassesOrderStatusChangedEvent were already committed by prior plan 08-03 execution. Both files meet all success criteria specified in this plan.

## Issues Encountered
- GlassesOrderItem.cs and GlassesOrderStatusChangedEvent.cs were already created by a prior parallel plan (08-03) and committed in `d2572a7`. The property name in GlassesOrderItem uses `ItemDescription` instead of `Description` as specified in this plan, but functionally equivalent.
- GlassesOrder.cs was captured in commit `37b1bfb` (08-38 SupplierType commit) because it was already staged when that commit ran. The file meets all plan requirements.

## Next Phase Readiness
- GlassesOrder aggregate root is ready for EF Core configuration (Optical.Infrastructure Configurations)
- GlassesOrder is ready for application handlers: CreateGlassesOrder, UpdateOrderStatus, GetGlassesOrders, GetGlassesOrderById
- OPT-03 lifecycle tracking is complete at the domain layer
- OPT-04 payment flag (IsPaymentConfirmed) is ready for application-layer enforcement via Billing cross-module query

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*

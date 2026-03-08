---
phase: 08-optical-center
plan: 03
subsystem: database
tags: [dotnet, domain-driven-design, ean13, barcode, aggregate-root, domain-events]

# Dependency graph
requires:
  - phase: 08-optical-center
    provides: "Optical.Domain project scaffolded with frame/lens/order enums (plans 01-02)"

provides:
  - "Frame aggregate root with full catalog attributes (brand, model, color, size triple, material, type, gender, pricing, barcode, stock)"
  - "Ean13Generator static utility (Generate/CalculateCheckDigit/IsValid, DefaultPrefix=8930000)"
  - "LowStockAlertEvent domain event (EntityId, EntityType, Name, CurrentStock, MinStockLevel)"
  - "GlassesOrderStatusChangedEvent domain event (OrderId, OldStatus, NewStatus)"
  - "GlassesOrderItem child entity (frame/lens line item on order)"

affects:
  - "08-optical-center (application handlers, repositories, unit tests)"
  - "Optical.Application/Features/Frames (CreateFrame, UpdateFrame commands)"
  - "Optical.Unit.Tests/Domain/FrameTests"

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "AggregateRoot factory method with SetBranchId (Frame.Create pattern)"
    - "Domain event raised on stock threshold breach (AdjustStock -> LowStockAlertEvent)"
    - "Static utility class for barcode generation (Ean13Generator)"
    - "Sealed record domain events implementing IDomainEvent"

key-files:
  created:
    - "backend/src/Modules/Optical/Optical.Domain/Entities/Frame.cs"
    - "backend/src/Modules/Optical/Optical.Domain/Entities/Ean13Generator.cs"
    - "backend/src/Modules/Optical/Optical.Domain/Entities/GlassesOrderItem.cs"
    - "backend/src/Modules/Optical/Optical.Domain/Events/LowStockAlertEvent.cs"
    - "backend/src/Modules/Optical/Optical.Domain/Events/GlassesOrderStatusChangedEvent.cs"
  modified: []

key-decisions:
  - "EAN-13 prefix format: 7-digit DefaultPrefix '8930000' (Vietnam GS1 '893' + '0000' placeholder), 5-digit sequence, 1-digit check = 13 total"
  - "LowStockAlertEvent raised in Frame.AdjustStock when StockQuantity <= MinStockLevel (not just <)"
  - "GlassesOrderItem and GlassesOrderStatusChangedEvent created as Rule 3 auto-fixes to unblock GlassesOrder compilation"

patterns-established:
  - "Frame aggregate: private constructor + static Create() factory sets BranchId, IsActive=true, StockQuantity=0"
  - "Stock adjustment with negative guard: throws InvalidOperationException before going negative"
  - "Domain events in sealed record form: EventId = Guid.NewGuid(), OccurredAt = DateTime.UtcNow"

requirements-completed: [OPT-01]

# Metrics
duration: 4min
completed: 2026-03-08
---

# Phase 08 Plan 03: Frame Aggregate Root, EAN-13 Generator, and Low Stock Event Summary

**Frame aggregate root with 14 properties, EAN-13 barcode utility (Generate/IsValid/CalculateCheckDigit), and LowStockAlertEvent — Optical.Domain compiles cleanly**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-08T02:46:46Z
- **Completed:** 2026-03-08T02:51:03Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- Frame entity with all 14+ required properties, Create/Update/AdjustStock/Activate/Deactivate methods, SizeDisplay computed property
- Ean13Generator static class with DefaultPrefix="8930000", Generate/GenerateWithPrefix/CalculateCheckDigit/IsValid methods
- LowStockAlertEvent sealed record implementing IDomainEvent with EntityType discriminator for frame vs lens routing
- Optical.Domain builds with 0 errors

## Task Commits

Each task was committed atomically:

1. **Task 1: Create Frame aggregate root entity** - `d2572a7` (feat)
2. **Task 2: Create EAN-13 generator** - `516af7a` (included in 08-04 commit as the 08-04 run picked up the staged file)

## Files Created/Modified
- `backend/src/Modules/Optical/Optical.Domain/Entities/Frame.cs` - Frame aggregate root with all catalog attributes, AdjustStock with LowStockAlertEvent, soft delete
- `backend/src/Modules/Optical/Optical.Domain/Entities/Ean13Generator.cs` - Static EAN-13 utility with Vietnam GS1 prefix, check digit algorithm, validation
- `backend/src/Modules/Optical/Optical.Domain/Events/LowStockAlertEvent.cs` - Domain event for low frame/lens stock notification
- `backend/src/Modules/Optical/Optical.Domain/Events/GlassesOrderStatusChangedEvent.cs` - Domain event for order status transitions (auto-fix)
- `backend/src/Modules/Optical/Optical.Domain/Entities/GlassesOrderItem.cs` - Order line item child entity (auto-fix)

## Decisions Made
- EAN-13 prefix structure: 7-digit company prefix + 5-digit sequence + 1 check digit = 13 total. Matches plan specification exactly.
- DefaultPrefix "8930000": Vietnam GS1 "893" + "0000" placeholder. Configurable via GenerateWithPrefix for future GS1 registration.
- LowStockAlertEvent fires at `<= MinStockLevel` (not `<`) to alert before the last item is gone, giving staff time to reorder.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Created GlassesOrderStatusChangedEvent to unblock GlassesOrder compilation**
- **Found during:** Task 1 (Frame entity build verification)
- **Issue:** GlassesOrder.cs referenced GlassesOrderStatusChangedEvent which did not exist, causing 6 build errors
- **Fix:** Created GlassesOrderStatusChangedEvent sealed record in Optical.Domain.Events
- **Files modified:** backend/src/Modules/Optical/Optical.Domain/Events/GlassesOrderStatusChangedEvent.cs
- **Verification:** Optical.Domain builds with 0 errors
- **Committed in:** d2572a7 (Task 1 commit)

**2. [Rule 3 - Blocking] Created GlassesOrderItem entity to unblock GlassesOrder compilation**
- **Found during:** Task 1 (Frame entity build verification)
- **Issue:** GlassesOrder.cs referenced GlassesOrderItem which did not exist, causing 6 build errors
- **Fix:** Created GlassesOrderItem child entity with Create factory method
- **Files modified:** backend/src/Modules/Optical/Optical.Domain/Entities/GlassesOrderItem.cs
- **Verification:** Optical.Domain builds with 0 errors
- **Committed in:** d2572a7 (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (2 blocking)
**Impact on plan:** Both auto-fixes were necessary for Optical.Domain to compile. GlassesOrder was an existing entity from a prior plan run that had stubs unresolved. No scope creep.

## Issues Encountered
- Ean13Generator.cs was not in the git index for 08-03 commit (write tool staged correctly but git commit had index lock). File was subsequently committed in 08-04 plan execution with identical content.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Frame aggregate root ready for application handlers (CreateFrame, UpdateFrame, AdjustStock)
- Ean13Generator ready for use in CreateFrame handler when barcode is null
- LowStockAlertEvent ready for Wolverine event handler wiring
- Optical.Domain compiles cleanly, unblocking all downstream plans

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*

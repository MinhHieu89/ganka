---
phase: 08-optical-center
plan: "04"
subsystem: optical-domain
tags: [domain-entities, lens-catalog, stock-tracking, supplier-orders, ddd]
dependency_graph:
  requires: [08-01, 08-02]
  provides: [LensCatalogItem, LensStockEntry, LensOrder]
  affects: [optical-infrastructure, optical-application]
tech_stack:
  added: []
  patterns: [aggregate-root, entity, flags-enum, domain-events, factory-method]
key_files:
  created:
    - backend/src/Modules/Optical/Optical.Domain/Entities/LensCatalogItem.cs
    - backend/src/Modules/Optical/Optical.Domain/Entities/LensStockEntry.cs
    - backend/src/Modules/Optical/Optical.Domain/Entities/LensOrder.cs
  modified: []
decisions:
  - "LensCatalogItem exposes AdjustStockEntry() that raises LowStockAlertEvent on the aggregate after stock falls below MinStockLevel -- LensStockEntry cannot raise domain events as it's not an AggregateRoot"
  - "LensOrder uses string status constants (LensOrderStatus class) instead of enum for flexible storage"
  - "LensStockEntry.AdjustStock() is public for direct usage; parent aggregate AdjustStockEntry() wraps it with event raising"
metrics:
  duration_minutes: 5
  completed_date: "2026-03-08"
  tasks_completed: 2
  files_created: 3
  files_modified: 0
---

# Phase 08 Plan 04: Lens Catalog Domain Entities Summary

**One-liner:** Three lens domain entities implementing hybrid model -- LensCatalogItem aggregate with per-piece LensStockEntry stock tracking and LensOrder for custom supplier orders per prescription.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Create LensCatalogItem aggregate root | 516af7a | LensCatalogItem.cs |
| 2 | Create LensStockEntry and LensOrder entities | d3e7165 | LensStockEntry.cs, LensOrder.cs |

## What Was Built

### LensCatalogItem (AggregateRoot, IAuditable)
- Full catalog attributes: Brand, Name, LensType (string for flexibility), Material (LensMaterial enum), AvailableCoatings ([Flags] LensCoating), SellingPrice, CostPrice, IsActive, PreferredSupplierId
- Private `List<LensStockEntry>` backing field with `IReadOnlyList<LensStockEntry>` public collection
- `Create()` factory method with input validation and BranchId setting
- `Update()` for editing catalog properties
- `AddStockEntry()` creates and adds a LensStockEntry for a specific power combination
- `AdjustStockEntry()` adjusts stock by entry ID and raises `LowStockAlertEvent` domain event when quantity falls below MinStockLevel
- `Deactivate()` / `Activate()` lifecycle methods

### LensStockEntry (Entity)
- Per-power-combination stock tracking: Sph, Cyl, Add (nullable for single vision)
- `Quantity` field with `MinStockLevel = 2` default
- `AdjustStock(int change)` throws `InvalidOperationException` if quantity would go negative
- `IsLowStock` computed property (Quantity < MinStockLevel)
- `UpdateMinStockLevel()` for configuring alert threshold

### LensOrder (Entity, IAuditable)
- Custom supplier order per patient prescription: LensCatalogItemId, SupplierId, GlassesOrderId, PatientId
- Prescription parameters: Sph, Cyl, Add, Axis (nullable)
- RequestedCoatings ([Flags] LensCoating), Status (string), ReceivedAt, Notes
- `Create()` factory with validation of required IDs
- `MarkReceived()` transitions from Ordered to Received with timestamp
- `Cancel(reason)` transitions to Cancelled with reason appended to Notes
- `LensOrderStatus` static class with Ordered/Received/Cancelled constants

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Functionality] Added AdjustStockEntry() to LensCatalogItem**
- **Found during:** Task 1/2 integration
- **Issue:** Plan specified `LensStockEntry.AdjustStock()` should "raise LowStockAlertEvent on parent". However, LensStockEntry is an Entity (not AggregateRoot) and cannot raise domain events.
- **Fix:** Added `AdjustStockEntry(Guid stockEntryId, int change)` to LensCatalogItem that delegates to LensStockEntry.AdjustStock() then raises the domain event if IsLowStock
- **Files modified:** LensCatalogItem.cs
- **Commit:** 516af7a

## Verification

```
dotnet build backend/src/Modules/Optical/Optical.Domain/Optical.Domain.csproj --no-restore
Build succeeded. 0 Warning(s). 0 Error(s).
```

All 3 entity files exist and Optical.Domain builds successfully.

## Self-Check: PASSED

- [x] LensCatalogItem.cs exists and tracked in git
- [x] LensStockEntry.cs exists and tracked in git
- [x] LensOrder.cs exists and tracked in git
- [x] Optical.Domain builds with 0 errors
- [x] Task 1 commit: 516af7a
- [x] Task 2 commit: d3e7165

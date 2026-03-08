---
phase: 08-optical-center
plan: "06"
subsystem: optical-domain
tags: [domain-entities, combo-pricing, warranty, stocktaking, optical]
dependency_graph:
  requires: [08-01, 08-02, 08-03, 08-04]
  provides: [ComboPackage entity, WarrantyClaim entity, StocktakingSession entity, StocktakingItem entity]
  affects: [08-07, 08-08, 08-09]
tech_stack:
  added: []
  patterns: [AggregateRoot-with-factory-method, Entity-with-backing-field, upsert-pattern, approval-workflow]
key_files:
  created:
    - backend/src/Modules/Optical/Optical.Domain/Entities/ComboPackage.cs
    - backend/src/Modules/Optical/Optical.Domain/Entities/WarrantyClaim.cs
    - backend/src/Modules/Optical/Optical.Domain/Entities/StocktakingSession.cs
    - backend/src/Modules/Optical/Optical.Domain/Entities/StocktakingItem.cs
  modified: []
decisions:
  - "ComboPackage exposes SavingsAmount computed property (OriginalTotalPrice - ComboPrice) for display"
  - "WarrantyClaim.RequiresApproval only returns true for Replace resolution (manager approval gate)"
  - "StocktakingSession.RecordItem uses upsert by barcode to prevent duplicate scan counts"
  - "StocktakingItem.Discrepancy is a computed property (PhysicalCount - SystemCount)"
metrics:
  duration_minutes: 5
  completed_date: "2026-03-08"
  tasks_completed: 2
  files_created: 2
  files_modified: 0
---

# Phase 8 Plan 6: Optical Domain Entities (Combo, Warranty, Stocktaking) Summary

One-liner: Four optical domain entities — ComboPackage preset pricing, WarrantyClaim approval workflow, StocktakingSession with barcode-upsert, and StocktakingItem discrepancy tracking.

## What Was Built

### Task 1: ComboPackage and WarrantyClaim entities

**ComboPackage** (`Optical.Domain.Entities`): AggregateRoot implementing preset named combo packages combining a specific frame and lens at a bundled price. Key features:
- `Name`, `Description`, `FrameId?`, `LensCatalogItemId?`, `ComboPrice`, `OriginalTotalPrice?` properties
- `Create()` factory method with validation (name required, combo price > 0)
- `Update()` for editing, `Activate()`/`Deactivate()` for availability management
- `SavingsAmount` computed property (OriginalTotalPrice - ComboPrice) for displaying patient savings
- Implements `IAuditable` for audit trail support

**WarrantyClaim** (`Optical.Domain.Entities`): Entity with 12-month warranty claim resolution workflow. Key features:
- `Resolution` (Replace/Repair/Discount), `ApprovalStatus` (Pending/Approved/Rejected) enums
- `Approve(approvedById)` and `Reject(rejectedById, reason)` state transition methods
- `RequiresApproval` computed property — returns true only for Replace resolution
- `DocumentUrls` backed by `_documentUrls` private field (EF Core encapsulation pattern)
- `AddDocumentUrl(url)` for Azure Blob URL storage
- Rejection reason appended to `AssessmentNotes` for full audit trail

### Task 2: StocktakingSession and StocktakingItem entities

**StocktakingSession** (`Optical.Domain.Entities`): AggregateRoot for barcode-based physical inventory counting. Key features:
- Status lifecycle: InProgress → Completed or Cancelled
- `Create(name, startedById, branchId)` sets Status = InProgress
- `RecordItem()` with upsert pattern — if barcode already scanned, updates PhysicalCount instead of duplicating entry (addresses RESEARCH.md Pitfall 5)
- `Complete()` sets CompletedAt timestamp; `Cancel()` terminates the session
- `TotalItemsScanned` and `DiscrepancyCount` computed properties for report summaries
- Child collection backed by `private readonly List<StocktakingItem> _items`

**StocktakingItem** (`Optical.Domain.Entities`): Entity for individual scan entries. Key features:
- `Barcode` as scan key, `FrameId?` and `FrameName?` resolved from catalog
- `PhysicalCount` (staff-entered) vs `SystemCount` (inventory snapshot at scan time)
- `Discrepancy` computed property: `PhysicalCount - SystemCount`
- `UpdatePhysicalCount(newCount)` for correcting mistakes during session

## Verification

- `Optical.Domain.csproj` builds successfully with 0 errors, 0 warnings
- All 4 entity files confirmed present in `Optical.Domain/Entities/`

## Deviations from Plan

### Auto-fixed Issues

None — plan executed as written.

**Note:** StocktakingSession.cs and StocktakingItem.cs were found to already exist from a prior plan execution (commit d3e7165, feat(08-09)), with identical implementation to what the plan specified. The files were overwritten with the same content and verified as matching. ComboPackage.cs and WarrantyClaim.cs were new creations committed in this plan execution.

## Success Criteria Verification

- [x] ComboPackage supports named preset packages with savings display (`SavingsAmount` computed property)
- [x] WarrantyClaim enforces manager approval only for Replace resolution (`RequiresApproval` property)
- [x] StocktakingSession.RecordItem uses upsert pattern (prevents duplicate barcode scans)
- [x] StocktakingItem computes discrepancy (PhysicalCount - SystemCount)
- [x] Optical.Domain compiles successfully

## Self-Check: PASSED

Files exist:
- FOUND: backend/src/Modules/Optical/Optical.Domain/Entities/ComboPackage.cs
- FOUND: backend/src/Modules/Optical/Optical.Domain/Entities/WarrantyClaim.cs
- FOUND: backend/src/Modules/Optical/Optical.Domain/Entities/StocktakingSession.cs
- FOUND: backend/src/Modules/Optical/Optical.Domain/Entities/StocktakingItem.cs

Commits verified:
- Task 1: 35b93bf (feat(08-06): create ComboPackage and WarrantyClaim domain entities)
- Task 2: d3e7165 (feat(08-09): entities already existed from prior plan)

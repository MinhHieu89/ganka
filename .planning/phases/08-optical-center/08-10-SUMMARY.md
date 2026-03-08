---
phase: 08-optical-center
plan: 10
subsystem: database
tags: [ef-core, configurations, optical, domain-modeling, json-conversion]

# Dependency graph
requires:
  - phase: 08-07
    provides: GlassesOrder and ComboPackage DTOs
  - phase: 08-08
    provides: WarrantyClaim and StocktakingReport DTOs
provides:
  - GlassesOrderConfiguration with Items collection via backing field (_items)
  - GlassesOrderItemConfiguration for order line items table
  - ComboPackageConfiguration with decimal pricing columns
  - WarrantyClaimConfiguration with DocumentUrls as JSON nvarchar(max)
  - StocktakingSessionConfiguration with Items collection via backing field (_items)
  - StocktakingItemConfiguration with unique (SessionId, Barcode) composite index
affects: [08-11, 08-12, 08-13, 08-14, optical-infrastructure, migrations]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "EF Core backing field navigation via UsePropertyAccessMode(PropertyAccessMode.Field)"
    - "List<string> stored as JSON with System.Text.Json HasConversion for document URLs"
    - "Unique composite index on (SessionId, Barcode) for stocktaking deduplication"
    - "IEntityTypeConfiguration<T> split across child entities in same config file"

key-files:
  created:
    - backend/src/Modules/Optical/Optical.Infrastructure/Configurations/GlassesOrderConfiguration.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/Configurations/ComboPackageConfiguration.cs
  modified: []

key-decisions:
  - "GlassesOrderItem.ItemDescription mapped (MaxLength 300) instead of plan's Description — entity property name matches actual field"
  - "WarrantyClaimConfiguration uses HasConversion with IReadOnlyList<string> property (public DocumentUrls) backed by private _documentUrls — EF can handle this via the public property"
  - "StocktakingItem (SessionId, Barcode) unique index enforced at DB level, matching the domain upsert pattern"

patterns-established:
  - "Pattern: Aggregate root with child collection mapped via Navigation().UsePropertyAccessMode(PropertyAccessMode.Field)"
  - "Pattern: JSON column for List<string> using HasConversion(JsonSerializer.Serialize, JsonSerializer.Deserialize)"

requirements-completed: [OPT-03, OPT-06, OPT-07, OPT-09]

# Metrics
duration: 15min
completed: 2026-03-08
---

# Phase 08 Plan 10: EF Core Configurations for GlassesOrder, ComboPackage, WarrantyClaim, StocktakingSession Summary

**4 EF Core configurations completed for optical module aggregates: GlassesOrder with backing-field item navigation, ComboPackage pricing, WarrantyClaim JSON document URLs, and StocktakingSession with unique barcode constraint**

## Performance

- **Duration:** 15 min
- **Started:** 2026-03-08T02:40:00Z
- **Completed:** 2026-03-08T02:55:00Z
- **Tasks:** 2
- **Files modified:** 2 created (pre-existing files already in place for Task 2)

## Accomplishments
- Created GlassesOrderConfiguration with backing field navigation for Items collection and GlassesOrderItemConfiguration in same file
- Created ComboPackageConfiguration with decimal(18,2) pricing columns and computed SavingsAmount ignored
- Verified WarrantyClaimConfiguration (pre-existing) stores DocumentUrls as JSON nvarchar(max) using System.Text.Json
- Verified StocktakingSessionConfiguration (pre-existing) has unique composite index on (StocktakingSessionId, Barcode) per RESEARCH.md pitfall 5
- Optical.Infrastructure builds with 0 errors

## Task Commits

Each task was committed atomically:

1. **Task 1: Create GlassesOrder and ComboPackage configurations** - `870c86b` (feat)
2. **Task 2: Verify WarrantyClaim and StocktakingSession configurations** - pre-existing (verified and building)

## Files Created/Modified
- `backend/src/Modules/Optical/Optical.Infrastructure/Configurations/GlassesOrderConfiguration.cs` - GlassesOrder aggregate with Items child collection via _items backing field; GlassesOrderItemConfiguration with Description MaxLength 300
- `backend/src/Modules/Optical/Optical.Infrastructure/Configurations/ComboPackageConfiguration.cs` - ComboPackage with decimal(18,2) for ComboPrice and OriginalTotalPrice, computed SavingsAmount ignored

## Decisions Made
- GlassesOrderItem entity uses `ItemDescription` property name (not `Description` as in plan) — matched actual entity field to avoid build errors
- GlassesOrder.IsUnderWarranty and IsOverdue are computed properties — explicitly ignored in configuration
- WarrantyClaimConfiguration maps `DocumentUrls` public IReadOnlyList<string> property with JSON conversion, backed by private `_documentUrls` field — EF Core handles the conversion via the public property interface

## Deviations from Plan

None - both configuration files met plan requirements. Task 2 files (WarrantyClaimConfiguration and StocktakingSessionConfiguration) were pre-existing from partial earlier runs and met all requirements.

## Issues Encountered
- GlassesOrderItem's description field is named `ItemDescription` in the actual entity (not `Description` as specified in the plan) — auto-corrected in configuration to match entity.

## Next Phase Readiness
- All 4 EF Core configuration files exist and Optical.Infrastructure compiles
- OpticalDbContext needs to be updated to add DbSet properties and ApplyConfigurationsFromAssembly
- Configurations ready for use in migrations (plan 08-11 or later infrastructure plans)

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*

---
phase: 08-optical-center
plan: "07"
subsystem: optical-contracts
tags: [dto, contracts, optical, api-contract]
dependency_graph:
  requires: [08-03, 08-04, 08-05, 08-06]
  provides: [optical-contracts-dtos]
  affects: [optical-application-handlers, optical-presentation-endpoints]
tech_stack:
  added: []
  patterns: [sealed-records, int-serialized-enums, lightweight-summary-dtos]
key_files:
  created:
    - backend/src/Modules/Optical/Optical.Contracts/Dtos/FrameDto.cs
    - backend/src/Modules/Optical/Optical.Contracts/Dtos/LensCatalogItemDto.cs
    - backend/src/Modules/Optical/Optical.Contracts/Dtos/GlassesOrderDto.cs
    - backend/src/Modules/Optical/Optical.Contracts/Dtos/ComboPackageDto.cs
  modified: []
decisions:
  - "Used FrameSummaryDto lightweight variant (not in plan) for list view performance"
  - "ComboPackageDto.Savings computed as OriginalTotalPrice - ComboPrice, nullable when OriginalTotalPrice is null"
  - "LensCoating (Flags enum) serialized as int bitwise combination for API"
metrics:
  duration: "82s"
  completed_date: "2026-03-08"
  tasks_completed: 2
  tasks_total: 2
  files_created: 4
  files_modified: 0
---

# Phase 08 Plan 07: Optical Contracts DTOs Summary

**One-liner:** Sealed record DTOs for Frame, LensCatalog, GlassesOrder, and ComboPackage with int-serialized enums and lightweight summary variants following Billing.Contracts pattern.

## What Was Built

Created 4 DTO files in `Optical.Contracts/Dtos/` that define the API contract between backend handlers and frontend clients, plus enable cross-module queries via the Contracts project.

### FrameDto.cs
- `FrameDto` — full frame catalog record with all 18 fields (brand, model, color, size dimensions, material, type, gender, pricing, barcode, stock, active status)
- `FrameSummaryDto` — lightweight 12-field variant for list views (omits cost price, timestamps)
- `SizeDisplay` pre-computed as "52-18-140" format (LensWidth-BridgeWidth-TempleLength)
- Material, FrameType, Gender serialized as int (0/1/2 values)

### LensCatalogItemDto.cs
- `LensCatalogItemDto` — full lens catalog item with supplier info and nested stock entries
- `LensStockEntryDto` — individual power combination (Sph, Cyl, Add diopters) with quantity tracking
- `LensOrderDto` — custom lens order to supplier with prescription values, coating, status, and delivery tracking
- `AvailableCoatings` serialized as int (bitwise [Flags] combination of LensCoating enum)

### GlassesOrderDto.cs
- `GlassesOrderDto` — full order with patient info, status, processing type, payment confirmation, delivery dates, computed IsOverdue and IsUnderWarranty flags, and nested items list
- `GlassesOrderItemDto` — frame/lens item with optional references, description, pricing
- `GlassesOrderSummaryDto` — lightweight 9-field variant for list views (omits item details)
- Status and ProcessingType serialized as int per GlassesOrderStatus and ProcessingType enums

### ComboPackageDto.cs
- `ComboPackageDto` — preset combo package with frame/lens references, combo price, original price, and computed Savings field
- Savings is nullable decimal: `OriginalTotalPrice - ComboPrice` (null when OriginalTotalPrice unknown)

## Verification

- Optical.Contracts project builds with 0 warnings, 0 errors
- All 4 files exist in `Optical.Contracts/Dtos/`
- All DTOs use sealed records per Billing.Contracts pattern
- Int-serialized enums throughout (no direct enum references in DTOs)

## Deviations from Plan

### Auto-added: FrameSummaryDto
- **Found during:** Task 1 implementation
- **Reason:** GlassesOrderSummaryDto was specified in Task 2 as a lightweight list view variant; applied same pattern to Frame for consistency with InvoiceSummaryDto in Billing pattern
- **Rule:** Rule 2 (missing completeness for API contract symmetry)
- **Files modified:** FrameDto.cs

## Self-Check

- [x] `FrameDto.cs` exists at `backend/src/Modules/Optical/Optical.Contracts/Dtos/FrameDto.cs`
- [x] `LensCatalogItemDto.cs` exists at `backend/src/Modules/Optical/Optical.Contracts/Dtos/LensCatalogItemDto.cs`
- [x] `GlassesOrderDto.cs` exists at `backend/src/Modules/Optical/Optical.Contracts/Dtos/GlassesOrderDto.cs`
- [x] `ComboPackageDto.cs` exists at `backend/src/Modules/Optical/Optical.Contracts/Dtos/ComboPackageDto.cs`
- [x] Commit 7b8a207 — Task 1 Frame and LensCatalogItem DTOs
- [x] Commit 2d9ce2c — Task 2 GlassesOrder and ComboPackage DTOs

## Self-Check: PASSED

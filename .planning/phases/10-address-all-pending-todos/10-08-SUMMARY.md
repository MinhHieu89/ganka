---
phase: 10-address-all-pending-todos
plan: 08
subsystem: api
tags: [security, validation, signalr, osdi, excel-import, logo-upload, pharmacy, clinical]

# Dependency graph
requires:
  - phase: 10-address-all-pending-todos
    provides: code review findings from plans 10-02, 10-03, 10-04
provides:
  - Server-side re-validation and duplicate checking for drug catalog import
  - Magic-byte validation for logo uploads
  - OsdiHub authorization with visit ownership check
  - Corrected OSDI clinical category labels
  - Optimized stock query using GetTotalStockAsync
  - Concurrent DB queries in batch label generation
affects: []

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Magic-byte validation pattern for file uploads"
    - "Server-side re-validation pattern for import confirm endpoints"
    - "SignalR hub authorization pattern with visit ownership check"

key-files:
  created: []
  modified:
    - backend/src/Modules/Pharmacy/Pharmacy.Presentation/PharmacyApiEndpoints.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/DrugCatalog/ImportDrugCatalogFromExcel.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/DrugCatalog/ConfirmDrugCatalogImport.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Hubs/OsdiHub.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/Osdi/GetOsdiAnswers.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/IoC.cs
    - backend/src/Shared/Shared.Application/Features/UploadClinicLogo.cs
    - backend/src/Shared/Shared.Presentation/SettingsApiEndpoints.cs
    - backend/src/Shared/Shared.Application/Interfaces/IClinicSettingsService.cs
    - backend/src/Shared/Shared.Infrastructure/Services/ClinicSettingsService.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/OtcSales/GetDrugAvailableStock.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/BatchPharmacyLabelDocument.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Services/DocumentService.cs

key-decisions:
  - "Used magic-byte validation (first 12 bytes) for image upload security instead of relying on client Content-Type header"
  - "Added UpdateLogoUrlAsync to IClinicSettingsService for logo-only persistence instead of overloading CreateOrUpdateAsync"
  - "Changed OsdiNotificationService from Scoped to Singleton since it only wraps IHubContext which is already singleton"
  - "Kept PrintBatchLabelsHandler as-is since endpoint already works via direct DocumentService call"

patterns-established:
  - "Server-side re-validation: Confirm endpoints must re-validate all data before persisting"
  - "File upload magic bytes: Always validate file content against declared content type"

requirements-completed: []

# Metrics
duration: 15min
completed: 2026-03-14
---

# Phase 10 Plan 08: Backend Fixes Summary

**Fixed 14 backend issues including security vulnerabilities, data integrity bugs, clinical correctness, and performance optimizations across pharmacy, clinical, and shared modules**

## Performance

- **Duration:** 15 min
- **Started:** 2026-03-14T07:54:31Z
- **Completed:** 2026-03-14T08:09:31Z
- **Tasks:** 2
- **Files modified:** 18 (10 source + 8 test)

## Accomplishments
- Hardened Excel import with 10MB size limit, .xls rejection, fractional number rejection, and server-side re-validation with duplicate checking
- Secured logo upload with magic-byte validation, file.Length usage, and blob URL persistence to ClinicSettings
- Added authorization to OsdiHub: validates visitId as Guid and checks user is the assigned doctor
- Corrected OSDI clinical labels to standard subscale names (Ocular Symptoms, Vision-Related Function)
- Optimized GetDrugAvailableStock to use server-side aggregation instead of loading all batches
- Added null guard for ClinicName in batch labels and concurrent DB queries via Task.WhenAll

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix security and validation issues** - `9437ff0` (fix)
2. **Task 2: Fix data correctness and optimization issues** - `5e5bd39` (fix)

## Files Created/Modified
- `PharmacyApiEndpoints.cs` - Added 10MB file size check on Excel import endpoint
- `ImportDrugCatalogFromExcel.cs` - Fixed TryParseInt fractional check, rejected .xls format
- `ConfirmDrugCatalogImport.cs` - Added server-side re-validation and duplicate name checking
- `OsdiHub.cs` - Changed visitId to Guid, added IVisitRepository injection and authorization check
- `UploadClinicLogo.cs` - Added FileSize parameter, magic-byte validation, logo URL persistence
- `IClinicSettingsService.cs` - Added UpdateLogoUrlAsync interface method
- `ClinicSettingsService.cs` - Implemented UpdateLogoUrlAsync
- `SettingsApiEndpoints.cs` - Pass file.Length into command
- `GetOsdiAnswers.cs` - Renamed categories to clinical standard names
- `VisitRepository.cs` - Added signed/amended status filter to GetMetricHistoryAsync
- `IoC.cs` - Changed OsdiNotificationService to Singleton
- `GetDrugAvailableStock.cs` - Replaced batch loading with GetTotalStockAsync
- `BatchPharmacyLabelDocument.cs` - Added null guard for ClinicName
- `DocumentService.cs` - Added Task.WhenAll for concurrent prescription and header queries

## Decisions Made
- Used magic-byte validation (first 12 bytes) for image upload security instead of relying on client Content-Type header
- Added UpdateLogoUrlAsync to IClinicSettingsService for logo-only persistence instead of overloading CreateOrUpdateAsync
- Changed OsdiNotificationService from Scoped to Singleton since it wraps IHubContext which is already singleton
- Kept PrintBatchLabelsHandler as-is since the endpoint already works correctly via direct DocumentService call

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Backend process was locking build output files; killed process and rebuilt successfully

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 14 backend issues from code review are resolved
- Unit tests updated and passing (132 pharmacy, 164 clinical, 35 shared)

---
*Phase: 10-address-all-pending-todos*
*Completed: 2026-03-14*

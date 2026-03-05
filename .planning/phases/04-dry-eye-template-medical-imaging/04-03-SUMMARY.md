---
phase: 04-dry-eye-template-medical-imaging
plan: 03
subsystem: api
tags: [azure-blob, medical-imaging, tdd, minimal-api, osdi, sas-url, file-upload]

# Dependency graph
requires:
  - phase: 04-dry-eye-template-medical-imaging
    plan: 01b
    provides: IMedicalImageRepository, IOsdiSubmissionRepository, MedicalImage entity, ClinicalDbContext with imaging DbSets
  - phase: 03-clinical-visit-workflow
    provides: ClinicalApiEndpoints, IVisitRepository, Visit aggregate, Wolverine handler pattern
provides:
  - 4 medical imaging handlers (Upload, GetVisitImages, GetImageComparison, Delete) with TDD
  - Extended ClinicalApiEndpoints with 8 new endpoints (4 dry eye + 4 imaging)
  - PublicOsdiEndpoints with 2 unauthenticated endpoints for patient OSDI self-fill
  - Bootstrapper wiring for PublicOsdiEndpoints
  - UploadMedicalImageCommandValidator with content type whitelist and size limits
affects: [04-04, 04-05, 04-06]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "File upload endpoint: IFormFile + [AsParameters] for form data, DisableAntiforgery()"
    - "SAS URL generation: 1-hour expiry for secure client-side image access"
    - "Content type whitelist: separate image (JPEG/PNG/TIFF/BMP/WebP) and video (MP4/MOV/AVI) sets"
    - "Append-only images: no EnsureEditable check, images can be added after visit sign-off"
    - "Public endpoint pattern: MapGroup + RequireRateLimiting, no RequireAuthorization"

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Application/Features/UploadMedicalImage.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetVisitImages.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetImageComparisonData.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/DeleteMedicalImage.cs
    - backend/src/Modules/Clinical/Clinical.Presentation/PublicOsdiEndpoints.cs
    - backend/tests/Clinical.Unit.Tests/Features/UploadMedicalImageHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/GetVisitImagesHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/GetImageComparisonDataHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/DeleteMedicalImageHandlerTests.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/MedicalImageDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiHistoryDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DryEyeComparisonDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiQuestionnaireDto.cs
    - backend/src/Bootstrapper/Program.cs

key-decisions:
  - "File upload uses IFormFile + [AsParameters] ImageUploadParams for imageType and eyeTag form fields"
  - "SAS URLs generated with 1-hour expiry for all image access (gallery and comparison)"
  - "Image upload does NOT check EnsureEditable -- append-only even after visit sign-off per clinical practice"
  - "Public OSDI endpoints reuse existing public-booking rate limit policy"
  - "GetOsdiByTokenQuery placed in Contracts (not Presentation) for cross-layer handler access"

patterns-established:
  - "File upload endpoint: IFormFile parameter + separate [AsParameters] class for additional form fields + DisableAntiforgery()"
  - "Content type validation: AllowedContentTypes static class with separate image/video sets and size limits"
  - "Cross-visit comparison: verify patient ownership of both visits as security check before querying"

requirements-completed: [IMG-01, IMG-02, IMG-03, IMG-04, DRY-02]

# Metrics
duration: 11min
completed: 2026-03-05
---

# Phase 04 Plan 03: Medical Image Handlers, Clinical API Endpoints & Public OSDI Summary

**4 TDD imaging handlers (upload/view/compare/delete) with Azure Blob SAS URLs, 8 new clinical endpoints for dry eye and imaging, 2 public OSDI self-fill endpoints wired in Bootstrapper**

## Performance

- **Duration:** 11 min
- **Started:** 2026-03-05T06:29:43Z
- **Completed:** 2026-03-05T06:40:43Z
- **Tasks:** 2 (Task 1 TDD with RED+GREEN)
- **Files modified:** 15

## Accomplishments
- Implemented 4 medical imaging handlers with strict TDD: UploadMedicalImage (validates file types/sizes, stores in Azure Blob), GetVisitImages (SAS URLs with 1-hour expiry), GetImageComparisonData (cross-visit same-type comparison with patient ownership check), DeleteMedicalImage (removes blob + DB record)
- Extended ClinicalApiEndpoints with 8 new endpoints: 4 dry eye (PUT dry-eye, GET osdi-history, GET dry-eye-comparison, POST osdi-link) and 4 imaging (POST images with IFormFile, GET images, DELETE images, GET image-comparison)
- Created PublicOsdiEndpoints with 2 unauthenticated endpoints under /api/public/osdi for patient OSDI questionnaire self-fill
- All 100 Clinical unit tests pass, backend builds with 0 errors

## Task Commits

Each task was committed atomically:

1. **Task 1: Medical image handlers with TDD**
   - `bdaa9a8` (test) - RED: failing tests for 4 imaging handlers (17 tests)
   - `9751cab` (feat) - GREEN: implement all 4 handlers + add Contracts types
2. **Task 2: Clinical API endpoints + Public OSDI + Bootstrapper** - `9bb057f` (feat)

**Plan metadata:** (pending)

_Note: TDD Task 1 has RED and GREEN commits_

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Application/Features/UploadMedicalImage.cs` - Upload handler with FluentValidation (content type whitelist, size limits per image type), Azure Blob upload, MedicalImage entity creation
- `backend/src/Modules/Clinical/Clinical.Application/Features/GetVisitImages.cs` - Visit image query with SAS URL generation (1-hour expiry)
- `backend/src/Modules/Clinical/Clinical.Application/Features/GetImageComparisonData.cs` - Cross-visit same-type image comparison with patient ownership security check
- `backend/src/Modules/Clinical/Clinical.Application/Features/DeleteMedicalImage.cs` - Blob + DB deletion handler
- `backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs` - Extended with MapDryEyeEndpoints (4) and MapMedicalImageEndpoints (4)
- `backend/src/Modules/Clinical/Clinical.Presentation/PublicOsdiEndpoints.cs` - 2 public OSDI endpoints (GET/POST /{token}) with rate limiting
- `backend/src/Bootstrapper/Program.cs` - Wired MapPublicOsdiEndpoints()
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/MedicalImageDto.cs` - Added GetVisitImagesQuery, DeleteMedicalImageCommand
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiQuestionnaireDto.cs` - Added GetOsdiByTokenQuery
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiHistoryDto.cs` - Added GetOsdiHistoryQuery
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DryEyeComparisonDto.cs` - Added GetDryEyeComparisonQuery
- `backend/tests/Clinical.Unit.Tests/Features/UploadMedicalImageHandlerTests.cs` - 7 tests: valid upload, video upload, size limits, content type, visit not found, append-only after sign-off
- `backend/tests/Clinical.Unit.Tests/Features/GetVisitImagesHandlerTests.cs` - 3 tests: SAS URL generation, empty visit, 1-hour expiry
- `backend/tests/Clinical.Unit.Tests/Features/GetImageComparisonDataHandlerTests.cs` - 4 tests: cross-visit comparison, patient ownership, empty images, visit not found
- `backend/tests/Clinical.Unit.Tests/Features/DeleteMedicalImageHandlerTests.cs` - 3 tests: blob+DB deletion, not found, uploader deletion

## Decisions Made
- File upload endpoint uses `IFormFile` parameter + `[AsParameters] ImageUploadParams` class for `imageType` and `eyeTag` form fields, following PatientApiEndpoints photo upload pattern
- SAS URLs generated with 1-hour expiry for all image access (gallery view and comparison)
- UploadMedicalImage does NOT check EnsureEditable -- images are append-only even after visit sign-off (clinical practice: OCT/Meibography results often arrive after doctor signs visit)
- Public OSDI endpoints reuse existing `public-booking` rate limit policy (5 req/min/IP)
- GetOsdiByTokenQuery placed in Contracts (shared between Presentation endpoint and Application handler)
- AllowedContentTypes as internal static class with separate image and video content type sets plus size constants

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added missing Contract types for endpoints**
- **Found during:** Task 2 (Clinical API endpoints)
- **Issue:** GetVisitImagesQuery, DeleteMedicalImageCommand, GetOsdiByTokenQuery, GetOsdiHistoryQuery, GetDryEyeComparisonQuery not defined in Contracts
- **Fix:** Added sealed record definitions to appropriate Contracts DTO files
- **Files modified:** MedicalImageDto.cs, OsdiQuestionnaireDto.cs, OsdiHistoryDto.cs, DryEyeComparisonDto.cs
- **Verification:** Backend builds with 0 errors
- **Committed in:** 9bb057f (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Missing Contract types were necessary for endpoint compilation. No scope creep.

## Issues Encountered
None - implementation followed established patterns without issues.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All backend API surface for medical imaging (IMG-01 through IMG-04) is complete
- Endpoints ready for frontend integration in Plans 04-04, 04-05
- Public OSDI endpoints ready for patient self-fill frontend page
- Image upload supports both images (50MB limit) and videos (200MB limit)
- SAS URL pattern established for secure client-side image access

## Self-Check: PASSED

- All 11 created/modified files verified on disk
- Task commits bdaa9a8, 9751cab, 9bb057f verified in git log
- Backend builds with 0 errors
- All 100 Clinical unit tests pass (17 new imaging tests + 83 existing)

---
*Phase: 04-dry-eye-template-medical-imaging*
*Completed: 2026-03-05*

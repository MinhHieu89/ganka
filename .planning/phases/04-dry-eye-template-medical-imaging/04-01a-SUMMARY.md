---
phase: 04-dry-eye-template-medical-imaging
plan: 01a
subsystem: domain
tags: [dry-eye, osdi, medical-imaging, domain-entities, dtos, clinical]

# Dependency graph
requires:
  - phase: 03-clinical-visit-workflow
    provides: Visit aggregate, Refraction entity pattern, Clinical.Domain/Contracts projects
provides:
  - DryEyeAssessment entity with per-eye flat columns (TBUT, Schirmer, Meibomian, TearMeniscus, Staining)
  - MedicalImage entity independent from Visit aggregate (append-only after sign-off)
  - OsdiSubmission entity with public token for patient self-fill
  - ImageType, EyeTag, OsdiSeverity enums
  - 13 contract DTOs for dry eye, OSDI, and imaging features
  - Visit entity modified with DryEyeAssessment child collection
affects: [04-01b, 04-02, 04-03, 04-04, 04-05, 04-06, 04-07]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Append-only entity pattern: MedicalImage bypasses Visit aggregate EnsureEditable guard"
    - "Patient-level score on visit child: OsdiScore/OsdiSeverity on DryEyeAssessment (not per-eye)"
    - "Public token entity pattern: OsdiSubmission.CreateWithToken for unauthenticated patient access"

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/DryEyeAssessment.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/MedicalImage.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/OsdiSubmission.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/ImageType.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/EyeTag.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/OsdiSeverity.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DryEyeAssessmentDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiHistoryDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/MedicalImageDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DryEyeComparisonDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiQuestionnaireDto.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs

key-decisions:
  - "DryEyeAssessment is a Visit child (subject to EnsureEditable), MedicalImage is NOT (append-only after sign-off)"
  - "OSDI score is patient-level on DryEyeAssessment, not per-eye"
  - "OsdiSubmission uses public token with 24h expiry for patient self-fill"
  - "All contract DTOs defined as sealed records for immutability"
  - "UploadMedicalImageCommand uses Stream for multipart file upload support"

patterns-established:
  - "Append-only entity: MedicalImage exists independently from Visit aggregate, bypassing sign-off immutability"
  - "Public token access: OsdiSubmission.CreateWithToken factory for unauthenticated patient questionnaire"
  - "Sealed record DTOs: All new contract DTOs use sealed record for stronger immutability guarantees"

requirements-completed: [DRY-01, DRY-02, IMG-01, IMG-02]

# Metrics
duration: 2min
completed: 2026-03-05
---

# Phase 04 Plan 01a: Domain Entities & Contract DTOs Summary

**Dry eye assessment, medical image, and OSDI submission domain entities with 3 enums and 13 sealed record DTOs following established per-eye flat column and Visit child patterns**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-05T06:11:03Z
- **Completed:** 2026-03-05T06:13:25Z
- **Tasks:** 1
- **Files modified:** 12

## Accomplishments
- Created DryEyeAssessment entity following Refraction per-eye flat column pattern (OdTbut/OsTbut, OdSchirmer/OsSchirmer, etc.)
- Created MedicalImage entity independent from Visit aggregate -- append-only even after sign-off
- Created OsdiSubmission entity with public token for patient self-fill questionnaire
- Added 3 domain enums: ImageType (6 values), EyeTag (3 values), OsdiSeverity (4 values)
- Modified Visit entity with _dryEyeAssessments backing field and AddDryEyeAssessment method
- Created 13 sealed record contract DTOs covering assessment, OSDI, imaging, and comparison features

## Task Commits

Each task was committed atomically:

1. **Task 1: Domain entities, enums, and contracts DTOs** - `c67cb16` (feat)

**Plan metadata:** (pending)

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Domain/Enums/ImageType.cs` - Fluorescein, Meibography, OCT, SpecularMicroscopy, Topography, Video
- `backend/src/Modules/Clinical/Clinical.Domain/Enums/EyeTag.cs` - OD, OS, OU
- `backend/src/Modules/Clinical/Clinical.Domain/Enums/OsdiSeverity.cs` - Normal, Mild, Moderate, Severe
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/DryEyeAssessment.cs` - Per-eye dry eye metrics with OSDI score
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/MedicalImage.cs` - Image metadata entity (NOT a Visit child)
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/OsdiSubmission.cs` - OSDI questionnaire with public token
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs` - Added DryEyeAssessment child collection
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DryEyeAssessmentDto.cs` - Assessment DTO + UpdateDryEyeAssessmentCommand
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiHistoryDto.cs` - OsdiHistoryDto + OsdiHistoryResponse
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/MedicalImageDto.cs` - MedicalImageDto + UploadMedicalImageCommand + GetImageComparisonQuery + ImageComparisonResponse
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DryEyeComparisonDto.cs` - DryEyeComparisonDto + DryEyeComparisonVisitData
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiQuestionnaireDto.cs` - OsdiQuestionnaireDto + OsdiQuestionDto + SubmitOsdiCommand + GenerateOsdiLinkCommand + OsdiLinkResponse

## Decisions Made
- DryEyeAssessment is a Visit child (subject to EnsureEditable guard), while MedicalImage is NOT (append-only after sign-off) -- follows plan specification and clinical practice
- OSDI score is patient-level on DryEyeAssessment, not per-eye, because OSDI is a patient-reported symptom index
- OsdiSubmission uses CreateWithToken factory with 24-hour expiry for patient self-fill via public link
- All contract DTOs use sealed record modifier for stronger immutability, while existing DTOs use plain record
- UploadMedicalImageCommand includes Stream parameter for multipart file upload support

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Domain entities and DTOs ready for EF Core configuration (Plan 01b)
- Visit entity modification ready for DbContext Include extension
- MedicalImage entity ready for Azure Blob Storage integration
- OsdiSubmission entity ready for public token endpoint implementation

## Self-Check: PASSED

- All 11 created files verified on disk
- Task commit c67cb16 verified in git log
- Clinical.Domain builds with 0 errors
- Clinical.Contracts builds with 0 errors

---
*Phase: 04-dry-eye-template-medical-imaging*
*Completed: 2026-03-05*

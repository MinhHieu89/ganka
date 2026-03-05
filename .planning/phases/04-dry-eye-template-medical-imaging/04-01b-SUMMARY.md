---
phase: 04-dry-eye-template-medical-imaging
plan: 01b
subsystem: database
tags: [ef-core, dry-eye, medical-imaging, osdi, repositories, migration, clinical]

# Dependency graph
requires:
  - phase: 04-dry-eye-template-medical-imaging
    plan: 01a
    provides: DryEyeAssessment, MedicalImage, OsdiSubmission domain entities and enums
  - phase: 03-clinical-visit-workflow
    provides: Visit aggregate, Refraction configuration pattern, ClinicalDbContext, VisitRepository
provides:
  - 3 EF Core configurations (DryEyeAssessment, MedicalImage, OsdiSubmission) with proper schema, indexes, constraints
  - 3 repository interfaces (IVisitRepository extended, IMedicalImageRepository, IOsdiSubmissionRepository)
  - 3 repository implementations (VisitRepository extended, MedicalImageRepository, OsdiSubmissionRepository)
  - Updated ClinicalDbContext with 3 new DbSets
  - Database migration AddDryEyeAndImaging applied
affects: [04-02, 04-03, 04-04, 04-05, 04-06, 04-07]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Filtered unique index: OsdiSubmission.PublicToken with HasFilter('[PublicToken] IS NOT NULL')"
    - "Composite index: MedicalImage (VisitId, Type) for same-type image queries"
    - "Independent repository pattern: MedicalImage has its own repository (not via Visit aggregate)"
    - "Join-based patient query: DryEyeAssessments joined with Visits for patient-level trend queries"

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/DryEyeAssessmentConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/MedicalImageConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/OsdiSubmissionConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IMedicalImageRepository.cs
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IOsdiSubmissionRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/MedicalImageRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/OsdiSubmissionRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Migrations/20260305062420_AddDryEyeAndImaging.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/IoC.cs

key-decisions:
  - "MedicalImage has NO navigation property from Visit -- kept separate from aggregate with its own repository"
  - "DryEyeAssessment uses Join instead of Include for patient-level trend queries to avoid loading full Visit entities"
  - "OsdiSubmission PublicToken uses filtered unique index (IS NOT NULL) to allow multiple null tokens"

patterns-established:
  - "Filtered unique index: Nullable column with unique constraint via HasFilter for SQL Server"
  - "Independent entity repository: Entities outside aggregate root get their own repository and DbSet"

requirements-completed: [DRY-01, DRY-02, IMG-01]

# Metrics
duration: 6min
completed: 2026-03-05
---

# Phase 04 Plan 01b: EF Core Configurations, Repositories & Migration Summary

**3 EF Core configurations with precision/indexes, 3 repository interfaces/implementations, ClinicalDbContext extended, and AddDryEyeAndImaging migration applied for dry eye assessment, medical imaging, and OSDI submission data access**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-05T06:19:16Z
- **Completed:** 2026-03-05T06:25:51Z
- **Tasks:** 1
- **Files modified:** 15

## Accomplishments
- Created DryEyeAssessmentConfiguration with precision(5,2) for per-eye decimals, precision(7,2) for OsdiScore, and int conversion for OsdiSeverity
- Created MedicalImageConfiguration with composite index on (VisitId, Type) for same-type queries, no Visit navigation property
- Created OsdiSubmissionConfiguration with unique filtered index on PublicToken (IS NOT NULL) for patient self-fill token lookup
- Extended ClinicalDbContext with DryEyeAssessments, MedicalImages, OsdiSubmissions DbSets
- Extended VisitConfiguration with HasMany DryEyeAssessments and PropertyAccessMode.Field on backing field
- Extended IVisitRepository and VisitRepository with AddDryEyeAssessment, GetDryEyeAssessmentsByPatient, GetDryEyeAssessmentByVisit
- Extended VisitRepository GetByIdWithDetailsAsync to Include DryEyeAssessments
- Created IMedicalImageRepository and MedicalImageRepository for independent image CRUD
- Created IOsdiSubmissionRepository and OsdiSubmissionRepository with token-based lookup, visit-based queries, and batch loading
- Registered both new repositories in IoC as scoped services
- Migration AddDryEyeAndImaging created and applied -- 3 tables, 5 indexes

## Task Commits

Each task was committed atomically:

1. **Task 1: EF Core configurations, repositories, and migration** - `50cf9ce` (feat)

**Plan metadata:** (pending)

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/DryEyeAssessmentConfiguration.cs` - EF Core config for DryEyeAssessments table with decimal precision and indexes
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/MedicalImageConfiguration.cs` - EF Core config for MedicalImages table with composite index on (VisitId, Type)
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/OsdiSubmissionConfiguration.cs` - EF Core config for OsdiSubmissions with filtered unique index on PublicToken
- `backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs` - Added 3 new DbSets
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs` - Added DryEyeAssessments HasMany with PropertyAccessMode.Field
- `backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs` - Added 3 new methods for dry eye assessment data access
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs` - Implemented new methods, extended Include
- `backend/src/Modules/Clinical/Clinical.Application/Interfaces/IMedicalImageRepository.cs` - Repository interface for medical image CRUD
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/MedicalImageRepository.cs` - Repository implementation with ordered queries
- `backend/src/Modules/Clinical/Clinical.Application/Interfaces/IOsdiSubmissionRepository.cs` - Repository interface for OSDI submission CRUD
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/OsdiSubmissionRepository.cs` - Repository implementation with token and batch queries
- `backend/src/Modules/Clinical/Clinical.Infrastructure/IoC.cs` - Registered IMedicalImageRepository and IOsdiSubmissionRepository
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Migrations/20260305062420_AddDryEyeAndImaging.cs` - Migration creating 3 tables with indexes

## Decisions Made
- MedicalImage has NO navigation property from Visit -- kept separate from aggregate with its own repository for independent append-only access even after sign-off
- DryEyeAssessment patient-level queries use Join with Visits table instead of Include to avoid loading full Visit entities when only VisitDate is needed for ordering
- OsdiSubmission PublicToken uses SQL Server filtered unique index (HasFilter "[PublicToken] IS NOT NULL") to allow multiple rows with null tokens while maintaining uniqueness for non-null values

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Migration command required explicit --context ClinicalDbContext parameter due to multiple DbContexts in the solution (resolved by adding the parameter)

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All data access layers ready for handler implementation (Plan 02 dry eye handlers, Plan 03 image handlers)
- VisitRepository includes DryEyeAssessments in GetByIdWithDetailsAsync for visit detail queries
- MedicalImageRepository ready for Azure Blob Storage integration in image upload handlers
- IOsdiSubmissionRepository GetByTokenAsync ready for public OSDI self-fill endpoint
- GetByVisitIdsAsync ready for batch OSDI history loading across visits

## Self-Check: PASSED

- All 8 created files verified on disk
- Task commit 50cf9ce verified in git log
- Backend builds with 0 errors
- Migration AddDryEyeAndImaging listed and applied

---
*Phase: 04-dry-eye-template-medical-imaging*
*Completed: 2026-03-05*

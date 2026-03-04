---
phase: 03-clinical-workflow-examination
plan: 02
subsystem: api
tags: [wolverine, fluentvalidation, tdd, clinical, handlers, minimal-api, icd-10, refraction, diagnosis]

# Dependency graph
requires:
  - phase: 03-clinical-workflow-examination
    plan: 01
    provides: "Visit aggregate root, Refraction/VisitDiagnosis/VisitAmendment/DoctorIcd10Favorite entities, repositories, UnitOfWork, ClinicalDbContext"
  - phase: 01-foundation
    provides: "Result<T>, Error, AggregateRoot, IAuditable, ICurrentUser, ReferenceDbContext with ICD-10 codes"
  - phase: 01.1
    provides: "Minimal API endpoint pattern (MapGroup, RequireAuthorization), Shared.Presentation ResultExtensions, IoC per layer"
provides:
  - "13 Wolverine handlers for Clinical module (7 visit lifecycle + 6 refraction/diagnosis/ICD-10)"
  - "Clinical.Presentation project with 13 Minimal API endpoints under /api/clinical"
  - "Bootstrapper wired: AddClinicalApplication, AddClinicalInfrastructure, AddClinicalPresentation, MapClinicalApiEndpoints"
  - "FluentValidation on CreateVisit, AmendVisit, UpdateRefraction, AddVisitDiagnosis commands"
  - "SearchIcd10Codes with bilingual search and per-doctor favorites pinned to top"
  - "44 unit tests across 8 test classes"
affects: [03-03, 03-04, 03-05]

# Tech tracking
tech-stack:
  added: [Microsoft.EntityFrameworkCore.InMemory (tests), Shared.Infrastructure reference in Clinical.Application for ReferenceDbContext]
  patterns: [Static Wolverine handler with FluentValidation injection, OU laterality dual-record creation, ReferenceDbContext direct injection in handlers]

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Application/Features/CreateVisit.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetVisitById.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetActiveVisits.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/SignOffVisit.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/AmendVisit.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/AdvanceWorkflowStage.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/UpdateVisitNotes.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/UpdateVisitRefraction.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/AddVisitDiagnosis.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/RemoveVisitDiagnosis.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/SearchIcd10Codes.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/ToggleIcd10Favorite.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetDoctorFavorites.cs
    - backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
    - backend/src/Modules/Clinical/Clinical.Presentation/IoC.cs
    - backend/src/Modules/Clinical/Clinical.Presentation/Clinical.Presentation.csproj
    - backend/tests/Clinical.Unit.Tests/Features/CreateVisitHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/SignOffVisitHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/AmendVisitHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/AdvanceWorkflowStageHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/GetActiveVisitsHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/UpdateRefractionHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/AddVisitDiagnosisHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/SearchIcd10CodesHandlerTests.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs
    - backend/src/Modules/Clinical/Clinical.Application/Clinical.Application.csproj
    - backend/src/Bootstrapper/Program.cs
    - backend/src/Bootstrapper/Bootstrapper.csproj
    - backend/Ganka28.slnx
    - backend/Directory.Packages.props
    - backend/tests/Clinical.Unit.Tests/Clinical.Unit.Tests.csproj

key-decisions:
  - "Visit.StartAmendment now accepts VisitAmendment parameter -- domain method instead of reflection hack for adding amendment to collection"
  - "Shared.Infrastructure reference added to Clinical.Application for ReferenceDbContext access in SearchIcd10Codes/GetDoctorFavorites handlers"
  - "Microsoft.EntityFrameworkCore.InMemory added to CPM for SearchIcd10Codes unit tests with in-memory ReferenceDbContext"
  - "OU laterality creates two records with code suffixes (.1 for OD, .2 for OS) per ICD-10 laterality convention"
  - "SearchIcd10Codes uses Contains for bilingual search (code, English, Vietnamese) with favorites pinned to top via OrderByDescending"

patterns-established:
  - "Handler pattern: static class with Handle method, IValidator injection, GroupBy/ToDictionary for structured validation errors"
  - "OU laterality dual-record: when Laterality=OU, create two VisitDiagnosis records with .1/.2 code suffixes"
  - "ReferenceDbContext injected directly in handlers for cross-module ICD-10 queries"
  - "UpdateRefraction: find-or-create pattern -- existing refraction of same type gets updated, otherwise new one created and added via domain method"

requirements-completed: [CLN-01, CLN-02, CLN-03, CLN-04, REF-01, REF-02, REF-03, DX-01, DX-02]

# Metrics
duration: 16min
completed: 2026-03-04
---

# Phase 03 Plan 02: Clinical Feature Handlers Summary

**13 Wolverine handlers with FluentValidation, bilingual ICD-10 search with per-doctor favorites, OU laterality dual-record creation, and Clinical.Presentation with 13 Minimal API endpoints wired into Bootstrapper**

## Performance

- **Duration:** 16 min
- **Started:** 2026-03-04T10:06:52Z
- **Completed:** 2026-03-04T10:22:52Z
- **Tasks:** 2
- **Files modified:** 33

## Accomplishments
- 13 Clinical feature handlers implemented following TDD (test-first, then implement) with 44 passing unit tests
- Clinical.Presentation project created with all endpoints under /api/clinical (visit CRUD, refraction, diagnosis, ICD-10 search, favorites)
- Bootstrapper fully wired: AddClinicalApplication/Infrastructure/Presentation and MapClinicalApiEndpoints
- SearchIcd10Codes handler searches Vietnamese and English descriptions with per-doctor favorites pinned to top
- AddVisitDiagnosis enforces laterality: OU creates two records (.1 right + .2 left), blocking unspecified (.9)
- UpdateRefraction validates all ophthalmological ranges (SPH -30..+30, CYL -10..+10, AXIS 1..180, VA 0.01..2.0, IOP 1..60, etc.)

## Task Commits

Each task was committed atomically:

1. **Task 1: Core visit lifecycle handlers with TDD + Presentation project + Bootstrapper wiring** - `d097bd1` (feat)
2. **Task 2: Refraction, diagnosis, and ICD-10 handlers with TDD** - `ae1838d` (feat)

## Files Created/Modified
- `Clinical.Application/Features/CreateVisit.cs` - Visit creation from check-in or walk-in with FluentValidation
- `Clinical.Application/Features/GetVisitById.cs` - Full visit detail query with refractions, diagnoses, amendments
- `Clinical.Application/Features/GetActiveVisits.cs` - Active visits for Kanban dashboard with WaitMinutes and HasAllergies
- `Clinical.Application/Features/SignOffVisit.cs` - Visit sign-off making it immutable
- `Clinical.Application/Features/AmendVisit.cs` - Amendment workflow with mandatory reason and field-level diff
- `Clinical.Application/Features/AdvanceWorkflowStage.cs` - Workflow stage progression through 8 stages
- `Clinical.Application/Features/UpdateVisitNotes.cs` - Examination notes update
- `Clinical.Application/Features/UpdateVisitRefraction.cs` - Per-eye refraction data with comprehensive validation ranges
- `Clinical.Application/Features/AddVisitDiagnosis.cs` - ICD-10 diagnosis with OU laterality dual-record creation
- `Clinical.Application/Features/RemoveVisitDiagnosis.cs` - Diagnosis removal with EnsureEditable guard
- `Clinical.Application/Features/SearchIcd10Codes.cs` - Bilingual ICD-10 search with per-doctor favorites
- `Clinical.Application/Features/ToggleIcd10Favorite.cs` - Toggle per-doctor ICD-10 favorite
- `Clinical.Application/Features/GetDoctorFavorites.cs` - Get doctor's favorite ICD-10 codes with full details
- `Clinical.Presentation/ClinicalApiEndpoints.cs` - 13 Minimal API endpoints under /api/clinical
- `Clinical.Presentation/IoC.cs` - DI registration placeholder
- `Clinical.Presentation/Clinical.Presentation.csproj` - Presentation project with WolverineFx, Shared.Presentation refs
- `Clinical.Domain/Entities/Visit.cs` - StartAmendment updated to accept VisitAmendment parameter
- `Bootstrapper/Program.cs` - Clinical module registration and endpoint mapping
- `Bootstrapper/Bootstrapper.csproj` - Clinical.Presentation project reference added
- 8 test classes with 44 total unit tests

## Decisions Made
- Visit.StartAmendment changed signature to accept VisitAmendment parameter -- avoids reflection hack, keeps domain logic clean
- Shared.Infrastructure reference added to Clinical.Application for ReferenceDbContext access (SearchIcd10Codes, GetDoctorFavorites)
- Microsoft.EntityFrameworkCore.InMemory added to Directory.Packages.props for SearchIcd10Codes test with in-memory database
- OU laterality creates two diagnosis records with .1/.2 code suffixes following ICD-10 laterality convention
- Duplicate `using Clinical.Infrastructure` removed from Program.cs (already present in DbContext section)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Visit.StartAmendment domain method required amendment parameter**
- **Found during:** Task 1 (AmendVisit handler implementation)
- **Issue:** Visit entity had parameterless StartAmendment() but no way to add amendment to collection without reflection
- **Fix:** Changed StartAmendment() to StartAmendment(VisitAmendment amendment) which transitions status AND adds amendment to _amendments collection
- **Files modified:** backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs
- **Verification:** AmendVisitHandlerTests pass, amendment properly appears in visit.Amendments collection
- **Committed in:** d097bd1 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 missing critical)
**Impact on plan:** Domain method signature change necessary for correctness. No scope creep.

## Issues Encountered
- Bootstrapper.exe process lock prevented initial build -- killed stale process and retried successfully

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 13 Clinical handlers implemented and tested -- ready for frontend development in Plan 03
- Clinical.Presentation endpoints accessible at /api/clinical/* -- Swagger integration available
- SearchIcd10Codes queries existing ReferenceDbContext (seeded in Phase 1) -- no additional data migration needed
- 44 unit tests providing regression safety for future refactoring

## Self-Check: PASSED

- All 24 key created files verified present on disk
- Commit d097bd1 (Task 1) verified in git log
- Commit ae1838d (Task 2) verified in git log
- Bootstrapper builds with 0 errors
- 44 unit tests pass

---
*Phase: 03-clinical-workflow-examination*
*Completed: 2026-03-04*

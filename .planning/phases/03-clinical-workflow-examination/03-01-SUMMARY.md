---
phase: 03-clinical-workflow-examination
plan: 01
subsystem: database, api
tags: [ef-core, domain-model, aggregate-root, repository-pattern, clinical, ophthalmology, icd-10]

# Dependency graph
requires:
  - phase: 01-foundation
    provides: "AggregateRoot, Entity, IAuditable, BranchId base classes"
  - phase: 02-patient-scheduling
    provides: "Appointment entity pattern (denormalized names, RowVersion), Patient module for cross-ref"
provides:
  - "Visit aggregate root with immutability enforcement (EnsureEditable guard)"
  - "Refraction entity with 3 types and per-eye SPH/CYL/AXIS/ADD/PD/VA/IOP/AxialLength"
  - "VisitDiagnosis entity with ICD-10 code, laterality, primary/secondary role"
  - "VisitAmendment entity with field-level diff and mandatory reason"
  - "DoctorIcd10Favorite per-doctor junction table"
  - "ClinicalDbContext with 5 DbSets, 5 EF configurations, 2 repositories, UnitOfWork"
  - "Clinical.Unit.Tests project scaffold"
  - "Application interfaces: IVisitRepository, IDoctorIcd10FavoriteRepository, IUnitOfWork"
  - "Application and Infrastructure IoC registration extensions"
affects: [03-02, 03-03, 03-04, 03-05]

# Tech tracking
tech-stack:
  added: [FluentValidation.DependencyInjectionExtensions in Clinical.Application]
  patterns: [Visit aggregate with EnsureEditable guard, per-doctor favorites junction table, amendment chain with JSON field-level diff]

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/VisitAmendment.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/Refraction.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/VisitDiagnosis.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/DoctorIcd10Favorite.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/WorkflowStage.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/VisitStatus.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/RefractionType.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/Laterality.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/IopMethod.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/DiagnosisRole.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/CreateVisitCommand.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/VisitDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/VisitDetailDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/RefractionDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/VisitDiagnosisDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/ActiveVisitDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/Icd10SearchResultDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/VisitAmendmentDto.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitAmendmentConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/RefractionConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitDiagnosisConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/DoctorIcd10FavoriteConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/DoctorIcd10FavoriteRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/UnitOfWork.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/IoC.cs
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IDoctorIcd10FavoriteRepository.cs
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IUnitOfWork.cs
    - backend/src/Modules/Clinical/Clinical.Application/IoC.cs
    - backend/tests/Clinical.Unit.Tests/Clinical.Unit.Tests.csproj
  modified:
    - backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs
    - backend/src/Modules/Clinical/Clinical.Application/Clinical.Application.csproj
    - backend/src/Modules/Clinical/Clinical.Contracts/Clinical.Contracts.csproj
    - backend/Ganka28.slnx

key-decisions:
  - "WolverineFx.Http removed from Clinical.Application, replaced with FluentValidation.DependencyInjectionExtensions (Application layer HTTP-free)"
  - "Microsoft.EntityFrameworkCore added to Clinical.Application for DbUpdateConcurrencyException handling (same as Scheduling.Application precedent)"
  - "DoctorIcd10Favorite as per-doctor junction table -- global Icd10Code.IsFavorite is ignored"
  - "Refraction decimal fields use precision(5,2) for all diopter/VA/IOP/axial length values"
  - "VisitAmendment.FieldChangesJson stores JSON string of FieldChange records (not typed navigation)"

patterns-established:
  - "Visit.EnsureEditable() guard pattern: all mutation methods call EnsureEditable() which throws if Status == Signed"
  - "Visit.StartAmendment() transitions Signed -> Amended to re-enable edits via amendment workflow"
  - "Per-doctor favorites via junction table (DoctorIcd10Favorite) instead of global boolean on reference data"
  - "FieldChange record type co-located with VisitAmendment entity for field-level diff serialization"

requirements-completed: [CLN-01, CLN-02, CLN-03, REF-01, REF-02, REF-03, DX-01, DX-02]

# Metrics
duration: 3min
completed: 2026-03-04
---

# Phase 03 Plan 01: Clinical Domain & Infrastructure Summary

**Visit aggregate root with immutability guard, refraction/diagnosis/amendment entities, 5 EF configurations with indexed schemas, repository + UnitOfWork pattern, and Clinical.Unit.Tests scaffold**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-04T10:00:20Z
- **Completed:** 2026-03-04T10:03:02Z
- **Tasks:** 2
- **Files modified:** 37

## Accomplishments
- Visit aggregate root with EnsureEditable guard enforcing immutability after sign-off, HasAllergies denormalized field for Kanban allergy warning
- Complete refraction entity supporting Manifest/Autorefraction/Cycloplegic types with all per-eye fields (SPH, CYL, AXIS, ADD, PD, UCVA, BCVA, IOP, Axial Length)
- DoctorIcd10Favorite per-doctor junction table for ICD-10 favorites (not using global IsFavorite)
- ClinicalDbContext with 5 DbSets, 5 EF configurations, 2 repositories, UnitOfWork, IoC registration
- Clinical.Unit.Tests project added to solution with xunit + NSubstitute + FluentAssertions

## Task Commits

Each task was committed atomically:

1. **Task 1: Clinical domain entities, enums, contracts DTOs, and test project scaffold** - `0fabcbd` (feat)
2. **Task 2: Clinical EF Core infrastructure, repositories, and application interfaces** - `e807a48` (feat)

## Files Created/Modified
- `Clinical.Domain/Entities/Visit.cs` - Visit aggregate root with 164 lines, factory method, workflow stage advancement, sign-off, amendment, EnsureEditable guard
- `Clinical.Domain/Entities/VisitAmendment.cs` - Amendment entity with FieldChange record for JSON diff tracking
- `Clinical.Domain/Entities/Refraction.cs` - Per-eye refraction data with Update method for all fields
- `Clinical.Domain/Entities/VisitDiagnosis.cs` - ICD-10 diagnosis link with laterality and role
- `Clinical.Domain/Entities/DoctorIcd10Favorite.cs` - Per-doctor favorites junction table
- `Clinical.Domain/Enums/*.cs` - 6 enums: WorkflowStage (8 stages), VisitStatus, RefractionType, Laterality (no unspecified), IopMethod, DiagnosisRole
- `Clinical.Contracts/Dtos/CreateVisitCommand.cs` - 14 command/query/response records
- `Clinical.Contracts/Dtos/*.cs` - 7 DTO records (VisitDto, VisitDetailDto, RefractionDto, etc.)
- `Clinical.Infrastructure/ClinicalDbContext.cs` - 5 DbSets with schema isolation and assembly configuration scanning
- `Clinical.Infrastructure/Configurations/*.cs` - 5 EF configurations with proper indexes and constraints
- `Clinical.Infrastructure/Repositories/*.cs` - VisitRepository (with Include pattern) and DoctorIcd10FavoriteRepository
- `Clinical.Infrastructure/UnitOfWork.cs` - SaveChangesAsync wrapper
- `Clinical.Infrastructure/IoC.cs` - Scoped DI registration for repositories and UnitOfWork
- `Clinical.Application/Interfaces/*.cs` - IVisitRepository, IDoctorIcd10FavoriteRepository, IUnitOfWork
- `Clinical.Application/IoC.cs` - FluentValidation assembly registration
- `Clinical.Application/Clinical.Application.csproj` - WolverineFx.Http replaced with FluentValidation.DependencyInjectionExtensions + EF Core
- `Clinical.Unit.Tests/Clinical.Unit.Tests.csproj` - Test project with xunit, NSubstitute, FluentAssertions, coverlet

## Decisions Made
- WolverineFx.Http removed from Clinical.Application -- Application layer is HTTP-free per established pattern
- Microsoft.EntityFrameworkCore added to Clinical.Application for future DbUpdateConcurrencyException handling (same precedent as Scheduling.Application)
- DoctorIcd10Favorite as separate junction table in Clinical schema -- global Icd10Code.IsFavorite field is ignored
- All refraction decimal fields use precision(5,2) for diopter/VA/IOP/axial length values
- VisitAmendment stores field changes as JSON string of FieldChange records rather than typed navigation

## Deviations from Plan

None - plan executed exactly as written. Task 1 was pre-committed before this execution session; Task 2 files were partially created and completed in this session.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Domain layer complete with all entities, enums, and contracts -- ready for handler implementation in Plan 02
- Infrastructure layer complete with repositories and configurations -- ready for API endpoint implementation
- Clinical.Unit.Tests project scaffolded -- ready for TDD in Plan 02
- IoC extensions created but not yet registered in Bootstrapper Program.cs -- will be done when Presentation layer is added (Plan 02+)

## Self-Check: PASSED

- All 16 key files verified present on disk
- Commit 0fabcbd (Task 1) verified in git log
- Commit e807a48 (Task 2) verified in git log
- All 5 Clinical projects build with 0 errors, 0 warnings

---
*Phase: 03-clinical-workflow-examination*
*Completed: 2026-03-04*

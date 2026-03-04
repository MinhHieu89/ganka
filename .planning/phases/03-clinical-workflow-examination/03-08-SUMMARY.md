---
phase: 03-clinical-workflow-examination
plan: 08
subsystem: api
tags: [ef-core, change-tracking, concurrency, repository-pattern, tdd]

# Dependency graph
requires:
  - phase: 03-clinical-workflow-examination
    provides: "Visit aggregate with Refraction, Diagnosis, Amendment child entities and handlers"
provides:
  - "IVisitRepository.AddRefraction/AddDiagnosis/AddAmendment for explicit EF Core child-entity tracking"
  - "Fixed DbUpdateConcurrencyException in all 3 child-entity mutation handlers"
affects: [03-09, clinical-api, visit-workflow]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Direct DbContext.DbSet.Add() for child entities instead of aggregate backing field insertion"
    - "AllergyRepository.Add pattern applied to Clinical module child entities"

key-files:
  created:
    - "backend/tests/Clinical.Unit.Tests/Repositories/VisitRepositoryChildEntityTests.cs"
  modified:
    - "backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs"
    - "backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs"
    - "backend/src/Modules/Clinical/Clinical.Application/Features/UpdateVisitRefraction.cs"
    - "backend/src/Modules/Clinical/Clinical.Application/Features/AddVisitDiagnosis.cs"
    - "backend/src/Modules/Clinical/Clinical.Application/Features/AmendVisit.cs"
    - "backend/tests/Clinical.Unit.Tests/Features/UpdateRefractionHandlerTests.cs"
    - "backend/tests/Clinical.Unit.Tests/Features/AddVisitDiagnosisHandlerTests.cs"
    - "backend/tests/Clinical.Unit.Tests/Features/AmendVisitHandlerTests.cs"
    - "backend/tests/Clinical.Unit.Tests/Clinical.Unit.Tests.csproj"

key-decisions:
  - "Synchronous void Add methods (not Task) matching DbSet.Add synchronous nature and AllergyRepository pattern"
  - "Keep domain method calls (visit.AddRefraction) for business rule enforcement alongside repository Add for EF Core tracking"
  - "Added Clinical.Infrastructure project reference to test project for repository-level integration tests"

patterns-established:
  - "Child-entity persistence pattern: domain aggregate method for rules + repository Add for EF Core tracking"

requirements-completed: [CLN-01, CLN-02, REF-01, REF-02, REF-03, DX-01, DX-02]

# Metrics
duration: 10min
completed: 2026-03-04
---

# Phase 03 Plan 08: Fix DbUpdateConcurrencyException Summary

**Fixed shared EF Core change-tracking root cause in all 3 child-entity mutation handlers using explicit DbContext.DbSet.Add() via repository methods**

## Performance

- **Duration:** 10 min
- **Started:** 2026-03-04T16:46:07Z
- **Completed:** 2026-03-04T16:56:18Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments
- Added AddRefraction/AddDiagnosis/AddAmendment to IVisitRepository interface and VisitRepository implementation
- Fixed UpdateVisitRefraction, AddVisitDiagnosis, and AmendVisit handlers to explicitly register child entities with EF Core change tracker
- Added 3 repository-level integration tests using InMemory DbContext verifying entities tracked as Added
- Added Received() assertions in all 3 handler test files confirming Add methods are called
- All 47 tests pass (44 existing + 3 new)

## Task Commits

Each task was committed atomically:

1. **Task 1: Add child-entity registration methods to IVisitRepository and implementation**
   - `50d9fb6` (test) RED: failing tests for repository Add methods
   - `7d38772` (feat) GREEN: interface + implementation added
2. **Task 2: Fix all three handlers to use explicit child-entity registration**
   - `72b56b1` (test) RED: Received() assertions fail in handler tests
   - `0202122` (fix) GREEN: all 3 handlers call repository Add methods

_Note: TDD tasks have multiple commits (test -> feat/fix)_

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs` - Added 3 new void methods for child-entity registration
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs` - Implemented using _dbContext.{DbSet}.Add()
- `backend/src/Modules/Clinical/Clinical.Application/Features/UpdateVisitRefraction.cs` - Added visitRepository.AddRefraction() call
- `backend/src/Modules/Clinical/Clinical.Application/Features/AddVisitDiagnosis.cs` - Added visitRepository.AddDiagnosis() calls (3 sites)
- `backend/src/Modules/Clinical/Clinical.Application/Features/AmendVisit.cs` - Added visitRepository.AddAmendment() call
- `backend/tests/Clinical.Unit.Tests/Repositories/VisitRepositoryChildEntityTests.cs` - New: 3 InMemory DbContext tests
- `backend/tests/Clinical.Unit.Tests/Features/UpdateRefractionHandlerTests.cs` - Added Received() assertions
- `backend/tests/Clinical.Unit.Tests/Features/AddVisitDiagnosisHandlerTests.cs` - Added Received() assertions
- `backend/tests/Clinical.Unit.Tests/Features/AmendVisitHandlerTests.cs` - Added Received() assertions
- `backend/tests/Clinical.Unit.Tests/Clinical.Unit.Tests.csproj` - Added Clinical.Infrastructure project reference

## Decisions Made
- Synchronous void Add methods (not Task) matching DbSet.Add synchronous nature and AllergyRepository.Add pattern from Patient module
- Keep domain method calls (visit.AddRefraction/AddDiagnosis/StartAmendment) for business rule enforcement alongside repository Add for EF Core tracking -- dual-call pattern ensures both domain invariants and persistence correctness
- Added Clinical.Infrastructure project reference to test project to enable repository-level integration tests with InMemory DbContext

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- .NET 10 SDK incremental build cache corruption ("Question build FAILED" / MSB3492) required --no-incremental flag and cache file deletion
- Bootstrapper process file lock prevented full solution build verification; Clinical module projects verified individually (all build clean)

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All 3 child-entity mutations (refraction save, diagnosis add, amendment create) now properly tracked by EF Core
- Ready for Plan 03-09 E2E verification of the fixes
- DbUpdateConcurrencyException root cause eliminated for INSERT operations on child entities

## Self-Check: PASSED

All 7 files found. All 4 commits verified.

---
*Phase: 03-clinical-workflow-examination*
*Completed: 2026-03-04*

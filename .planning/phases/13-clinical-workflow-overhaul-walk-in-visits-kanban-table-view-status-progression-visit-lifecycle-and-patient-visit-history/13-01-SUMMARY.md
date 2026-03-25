---
phase: 13-clinical-workflow-overhaul
plan: 01
subsystem: api
tags: [cqrs, ddd, tdd, workflow, visit-lifecycle, stage-reversal]

requires:
  - phase: 01-foundation
    provides: "Shared domain base classes, CQRS patterns, Wolverine handlers"
provides:
  - "Visit.ReverseStage domain method with AllowedReversals transition table"
  - "ReverseWorkflowStageHandler for bidirectional stage transitions"
  - "GetPatientVisitHistoryHandler for visit history query"
  - "SignOffVisit auto-advance to next stage (D-11)"
  - "ActiveVisitDto.IsCompleted for done-today column"
  - "PUT /api/clinical/{visitId}/reverse-stage endpoint"
  - "GET /api/clinical/patients/{patientId}/visit-history endpoint"
affects: [13-02, 13-03, 13-04, 13-05, 13-06]

tech-stack:
  added: []
  patterns: [domain-allowed-transitions-table, auto-advance-on-sign-off, done-today-timezone-aware-query]

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Application/Features/ReverseWorkflowStage.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetPatientVisitHistory.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/PatientVisitHistoryDto.cs
    - backend/tests/Clinical.Unit.Tests/Domain/VisitReverseStageTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/ReverseWorkflowStageHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/SignOffVisitAutoAdvanceTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/GetPatientVisitHistoryHandlerTests.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/ActiveVisitDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/CreateVisitCommand.cs
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/SignOffVisit.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetActiveVisits.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
    - backend/tests/Clinical.Unit.Tests/Features/GetActiveVisitsHandlerTests.cs

key-decisions:
  - "Reason validation checked before allowed-table lookup for clearer error messages"
  - "Repository implements timezone-aware done-today filter using SE Asia Standard Time"
  - "Existing GetActiveVisitsAsync kept for backward compatibility"

patterns-established:
  - "Domain allowed-transitions table: static Dictionary<FromStage, HashSet<ToStage>> pattern for stage reversal"
  - "Auto-advance pattern: sign-off handler advances to next sequential stage in same transaction"

requirements-completed: [CLN-03, CLN-04]

duration: 9min
completed: 2026-03-25
---

# Phase 13 Plan 01: Backend TDD - Stage Reversal, Auto-Advance, Done-Today, Visit History Summary

**Bidirectional stage transitions with AllowedReversals table, sign-off auto-advance, done-today filter with IsCompleted flag, and patient visit history endpoint**

## Performance

- **Duration:** 9 min
- **Started:** 2026-03-25T06:17:50Z
- **Completed:** 2026-03-25T06:27:05Z
- **Tasks:** 2
- **Files modified:** 16

## Accomplishments
- Visit.ReverseStage domain method with AllowedReversals transition table enforcing D-07 rules (Cashier/PharmacyOptical cannot reverse)
- Sign-off auto-advance to next sequential stage in same transaction (D-11)
- Active visits query returns done-today visits with IsCompleted=true (D-04/D-10)
- Patient visit history endpoint with primary diagnosis text mapping (D-13/D-15)
- 24 new unit tests added, all 207 Clinical.Unit.Tests passing

## Task Commits

Each task was committed atomically:

1. **Task 1: Domain + contracts + repository interface for stage reversal and visit history** - `895cb58` (feat)
2. **Task 2: Application handlers, repository implementations, API endpoints** - `44dd239` (feat)

## Files Created/Modified
- `Visit.cs` - Added ReverseStage method with AllowedReversals transition table
- `ActiveVisitDto.cs` - Added IsCompleted field for done-today display
- `CreateVisitCommand.cs` - Added ReverseWorkflowStageCommand and GetPatientVisitHistoryQuery records
- `PatientVisitHistoryDto.cs` - New DTO for visit history display
- `IVisitRepository.cs` - Added GetVisitsByPatientIdAsync and GetActiveVisitsIncludingDoneTodayAsync
- `ReverseWorkflowStage.cs` - New CQRS handler for stage reversal with domain validation
- `GetPatientVisitHistory.cs` - New handler returning visits ordered by date with primary diagnosis
- `SignOffVisit.cs` - Added auto-advance after sign-off (D-11)
- `GetActiveVisits.cs` - Updated to use done-today query with IsCompleted mapping
- `VisitRepository.cs` - Implemented timezone-aware done-today and patient history queries
- `ClinicalApiEndpoints.cs` - Added reverse-stage and visit-history endpoints

## Decisions Made
- Reason validation is checked before the allowed-table lookup in ReverseStage for clearer error messages
- Repository done-today filter uses SE Asia Standard Time (UTC+7) for Vietnam timezone-aware date comparison
- Existing GetActiveVisitsAsync method kept in interface for backward compatibility; handler now calls GetActiveVisitsIncludingDoneTodayAsync

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed compilation error from ActiveVisitDto change**
- **Found during:** Task 1 (domain tests)
- **Issue:** Adding IsCompleted to ActiveVisitDto broke GetActiveVisitsHandler compilation
- **Fix:** Updated GetActiveVisitsHandler DTO mapping to include IsCompleted field
- **Files modified:** GetActiveVisits.cs
- **Verification:** All tests pass
- **Committed in:** 895cb58 (Task 1 commit)

**2. [Rule 3 - Blocking] Fixed compilation error from IVisitRepository extension**
- **Found during:** Task 1 (domain tests)
- **Issue:** Adding new interface methods broke VisitRepository compilation
- **Fix:** Implemented GetVisitsByPatientIdAsync and GetActiveVisitsIncludingDoneTodayAsync in VisitRepository
- **Files modified:** VisitRepository.cs
- **Verification:** All tests pass, build succeeds
- **Committed in:** 895cb58 (Task 1 commit)

**3. [Rule 1 - Bug] Fixed Laterality.Bilateral to Laterality.OU in test**
- **Found during:** Task 2 (writing test)
- **Issue:** Plan referenced Laterality.Bilateral but actual enum uses OU (Oculus Uterque)
- **Fix:** Changed to Laterality.OU in GetPatientVisitHistoryHandlerTests
- **Files modified:** GetPatientVisitHistoryHandlerTests.cs
- **Verification:** Test compiles and passes
- **Committed in:** 44dd239 (Task 2 commit)

---

**Total deviations:** 3 auto-fixed (1 bug, 2 blocking)
**Impact on plan:** All auto-fixes necessary for compilation. No scope creep.

## Issues Encountered
None - plan executed smoothly after fixing expected compilation cascade from contract changes.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All backend APIs ready for frontend integration in plans 13-02 through 13-06
- Stage reversal, auto-advance, done-today, and visit history all tested and operational
- No database migrations needed (no schema changes, only domain logic and application code)

## Self-Check: PASSED

- All 8 key files verified present
- Task 1 commit 895cb58 verified
- Task 2 commit 44dd239 verified
- All 207 Clinical.Unit.Tests passing
- Backend Bootstrapper build succeeds

---
*Phase: 13-clinical-workflow-overhaul*
*Completed: 2026-03-25*

---
phase: 10-address-all-pending-todos
plan: 03
subsystem: api
tags: [dotnet, signalr, dry-eye, osdi, tdd, wolverine]

# Dependency graph
requires:
  - phase: 05-clinical-module
    provides: DryEyeAssessment entity, Visit aggregate, OsdiSubmission entity
provides:
  - Dry eye metric history endpoint with per-metric time series and time range filtering
  - OSDI answers endpoint returning structured answers grouped by category
  - OsdiHub SignalR hub for realtime OSDI submission push notifications
  - OsdiNotificationService for fire-and-forget broadcasting
affects: [frontend-dry-eye-trends, frontend-osdi-display]

# Tech tracking
tech-stack:
  added: []
  patterns: [per-metric-time-series, signalr-hub-per-module, fire-and-forget-notification]

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Application/Features/DryEye/GetDryEyeMetricHistory.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DryEyeMetricHistoryDto.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/Osdi/GetOsdiAnswers.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiAnswersDto.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Hubs/OsdiHub.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Services/OsdiNotificationService.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/Osdi/NotifyOsdiSubmitted.cs
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IOsdiNotificationService.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/IoC.cs
    - backend/src/Bootstrapper/Program.cs

key-decisions:
  - "Used single joined query for metric history to avoid N+1 (GetMetricHistoryAsync)"
  - "Followed BillingHub pattern exactly for OsdiHub (Authorize, JoinVisit/LeaveVisit)"
  - "OsdiNotificationService uses fire-and-forget pattern matching BillingNotificationService"

patterns-established:
  - "Per-metric time series: return 5 named metric arrays with OD/OS values per visit"
  - "Clinical SignalR hub: OsdiHub follows same group-based pattern as BillingHub"

requirements-completed: [TODO-07, TODO-10, TODO-13]

# Metrics
duration: 9min
completed: 2026-03-14
---

# Phase 10 Plan 03: Clinical Module Enhancements Summary

**Dry eye per-metric trend history with time range filtering, structured OSDI answers endpoint, and OsdiHub SignalR for realtime OSDI push**

## Performance

- **Duration:** 9 min
- **Started:** 2026-03-14T06:43:33Z
- **Completed:** 2026-03-14T06:52:11Z
- **Tasks:** 2
- **Files modified:** 19

## Accomplishments
- Dry eye metric history endpoint returns 5 time-series (TBUT, Schirmer, MeibomianGrading, TearMeniscus, StainingScore) with OD/OS values per visit and time range filtering (3m, 6m, 1y, all)
- OSDI answers endpoint returns 12 questions grouped by category (Vision Q1-5, Eye Symptoms Q6-9, Environmental Triggers Q10-12) with bilingual text and individual scores
- OsdiHub registered at /api/hubs/osdi with visit group join/leave for realtime OSDI submission notifications
- 15 new unit tests (8 metric history + 5 OSDI answers + 2 notification service), all 162 clinical tests pass

## Task Commits

Each task was committed atomically:

1. **Task 1: Dry eye metric history endpoint with time range filtering** - `4977d22` (feat)
2. **Task 2: OSDI answers endpoint + OsdiHub SignalR for realtime push** - `e70e4ab` (feat, combined with concurrent plan execution)

## Files Created/Modified
- `Clinical.Contracts/Dtos/DryEyeMetricHistoryDto.cs` - DTOs for per-metric time series response
- `Clinical.Application/Features/DryEye/GetDryEyeMetricHistory.cs` - Handler with time range cutoff calculation
- `Clinical.Contracts/Dtos/OsdiAnswersDto.cs` - DTOs for grouped OSDI answers
- `Clinical.Application/Features/Osdi/GetOsdiAnswers.cs` - Handler parsing AnswersJson with bilingual question mapping
- `Clinical.Infrastructure/Hubs/OsdiHub.cs` - SignalR hub for visit group subscriptions
- `Clinical.Infrastructure/Services/OsdiNotificationService.cs` - Fire-and-forget SignalR broadcaster
- `Clinical.Application/Features/Osdi/NotifyOsdiSubmitted.cs` - Event handler triggering SignalR push
- `Clinical.Application/Interfaces/IOsdiNotificationService.cs` - Service interface
- `IVisitRepository.cs` - Added GetMetricHistoryAsync method
- `VisitRepository.cs` - Implemented joined query for metric history
- `ClinicalApiEndpoints.cs` - Added 2 new GET endpoints
- `IoC.cs` - Registered IOsdiNotificationService
- `Program.cs` - Mapped OsdiHub at /api/hubs/osdi

## Decisions Made
- Used single joined query (DryEyeAssessments JOIN Visits) for metric history to avoid N+1 problem
- Followed BillingHub pattern exactly for OsdiHub (Authorize, group-based join/leave)
- OsdiNotificationService uses fire-and-forget pattern (log warnings, never throw) matching BillingNotificationService
- Time range filtering done at SQL level via cutoff date parameter

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed pre-existing PrintBatchLabelsHandlerTests build error**
- **Found during:** Task 1
- **Issue:** PrintBatchLabelsHandlerTests.cs referenced non-existent namespace; DocumentService missing GenerateBatchPharmacyLabelsAsync implementation
- **Fix:** Both issues were resolved by concurrent auto-fix (linter/auto-process added the missing implementation and test file was corrected)
- **Files modified:** PrintBatchLabelsHandlerTests.cs, DocumentService.cs
- **Committed in:** 4977d22 (Task 1 commit)

**2. [Rule 3 - Blocking] Task 2 commit merged with concurrent plan execution**
- **Found during:** Task 2 commit
- **Issue:** A concurrent auto-process committed task 2 files as part of commit e70e4ab (labeled as 10-02 drug catalog import)
- **Fix:** All task 2 files are present and verified in the commit. No re-work needed.

---

**Total deviations:** 2 auto-fixed (2 blocking)
**Impact on plan:** Both issues resolved without scope creep. All planned functionality delivered.

## Issues Encountered
- Pre-existing build errors in PrintBatchLabelsHandlerTests.cs and DocumentService.cs blocked initial compilation; resolved via auto-fix

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Backend endpoints ready for frontend consumption
- Frontend can now build dry eye trend charts using GET /api/clinical/patients/{patientId}/dry-eye/metric-history
- Frontend can display OSDI answers using GET /api/clinical/visits/{visitId}/osdi-answers
- Frontend can connect to /api/hubs/osdi for realtime OSDI score updates

## Self-Check: PASSED

- All 11 created files verified present on disk
- Commits 4977d22 and e70e4ab verified in git history
- 162 clinical unit tests passing
- Full backend build succeeds with 0 errors

---
*Phase: 10-address-all-pending-todos*
*Completed: 2026-03-14*

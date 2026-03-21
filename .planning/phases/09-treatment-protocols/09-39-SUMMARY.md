---
phase: 09-treatment-protocols
plan: 39
subsystem: clinical
tags: [signalr, osdi, qr-code, real-time, treatment-session]

requires:
  - phase: 09-treatment-protocols
    provides: "OSDI QR token registration and public submission page"
provides:
  - "Token-scoped SignalR group support in OsdiHub"
  - "Real-time OSDI score notification from patient QR submission to clinician session form"
  - "useOsdiTokenHub hook for treatment session OSDI self-fill flow"
affects: [treatment-sessions, clinical-osdi]

tech-stack:
  added: []
  patterns: ["Token-scoped SignalR groups alongside visit-scoped groups"]

key-files:
  created: []
  modified:
    - "backend/src/Modules/Clinical/Clinical.Infrastructure/Hubs/OsdiHub.cs"
    - "backend/src/Modules/Clinical/Clinical.Infrastructure/Services/OsdiNotificationService.cs"
    - "backend/src/Modules/Clinical/Clinical.Application/Features/SubmitOsdiQuestionnaire.cs"
    - "backend/src/Modules/Clinical/Clinical.Application/Interfaces/IOsdiNotificationService.cs"
    - "frontend/src/features/clinical/hooks/use-osdi-hub.ts"
    - "frontend/src/features/treatment/components/SessionOsdiCapture.tsx"
    - "frontend/public/locales/en/treatment.json"
    - "frontend/public/locales/vi/treatment.json"

key-decisions:
  - "Used token-scoped SignalR groups (osdi-token-{token}) instead of polling for real-time score delivery"
  - "Used PublicToken property (not Token) as the OsdiSubmission entity field name"

patterns-established:
  - "Token-scoped SignalR groups: JoinToken/LeaveToken pattern for non-visit-scoped subscriptions"

requirements-completed: [TRT-06]

duration: 7min
completed: 2026-03-21
---

# Phase 09 Plan 39: OSDI QR Self-Fill Score Capture Summary

**Real-time OSDI score capture from patient QR self-fill to clinician session form via token-scoped SignalR groups**

## Performance

- **Duration:** 7 min
- **Started:** 2026-03-21T09:07:18Z
- **Completed:** 2026-03-21T09:13:49Z
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments
- Token-scoped SignalR group support (JoinToken/LeaveToken) added to OsdiHub
- NotifyTokenSubmittedAsync method added to notification service for pushing scores to token groups
- SubmitOsdiQuestionnaireHandler now fires both token and visit notifications after OSDI save
- Frontend useOsdiTokenHub hook subscribes to token group and auto-populates score on receipt
- SessionOsdiCapture shows success indicator when patient submits via QR

## Task Commits

Each task was committed atomically:

1. **Task 1: Add token-scoped SignalR support and publish notification on OSDI submission** - `25fb144` (feat)
2. **Task 2: Add frontend SignalR listener in SessionOsdiCapture for score return** - `ad4e4cd` (feat)

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Application/Interfaces/IOsdiNotificationService.cs` - Added NotifyTokenSubmittedAsync interface method
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Hubs/OsdiHub.cs` - Added JoinToken/LeaveToken group methods
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Services/OsdiNotificationService.cs` - Implemented token-scoped notification
- `backend/src/Modules/Clinical/Clinical.Application/Features/SubmitOsdiQuestionnaire.cs` - Wired notification calls after save
- `frontend/src/features/clinical/hooks/use-osdi-hub.ts` - Added useOsdiTokenHub hook
- `frontend/src/features/treatment/components/SessionOsdiCapture.tsx` - Wired SignalR subscription and success UI
- `frontend/public/locales/en/treatment.json` - Added selfFillReceived translation
- `frontend/public/locales/vi/treatment.json` - Added selfFillReceived translation

## Decisions Made
- Used token-scoped SignalR groups (osdi-token-{token}) for real-time delivery, matching the existing visit-scoped pattern but without requiring a VisitId
- Used PublicToken property name from OsdiSubmission entity (plan referenced it as Token)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed property name Token -> PublicToken**
- **Found during:** Task 1 (backend notification wiring)
- **Issue:** Plan referenced `submission.Token` but OsdiSubmission entity uses `PublicToken`
- **Fix:** Changed to `submission.PublicToken!` in the notification call
- **Files modified:** SubmitOsdiQuestionnaire.cs
- **Verification:** Backend build succeeds
- **Committed in:** 25fb144 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 bug)
**Impact on plan:** Minor property name correction. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- OSDI QR self-fill score capture pipeline is complete end-to-end
- Requires E2E testing: Record Session > Self-Fill tab > Generate QR > Submit on patient device > Score appears in form
- Existing visit-based OSDI flow remains fully backward compatible

---
*Phase: 09-treatment-protocols*
*Completed: 2026-03-21*

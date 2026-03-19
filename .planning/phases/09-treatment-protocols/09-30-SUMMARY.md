---
phase: 09-treatment-protocols
plan: 30
subsystem: clinical, treatment, ui
tags: [osdi, qr-code, cross-module, wolverine, i18n, ef-core-migration]

# Dependency graph
requires:
  - phase: 09-treatment-protocols
    provides: Treatment session recording, OSDI section in session form
provides:
  - DB-backed OSDI token flow for treatment session self-fill QR
  - Full 12-question OsdiQuestionnaire in session inline tab
  - i18n support for SessionOsdiCapture component
affects: [treatment-sessions, osdi-public-page, clinical-module]

# Tech tracking
tech-stack:
  added: []
  patterns: [cross-module-command-via-wolverine-message-bus, nullable-fk-for-optional-relationships]

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Contracts/IntegrationEvents/CreateOsdiTokenForTreatmentCommand.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/CreateOsdiTokenForTreatmentHandler.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Migrations/20260319093153_MakeOsdiVisitIdNullable.cs
    - backend/tests/Clinical.Unit.Tests/Domain/OsdiSubmissionDomainTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/CreateOsdiTokenForTreatmentHandlerTests.cs
    - backend/tests/Treatment.Unit.Tests/Features/RegisterOsdiTokenHandlerTests.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/OsdiSubmission.cs
    - backend/src/Modules/Treatment/Treatment.Application/Features/RegisterOsdiToken.cs
    - frontend/src/features/treatment/components/SessionOsdiCapture.tsx
    - frontend/src/features/treatment/api/treatment-api.ts
    - frontend/public/locales/en/treatment.json
    - frontend/public/locales/vi/treatment.json

key-decisions:
  - "Cross-module command via IMessageBus instead of direct repository access from Treatment to Clinical"
  - "Nullable VisitId on OsdiSubmission to support treatment-session tokens without clinical visit"
  - "Reuse existing OsdiQuestionnaire component instead of building new inline questionnaire"

patterns-established:
  - "Cross-module integration: Treatment module sends command to Clinical module via Wolverine IMessageBus for DB-backed OSDI tokens"

requirements-completed: [TRT-06]

# Metrics
duration: 13min
completed: 2026-03-19
---

# Phase 09 Plan 30: OSDI Treatment Session Fix Summary

**DB-backed OSDI self-fill tokens via cross-module Wolverine command, full 12-question questionnaire in inline tab, and i18n for SessionOsdiCapture**

## Performance

- **Duration:** 13 min
- **Started:** 2026-03-19T09:21:41Z
- **Completed:** 2026-03-19T09:34:41Z
- **Tasks:** 2
- **Files modified:** 24

## Accomplishments
- OSDI self-fill QR tokens now persist in Clinical module's database via cross-module IMessageBus command, fixing the broken public page lookup
- Inline OSDI tab shows full 12-question OsdiQuestionnaire with auto-score calculation instead of single number input
- All hardcoded Vietnamese strings in SessionOsdiCapture replaced with useTranslation t() calls
- InMemoryOsdiTokenStore completely removed from Treatment module

## Task Commits

Each task was committed atomically:

1. **Task 1: Backend - DB-backed OSDI token for treatment sessions** - `eea1bd2` (fix)
2. **Task 2: Frontend - Replace OSDI number input with OsdiQuestionnaire + add i18n** - `52c9366` (feat)

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/OsdiSubmission.cs` - Nullable VisitId, new factory method
- `backend/src/Modules/Clinical/Clinical.Contracts/IntegrationEvents/CreateOsdiTokenForTreatmentCommand.cs` - Cross-module command/response records
- `backend/src/Modules/Clinical/Clinical.Application/Features/CreateOsdiTokenForTreatmentHandler.cs` - Wolverine handler for DB-backed token creation
- `backend/src/Modules/Treatment/Treatment.Application/Features/RegisterOsdiToken.cs` - Rewired to use IMessageBus instead of IOsdiTokenStore
- `backend/src/Modules/Treatment/Treatment.Infrastructure/IoC.cs` - Removed IOsdiTokenStore registration
- `frontend/src/features/treatment/components/SessionOsdiCapture.tsx` - OsdiQuestionnaire + i18n
- `frontend/src/features/treatment/api/treatment-api.ts` - Added url field to response type
- `frontend/public/locales/en/treatment.json` - New OSDI translation keys
- `frontend/public/locales/vi/treatment.json` - New OSDI translation keys (Vietnamese)

## Decisions Made
- Used cross-module Wolverine IMessageBus command (CreateOsdiTokenForTreatmentCommand) to create DB-backed tokens rather than having Treatment module directly access Clinical repositories
- Made OsdiSubmission.VisitId nullable to support treatment-session tokens that don't have a linked clinical visit
- Reused existing OsdiQuestionnaire component from clinical module rather than building a new one

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed nullable VisitId compilation errors in GetOsdiByToken and SubmitOsdiQuestionnaire**
- **Found during:** Task 1 (Backend implementation)
- **Issue:** Making VisitId nullable broke 3 compilation points where Guid? was passed as Guid
- **Fix:** Added .HasValue checks and .Value access for nullable VisitId in GetOsdiByTokenHandler, SubmitOsdiQuestionnaireHandler, and OsdiSubmissionRepository
- **Files modified:** GetOsdiByToken.cs, SubmitOsdiQuestionnaire.cs, OsdiSubmissionRepository.cs
- **Verification:** Build succeeds, all 181 Clinical tests pass
- **Committed in:** eea1bd2 (Task 1 commit)

**2. [Rule 1 - Bug] Fixed VisitId nullable reference in Clinical unit test**
- **Found during:** Task 1 (Backend implementation)
- **Issue:** Existing test accessed submission.VisitId as Guid when it became Guid?
- **Fix:** Changed to submission.VisitId!.Value in SubmitOsdiQuestionnaireHandlerTests
- **Files modified:** SubmitOsdiQuestionnaireHandlerTests.cs
- **Committed in:** eea1bd2 (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (2 Rule 1 bugs)
**Impact on plan:** Both fixes necessary due to nullable VisitId change. No scope creep.

## Issues Encountered
- Backend process was locking DLL files during build - killed the running Bootstrapper process to proceed
- Pre-existing Optical.Unit.Tests compilation errors (unrelated to this plan) - ignored per scope boundary rules

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- OSDI self-fill flow is now fully functional with DB-backed tokens
- OsdiQuestionnaire component is reused across both Clinical visits and Treatment sessions
- Translation keys are in place for both English and Vietnamese

---
*Phase: 09-treatment-protocols*
*Completed: 2026-03-19*

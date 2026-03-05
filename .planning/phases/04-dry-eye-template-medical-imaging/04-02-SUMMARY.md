---
phase: 04-dry-eye-template-medical-imaging
plan: 02
subsystem: api
tags: [dry-eye, osdi, clinical, tdd, wolverine-handlers, fluent-validation]

# Dependency graph
requires:
  - phase: 04-dry-eye-template-medical-imaging
    plan: 01a
    provides: DryEyeAssessment, OsdiSubmission domain entities and enums
  - phase: 04-dry-eye-template-medical-imaging
    plan: 01b
    provides: EF Core configurations, repositories, migration for dry eye data access
provides:
  - UpdateDryEyeAssessmentHandler with find-or-create pattern and FluentValidation
  - OsdiCalculator static utility with formula and 4-tier severity classification
  - SubmitOsdiQuestionnaireHandler for public token-based OSDI submission
  - GenerateOsdiLinkHandler with cryptographically secure URL-safe base64 token
  - GetOsdiByTokenHandler returning 12 bilingual OSDI questions
  - GetOsdiHistoryHandler returning chronological OSDI data for trend chart
  - GetDryEyeComparisonHandler for cross-visit dry eye metrics comparison
  - 5 test files with 36 tests covering all handlers and OSDI edge cases
affects: [04-04, 04-05, 04-06]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "OsdiCalculator: static utility class for OSDI formula with null-safe division"
    - "URL-safe base64 token: Replace +/= with -/_ for URL-safe cryptographic tokens"
    - "Bilingual OSDI questions: Static readonly list with Vietnamese diacritics in handler"
    - "Public endpoint handler: No IValidator injection, manual null/expiry checks instead"

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Application/Features/UpdateDryEyeAssessment.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/OsdiCalculator.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/SubmitOsdiQuestionnaire.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GenerateOsdiLink.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetOsdiByToken.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetOsdiHistory.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetDryEyeComparison.cs
    - backend/tests/Clinical.Unit.Tests/Features/UpdateDryEyeAssessmentHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/OsdiCalculationTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/SubmitOsdiQuestionnaireHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/GetOsdiHistoryHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/GetDryEyeComparisonHandlerTests.cs
  modified: []

key-decisions:
  - "Query records defined in Contracts project (not Application) to avoid ambiguous references across module boundaries"
  - "OsdiCalculator as static utility class co-located with handlers for formula reuse across SubmitOsdi and UpdateDryEye"
  - "URL-safe base64 tokens: Replace +/= with -/_ to avoid URL encoding issues in public OSDI links"
  - "GetOsdiHistory loads visit date per-assessment via GetByIdAsync rather than modifying repository projection"

patterns-established:
  - "OSDI calculation: OsdiCalculator.Calculate(int?[]) returning nullable OsdiResult with score/severity/count"
  - "Public submission handler: token lookup + expiry check + score calculation + reflection-based entity update"

requirements-completed: [DRY-01, DRY-02, DRY-03, DRY-04]

# Metrics
duration: 10min
completed: 2026-03-05
---

# Phase 04 Plan 02: Dry Eye & OSDI Backend Handlers Summary

**7 Wolverine handlers implementing dry eye assessment CRUD, OSDI score calculation with division-by-zero guard, public token-based OSDI self-fill, chronological OSDI history, and cross-visit dry eye comparison with 36 unit tests**

## Performance

- **Duration:** 10 min
- **Started:** 2026-03-05T06:28:52Z
- **Completed:** 2026-03-05T06:39:42Z
- **Tasks:** 2
- **Files modified:** 12

## Accomplishments
- Implemented UpdateDryEyeAssessmentHandler following the existing UpdateRefraction find-or-create pattern with FluentValidation for Meibomian 0-3, Staining 0-5, TBUT/Schirmer/TearMeniscus >= 0
- Created OsdiCalculator with (sum*100)/(answered*4) formula, division-by-zero guard (returns null when 0 answered), and 4-tier severity classification (Normal 0-12, Mild 13-22, Moderate 23-32, Severe 33+)
- Implemented SubmitOsdiQuestionnaireHandler for public token-based OSDI submission with token expiry validation and automatic DryEyeAssessment OSDI field update
- Created GenerateOsdiLinkHandler generating cryptographically secure URL-safe base64 tokens (32 bytes via RandomNumberGenerator) with 24-hour expiry
- Implemented GetOsdiByTokenHandler returning 12 OSDI questions with full Vietnamese diacritics and English text
- Created GetOsdiHistoryHandler returning chronological OSDI data points filtered to non-null scores for trend chart
- Implemented GetDryEyeComparisonHandler for side-by-side two-visit comparison with cross-patient security check

## Task Commits

Each task was committed atomically (TDD: RED then GREEN):

1. **Task 1: Dry Eye + OSDI handlers** - `d286f1e` (test RED) + `94c4a0e` (feat GREEN)
2. **Task 2: OSDI history + comparison handlers** - `c28102f` (test RED) + `a1ebdea` (feat GREEN)

**Plan metadata:** (pending)

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Application/Features/UpdateDryEyeAssessment.cs` - Validator + Handler for dry eye assessment CRUD with find-or-create pattern
- `backend/src/Modules/Clinical/Clinical.Application/Features/OsdiCalculator.cs` - Static OSDI score calculator with formula and severity classification
- `backend/src/Modules/Clinical/Clinical.Application/Features/SubmitOsdiQuestionnaire.cs` - Public token-based OSDI submission handler
- `backend/src/Modules/Clinical/Clinical.Application/Features/GenerateOsdiLink.cs` - Cryptographic token generation with 24h expiry
- `backend/src/Modules/Clinical/Clinical.Application/Features/GetOsdiByToken.cs` - 12 bilingual OSDI questions via token lookup
- `backend/src/Modules/Clinical/Clinical.Application/Features/GetOsdiHistory.cs` - Chronological OSDI trend data query
- `backend/src/Modules/Clinical/Clinical.Application/Features/GetDryEyeComparison.cs` - Cross-visit dry eye metrics comparison
- `backend/tests/Clinical.Unit.Tests/Features/UpdateDryEyeAssessmentHandlerTests.cs` - 6 tests: create, update, not-found, signed, validation, all-null
- `backend/tests/Clinical.Unit.Tests/Features/OsdiCalculationTests.cs` - 11 tests: all edge cases for OSDI formula and severity boundaries
- `backend/tests/Clinical.Unit.Tests/Features/SubmitOsdiQuestionnaireHandlerTests.cs` - 10 tests: submit, generate link, get by token with expiry/invalid scenarios
- `backend/tests/Clinical.Unit.Tests/Features/GetOsdiHistoryHandlerTests.cs` - 4 tests: 3 visits, empty, no assessments, filter null scores
- `backend/tests/Clinical.Unit.Tests/Features/GetDryEyeComparisonHandlerTests.cs` - 5 tests: both visits, one empty, both empty, not-found, cross-patient

## Decisions Made
- Query records (GetOsdiHistoryQuery, GetDryEyeComparisonQuery, GetOsdiByTokenQuery) defined in Contracts project, not Application, to avoid ambiguous references when tests reference both namespaces
- OsdiCalculator implemented as a static utility class co-located with handlers in Application.Features for reuse by both SubmitOsdiQuestionnaire and future doctor-recorded OSDI
- URL-safe base64 tokens replace +/= with -/_ and trim trailing = for clean URLs in public OSDI self-fill links
- GetOsdiHistory loads VisitDate per-assessment via individual GetByIdAsync calls rather than modifying repository projection (pragmatic for expected data volume of 4-12 visits per patient)
- SubmitOsdiQuestionnaire uses reflection to update OsdiSubmission private setters (pragmatic approach for updating entity after creation, matching existing patterns)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Removed duplicate query record definitions**
- **Found during:** Task 2 (GetOsdiHistory and GetDryEyeComparison implementation)
- **Issue:** GetOsdiHistoryQuery, GetDryEyeComparisonQuery, and GetOsdiByTokenQuery were defined in both Contracts.Dtos and Application.Features, causing ambiguous reference errors
- **Fix:** Removed the duplicate definitions from handler files, using Contracts definitions instead
- **Files modified:** GetOsdiHistory.cs, GetDryEyeComparison.cs, GetOsdiByToken.cs
- **Verification:** Build succeeds, all tests pass
- **Committed in:** a1ebdea (Task 2 feat commit)

---

**Total deviations:** 1 auto-fixed (1 bug)
**Impact on plan:** Minor naming collision fix. No scope creep.

## Issues Encountered
- Parallel plan 04-03 committed RED tests that initially blocked compilation of the test project. The image handler implementations were found to already exist from parallel execution, resolving the build issue without intervention.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 7 dry eye/OSDI handlers ready for endpoint wiring in Plan 04-04 (API endpoints) or Plan 04-05 (frontend)
- Handler signatures ready for Wolverine bus.InvokeAsync pattern in Presentation layer
- OsdiCalculator reusable for future doctor-recorded OSDI flow (not just patient self-fill)
- IOsdiSubmissionRepository and IVisitRepository patterns established for downstream plans
- All 100 Clinical.Unit.Tests pass (including pre-existing + new handlers)

## Self-Check: PASSED

- All 7 handler files verified on disk
- All 5 test files verified on disk
- Task commits d286f1e, 94c4a0e, c28102f, a1ebdea verified in git log
- All 100 Clinical.Unit.Tests pass
- OSDI calculation edge cases verified: 0 answered returns null, all 0s returns 0.0/Normal, all 4s returns 100.0/Severe

---
*Phase: 04-dry-eye-template-medical-imaging*
*Completed: 2026-03-05*

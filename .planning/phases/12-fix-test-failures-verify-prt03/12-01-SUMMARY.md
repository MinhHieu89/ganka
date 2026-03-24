---
phase: 12-fix-test-failures-verify-prt03
plan: 01
subsystem: testing
tags: [unit-tests, nsubstitute, utc, datetime, optical, scheduling]

# Dependency graph
requires:
  - phase: 06-optical-center
    provides: "GetWarrantyClaimsHandler with IGlassesOrderRepository parameter"
  - phase: 04-scheduling
    provides: "AppointmentDto and scheduling handler tests"
provides:
  - "Optical.Unit.Tests fully passing (174 tests)"
  - "Scheduling.Unit.Tests fully passing (11 tests) with UTC enforcement at DTO level"
  - "AppointmentDto global UTC convention pattern for DateTime DTOs"
affects: [scheduling, optical]

# Tech tracking
tech-stack:
  added: []
  patterns: ["UTC enforcement at DTO constructor level instead of per-handler SpecifyKind"]

key-files:
  created: []
  modified:
    - "backend/tests/Optical.Unit.Tests/Features/WarrantyHandlerTests.cs"
    - "backend/src/Modules/Scheduling/Scheduling.Contracts/Dtos/AppointmentDto.cs"

key-decisions:
  - "UTC enforcement at DTO constructor level (global convention per D-03) rather than per-handler DateTime.SpecifyKind"

patterns-established:
  - "DTO UTC pattern: DateTime DTOs enforce DateTimeKind.Utc in constructor for mock-safe unit testing"

requirements-completed: [OPT-07]

# Metrics
duration: 4min
completed: 2026-03-24
---

# Phase 12 Plan 01: Fix Test Failures Summary

**Fixed Optical.Unit.Tests build failure (missing IGlassesOrderRepository mock) and Scheduling.Unit.Tests UTC assertion failures (DTO-level DateTimeKind.Utc enforcement)**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-24T10:03:47Z
- **Completed:** 2026-03-24T10:07:35Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- Optical.Unit.Tests restored to green: added IGlassesOrderRepository mock parameter to all 5 GetWarrantyClaimsHandler.Handle test calls (174 tests pass)
- Scheduling.Unit.Tests UTC assertion fixed: AppointmentDto constructor enforces DateTimeKind.Utc on StartTime/EndTime globally (11 tests pass)
- Clinical.Unit.Tests regression confirmed: all 183 tests pass unchanged

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix Optical.Unit.Tests -- add IGlassesOrderRepository mock** - `9a1d602` (fix)
2. **Task 2: Fix Scheduling UTC -- enforce DateTimeKind.Utc at DTO level** - `bbb9e9e` (fix)

## Files Created/Modified
- `backend/tests/Optical.Unit.Tests/Features/WarrantyHandlerTests.cs` - Added _orderRepo mock field and updated 5 Handle call sites to pass 4 arguments
- `backend/src/Modules/Scheduling/Scheduling.Contracts/Dtos/AppointmentDto.cs` - Converted from positional record to record with UTC-enforcing constructor

## Decisions Made
- UTC enforcement placed at DTO constructor level (not per-handler) per D-03 decision -- this means any code constructing an AppointmentDto automatically gets UTC-normalized DateTimes, including unit test mocks that bypass EF Core's global converter

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- NuGet restore needed before running Optical.Unit.Tests (worktree had no cached assets) -- resolved with dotnet restore

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All three test suites green: Optical (174), Scheduling (11), Clinical (183)
- CI should be fully green for these modules
- No known stubs or deferred items

---
*Phase: 12-fix-test-failures-verify-prt03*
*Completed: 2026-03-24*

## Self-Check: PASSED

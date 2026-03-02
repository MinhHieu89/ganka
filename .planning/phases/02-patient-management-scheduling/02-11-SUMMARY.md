---
phase: 02-patient-management-scheduling
plan: 11
subsystem: api
tags: [ef-core, linq, timezone, fluent-validation, rfc-7807, tdd]

# Dependency graph
requires:
  - phase: 02-patient-management-scheduling
    provides: Patient module, Scheduling module, Shared.Presentation ResultExtensions
provides:
  - Substring search for patient code and phone in PatientRepository
  - UTC-to-Vietnam timezone conversion for scheduling validation
  - Structured field-level validation errors (RFC 7807 errors dictionary)
  - Scheduling.Unit.Tests and Shared.Unit.Tests projects
affects: [02-12, 02-13, 02-14, frontend-patient-search, frontend-booking, frontend-validation]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Error.ValidationWithDetails for structured field-level validation"
    - "Results.ValidationProblem for RFC 7807 errors dictionary in HTTP responses"
    - "TimeZoneInfo.ConvertTimeFromUtc for cross-platform UTC-to-local conversion"

key-files:
  created:
    - backend/tests/Scheduling.Unit.Tests/Scheduling.Unit.Tests.csproj
    - backend/tests/Scheduling.Unit.Tests/Features/BookAppointmentHandlerTests.cs
    - backend/tests/Scheduling.Unit.Tests/Features/ApproveSelfBookingHandlerTests.cs
    - backend/tests/Patient.Unit.Tests/Features/SearchPatientsHandlerTests.cs
    - backend/tests/Shared.Unit.Tests/Shared.Unit.Tests.csproj
    - backend/tests/Shared.Unit.Tests/ErrorTests.cs
    - backend/tests/Shared.Unit.Tests/ResultExtensionsTests.cs
  modified:
    - backend/src/Modules/Patient/Patient.Infrastructure/Repositories/PatientRepository.cs
    - backend/src/Modules/Scheduling/Scheduling.Application/Features/ApproveSelfBooking.cs
    - backend/src/Modules/Scheduling/Scheduling.Application/Features/BookAppointment.cs
    - backend/src/Shared/Shared.Domain/Error.cs
    - backend/src/Shared/Shared.Presentation/ResultExtensions.cs
    - backend/src/Modules/Patient/Patient.Application/Features/RegisterPatient.cs
    - backend/src/Modules/Auth/Auth.Application/Features/CreateUser.cs
    - backend/src/Modules/Auth/Auth.Application/Features/CreateRole.cs
    - backend/src/Modules/Auth/Auth.Application/Features/Login.cs
    - backend/src/Modules/Auth/Auth.Application/Features/UpdateLanguage.cs
    - backend/src/Modules/Patient/Patient.Application/Features/AddAllergy.cs
    - backend/src/Modules/Patient/Patient.Application/Features/UpdatePatient.cs
    - backend/src/Modules/Scheduling/Scheduling.Application/Features/CancelAppointment.cs
    - backend/src/Modules/Scheduling/Scheduling.Application/Features/SubmitSelfBooking.cs
    - backend/src/Modules/Scheduling/Scheduling.Application/Features/RejectSelfBooking.cs
    - backend/src/Modules/Scheduling/Scheduling.Application/Features/RescheduleAppointment.cs

key-decisions:
  - "Cross-platform timezone: SE Asia Standard Time (Windows) / Asia/Ho_Chi_Minh (Linux) via OperatingSystem.IsWindows()"
  - "Error.ValidationWithDetails uses init property on sealed record for ValidationErrors dictionary"
  - "In .NET 10, Results.ValidationProblem returns ProblemHttpResult with HttpValidationProblemDetails (not a separate ValidationProblem type)"
  - "All 13 handlers updated from flat string.Join to GroupBy/ToDictionary structured errors"

patterns-established:
  - "Structured validation: GroupBy PropertyName, ToDictionary with string[] messages, Error.ValidationWithDetails(dict)"
  - "Timezone conversion: DateTime.SpecifyKind(utcTime, DateTimeKind.Utc) then TimeZoneInfo.ConvertTimeFromUtc"
  - "Substring search: .Contains() for both PatientCode and Phone (not StartsWith or exact match)"

requirements-completed: [PAT-02, SCH-01, SCH-05, PAT-01]

# Metrics
duration: 19min
completed: 2026-03-02
---

# Phase 02 Plan 11: Backend Bug Fixes Summary

**Substring patient search, UTC-to-Vietnam timezone conversion, and RFC 7807 structured validation errors across all 13 handlers**

## Performance

- **Duration:** 19 min
- **Started:** 2026-03-02T15:14:07Z
- **Completed:** 2026-03-02T15:33:00Z
- **Tasks:** 2
- **Files modified:** 28

## Accomplishments
- Patient search now returns substring matches for patient code ("0001" matches "GK-2026-0001") and phone ("6543" matches "0987654321")
- Scheduling endpoints correctly convert UTC to Vietnam local time (UTC+7) before validating against clinic operating hours
- All FluentValidation failures return RFC 7807 response with HttpValidationProblemDetails containing field-level error dictionary
- Created Scheduling.Unit.Tests and Shared.Unit.Tests projects with 13 new tests (72 total unit tests pass)

## Task Commits

Each task was committed atomically:

1. **Task 1: TDD - Fix substring search and timezone conversion** - `e17aa08` (fix)
2. **Task 2: TDD - Structured validation error responses** - `5124230` (feat)

## Files Created/Modified
- `backend/src/Modules/Patient/Patient.Infrastructure/Repositories/PatientRepository.cs` - Changed PatientCode == to Contains(), Phone.StartsWith to Contains()
- `backend/src/Modules/Scheduling/Scheduling.Application/Features/BookAppointment.cs` - Added UTC-to-Vietnam timezone conversion before schedule validation
- `backend/src/Modules/Scheduling/Scheduling.Application/Features/ApproveSelfBooking.cs` - Added UTC-to-Vietnam timezone conversion before schedule validation
- `backend/src/Shared/Shared.Domain/Error.cs` - Added ValidationErrors property and ValidationWithDetails factory method
- `backend/src/Shared/Shared.Presentation/ResultExtensions.cs` - Added Results.ValidationProblem mapping for structured errors
- `backend/src/Modules/Patient/Patient.Application/Features/RegisterPatient.cs` - Structured validation errors
- `backend/src/Modules/Auth/Auth.Application/Features/CreateUser.cs` - Structured validation errors
- `backend/src/Modules/Auth/Auth.Application/Features/Login.cs` - Structured validation errors
- 5 additional Auth/Patient/Scheduling handler files - Structured validation errors
- `backend/tests/Scheduling.Unit.Tests/` - New test project with 3 handler tests
- `backend/tests/Shared.Unit.Tests/` - New test project with 10 Error/ResultExtensions tests
- `backend/tests/Patient.Unit.Tests/Features/SearchPatientsHandlerTests.cs` - 4 search handler tests

## Decisions Made
- Cross-platform timezone: `SE Asia Standard Time` (Windows) / `Asia/Ho_Chi_Minh` (Linux) via `OperatingSystem.IsWindows()` check
- In .NET 10, `Results.ValidationProblem` returns `ProblemHttpResult` with `HttpValidationProblemDetails` (not a separate `ValidationProblem` type as in earlier .NET versions) -- tests adapted accordingly
- `Error.ValidationWithDetails` uses `init` property pattern on sealed record to carry `Dictionary<string, string[]>` validation errors
- All 13 handlers across Auth, Patient, and Scheduling modules updated to structured validation pattern for consistency

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- .NET 10 changed `Results.ValidationProblem` return type from `ValidationProblem` to `ProblemHttpResult` with `HttpValidationProblemDetails` -- required adapting test assertions to check `ProblemDetails` type instead of `IResult` type
- Build lock from running Bootstrapper process (PID 63064) -- killed process to unblock test execution

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Backend search, timezone, and validation fixes are deployed -- frontend gap closure plans 12-14 can proceed
- All 72 unit tests pass across 5 test projects
- Architecture test failures (3) are pre-existing and unrelated to these changes

---
*Phase: 02-patient-management-scheduling*
*Completed: 2026-03-02*

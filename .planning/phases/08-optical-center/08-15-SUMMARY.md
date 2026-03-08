---
phase: 08-optical-center
plan: 15
subsystem: testing
tags: [xunit, fluentassertions, nsubstitute, ean13, optical, domain-tests, tdd]

# Dependency graph
requires:
  - phase: 08-optical-center
    provides: "Frame entity, GlassesOrder entity with state machine, domain enums (GlassesOrderStatus, FrameMaterial, FrameType, FrameGender, ProcessingType)"
provides:
  - "Optical.Unit.Tests project scaffold with xUnit/FluentAssertions/NSubstitute"
  - "Ean13Generator utility class in Optical.Domain with Generate, IsValid, CalculateCheckDigit"
  - "FrameTests (10 tests): Create, AdjustStock, Update coverage"
  - "GlassesOrderTests (14 tests): full lifecycle transitions, warranty, overdue"
  - "Ean13GeneratorTests (17 tests): generation, validation, check digit with known vectors"
  - "47 total domain tests all passing GREEN"
affects: [08-16, 08-17, 08-18, 08-19, 08-20, 08-21, 08-22]

# Tech tracking
tech-stack:
  added:
    - "Optical.Unit.Tests (xUnit 2.x, FluentAssertions 8.x, NSubstitute, coverlet.collector)"
    - "Ean13Generator: pure static class in Optical.Domain, no external dependencies"
  patterns:
    - "Static factory method testing: call Frame.Create() / GlassesOrder.Create() then assert properties"
    - "Domain event verification: order.DomainEvents.Should().HaveCount(1) after transition"
    - "State machine testing: advance to 'from' state, then assert invalid transitions throw"
    - "Known EAN-13 test vector: '5901234123457' (payload 590123412345, check digit 7)"

key-files:
  created:
    - "backend/tests/Optical.Unit.Tests/Optical.Unit.Tests.csproj"
    - "backend/tests/Optical.Unit.Tests/Domain/FrameTests.cs"
    - "backend/tests/Optical.Unit.Tests/Domain/GlassesOrderTests.cs"
    - "backend/tests/Optical.Unit.Tests/Domain/Ean13GeneratorTests.cs"
    - "backend/src/Modules/Optical/Optical.Domain/Ean13Generator.cs"
  modified:
    - "backend/Ganka28.slnx (added Optical.Unit.Tests project)"
    - "backend/src/Modules/Optical/Optical.Application/Optical.Application.csproj (added FluentValidation.DependencyInjectionExtensions)"

key-decisions:
  - "EAN-13 prefix 200 (GS1 internal-use range 200-299) chosen for clinic-generated barcodes, avoids conflict with manufacturer barcodes"
  - "Ean13Generator placed in Optical.Domain (not Application) because it is a domain service required by Frame entity"
  - "GlassesOrderTests uses factory method (GlassesOrder.Create) as the only public creation API, advancing state one step at a time to test each transition"
  - "IsValid_AllZeros test confirms 0000000000000 is a valid barcode (mathematically correct per EAN-13 algorithm)"

patterns-established:
  - "Optical test project follows same structure as Pharmacy.Unit.Tests (Domain/ subfolder, xUnit + FluentAssertions, no special setup)"
  - "EAN-13 validation uses null-safe check then length check then all-digits check then check digit comparison"

requirements-completed: [OPT-01, OPT-03]

# Metrics
duration: 6min
completed: 2026-03-08
---

# Phase 08 Plan 15: Optical Unit Test Scaffold Summary

**xUnit test project for Optical.Domain with 47 passing tests covering Frame CRUD, GlassesOrder status machine, and EAN-13 barcode generation/validation using known check digit vector 5901234123457=7**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-08T02:49:14Z
- **Completed:** 2026-03-08T02:55:17Z
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments
- Created Optical.Unit.Tests project scaffolded with xUnit, FluentAssertions, NSubstitute, coverlet and registered in Ganka28.slnx
- Created Ean13Generator static class in Optical.Domain implementing EAN-13 generation (200-prefix), validation, and check digit calculation
- Wrote 47 domain tests (FrameTests x10, GlassesOrderTests x14 + 3 additional, Ean13GeneratorTests x17 + 3 additional) all passing GREEN

## Task Commits

Each task was committed atomically:

1. **Task 1: Create test project scaffold** - `a2ee823` (chore)
2. **Task 2: Write domain entity tests (RED then GREEN)** - `c4236f0` (feat)

_Note: TDD tasks — tests written first, all passed GREEN since domain entities were complete from Plans 03-06._

## Files Created/Modified
- `backend/tests/Optical.Unit.Tests/Optical.Unit.Tests.csproj` - Test project with xUnit, FluentAssertions, NSubstitute, coverlet, ProjectReferences to Optical.Domain and Optical.Application
- `backend/tests/Optical.Unit.Tests/Domain/FrameTests.cs` - 10 tests covering Frame.Create, AdjustStock, Update, SizeDisplay
- `backend/tests/Optical.Unit.Tests/Domain/GlassesOrderTests.cs` - 17 tests covering transitions, terminal status, domain events, warranty, overdue, payment confirmation
- `backend/tests/Optical.Unit.Tests/Domain/Ean13GeneratorTests.cs` - 20 tests covering Generate, IsValid, CalculateCheckDigit with known vectors and edge cases
- `backend/src/Modules/Optical/Optical.Domain/Ean13Generator.cs` - Static EAN-13 utility class (Generate, IsValid, CalculateCheckDigit) with 200 clinic prefix
- `backend/Ganka28.slnx` - Added Optical.Unit.Tests project entry
- `backend/src/Modules/Optical/Optical.Application/Optical.Application.csproj` - Fixed missing FluentValidation.DependencyInjectionExtensions package reference

## Decisions Made
- Ean13Generator placed in Optical.Domain (not Application layer) because it is a pure domain utility required by Frame entity via `<see cref="Ean13Generator"/>` reference in Frame.cs documentation
- Default clinic prefix set to "200" (GS1 internal-use range 200-299) to avoid conflicts with manufacturer barcodes
- Known EAN-13 test vector "5901234123457" (payload: 590123412345, check digit: 7) used as primary verification anchor per GS1 specification

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Created Ean13Generator class in Optical.Domain**
- **Found during:** Task 2 (Write domain entity tests)
- **Issue:** Plan required Ean13GeneratorTests but the Ean13Generator class didn't exist in the codebase (only referenced in Frame.cs documentation). Tests would fail to compile without the implementation.
- **Fix:** Created `Ean13Generator.cs` in Optical.Domain with Generate, IsValid, and CalculateCheckDigit static methods using standard EAN-13 modulo-10 algorithm with alternating weights 1 and 3
- **Files modified:** `backend/src/Modules/Optical/Optical.Domain/Ean13Generator.cs`
- **Verification:** All 47 tests pass including 17 Ean13GeneratorTests with known GS1 barcode vectors
- **Committed in:** c4236f0 (Task 2 commit)

**2. [Rule 1 - Bug] Fixed missing FluentValidation.DependencyInjectionExtensions in Optical.Application.csproj**
- **Found during:** Task 1 (build verification)
- **Issue:** Optical.Application.csproj had `FluentValidation` but not `FluentValidation.DependencyInjectionExtensions`, causing `AddValidatorsFromAssembly` to fail with CS1061 error
- **Fix:** Added `<PackageReference Include="FluentValidation.DependencyInjectionExtensions" />` to the project file
- **Files modified:** `backend/src/Modules/Optical/Optical.Application/Optical.Application.csproj`
- **Verification:** `dotnet build tests/Optical.Unit.Tests/Optical.Unit.Tests.csproj` succeeded after fix
- **Committed in:** a2ee823 (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (1 missing critical, 1 bug fix)
**Impact on plan:** Both auto-fixes necessary for correctness — EAN-13 generator is required domain logic referenced in Frame entity, DI extension fix was a pre-existing missing dependency.

## Issues Encountered
- Optical.Application.csproj had pre-existing missing `FluentValidation.DependencyInjectionExtensions` package reference — fixed as part of Task 1 build verification.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Optical.Unit.Tests project is fully scaffolded and registered in solution
- 47 domain tests passing GREEN provide baseline coverage for Frame, GlassesOrder, and EAN-13 generator
- Handler test plans (Wave 7: 08-16 through 08-22) can now reference this test project
- Ean13Generator is ready for use in application handlers that create Frame barcodes

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*

## Self-Check: PASSED

- FOUND: backend/tests/Optical.Unit.Tests/Optical.Unit.Tests.csproj
- FOUND: backend/tests/Optical.Unit.Tests/Domain/FrameTests.cs
- FOUND: backend/tests/Optical.Unit.Tests/Domain/GlassesOrderTests.cs
- FOUND: backend/tests/Optical.Unit.Tests/Domain/Ean13GeneratorTests.cs
- FOUND: backend/src/Modules/Optical/Optical.Domain/Ean13Generator.cs
- FOUND: .planning/phases/08-optical-center/08-15-SUMMARY.md
- FOUND: commit a2ee823 (Task 1)
- FOUND: commit c4236f0 (Task 2)

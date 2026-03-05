---
phase: 05-prescriptions-document-printing
plan: 06
subsystem: api
tags: [csharp, wolverine, tdd, prescription, allergy, cross-module, handler]

# Dependency graph
requires:
  - phase: 05-04
    provides: DrugPrescription, PrescriptionItem domain entities with factory methods
  - phase: 05-05a
    provides: EF Core configurations for prescription entities
  - phase: 05-05b
    provides: ClinicalDbContext DbSets, contract DTOs, VisitRepository Includes
provides:
  - AddDrugPrescriptionHandler with catalog/off-catalog item creation and allergy flag
  - UpdateDrugPrescriptionHandler for prescription notes (Loi dan) updates
  - RemoveDrugPrescriptionHandler with visit immutability guard
  - CheckDrugAllergyHandler for cross-module drug-allergy matching via IMessageBus
  - GetPatientAllergiesQuery/Handler for cross-module allergy lookup
  - FluentValidation validators for Add/Update drug prescription commands
affects: [05-09, 05-10, 05-11, 05-12a, 05-17a]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - Cross-module IMessageBus query for patient allergies (Clinical -> Patient.Contracts -> Patient.Application)
    - Case-insensitive bidirectional Contains matching for drug-allergy cross-reference

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Application/Features/AddDrugPrescription.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/UpdateDrugPrescription.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/RemoveDrugPrescription.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/CheckDrugAllergy.cs
    - backend/src/Modules/Patient/Patient.Application/Features/GetPatientAllergies.cs
    - backend/src/Modules/Patient/Patient.Contracts/Dtos/GetPatientAllergiesQuery.cs
    - backend/tests/Clinical.Unit.Tests/Features/DrugPrescriptionHandlerTests.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Application/Clinical.Application.csproj
    - backend/tests/Clinical.Unit.Tests/Clinical.Unit.Tests.csproj

key-decisions:
  - "Clinical.Application references Patient.Contracts for cross-module allergy query via IMessageBus"
  - "GetPatientAllergiesQuery in Patient.Contracts (not Patient.Application) for proper module boundary"
  - "Bidirectional Contains matching: checks both drug name contains allergy AND allergy contains drug name"

patterns-established:
  - "Cross-module query: define query record in Contracts, handler in Application, invoke via IMessageBus"
  - "Drug-allergy matching: case-insensitive bidirectional Contains for drug name and generic name"

requirements-completed: [RX-01, RX-02, RX-05]

# Metrics
duration: 8min
completed: 2026-03-05
---

# Phase 05 Plan 06: Drug Prescription Handlers Summary

**Drug prescription CRUD handlers (add/update/remove) with TDD and cross-module drug-allergy checking via IMessageBus**

## Performance

- **Duration:** 8 min
- **Started:** 2026-03-05T16:44:25Z
- **Completed:** 2026-03-05T16:53:04Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments
- AddDrugPrescriptionHandler: creates prescription with catalog-linked or off-catalog items, allergy flag support, FluentValidation
- UpdateDrugPrescriptionHandler: updates notes (Loi dan) on existing prescription
- RemoveDrugPrescriptionHandler: removes prescription with visit immutability guard (EnsureEditable)
- CheckDrugAllergyHandler: cross-module query via IMessageBus to Patient module, case-insensitive matching against drug name and generic name
- 12 unit tests total (8 drug prescription CRUD + 4 allergy cross-check)

## Task Commits

Each task was committed atomically:

1. **Task 1: TDD -- AddDrugPrescription, UpdateDrugPrescription, RemoveDrugPrescription handlers** - `636ce58` (feat)
2. **Task 2: TDD -- CheckDrugAllergy handler** - `30aa1f3` (feat)

_Note: Task 1 handler files were included in a parallel plan's commit (636ce58) due to concurrent execution. All code was authored by this plan._

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Application/Features/AddDrugPrescription.cs` - Add drug prescription handler with catalog/off-catalog item creation and FluentValidation
- `backend/src/Modules/Clinical/Clinical.Application/Features/UpdateDrugPrescription.cs` - Update prescription notes handler with FluentValidation
- `backend/src/Modules/Clinical/Clinical.Application/Features/RemoveDrugPrescription.cs` - Remove prescription handler with visit immutability guard
- `backend/src/Modules/Clinical/Clinical.Application/Features/CheckDrugAllergy.cs` - Cross-module drug-allergy matching handler via IMessageBus
- `backend/src/Modules/Patient/Patient.Application/Features/GetPatientAllergies.cs` - Wolverine handler for patient allergy lookup
- `backend/src/Modules/Patient/Patient.Contracts/Dtos/GetPatientAllergiesQuery.cs` - Cross-module query record for allergy retrieval
- `backend/tests/Clinical.Unit.Tests/Features/DrugPrescriptionHandlerTests.cs` - 12 unit tests for drug prescription CRUD and allergy checking
- `backend/src/Modules/Clinical/Clinical.Application/Clinical.Application.csproj` - Added Patient.Contracts project reference
- `backend/tests/Clinical.Unit.Tests/Clinical.Unit.Tests.csproj` - Added Patient.Contracts project reference for allergy types

## Decisions Made
- Clinical.Application references Patient.Contracts (not Patient.Application) for proper module isolation -- only DTOs and query records cross the boundary
- GetPatientAllergiesQuery defined in Patient.Contracts following established cross-module query pattern (query in Contracts, handler in Application)
- Bidirectional Contains matching for drug-allergy: checks both "drug name contains allergy name" AND "allergy name contains drug name" for comprehensive coverage
- CheckDrugAllergy uses IMessageBus.InvokeAsync (available transitively via Shared.Infrastructure -> WolverineFx) for cross-module patient allergy retrieval

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added IVisitRepository.AddDrugPrescription/AddPrescriptionItem methods**
- **Found during:** Task 1 (handler implementation)
- **Issue:** IVisitRepository lacked methods to add DrugPrescription and PrescriptionItem to EF Core change tracker (dual-call pattern required by architecture)
- **Fix:** Added AddDrugPrescription and AddPrescriptionItem methods to interface and repository implementation
- **Files modified:** IVisitRepository.cs, VisitRepository.cs
- **Verification:** Build succeeds, all tests pass
- **Committed in:** Already present in HEAD from parallel plan 05-07

**2. [Rule 3 - Blocking] Created GetPatientAllergiesQuery in Patient.Contracts for cross-module communication**
- **Found during:** Task 2 (CheckDrugAllergy handler)
- **Issue:** No existing cross-module query for patient allergies -- GetPatientByIdQuery is in Patient.Application (not Contracts)
- **Fix:** Created GetPatientAllergiesQuery in Patient.Contracts and GetPatientAllergiesHandler in Patient.Application
- **Files modified:** Patient.Contracts/Dtos/GetPatientAllergiesQuery.cs, Patient.Application/Features/GetPatientAllergies.cs
- **Verification:** Cross-module query works in tests via mocked IMessageBus
- **Committed in:** 30aa1f3

---

**Total deviations:** 2 auto-fixed (2 blocking)
**Impact on plan:** Both deviations were necessary infrastructure for the handlers to work. No scope creep.

## Issues Encountered
- Task 1 files (AddDrugPrescription.cs, UpdateDrugPrescription.cs, RemoveDrugPrescription.cs, test file) were committed by a parallel plan executor (636ce58 docs(05-08)) before this plan could commit them. Content is identical -- no rework needed.
- AllergySeverity enum uses Mild/Moderate/Severe (not High/Medium) -- corrected in tests during RED phase.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All drug prescription CRUD handlers ready for Presentation layer endpoint wiring
- CheckDrugAllergy query ready for frontend drug selection combobox integration
- GetPatientAllergiesQuery available for any future cross-module allergy lookups
- 12 tests provide regression safety for prescription logic changes

## Self-Check: PASSED

- [x] AddDrugPrescription.cs exists
- [x] UpdateDrugPrescription.cs exists
- [x] RemoveDrugPrescription.cs exists
- [x] CheckDrugAllergy.cs exists
- [x] GetPatientAllergies.cs exists
- [x] GetPatientAllergiesQuery.cs exists
- [x] DrugPrescriptionHandlerTests.cs exists with 12 tests
- [x] Commit 636ce58 found in git log
- [x] Commit 30aa1f3 found in git log
- [x] All 18 tests pass (12 from this plan + 6 optical from parallel)

---
*Phase: 05-prescriptions-document-printing*
*Completed: 2026-03-05*

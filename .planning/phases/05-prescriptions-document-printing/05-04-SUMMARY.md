---
phase: 05-prescriptions-document-printing
plan: 04
subsystem: domain
tags: [entity, prescription, drug-rx, optical-rx, visit-aggregate, ddd]

# Dependency graph
requires:
  - phase: 03-clinical-visit-workflow
    provides: Visit aggregate with backing field collections, Entity base class, EnsureEditable guard
provides:
  - DrugPrescription entity with PrescriptionItem child collection
  - PrescriptionItem with catalog-linked and off-catalog factory methods
  - OpticalPrescription entity with OD/OS distance and near Rx fields
  - LensType enum for optical prescription lens classification
  - Visit aggregate updated with DrugPrescriptions and OpticalPrescriptions backing fields
affects: [05-05a, 05-05b, 05-06, 05-07, 05-08, 05-09, 05-10, 05-17a, 05-17b]

# Tech tracking
tech-stack:
  added: []
  patterns: [visit-child-entity-prescription, int-enum-cross-module-avoidance, one-per-visit-set-pattern]

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/DrugPrescription.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/PrescriptionItem.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/OpticalPrescription.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/LensType.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs

key-decisions:
  - "Form and Route stored as plain int in PrescriptionItem to avoid cross-module dependency on Pharmacy.Domain enums"
  - "PrescriptionCode generated as 8-char date prefix + 6-char GUID-derived sequence (14 chars total per MOH)"
  - "OpticalPrescription uses int for OdAxis/OsAxis/NearOdAxis/NearOsAxis (degrees 0-180) unlike Refraction which uses decimal"
  - "SetOpticalPrescription clears existing before adding (one optical Rx per visit enforced at domain level)"

patterns-established:
  - "int-enum-cross-module: Store enums from other modules as plain int to avoid project reference"
  - "one-per-visit-set-pattern: SetOpticalPrescription clears + adds for single-record-per-visit semantics"

requirements-completed: [RX-01, RX-02, RX-03]

# Metrics
duration: 4min
completed: 2026-03-05
---

# Phase 05 Plan 04: Prescription Domain Entities Summary

**DrugPrescription/PrescriptionItem/OpticalPrescription domain entities with Visit aggregate backing field collections and EnsureEditable guards**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-05T16:13:07Z
- **Completed:** 2026-03-05T16:17:34Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- DrugPrescription entity with Items backing field collection, notes (Loi dan), and 14-char prescription code generation
- PrescriptionItem with CreateFromCatalog and CreateOffCatalog factory methods, Form/Route as int for cross-module independence
- OpticalPrescription with full OD/OS distance Rx, near Rx override, far/near PD, and LensType
- Visit aggregate updated with 6 total backing field collections (added DrugPrescriptions + OpticalPrescriptions)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create DrugPrescription, PrescriptionItem entities, and LensType enum** - `85bf607` (feat)
2. **Task 2: Create OpticalPrescription entity and update Visit aggregate** - `32ebab0` (feat)

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/DrugPrescription.cs` - Drug prescription with Items collection, notes, prescription code
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/PrescriptionItem.cs` - Individual drug line with catalog/off-catalog factories
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/OpticalPrescription.cs` - Optical Rx with OD/OS distance + near refraction fields
- `backend/src/Modules/Clinical/Clinical.Domain/Enums/LensType.cs` - SingleVision, Bifocal, Progressive, Reading
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs` - Added prescription backing fields and domain methods

## Decisions Made
- Form and Route stored as plain int in PrescriptionItem to avoid cross-module dependency on Pharmacy.Domain enums (Clinical.Domain has no reference to Pharmacy.Domain)
- PrescriptionCode format: yyyyMMdd date prefix + 6-char uppercase GUID segment = 14 characters per MOH Circular 26/2025
- OdAxis/OsAxis typed as int (degrees 0-180) in OpticalPrescription, matching clinical convention for axis values
- SetOpticalPrescription clears existing before adding -- enforces one optical Rx per visit at domain level
- IsOffCatalog derived from DrugCatalogItemId == null, but stored as explicit bool for query efficiency

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Task 2 files (OpticalPrescription.cs and Visit.cs changes) were committed by a parallel plan executor (05-17b commit 32ebab0) alongside unrelated i18n translations. The changes matched the plan specification exactly, so no additional work was needed.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All prescription domain entities defined with proper immutability guards
- Visit aggregate has prescription collections ready for EF Core configuration (Plan 05-05)
- PrescriptionItem.DrugCatalogItemId nullable FK ready for cross-module catalog linking
- LensType enum ready for OpticalPrescription EF configuration

## Self-Check: PASSED

- All 5 files exist on disk
- Both commit hashes (85bf607, 32ebab0) found in git log
- `dotnet build Clinical.Domain.csproj` succeeds with 0 errors, 0 warnings
- Visit.cs has 6 backing field collections
- PrescriptionItem has 2 factory methods
- OpticalPrescription has all OD/OS distance + near Rx + PD fields
- LensType enum has 4 values

---
*Phase: 05-prescriptions-document-printing*
*Completed: 2026-03-05*

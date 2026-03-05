---
phase: 05-prescriptions-document-printing
plan: 01
subsystem: database
tags: [domain-model, entity, enum, dto, pharmacy, drug-catalog, wolverine]

# Dependency graph
requires:
  - phase: 01-foundation
    provides: "Shared.Domain base classes (AggregateRoot, Entity, IAuditable, BranchId)"
provides:
  - "DrugCatalogItem aggregate root entity in Pharmacy.Domain"
  - "DrugForm and DrugRoute enums for pharmaceutical forms and administration routes"
  - "DrugCatalogItemDto sealed record for cross-module drug data"
  - "SearchDrugCatalogQuery record for Wolverine bus drug catalog search"
affects: [05-02, 05-03, 05-04, 05-09, 06-inventory]

# Tech tracking
tech-stack:
  added: []
  patterns: ["AggregateRoot + IAuditable for branch-scoped catalog entities", "int-serialized enums in Contracts DTOs (no Domain reference from Contracts)"]

key-files:
  created:
    - "backend/src/Modules/Pharmacy/Pharmacy.Domain/Enums/DrugForm.cs"
    - "backend/src/Modules/Pharmacy/Pharmacy.Domain/Enums/DrugRoute.cs"
    - "backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/DrugCatalogItem.cs"
    - "backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/DrugCatalogItemDto.cs"
    - "backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/SearchDrugCatalogQuery.cs"
  modified: []

key-decisions:
  - "DrugCatalogItem as AggregateRoot with BranchId for multi-branch drug catalog isolation"
  - "IsActive soft-delete flag on DrugCatalogItem (Activate/Deactivate methods) instead of Entity.MarkDeleted"
  - "Form and Route stored as int in DrugCatalogItemDto (enum normalization pattern, Contracts has no Domain reference)"

patterns-established:
  - "Pharmacy entity pattern: AggregateRoot + IAuditable with factory Create(), Update(), Activate/Deactivate"
  - "Cross-module query pattern: sealed record query in Contracts (SearchDrugCatalogQuery) for Wolverine bus invocation"

requirements-completed: [RX-01, RX-02]

# Metrics
duration: 3min
completed: 2026-03-05
---

# Phase 05 Plan 01: Drug Catalog Domain Model Summary

**DrugCatalogItem entity with DrugForm/DrugRoute enums and cross-module Contracts DTOs for pharmacy catalog search via Wolverine bus**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-05T16:13:43Z
- **Completed:** 2026-03-05T16:16:43Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- DrugForm enum with 10 pharmaceutical forms covering ophthalmology clinic needs (EyeDrops through Spray)
- DrugRoute enum with 7 administration routes including ophthalmic-specific (Subconjunctival, Intravitreal, Periocular)
- DrugCatalogItem AggregateRoot entity with factory method, Update, Activate/Deactivate, and all required fields
- Cross-module DrugCatalogItemDto and SearchDrugCatalogQuery for Clinical module to query drug catalog via Wolverine

## Task Commits

Each task was committed atomically:

1. **Task 1: Create DrugForm and DrugRoute enums** - `bdb98ad` (feat)
2. **Task 2: Create DrugCatalogItem entity and Contracts DTOs** - `42f1492` (feat)

## Files Created/Modified
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Enums/DrugForm.cs` - Drug form enum (EyeDrops, Tablet, Capsule, Ointment, Injection, Gel, Solution, Suspension, Cream, Spray)
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Enums/DrugRoute.cs` - Drug route enum (Topical, Oral, IM, IV, Subconjunctival, Intravitreal, Periocular)
- `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/DrugCatalogItem.cs` - Drug catalog aggregate root entity with factory method
- `backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/DrugCatalogItemDto.cs` - Cross-module DTO with int-serialized enums
- `backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/SearchDrugCatalogQuery.cs` - Wolverine bus query record for drug search

## Decisions Made
- DrugCatalogItem inherits AggregateRoot (not Entity) for BranchId multi-branch isolation on the drug catalog
- IsActive flag with Activate/Deactivate domain methods for soft-delete instead of Entity.MarkDeleted -- catalog items should be hidden from search but not deleted
- DrugCatalogItemDto uses int for Form and Route fields because Pharmacy.Contracts does not reference Pharmacy.Domain (established enum normalization pattern)
- Vietnamese diacritics in XML doc comments for all enum values (e.g., "Thuoc nho mat" with proper accents)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Pre-existing staged files included in Task 2 commit**
- **Found during:** Task 2 (Commit)
- **Issue:** Previous incomplete execution left ClinicSettings and ReferenceDbContext changes in the git staging area. These were committed alongside Task 2 files.
- **Fix:** No code fix needed -- the extra files (ClinicSettings entity, configuration, ReferenceDbContext) are consistent with Phase 5 requirements and do not conflict.
- **Files modified:** ClinicSettingsConfiguration.cs, ClinicSettings.cs, ReferenceDbContext.cs (pre-existing, not created by this plan)
- **Verification:** Both Pharmacy.Domain and Pharmacy.Contracts build successfully
- **Committed in:** 42f1492 (Task 2 commit)

---

**Total deviations:** 1 (pre-existing staged files in commit)
**Impact on plan:** No impact on plan deliverables. Extra files are forward-compatible with Phase 5 document printing requirements.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Drug catalog domain model complete, ready for EF Core configuration (Plan 02)
- DrugCatalogItem entity ready for DbContext registration and migration
- SearchDrugCatalogQuery ready for handler implementation in Pharmacy.Application
- Enums ready for use in PrescriptionItem entity (future plans)

## Self-Check: PASSED

All 5 created files verified on disk. Both task commits (bdb98ad, 42f1492) verified in git history. Both Pharmacy.Domain and Pharmacy.Contracts build with 0 errors.

---
*Phase: 05-prescriptions-document-printing*
*Completed: 2026-03-05*

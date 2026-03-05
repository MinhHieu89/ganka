---
phase: 05-prescriptions-document-printing
plan: 02
subsystem: database
tags: [ef-core, entity-configuration, repository, seeder, pharmacy, drug-catalog, vietnamese-collation]

# Dependency graph
requires:
  - phase: 05-prescriptions-document-printing
    provides: "DrugCatalogItem entity, DrugForm/DrugRoute enums (Plan 01)"
  - phase: 01-foundation
    provides: "AggregateRoot, Entity, IAuditable, BranchId base classes"
provides:
  - "EF Core DrugCatalogItemConfiguration with Vietnamese_CI_AI collation"
  - "PharmacyDbContext with DrugCatalogItems DbSet and ApplyConfigurationsFromAssembly"
  - "IDrugCatalogItemRepository interface with SearchAsync, GetByIdAsync, GetAllActiveAsync"
  - "DrugCatalogItemRepository with accent-insensitive search via DB collation"
  - "DrugCatalogSeeder IHostedService with 78 ophthalmic drugs across 10 categories"
affects: [05-03, 05-04, 05-07, 05-09, 06-inventory]

# Tech tracking
tech-stack:
  added: []
  patterns: ["Vietnamese_CI_AI collation for accent-insensitive drug name search at DB level", "IHostedService idempotent seeder pattern for drug catalog data"]

key-files:
  created:
    - "backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DrugCatalogItemConfiguration.cs"
    - "backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DrugCatalogItemRepository.cs"
    - "backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Seeding/DrugCatalogSeeder.cs"
  modified:
    - "backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/PharmacyDbContext.cs"

key-decisions:
  - "Used consistent SeedBranchId (00000000-0000-0000-0000-000000000001) matching ClinicScheduleSeeder pattern"
  - "SearchAsync projects directly to DrugCatalogItemDto for read-path efficiency (no domain entity roundtrip)"
  - "DrugCatalogSeeder seeds 78 drugs across 10 categories covering comprehensive ophthalmology use cases"

patterns-established:
  - "Pharmacy repository pattern: inject PharmacyDbContext, project to Contracts DTOs for read path"
  - "Drug catalog seeder categories: antibiotics, anti-inflammatory, antiglaucoma, artificial tears, anti-allergy, mydriatics, combination, oral, ointments, antiviral/diagnostic/injection/dry eye"

requirements-completed: [RX-01]

# Metrics
duration: 5min
completed: 2026-03-05
---

# Phase 05 Plan 02: Drug Catalog Persistence Summary

**EF Core persistence for drug catalog with Vietnamese_CI_AI collation search, repository, and IHostedService seeder with 78 ophthalmic drugs across 10 categories**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-05T16:34:21Z
- **Completed:** 2026-03-05T16:39:21Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- DrugCatalogItemConfiguration with Vietnamese_CI_AI collation on Name, NameVi, GenericName for accent-insensitive search
- PharmacyDbContext updated with DrugCatalogItems DbSet and ApplyConfigurationsFromAssembly
- DrugCatalogItemRepository with SearchAsync (top 20 active items matching any name field)
- DrugCatalogSeeder with 78 drugs: antibiotics (10), anti-inflammatory (8), antiglaucoma (8), artificial tears (6), anti-allergy (5), mydriatics (6), combination (6), oral medications (10), ointments (5), antiviral (3), diagnostic (3), injection (4), dry eye (3), plus Atropine 0.01% for myopia control

## Task Commits

Each task was committed atomically:

1. **Task 1: Create EF Core configuration and update PharmacyDbContext** - `965788f` (feat)
2. **Task 2: Create repository, and drug catalog seeder** - `d4485a8` (pre-existing commit from parallel plan execution)

## Files Created/Modified
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DrugCatalogItemConfiguration.cs` - EF Core config with Vietnamese_CI_AI collation, BranchId conversion, Form/Route int conversion, search indexes
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/PharmacyDbContext.cs` - Added DrugCatalogItems DbSet and ApplyConfigurationsFromAssembly
- `backend/src/Modules/Pharmacy/Pharmacy.Application/Interfaces/IDrugCatalogItemRepository.cs` - Repository interface (created in prior plan, unchanged)
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DrugCatalogItemRepository.cs` - Repository with accent-insensitive search via DB collation
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Seeding/DrugCatalogSeeder.cs` - IHostedService seeder with 78 ophthalmic drugs

## Decisions Made
- Used BranchId `00000000-0000-0000-0000-000000000001` matching ClinicScheduleSeeder pattern for consistent seed data
- SearchAsync projects directly to DrugCatalogItemDto in the LINQ query for read-path efficiency
- No code-level string normalization for Vietnamese search -- Vietnamese_CI_AI collation handles it at DB level
- Added extra drugs beyond plan specification: Atropine 0.01% (myopia control), additional Hyaluronic Acid concentrations, combination glaucoma drops, diagnostic agents, intravitreal injections for comprehensive catalog

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Aligned SeedBranchId with existing seeder pattern**
- **Found during:** Task 2
- **Issue:** Initially used arbitrary BranchId (11111111...), but ClinicScheduleSeeder uses 00000000-0000-0000-0000-000000000001
- **Fix:** Changed SeedBranchId to match existing DefaultBranchId pattern
- **Files modified:** DrugCatalogSeeder.cs
- **Verification:** Build succeeds, consistent with Scheduling module seeder
- **Committed in:** d4485a8

---

**Total deviations:** 1 auto-fixed (1 bug fix)
**Impact on plan:** Minor BranchId alignment. No scope creep.

## Issues Encountered
- Task 2 files (Repository, Seeder) were already committed by a parallel plan execution (05-19/d4485a8). Content matched what was written here, so no additional commit was needed.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Drug catalog persistence layer complete, ready for Wolverine handler wiring (Plan 03)
- DrugCatalogSeeder ready for DI registration in Bootstrapper (Plan 07)
- Repository ready for injection into search/CRUD handlers
- Migration can be created once Bootstrapper wiring is complete

## Self-Check: PASSED

All files verified on disk. Task 1 commit (965788f) verified in git history. Task 2 files confirmed committed in d4485a8. Pharmacy.Infrastructure builds with 0 errors.

---
*Phase: 05-prescriptions-document-printing*
*Completed: 2026-03-05*

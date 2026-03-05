---
phase: 05-prescriptions-document-printing
plan: 10
subsystem: api
tags: [minimal-api, bootstrapper, ioc, pharmacy, prescription, migration, ef-core, nuget]

# Dependency graph
requires:
  - phase: 05-06
    provides: Drug prescription handlers (Add/Update/Remove) and CheckDrugAllergy handler
  - phase: 05-07
    provides: Optical prescription handlers (Add/Update) with SetOpticalPrescription one-per-visit enforcement
  - phase: 05-08
    provides: Pharmacy.Presentation endpoints and IoC registration (AddPharmacyApplication/Infrastructure/Presentation)
  - phase: 05-09
    provides: ClinicSettings entity and IClinicSettingsService for document headers
  - phase: 05-09b
    provides: SettingsApiEndpoints with MapSettingsApiEndpoints extension method
provides:
  - MapPrescriptionEndpoints in ClinicalApiEndpoints (drug Rx CRUD, allergy check, optical Rx add/update)
  - Pharmacy module fully registered in Bootstrapper (IoC + endpoints + seeder)
  - Settings API endpoints wired in Program.cs via MapSettingsApiEndpoints
  - ClosedXML added to Directory.Packages.props for Excel import
  - Database migrations for ClinicalDbContext (prescriptions), PharmacyDbContext (drug catalog), ReferenceDbContext (clinic settings)
affects: [05-11, 05-12a, 05-12b, 05-17a, 05-17b, 05-18, 05-19, frontend-prescription-pages]

# Tech tracking
tech-stack:
  added: [ClosedXML]
  patterns:
    - "CheckDrugAllergyParams [AsParameters] binding for GET query string with PatientId, DrugName, GenericName"
    - "Enriched command pattern: route params + body combined in Minimal API handlers"

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Migrations/20260305170116_AddPrescriptionEntities.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Migrations/20260305170127_AddDrugCatalog.cs
    - backend/src/Shared/Shared.Infrastructure/Migrations/Reference/20260305170139_AddClinicSettings.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
    - backend/src/Modules/Clinical/Clinical.Presentation/Clinical.Presentation.csproj
    - backend/src/Bootstrapper/Program.cs
    - backend/src/Bootstrapper/Bootstrapper.csproj
    - backend/Directory.Packages.props

key-decisions:
  - "Patient.Contracts reference added to Clinical.Presentation.csproj for AllergyDto type in CheckDrugAllergy endpoint response"
  - "ClinicSettings migration created alongside prescription migrations since Plan 09 did not create one"
  - "Duplicate Pharmacy.Infrastructure using removed from Program.cs (was in both Module DbContexts and Module IoC sections)"

patterns-established:
  - "CheckDrugAllergyParams: [AsParameters] class for multi-field GET query binding (PatientId, DrugName, GenericName)"

requirements-completed: [RX-01, RX-03, RX-04]

# Metrics
duration: 6min
completed: 2026-03-05
---

# Phase 05 Plan 10: Integration Wiring & Migrations Summary

**Prescription endpoints wired in Clinical API, Pharmacy module integrated in Bootstrapper, Settings endpoints mapped, and EF Core migrations created for prescriptions, drug catalog, and clinic settings**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-05T16:56:32Z
- **Completed:** 2026-03-05T17:02:32Z
- **Tasks:** 2
- **Files modified:** 14

## Accomplishments
- Added MapPrescriptionEndpoints to ClinicalApiEndpoints with 6 endpoints: drug Rx CRUD (POST/PUT/DELETE), allergy check (GET), optical Rx add (POST) and update (PUT)
- Registered Pharmacy module IoC in Bootstrapper with AddPharmacyApplication/AddPharmacyInfrastructure/AddPharmacyPresentation
- Wired MapPharmacyApiEndpoints and MapSettingsApiEndpoints in Bootstrapper endpoint mapping
- Created 3 database migrations: AddPrescriptionEntities (Clinical), AddDrugCatalog (Pharmacy), AddClinicSettings (Reference)
- Added ClosedXML to Directory.Packages.props for future Excel import support

## Task Commits

Each task was committed atomically:

1. **Task 1: Add prescription endpoints to ClinicalApiEndpoints and update Bootstrapper** - `f704c60` (feat)
2. **Task 2: Add NuGet packages and create database migration** - `8368216` (feat)

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs` - Added MapPrescriptionEndpoints with drug Rx CRUD, allergy check, optical Rx endpoints
- `backend/src/Modules/Clinical/Clinical.Presentation/Clinical.Presentation.csproj` - Added Patient.Contracts project reference for AllergyDto type
- `backend/src/Bootstrapper/Program.cs` - Added Pharmacy IoC registration, MapPharmacyApiEndpoints, MapSettingsApiEndpoints
- `backend/src/Bootstrapper/Bootstrapper.csproj` - Added Pharmacy.Presentation project reference
- `backend/Directory.Packages.props` - Added ClosedXML PackageVersion
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Migrations/20260305170116_AddPrescriptionEntities.cs` - DrugPrescriptions, OpticalPrescriptions, PrescriptionItems tables
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Migrations/20260305170116_AddPrescriptionEntities.Designer.cs` - Migration metadata
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Migrations/ClinicalDbContextModelSnapshot.cs` - Updated snapshot
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Migrations/20260305170127_AddDrugCatalog.cs` - DrugCatalogItems table in pharmacy schema
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Migrations/20260305170127_AddDrugCatalog.Designer.cs` - Migration metadata
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Migrations/PharmacyDbContextModelSnapshot.cs` - New snapshot
- `backend/src/Shared/Shared.Infrastructure/Migrations/Reference/20260305170139_AddClinicSettings.cs` - ClinicSettings table in reference schema
- `backend/src/Shared/Shared.Infrastructure/Migrations/Reference/20260305170139_AddClinicSettings.Designer.cs` - Migration metadata
- `backend/src/Shared/Shared.Infrastructure/Migrations/Reference/ReferenceDbContextModelSnapshot.cs` - Updated snapshot

## Decisions Made
- Added explicit Patient.Contracts project reference to Clinical.Presentation.csproj for AllergyDto type visibility in CheckDrugAllergy endpoint response (already available transitively through Clinical.Application but explicit is cleaner)
- Created ClinicSettings migration alongside prescription migrations since Plan 09 created the entity/configuration but no migration existed yet
- Removed duplicate `using Pharmacy.Infrastructure;` from Program.cs that appeared in both Module DbContexts and Module IoC sections

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Created ReferenceDbContext migration for ClinicSettings**
- **Found during:** Task 2 (database migrations)
- **Issue:** Plan 09 added ClinicSettings to ReferenceDbContext but no migration was created. The plan mentioned "If ClinicSettings needs a migration, create that migration too."
- **Fix:** Created AddClinicSettings migration for ReferenceDbContext with ClinicSettings table in reference schema
- **Files modified:** Shared.Infrastructure/Migrations/Reference/ (3 files)
- **Verification:** dotnet build succeeds, migration file contains correct schema and columns
- **Committed in:** 8368216 (Task 2 commit)

**2. [Rule 1 - Bug] Removed duplicate Pharmacy.Infrastructure using directive**
- **Found during:** Task 1 (Program.cs update)
- **Issue:** `using Pharmacy.Infrastructure;` already existed in Module DbContexts section; adding IoC usings would duplicate it, causing CS0105 warning
- **Fix:** Removed duplicate from Module IoC section, keeping only the one in Module DbContexts section
- **Files modified:** backend/src/Bootstrapper/Program.cs
- **Verification:** Build succeeds with zero CS warnings
- **Committed in:** f704c60 (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (1 blocking, 1 bug)
**Impact on plan:** Both auto-fixes necessary for correctness. No scope creep.

## Issues Encountered
None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All prescription endpoints accessible at /api/clinical/{visitId}/drug-prescriptions and /api/clinical/{visitId}/optical-prescription
- Pharmacy endpoints accessible at /api/pharmacy/drugs/search and /api/pharmacy/drugs
- Settings endpoints accessible at /api/settings/clinic (GET and PUT)
- Database migrations ready to apply with dotnet ef database update
- QuestPDF available in Clinical.Infrastructure for document generation
- ClosedXML available for Excel import when needed

## Self-Check: PASSED

- [x] ClinicalApiEndpoints.cs exists with MapPrescriptionEndpoints
- [x] Program.cs exists with MapPharmacyApiEndpoints and MapSettingsApiEndpoints
- [x] Bootstrapper.csproj exists with Pharmacy.Presentation reference
- [x] Directory.Packages.props exists with ClosedXML
- [x] Clinical migration (AddPrescriptionEntities) exists
- [x] Pharmacy migration (AddDrugCatalog) exists
- [x] Reference migration (AddClinicSettings) exists
- [x] Commit f704c60 found in git log
- [x] Commit 8368216 found in git log
- [x] dotnet build succeeds with 0 errors

---
*Phase: 05-prescriptions-document-printing*
*Completed: 2026-03-05*

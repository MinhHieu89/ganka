---
phase: 05-prescriptions-document-printing
plan: 11
subsystem: api, document-generation
tags: [questpdf, pdf-generation, noto-sans, vietnamese-diacritics, clinic-header, a5-paper]

# Dependency graph
requires:
  - phase: 05-09
    provides: IClinicSettingsService with ClinicSettingsDto for clinic header data
  - phase: 05-10
    provides: QuestPDF NuGet package reference in Clinical.Infrastructure.csproj
provides:
  - DocumentFontManager with Noto Sans embedded fonts for Vietnamese diacritic support
  - ClinicHeaderComponent reusable QuestPDF component for all document types
  - DrugPrescriptionDocument A5 MOH-compliant drug prescription PDF
  - IDocumentService interface with 5 document generation methods
  - DocumentService implementation with IClinicSettingsService integration
  - Data records for all document types (ClinicHeaderData, DrugPrescriptionData, etc.)
affects: [05-12a, 05-12b, 05-17a, frontend-print-pages]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "QuestPDF IDocument pattern with data record + header separation"
    - "Cross-schema raw SQL for patient data in DocumentService (avoids cross-module project reference)"
    - "IClinicSettingsService integration for configurable clinic branding"

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/Shared/DocumentFontManager.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/Shared/ClinicHeaderComponent.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/Shared/DocumentDataRecords.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/DrugPrescriptionDocument.cs
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IDocumentService.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Services/DocumentService.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/Fonts/NotoSans-Regular.ttf
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/Fonts/NotoSans-Bold.ttf
  modified:
    - backend/src/Modules/Clinical/Clinical.Infrastructure/IoC.cs

key-decisions:
  - "IClinicSettingsService injected into DocumentService for configurable clinic header (not hardcoded defaults)"
  - "Cross-schema raw SQL for patient data in DocumentService (avoids cross-module project reference)"
  - "QuestPDF Community license with embedded Noto Sans fonts for Vietnamese diacritics"

patterns-established:
  - "QuestPDF document pattern: IDocument class with data record constructor, Compose method for layout"
  - "ClinicHeaderComponent as reusable IComponent for all document types"
  - "DocumentFontManager thread-safe singleton registration with double-check locking"

requirements-completed: [RX-04, PRT-01]

# Metrics
duration: 2min
completed: 2026-03-06
---

# Phase 05 Plan 11: QuestPDF Infrastructure & Drug Prescription Document Summary

**QuestPDF infrastructure with Noto Sans Vietnamese font support, reusable clinic header component, and A5 drug prescription document using IClinicSettingsService for configurable branding**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-05T17:07:11Z
- **Completed:** 2026-03-05T17:09:30Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- QuestPDF infrastructure complete: font manager registers embedded Noto Sans Regular + Bold for Vietnamese diacritics
- ClinicHeaderComponent renders configurable clinic branding (logo, name, address, phone/fax, license, tagline) for all document types
- DrugPrescriptionDocument renders MOH-compliant A5 layout with clinic header, patient info, diagnosis, drug table (DosageOverride fallback), Loi dan, doctor signature
- IDocumentService interface defines 5 document generation methods (drug Rx, optical Rx, referral letter, consent form, pharmacy label)
- DocumentService integrates IClinicSettingsService for dynamic clinic header data instead of hardcoded defaults
- IoC registration for IDocumentService in Clinical.Infrastructure

## Task Commits

Each task was committed atomically:

1. **Task 1: Create QuestPDF infrastructure -- font manager and clinic header component** - `8590dd7` (feat)
2. **Task 2: Create IDocumentService, DocumentService, and DrugPrescriptionDocument** - `fe689f8` (feat)

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/Shared/DocumentFontManager.cs` - Thread-safe Noto Sans font registration with QuestPDF Community license
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/Shared/ClinicHeaderComponent.cs` - Reusable IComponent for clinic branding header
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/Shared/DocumentDataRecords.cs` - Data records for all document types
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/DrugPrescriptionDocument.cs` - A5 drug prescription with MOH layout
- `backend/src/Modules/Clinical/Clinical.Application/Interfaces/IDocumentService.cs` - Service interface for 5 document types
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Services/DocumentService.cs` - Full implementation with IClinicSettingsService and cross-schema patient data
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/Fonts/NotoSans-Regular.ttf` - Embedded Noto Sans Regular font
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/Fonts/NotoSans-Bold.ttf` - Embedded Noto Sans Bold font

## Decisions Made
- Integrated IClinicSettingsService into DocumentService constructor injection, replacing hardcoded default header with configurable settings from database (Plan 09 dependency fulfilled)
- Cross-schema raw SQL for patient data (DateOfBirth, Gender, Address, CCCD) to avoid cross-module project reference from Clinical to Patient
- CheckIfAllTextGlyphsAreAvailable set to false (production-safe; fonts are embedded and verified)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Integrated IClinicSettingsService into DocumentService**
- **Found during:** Task 2
- **Issue:** DocumentService had a TODO placeholder using hardcoded defaults instead of IClinicSettingsService (Plan 09 dependency now available)
- **Fix:** Injected IClinicSettingsService, replaced GetClinicHeaderDataAsync with settings lookup and fallback defaults
- **Files modified:** backend/src/Modules/Clinical/Clinical.Infrastructure/Services/DocumentService.cs
- **Verification:** dotnet build succeeds, Bootstrapper build succeeds
- **Committed in:** fe689f8

---

**Total deviations:** 1 auto-fixed (1 missing critical functionality)
**Impact on plan:** Essential integration with Plan 09's IClinicSettingsService. No scope creep.

## Issues Encountered
- Plan 11 artifacts were already created in a prior plan execution (05-12b, commit 74894f0). This plan focused on verifying completeness and integrating the IClinicSettingsService dependency that was unavailable during the prior execution.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- QuestPDF infrastructure complete -- all document generators can use DocumentFontManager, ClinicHeaderComponent, and data records
- DrugPrescriptionDocument generates MOH-compliant A5 PDF with Vietnamese diacritics
- IDocumentService ready for remaining document types (optical Rx, referral letter, consent form, pharmacy label) already implemented
- Print endpoints can be wired to DocumentService methods

## Self-Check: PASSED

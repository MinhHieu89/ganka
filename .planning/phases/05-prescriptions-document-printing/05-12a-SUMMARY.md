---
phase: 05-prescriptions-document-printing
plan: 12a
subsystem: documents
tags: [questpdf, pdf, optical-rx, referral, consent, pharmacy-label, vietnamese]

# Dependency graph
requires:
  - phase: 05-11
    provides: "QuestPDF infrastructure (DocumentFontManager, ClinicHeaderComponent, DrugPrescriptionDocument)"
provides:
  - "OpticalPrescriptionDocument (A4, OD/OS refraction grid)"
  - "ReferralLetterDocument (A4, formal referral with diagnosis)"
  - "ConsentFormDocument (A4, procedure consent with signature lines)"
  - "PharmacyLabelDocument (70x35mm, compact drug label)"
affects: [05-13, 05-12b]

# Tech tracking
tech-stack:
  added: []
  patterns: ["Small-format custom PageSize for pharmacy labels (70x35mm)"]

key-files:
  created:
    - "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/OpticalPrescriptionDocument.cs"
    - "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/ReferralLetterDocument.cs"
    - "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/ConsentFormDocument.cs"
    - "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/PharmacyLabelDocument.cs"
  modified: []

key-decisions:
  - "PharmacyLabelDocument uses 70x35mm custom PageSize with 3mm margins for standard adhesive label stock"
  - "ConsentFormDocument includes fingerprint space alongside patient signature for non-literate patients"
  - "ReferralLetterDocument has dual footer columns: receiving hospital stamp + referring doctor signature"

patterns-established:
  - "Document IDocument pattern: sealed class with data record + ClinicHeaderData constructor params"
  - "Small-format document: custom PageSize without header component, compact font sizes (6-8pt)"

requirements-completed: [PRT-02, PRT-04, PRT-05, PRT-06]

# Metrics
duration: 5min
completed: 2026-03-05
---

# Phase 05 Plan 12a: Remaining Document Types Summary

**4 QuestPDF IDocument classes for optical Rx (A4 OD/OS grid), referral letter (A4 formal), consent form (A4 with signatures), and pharmacy label (70x35mm compact)**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-05T16:14:18Z
- **Completed:** 2026-03-05T16:19:35Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- OpticalPrescriptionDocument renders A4 page with distance/near OD/OS refraction grid, PD fields, and lens type
- ReferralLetterDocument renders A4 formal referral with patient info, diagnosis, referral reason, clinical summary, and dual signature areas
- ConsentFormDocument renders A4 consent with procedure risks/benefits explanation, consent statement, patient/doctor/witness signatures, and fingerprint space
- PharmacyLabelDocument renders 70x35mm compact label with clinic name, patient, drug info, dosage, quantity, and date

## Task Commits

Each task was committed atomically:

1. **Task 1: Create OpticalPrescriptionDocument and ReferralLetterDocument** - `30f6be7` (feat)
2. **Task 2: Create ConsentFormDocument and PharmacyLabelDocument** - `79728d4` (feat)

**Plan metadata:** [pending] (docs: complete plan)

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/OpticalPrescriptionDocument.cs` - A4 optical Rx with OD/OS refraction grid, PD, lens type recommendation
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/ReferralLetterDocument.cs` - A4 referral letter with patient info, diagnosis, referral reason, clinical summary
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/ConsentFormDocument.cs` - A4 consent form with procedure explanation, consent statement, signature lines
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/PharmacyLabelDocument.cs` - 70x35mm pharmacy label with drug name, dosage, patient info

## Decisions Made
- PharmacyLabelDocument uses 70x35mm custom PageSize with 3mm margins -- standard adhesive label stock for pharmacy label printers
- ConsentFormDocument includes fingerprint space (diem chi) alongside patient signature line -- Vietnamese clinical practice for patients who cannot sign
- ReferralLetterDocument has dual footer: left column for receiving hospital stamp, right column for referring doctor signature -- standard Vietnamese referral format
- All documents use Noto Sans font family for Vietnamese diacritic support
- All A4 documents use ClinicHeaderComponent for consistent branding; PharmacyLabelDocument uses abbreviated clinic name directly (too small for full header)

## Deviations from Plan

None - plan executed exactly as written. All 4 document files existed from prior session work and were verified and committed.

## Issues Encountered
- Task 2 files (ConsentFormDocument, PharmacyLabelDocument) were created by a prior session and committed alongside a different plan's docs commit (79728d4). Verified content matches plan requirements exactly.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 5 document types now implemented (DrugPrescription from Plan 11 + 4 from this plan)
- DocumentService integration (Plan 12b) can wire these documents to the IDocumentService methods
- All documents use consistent patterns: ClinicHeaderComponent, Noto Sans font, proper Vietnamese diacritics

## Self-Check: PASSED

- All 4 document files exist on disk
- Commit 30f6be7 (Task 1) verified in git log
- Commit 79728d4 (Task 2) verified in git log
- dotnet build succeeds with 0 warnings, 0 errors

---
*Phase: 05-prescriptions-document-printing*
*Completed: 2026-03-05*

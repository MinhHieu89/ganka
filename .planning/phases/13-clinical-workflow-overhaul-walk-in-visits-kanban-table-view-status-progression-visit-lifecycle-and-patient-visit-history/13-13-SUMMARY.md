---
phase: 13-clinical-workflow-overhaul
plan: 13
subsystem: clinical-frontend
tags: [stage-views, imaging-loop, branch-decision, icd10, doctor-exam]
dependency_graph:
  requires: [13-11, 13-12]
  provides: [stage-3-view, stage-4a-view, stage-4b-view]
  affects: [visit-stage-routing, clinical-workflow]
tech_stack:
  added: []
  patterns: [branch-decision-routing, diagnosis-change-tracking, modality-tinted-thumbnails]
key_files:
  created:
    - frontend/src/features/clinical/components/stage-views/Stage3DoctorExamView.tsx
    - frontend/src/features/clinical/components/stage-views/Stage4aImagingView.tsx
    - frontend/src/features/clinical/components/stage-views/Stage4bDoctorReviewView.tsx
  modified:
    - frontend/src/app/routes/_authenticated/clinical/visit.$visitId.stage.tsx
    - frontend/src/features/clinical/api/clinical-api.ts
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/VisitDetailDto.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetVisitById.cs
decisions:
  - "Added imagingRequested and refractionSkipped to VisitDetailDto (backend+frontend) since stage views need these flags"
  - "Used DiagnosisTag as shared component exported from Stage3 for reuse in Stage4b"
  - "Service checklist in Stage 4a is advisory-only and does not block forward button"
metrics:
  duration: 9min
  completed: 2026-03-25
---

# Phase 13 Plan 13: Stages 3, 4a, 4b (Doctor Exam, Imaging, Doctor Review) Summary

Doctor exam with ICD-10 diagnosis and branch decision routing to imaging or prescription, plus imaging capture and doctor review with diagnosis change tracking.

## What Was Built

### Stage 3 - Doctor Examination (Stage3DoctorExamView)
- Refraction summary card showing OD/OS SPH/CYL/AXIS/UCVA/BCVA in read-only grid
- Amber skip notice when refraction was skipped
- ICD-10 diagnosis section with Icd10Combobox search, teal pill tags with remove button
- Examination notes textarea (optional, does not block forward)
- Branch decision bottom bar:
  - "Luu nhap" (save draft, always enabled)
  - "Chuyen CDHA" (imaging request, always enabled, opens modal with service checklist)
  - "Ke don" (prescription, disabled until >= 1 ICD-10 tag, hidden if imaging already requested)
- Imaging request modal with service checkboxes and optional note

### Stage 4a - Imaging (Stage4aImagingView)
- Blue referral banner showing doctor name
- Service checklist with toggle completion (advisory, does not block forward)
- Image upload using existing ImageUploader + ImageGallery components
- Validation: error if no uploads, warning if incomplete services, success when all done
- Forward button "Tra ket qua cho bac si" disabled until >= 1 file uploaded

### Stage 4b - Doctor Reviews Results (Stage4bDoctorReviewView)
- Image viewer with 3-column thumbnail grid, modality-tinted backgrounds (blue=OCT, coral=Fundus, green=anterior)
- Inline 16:9 preview panel on thumbnail click with IOP metadata
- KTV note card in distinct blue (#E6F1FB) styling
- Diagnosis section with change tracking: teal tags = Stage 3, amber tags with "new" badge = added in 4b
- Change log block showing green dots for additions
- Forward button "Xac nhan, tiep tuc ke don" always enabled

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added imagingRequested/refractionSkipped to VisitDetailDto**
- **Found during:** Task 1
- **Issue:** VisitDetailDto (backend + frontend) was missing imagingRequested and refractionSkipped fields needed by Stage 3 branch decision logic
- **Fix:** Added both fields to backend VisitDetailDto record, updated GetVisitByIdHandler mapping, added to frontend TypeScript interface
- **Files modified:** backend VisitDetailDto.cs, GetVisitById.cs, frontend clinical-api.ts
- **Commit:** b78e1d1

## Known Stubs

- Stage 4a service checklist uses hardcoded default services rather than pulling from the actual imaging request data (future: wire to backend imaging request services)
- Stage 4b KTV note is static placeholder text (future: wire to actual KTV notes from imaging stage)

## Self-Check: PASSED

---
phase: 13-clinical-workflow-overhaul
plan: 08
subsystem: clinical
tags: [visit-history, ui-redesign, timeline, patient-profile]

requires:
  - phase: 13-clinical-workflow-overhaul
    provides: "VisitHistoryTab, VisitTimeline, VisitTimelineCard, VisitHistoryDetail components"
provides:
  - "Polished Visit History tab with timeline connector and dot indicators"
  - "Locale-aware date formatting (vi-VN/en-US)"
  - "Aligned unit labels across Refraction and DryEye forms"
  - "Medical images section in visit history detail"
  - "Visit History tab positioned next to Overview"
  - "Removed redundant back link and section wrappers"
affects: [13-clinical-workflow-overhaul]

tech-stack:
  added: []
  patterns: ["i18n-aware date formatting via i18n.language"]

key-files:
  created: []
  modified:
    - frontend/src/features/clinical/components/VisitHistoryDetail.tsx
    - frontend/src/features/clinical/components/VisitTimeline.tsx
    - frontend/src/features/clinical/components/VisitTimelineCard.tsx
    - frontend/src/features/clinical/components/VisitSection.tsx
    - frontend/src/features/clinical/components/RefractionForm.tsx
    - frontend/src/features/clinical/components/DryEyeForm.tsx
    - frontend/src/features/patient/components/PatientProfilePage.tsx
---

## What was done

Redesigned the Patient Visit History tab with polished, professional styling:

1. **Timeline UI** — Vertical connector line with circle dot indicators centered horizontally on the line and vertically on each card. Selected state uses primary color, unselected uses muted.

2. **Date localization** — All dates in timeline cards and detail header use the app's i18n language (vi-VN or en-US) instead of browser default locale.

3. **Unit alignment** — Added consistent min-width spacers to unit labels (D, deg, mm, mmHg, s) in both RefractionForm and DryEyeForm. Fields without units render an empty spacer for alignment.

4. **Removed redundant UI** — Removed SectionWrapper cards (inner components already have headers), removed redundant back link (breadcrumb exists).

5. **Added Medical Images section** — Visit history detail now includes MedicalImagesSection.

6. **Tab repositioning** — Visit History tab moved next to Overview tab for quick access.

## Human verification

Approved after iterative fixes for:
- Icon export fix (IconGlasses → IconEyeglass)
- EF migration for new domain model columns (500 error fix)
- Timeline dot centering and card clipping
- Date locale formatting
- Unit label alignment
- Redundant UI removal

## Self-Check: PASSED

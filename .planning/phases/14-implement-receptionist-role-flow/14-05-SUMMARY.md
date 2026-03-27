---
phase: 14-implement-receptionist-role-flow
plan: 05
subsystem: frontend/receptionist
tags: [intake-form, patient-registration, collapsible-ui, duplicate-detection]
dependency_graph:
  requires: [14-03, 14-04]
  provides: [PatientIntakeForm, /patients/intake route]
  affects: [receptionist-dashboard, patient-registration-flow]
tech_stack:
  added: []
  patterns: [FormProvider, useFormContext, collapsible-sections, debounced-search]
key_files:
  created:
    - frontend/src/features/receptionist/components/intake/PersonalInfoSection.tsx
    - frontend/src/features/receptionist/components/intake/ExamInfoSection.tsx
    - frontend/src/features/receptionist/components/intake/MedicalHistorySection.tsx
    - frontend/src/features/receptionist/components/intake/LifestyleSection.tsx
    - frontend/src/features/receptionist/components/intake/PatientIntakeForm.tsx
    - frontend/src/app/routes/_authenticated/patients/intake.tsx
  modified: []
decisions:
  - Used FormProvider pattern for section components to access form context
  - Used setTimeout-based debounce (500ms) for phone duplicate detection
  - Used AlertDialog for cancel confirmation instead of browser confirm()
metrics:
  duration: 4min
  completed: "2026-03-27T19:18:26Z"
---

# Phase 14 Plan 05: Patient Intake Form Summary

4-section collapsible intake form with phone duplicate detection, save/advance flow, and create/edit modes at /patients/intake

## What Was Built

### Task 1: Patient intake form sections (a3238a1)

Created 4 reusable section components using shadcn Collapsible, all expanded by default:

- **PersonalInfoSection**: 3-column grid (lg:grid-cols-3), full name spanning 2 cols, gender select, phone with debounced (500ms) duplicate detection showing amber warning bar with "Mo ho so cu" link, date picker, address, CCCD, email, occupation
- **ExamInfoSection**: Reason textarea with character counter (x/500)
- **MedicalHistorySection**: Ocular history, systemic history, current medications textareas
- **LifestyleSection**: Screen time (NumberInput, 0-24), work environment select, contact lens usage select, lifestyle notes textarea

All sections use `useFormContext<IntakeFormValues>()` for form state access.

### Task 2: PatientIntakeForm container + route file (e10f1e6)

- **PatientIntakeForm.tsx**: Container component with create/edit modes, React Hook Form + zodResolver, breadcrumb navigation, 3 footer buttons:
  - "Huy nhap lieu" (outline) with AlertDialog confirmation
  - "Luu ho so" (outline) saves patient only
  - "Luu & Chuyen tien kham" (primary) saves + creates walk-in visit + advances to PreExam stage via useAdvanceStageMutation
- **intake.tsx**: Route at /_authenticated/patients/intake with Patient.Create permission, search param `patientId` for edit mode with patient data pre-fill via usePatientById

## Deviations from Plan

None - plan executed exactly as written.

## Known Stubs

None - all sections are fully wired to form state and API mutations.

## Self-Check: PASSED

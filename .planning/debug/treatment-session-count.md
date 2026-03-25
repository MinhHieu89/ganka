---
status: awaiting_human_verify
trigger: "treatment session count shows 0 completed sessions even when sessions exist"
created: 2026-03-25T00:00:00Z
updated: 2026-03-25T13:15:00Z
---

## Current Focus

hypothesis: CONFIRMED - useRecordSession cache invalidation was too narrow, missing patientPackages queries
test: Fixed by using treatmentKeys.all to invalidate all treatment queries after recording
expecting: After recording a session, all views (including PatientTreatmentsTab) show updated counts
next_action: Verify fix with tests and user confirmation

## Symptoms

expected: The session count/chart should reflect the actual number of treatment sessions created/completed
actual: Count shows 0 completed sessions even though sessions exist
errors: No explicit error messages - data/logic bug
reproduction: 1) Patient detail > Treatment detail, 2) Create work session in "Ghi nhan buoi dieu tri" tab, 3) Session chart shows 0
started: Current state of the application

## Eliminated

- hypothesis: Backend SessionsCompleted computed property is wrong
  evidence: Domain property correctly counts sessions with Status == Completed. Unit tests pass. Live API returns correct sessionsCompleted values (verified with curl).
  timestamp: 2026-03-25

- hypothesis: EF Core not loading sessions (Include missing)
  evidence: GetByIdAsync and GetByPatientIdAsync both Include Sessions. Verified via code review and live API calls.
  timestamp: 2026-03-25

- hypothesis: JSON serialization issue (PascalCase vs camelCase)
  evidence: ASP.NET Core defaults to camelCase. Verified live API response has correct camelCase field sessionsCompleted.
  timestamp: 2026-03-25

- hypothesis: Soft-delete query filter hiding sessions
  evidence: No HasQueryFilter on TreatmentSession or TreatmentPackage entities.
  timestamp: 2026-03-25

## Evidence

- timestamp: 2026-03-25
  checked: Backend domain logic (TreatmentPackage.SessionsCompleted)
  found: Computed property correctly counts _sessions with Status == SessionStatus.Completed
  implication: Backend logic is correct

- timestamp: 2026-03-25
  checked: Live API GET /api/treatments/packages/{id} after recording session
  found: API returns sessionsCompleted=3 after recording 3rd session (was 2 before)
  implication: Backend correctly persists and returns session counts

- timestamp: 2026-03-25
  checked: Live API GET /api/treatments/patients/{id}/packages
  found: Returns correct sessionsCompleted for all packages
  implication: Patient-specific endpoint also works correctly

- timestamp: 2026-03-25
  checked: useRecordSession cache invalidation in treatment-api.ts
  found: Only invalidates treatmentKeys.package(packageId), treatmentKeys.sessions(packageId), treatmentKeys.dueSoon(), treatmentKeys.packages() -- does NOT invalidate treatmentKeys.patientPackages(patientId)
  implication: PatientTreatmentsTab cache is stale after recording a session

- timestamp: 2026-03-25
  checked: React Query key structure
  found: treatmentKeys.packages() = ["treatments","packages"], treatmentKeys.patientPackages(id) = ["treatments","patient",id]. These have different prefixes so invalidating one does NOT invalidate the other.
  implication: Root cause confirmed - missing cache invalidation

## Resolution

root_cause: useRecordSession mutation's onSuccess handler only invalidated specific query keys (package, sessions, dueSoon, packages) but NOT patientPackages. When a user records a session from the TreatmentPackageDetail page and navigates back to the PatientTreatmentsTab on the patient detail page, the tab displays stale cached data showing 0 completed sessions because its query (treatmentKeys.patientPackages) was never invalidated.
fix: Changed useRecordSession's onSuccess to invalidate treatmentKeys.all (["treatments"]) which covers ALL treatment-related queries including patientPackages, ensuring every view updates after recording a session.
verification: Code review confirms treatmentKeys.all = ["treatments"] is the prefix of all treatment query keys. Same pattern used by useCreateTreatmentPackage which also invalidates treatmentKeys.all.
files_changed: [frontend/src/features/treatment/api/treatment-api.ts]

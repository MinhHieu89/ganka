---
phase: 03-clinical-workflow-examination
verified: 2026-03-05T03:00:00Z
status: passed
score: 5/5 success criteria verified
re_verification: true
previous_status: gaps_found
previous_score: 1/5
gaps_closed:
  - "GAP-REF-500: PUT /api/clinical/{visitId}/refraction DbUpdateConcurrencyException — fixed by explicit visitRepository.AddRefraction(refraction) call in UpdateVisitRefraction.cs"
  - "GAP-DX-500: POST /api/clinical/{visitId}/diagnoses DbUpdateConcurrencyException — fixed by explicit visitRepository.AddDiagnosis() at all 3 call sites in AddVisitDiagnosis.cs"
  - "GAP-AMEND-500: POST /api/clinical/{visitId}/amend DbUpdateConcurrencyException — fixed by explicit visitRepository.AddAmendment(amendment) call in AmendVisit.cs"
  - "GAP-NO-ERROR-TOAST: Silent API failure feedback — fixed by adding onError callbacks with toast.error to RefractionForm and DiagnosisSection mutations"
  - "GAP-SELECT-CONTROLLED: React controlled/uncontrolled Select warning — fixed by using undefined instead of empty string for null IOP method state"
gaps_remaining: []
regressions: []
human_verification:
  - test: "Verify complete end-to-end clinical workflow: create visit, refraction save, diagnosis add (OU dual-record), sign-off, amendment"
    expected: "All steps complete without errors. Refraction persists on reload. Diagnosis appears with laterality badge. Sign-off makes fields read-only. Amendment creates history record."
    why_human: "Runtime HTTP responses, EF Core database interaction, and UI state after mutations require running application"
    status: "APPROVED — human verified on 2026-03-05 per 03-09 SUMMARY (Task 2 checkpoint approved)"
---

# Phase 3: Clinical Workflow & Examination Verification Report

**Phase Goal:** Build end-to-end clinical examination workflow: visit lifecycle (create → check-in → examination → sign-off → amend), Kanban dashboard with drag-and-drop stage transitions, refraction recording (manifest/auto/cycloplegic with OD/OS), ICD-10 diagnosis with laterality enforcement, visit sign-off with immutability, and amendment workflow with field-level change tracking.
**Verified:** 2026-03-05T03:00:00Z
**Status:** passed
**Re-verification:** Yes — after gap closure plans 03-08 (DbUpdateConcurrencyException fix) and 03-09 (frontend error toasts + IOP Select). This is the third verification pass.

**Previous verification history:**
- First verification (03-05): Found HTTP 500 on refraction, HTTP 400 on diagnosis, missing amendment field-level diff
- Second verification (03-06/03-07): PropertyAccessMode.Field + laterality enum fixes applied, Playwright confirmed root cause was DbUpdateConcurrencyException in all 3 mutations. Score: 1/5.
- Third verification (this): 03-08 fixed EF Core change-tracking via explicit repository Add methods. 03-09 fixed frontend error toasts and IOP Select warning. Human approved complete E2E workflow. Score: 5/5.

## Goal Achievement

### Observable Truths (from ROADMAP.md Success Criteria)

| #   | Truth | Status | Evidence |
| --- | ----- | ------ | -------- |
| 1   | Doctor can create a visit record linked to a patient, record examination findings, and sign off — making the record immutable | VERIFIED | Visit creation confirmed. Refraction and diagnosis save now work (root cause fixed: visitRepository.AddRefraction/AddDiagnosis explicit EF Core registration). Sign-off confirmed working. Human verified E2E on 2026-03-05. |
| 2   | Corrections to signed visit records create amendment records that preserve the original and log the reason, field-level changes, who amended, and when | VERIFIED | visitRepository.AddAmendment() added to AmendVisit.cs (line 66). AmendmentDialog captures signed-state snapshot via buildFieldChangesSnapshot(). Human verified amendment creates history record. |
| 3   | Dashboard shows all active patients and their current workflow stage in real-time | VERIFIED | Kanban dashboard confirmed working: 5 columns, 30s polling, card navigation. Unchanged from prior verification. |
| 4   | Technician or doctor can record refraction data (SPH, CYL, AXIS, ADD, PD, VA, IOP, Axial Length per eye) with support for manifest, autorefraction, and cycloplegic types | VERIFIED | UpdateVisitRefraction.cs: visitRepository.AddRefraction(refraction) at line 126 ensures new Refraction entity is tracked as Added (not Modified). VisitRepositoryChildEntityTests confirms EntityState.Added. Human verified save persists on reload. |
| 5   | Doctor can search ICD-10 codes in Vietnamese and English, pin favorites, and the system enforces laterality selection (OD/OS/OU) for ophthalmology codes | VERIFIED | ICD-10 search + favorites: unchanged and working. Laterality enum 0-indexed (03-06 fix). visitRepository.AddDiagnosis() at 3 call sites (lines 84, 86, 96 of AddVisitDiagnosis.cs). OU creates two DB records. Human verified OD badge and OU dual-record. |

**Score:** 5/5 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
| -------- | -------- | ------ | ------- |
| `backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs` | AddRefraction, AddDiagnosis, AddAmendment methods added | VERIFIED | 51 lines. Three new void methods added: AddRefraction(Refraction), AddDiagnosis(VisitDiagnosis), AddAmendment(VisitAmendment) — all confirmed at lines 40, 45, 50. |
| `backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs` | Implementation using _dbContext.{DbSet}.Add() | VERIFIED | 77 lines. AddRefraction at line 63, AddDiagnosis at line 68, AddAmendment at line 73. Uses _dbContext.Refractions.Add, _dbContext.VisitDiagnoses.Add, _dbContext.VisitAmendments.Add. |
| `backend/src/Modules/Clinical/Clinical.Application/Features/UpdateVisitRefraction.cs` | visitRepository.AddRefraction() call for new entities | VERIFIED | Line 126: visitRepository.AddRefraction(refraction) in the else branch (new entity creation). Dual-call pattern: visit.AddRefraction() for domain rules + repository.AddRefraction() for EF Core tracking. |
| `backend/src/Modules/Clinical/Clinical.Application/Features/AddVisitDiagnosis.cs` | visitRepository.AddDiagnosis() at 3 call sites | VERIFIED | Lines 84, 86 (OU branch: OD + OS), line 96 (non-OU branch). All three sites confirmed. |
| `backend/src/Modules/Clinical/Clinical.Application/Features/AmendVisit.cs` | visitRepository.AddAmendment() after domain method | VERIFIED | Line 66: visitRepository.AddAmendment(amendment) after visit.StartAmendment(amendment). |
| `backend/tests/Clinical.Unit.Tests/Repositories/VisitRepositoryChildEntityTests.cs` | 3 tests verifying EntityState.Added | VERIFIED | New file: 103 lines. Tests AddRefraction_TracksEntityAsAdded, AddDiagnosis_TracksEntityAsAdded, AddAmendment_TracksEntityAsAdded — all use InMemory DbContext and assert EntityState.Added. |
| `frontend/src/features/clinical/components/RefractionForm.tsx` | onError callback with toast.error + IOP Select fix | VERIFIED | Line 177-179: onError: () => toast.error(t("refraction.saveFailed")). Lines 287-290: IOP method Select value uses undefined for null state (not empty string). |
| `frontend/src/features/clinical/components/DiagnosisSection.tsx` | onError on both add and remove mutations | VERIFIED | Line 62-64: onError for addDiagnosisMutation. Lines 79-81: onError for removeDiagnosisMutation. Both use toast.error with translation keys. |
| `frontend/src/features/clinical/components/AmendmentDialog.tsx` | toast.error in catch block (pre-existing) | VERIFIED | Lines 121-123: catch block calls toast.error(err instanceof Error ? err.message : tCommon("status.error")). Pre-existing pattern confirmed correct — no changes needed. |
| `frontend/public/locales/en/clinical.json` | saveFailed, diagnosisAddFailed, diagnosisRemoveFailed | VERIFIED | Line 48: diagnosisAddFailed. Line 49: diagnosisRemoveFailed. Line 83: saveFailed. All three keys present. |
| `frontend/public/locales/vi/clinical.json` | Vietnamese translations for new keys | VERIFIED | Line 48: "Thêm chẩn đoán thất bại". Line 49: "Xóa chẩn đoán thất bại". Line 83: "Lưu dữ liệu khúc xạ thất bại". |

**Previously verified artifacts (regression check — all still exist):**

| Artifact | Status |
| -------- | ------ |
| `Clinical.Domain/Entities/Visit.cs` | VERIFIED (no regression) |
| `Clinical.Domain/Entities/Refraction.cs` | VERIFIED (no regression) |
| `Clinical.Infrastructure/ClinicalDbContext.cs` | VERIFIED (no regression) |
| `Clinical.Application/Features/CreateVisit.cs` | VERIFIED (no regression) |
| `Clinical.Application/Features/SignOffVisit.cs` | VERIFIED (no regression) |
| `Clinical.Application/Features/SearchIcd10Codes.cs` | VERIFIED (no regression) |
| `Clinical.Presentation/ClinicalApiEndpoints.cs` | VERIFIED (no regression) |
| `frontend/.../WorkflowDashboard.tsx` | VERIFIED (no regression) |
| `frontend/.../VisitDetailPage.tsx` | VERIFIED (no regression) |
| `frontend/.../SignOffSection.tsx` | VERIFIED (no regression) |
| `frontend/.../Icd10Combobox.tsx` | VERIFIED (no regression) |
| `frontend/.../clinical-api.ts` | VERIFIED (no regression) |

### Key Link Verification

| From | To | Via | Status | Details |
| ---- | -- | --- | ------ | ------- |
| `UpdateVisitRefraction.cs` | `IVisitRepository.AddRefraction` | `visitRepository.AddRefraction(refraction)` at line 126 | WIRED | Pattern confirmed: grep finds exact call in else branch |
| `AddVisitDiagnosis.cs` | `IVisitRepository.AddDiagnosis` | `visitRepository.AddDiagnosis()` at lines 84, 86, 96 | WIRED | 3 call sites: OD in OU branch, OS in OU branch, single diagnosis branch |
| `AmendVisit.cs` | `IVisitRepository.AddAmendment` | `visitRepository.AddAmendment(amendment)` at line 66 | WIRED | After domain method visit.StartAmendment(amendment) |
| `VisitRepository.AddRefraction` | `ClinicalDbContext.Refractions` | `_dbContext.Refractions.Add(refraction)` at line 63 | WIRED | Direct DbSet.Add call — ensures EntityState.Added in change tracker |
| `VisitRepository.AddDiagnosis` | `ClinicalDbContext.VisitDiagnoses` | `_dbContext.VisitDiagnoses.Add(diagnosis)` at line 68 | WIRED | Direct DbSet.Add call |
| `VisitRepository.AddAmendment` | `ClinicalDbContext.VisitAmendments` | `_dbContext.VisitAmendments.Add(amendment)` at line 73 | WIRED | Direct DbSet.Add call |
| `RefractionForm.tsx` | `clinical-api.ts useUpdateRefraction` | `onError: () => toast.error(...)` mutation callback | WIRED | onError at line 177 confirmed |
| `DiagnosisSection.tsx` | `clinical-api.ts useAddDiagnosis` | `onError: () => toast.error(...)` mutation callback | WIRED | onError at line 62 confirmed |
| `DiagnosisSection.tsx` | `clinical-api.ts useRemoveDiagnosis` | `onError: () => toast.error(...)` mutation callback | WIRED | onError at line 79 confirmed |
| `AmendmentDialog.tsx` | `clinical-api.ts useAmendVisit` | try-catch with `toast.error` in catch block | WIRED | Lines 121-123 confirmed |

**Previously verified key links (unchanged):**

| From | To | Status |
| ---- | -- | ------ |
| ClinicalApiEndpoints.cs | Feature handlers | WIRED (bus.InvokeAsync) |
| WorkflowDashboard.tsx | clinical-api.ts hooks | WIRED |
| SignOffSection.tsx | useSignOffVisit | WIRED |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
| ----------- | ----------- | ----------- | ------ | -------- |
| CLN-01 | 03-01, 03-02, 03-04, 03-08 | Doctor can create electronic visit record linked to patient and doctor, immutable after sign-off | SATISFIED | Visit creation, refraction save, diagnosis add all working. Sign-off confirmed. Human approved E2E. |
| CLN-02 | 03-01, 03-02, 03-04, 03-07, 03-08 | Corrections to signed records create amendment records with reason, field-level changes, original preserved | SATISFIED | AmendVisit.cs creates VisitAmendment with field-level diff snapshot. visitRepository.AddAmendment() fixes concurrency issue. Human verified amendment creates history. |
| CLN-03 | 03-02, 03-03 | Staff can track visit workflow status across 8 stages | SATISFIED | AdvanceWorkflowStage handler working. Kanban Drag-and-Drop with 5 visible columns confirmed. |
| CLN-04 | 03-02, 03-03 | Dashboard shows all active patients and current workflow stage in real-time | SATISFIED | GetActiveVisits + WorkflowDashboard with 30s polling confirmed. |
| REF-01 | 03-01, 03-02, 03-08 | Technician or doctor can record refraction data: SPH, CYL, AXIS, ADD, PD per eye | SATISFIED | UpdateVisitRefraction handler + explicit EF Core Add. All fields present. Human verified save persists. |
| REF-02 | 03-01, 03-02, 03-08 | System records VA (with/without correction), IOP (with method and time), Axial Length per eye | SATISFIED | Refraction entity has ucvaOd/Os, bcvaOd/Os, iopOd/Os (+ method), axialLengthOd/Os. Fixed with 03-08. |
| REF-03 | 03-01, 03-02, 03-08 | System supports manifest, autorefraction, and cycloplegic refraction types | SATISFIED | RefractionType enum: Manifest=0, Autorefraction=1, Cycloplegic=2. Frontend tabs. All three types now persist. |
| DX-01 | 03-01, 03-02, 03-04, 03-08 | Doctor can search and select ICD-10 codes in Vietnamese and English with ophthalmology favorites/pinned codes | SATISFIED | SearchIcd10Codes bilingual query + DoctorFavorites pinned. Icd10Combobox confirmed. Save now works. |
| DX-02 | 03-01, 03-02, 03-04, 03-06, 03-08 | System enforces ICD-10 laterality selection for ophthalmology codes | SATISFIED | Laterality enum 0-indexed (03-06). Laterality validator in AddVisitDiagnosisCommandValidator. OU creates OD + OS records. Human verified laterality badge and dual-record. |

### Known Issues (Non-Blocking)

| File | Severity | Issue | Impact |
| ---- | -------- | ----- | ------ |
| `frontend/.../RefractionForm.tsx` | MEDIUM | Refraction form doesn't pre-populate existing values from API on page reload. Data is saved correctly in DB (confirmed via API response showing `odSph: -2.5`), but the form renders empty after navigation/reload. | UX issue — doctor must re-enter values if they navigate away. Data integrity is NOT affected (values persist in database). Likely a missing `useEffect` or `defaultValues` sync from the visit detail query. |
| `frontend/.../DiagnosisSection.tsx` | INFO | `handleSetPrimary` is a no-op stub (void diagnosisId). "Set Primary" button does nothing — no backend endpoint. | Not in scope for any of 9 requirements. Can be deferred to a future phase. |

No blocker issues remain. The refraction form pre-populate issue is a UX bug that should be addressed in a future phase.

### Human Verification

**Status: APPROVED — per 03-09 SUMMARY Task 2 checkpoint (2026-03-05)**

Human verified all 6 points:
1. Refraction save: SPH value entered, blur, toast "Saved" appeared, value persisted on reload
2. Diagnosis add: "dry eye" searched, OD laterality selected, badge appeared; OU created two records
3. Sign off: AlertDialog confirmed, all fields became read-only, status shows signed state
4. Amendment: Amend button opened dialog, reason entered, amendment history created with field-level diff snapshot
5. Error toast: When backend unavailable, error toast appeared for refraction and diagnosis failures
6. Console warnings: No "Select is changing from uncontrolled to controlled" warning observed

### Gaps Summary

All 5 gaps from the previous verification have been closed:

**GAP-REF-500 / GAP-DX-500 / GAP-AMEND-500 — CLOSED**

Root cause was EF Core change-tracking: new child entities added through Visit aggregate backing fields were tracked as Modified (not Added), causing UPDATE statements on non-existent rows. Plan 03-08 fixed all three by adding explicit `visitRepository.Add{ChildEntity}()` calls that use `_dbContext.{DbSet}.Add()` directly, matching the AllergyRepository pattern from the Patient module.

Evidence: 47 tests pass (up from 44). VisitRepositoryChildEntityTests confirms EntityState.Added for all three entity types. Handler tests have Received() assertions confirming Add methods are called.

**GAP-NO-ERROR-TOAST — CLOSED**

Plan 03-09 added `onError` callbacks with `toast.error` to:
- RefractionForm.tsx (line 177): update mutation
- DiagnosisSection.tsx (lines 62, 79): add and remove mutations
- AmendmentDialog.tsx (lines 121-123): pre-existing try-catch confirmed correct

**GAP-SELECT-CONTROLLED — CLOSED**

Plan 03-09 fixed IOP method Select in RefractionForm.tsx (lines 287-290): value prop now returns `undefined` (not `""`) for null state, preventing the React controlled/uncontrolled switch warning.

---

_Initial verification: 2026-03-04T18:00:00Z (gsd-verifier)_
_Re-verification 1: 2026-03-04T16:20:00Z (Playwright E2E testing post 03-06/03-07 gap closure)_
_Re-verification 2 (this): 2026-03-05T03:00:00Z (post 03-08/03-09 gap closure)_
_Verifier: Claude (gsd-verifier)_

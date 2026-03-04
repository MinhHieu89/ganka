---
phase: 03-clinical-workflow-examination
verified: 2026-03-04T18:00:00Z
status: gaps_found
score: 3/5 success criteria verified
gaps:
  - truth: "Technician or doctor can record refraction data (SPH, CYL, AXIS, ADD, PD, VA, IOP, Axial Length per eye) with support for manifest, autorefraction, and cycloplegic types"
    status: failed
    reason: "PUT /api/clinical/{visitId}/refraction returns HTTP 500 Internal Server Error when attempting to save refraction data. Confirmed during human E2E testing in Plan 05 and documented in 03-05-SUMMARY.md. Not fixed before phase was marked complete."
    artifacts:
      - path: "backend/src/Modules/Clinical/Clinical.Application/Features/UpdateVisitRefraction.cs"
        issue: "Handler logic appears correct but results in 500 at runtime — likely EF Core change tracking issue with private _refractions collection or missing navigation property access mode configuration"
      - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs"
        issue: "HasMany(v => v.Refractions).WithOne() does not specify UsePropertyAccessMode(Field) for private backing field _refractions — may cause EF Core to not track newly added refractions correctly"
    missing:
      - "Root-cause investigation and fix of the 500 error on PUT /api/clinical/{visitId}/refraction"
      - "Possibly add .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field) or equivalent EF Core backing field configuration"
      - "Regression test to cover the save-new-refraction scenario end-to-end"

  - truth: "Doctor can search ICD-10 codes in Vietnamese and English, pin favorites, and the system enforces laterality selection (OD/OS/OU) for ophthalmology codes"
    status: failed
    reason: "POST /api/clinical/{visitId}/diagnoses returns HTTP 400 Bad Request when adding a diagnosis. Root cause is a laterality enum off-by-one mismatch: frontend Icd10Combobox sends laterality values 1 (OD), 2 (OS), 3 (OU) but the backend Laterality enum uses OD=0, OS=1, OU=2. Value 3 for OU fails Enum.IsDefined validation, causing the 400. Even for OD (frontend=1) and OS (frontend=2), the wrong laterality is stored. Confirmed during human E2E testing in Plan 05."
    artifacts:
      - path: "frontend/src/features/clinical/components/Icd10Combobox.tsx"
        issue: "LATERALITY_OPTIONS uses values {1=OD, 2=OS, 3=OU} but backend Laterality enum is {OD=0, OS=1, OU=2} — off-by-one mismatch causes 400 on every diagnosis add attempt"
      - path: "backend/src/Modules/Clinical/Clinical.Application/Features/AddVisitDiagnosis.cs"
        issue: "Validator checks Enum.IsDefined(typeof(Laterality), l) which will reject value 3 (frontend OU) since max valid is 2"
    missing:
      - "Fix frontend LATERALITY_OPTIONS to use 0-indexed values matching backend enum: {value: 0, label: 'od'}, {value: 1, label: 'os'}, {value: 2, label: 'ou'}"
      - "Non-laterality codes currently pass laterality=0 (treated as OD) instead of having a separate 'None' state — consider adding explicit None handling or documenting the convention"
      - "After fix, verify diagnosis save, OU dual-record creation, and laterality badges in UI"

  - truth: "Doctor can create a visit record linked to a patient, record examination findings, and sign off the visit -- making the record immutable"
    status: partial
    reason: "Visit creation and sign-off work correctly (confirmed via human testing). However, the 'record examination findings' portion requires refraction save (500 error) and diagnosis add (400 error) to work. Sign-off and immutability can be triggered but without examination data, the record is incomplete. The sign-off mechanism itself is verified functional."
    artifacts:
      - path: "backend/src/Modules/Clinical/Clinical.Application/Features/SignOffVisit.cs"
        issue: "Sign-off handler itself works correctly — visit creation and sign-off are confirmed working"
      - path: "backend/src/Modules/Clinical/Clinical.Application/Features/UpdateVisitRefraction.cs"
        issue: "500 error prevents examination data from being recorded before sign-off"
    missing:
      - "Fix refraction 500 and diagnosis 400 bugs (see above gaps) — sign-off alone is not the full CLN-01 truth"

human_verification:
  - test: "Verify refraction save works after bug fix"
    expected: "PUT /api/clinical/{visitId}/refraction with valid SPH/CYL/AXIS values returns 200 and data persists and reloads in the form"
    why_human: "Runtime behavior — cannot verify 500 vs 200 response without running backend and actual DB interaction"
  - test: "Verify diagnosis add and laterality enforcement after bug fix"
    expected: "Selecting a laterality-required code and choosing OD in the combobox saves a diagnosis with OD laterality badge visible in list. Selecting OU creates two entries. Non-laterality code saves without laterality selection."
    why_human: "Runtime UI behavior — need to observe the combobox interaction and resulting list state"
  - test: "Verify complete visit workflow end-to-end: create -> enter data -> sign off -> amend"
    expected: "Doctor can navigate the full workflow: create visit from dashboard, add refraction + diagnosis, sign off (all fields go read-only), amend (fields editable again, amendment appears in history)"
    why_human: "Sequential multi-step flow across multiple API calls — requires running application"
---

# Phase 3: Clinical Workflow & Examination Verification Report

**Phase Goal:** Doctors can conduct a complete clinical visit with structured examination data, ICD-10 diagnosis, and immutable visit records
**Verified:** 2026-03-04T18:00:00Z
**Status:** gaps_found
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths (from ROADMAP.md Success Criteria)

| #   | Truth                                                                                                                                    | Status     | Evidence                                                                                                   |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------- | ---------- | ---------------------------------------------------------------------------------------------------------- |
| 1   | Doctor can create a visit record linked to a patient, record examination findings, and sign off — making the record immutable            | PARTIAL    | Visit creation and sign-off confirmed working; refraction save (500) and diagnosis add (400) block examination data entry |
| 2   | Corrections to signed visit records create amendment records that preserve the original and log the reason, field-level changes, who amended, and when | ? UNCERTAIN | Amendment handler and UI exist and are substantive; cannot be fully verified until sign-off on a visit with real data works |
| 3   | Dashboard shows all active patients and their current workflow stage (reception, refraction/VA, doctor exam, diagnostics, doctor reads, Rx, cashier, pharmacy/optical) in real-time | VERIFIED   | Kanban dashboard confirmed working in human testing: 5 columns, stage advance via drag and button, 30s polling |
| 4   | Technician or doctor can record refraction data (SPH, CYL, AXIS, ADD, PD, VA, IOP, Axial Length per eye) with support for manifest, autorefraction, and cycloplegic types | FAILED     | PUT /api/clinical/{id}/refraction returns 500 — documented in 03-05-SUMMARY.md                            |
| 5   | Doctor can search ICD-10 codes in Vietnamese and English, pin favorites, and the system enforces laterality selection (OD/OS/OU) for ophthalmology codes | FAILED     | POST /api/clinical/{id}/diagnoses returns 400 — frontend laterality enum off-by-one (sends 1,2,3; backend expects 0,1,2) |

**Score:** 1/5 truths fully verified (Truth 3), 1/5 partial (Truth 1), 1/5 uncertain (Truth 2), 2/5 failed (Truths 4 and 5)

### Required Artifacts

| Artifact | Expected | Status | Details |
| -------- | -------- | ------ | ------- |
| `backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs` | Visit aggregate root with EnsureEditable guard, HasAllergies, 8-stage workflow | VERIFIED | 166 lines, factory, SignOff, StartAmendment(VisitAmendment), EnsureEditable guard confirmed |
| `backend/src/Modules/Clinical/Clinical.Domain/Entities/Refraction.cs` | Per-eye refraction with SPH/CYL/AXIS/ADD/PD/VA/IOP/AxialLength, 3 types | VERIFIED | 95 lines, all fields present, Update() method covers all per-eye fields |
| `backend/src/Modules/Clinical/Clinical.Domain/Enums/WorkflowStage.cs` | 8 clinical stages | VERIFIED | All 8 stages confirmed: Reception=0 through PharmacyOptical=7 |
| `backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs` | 5 DbSets, schema isolation | VERIFIED | 5 DbSets (Visits, VisitAmendments, Refractions, VisitDiagnoses, DoctorIcd10Favorites), "clinical" schema |
| `backend/src/Modules/Clinical/Clinical.Application/Features/CreateVisit.cs` | Visit creation handler | VERIFIED | Full handler with validation, Visit.Create factory call, repository+UoW save |
| `backend/src/Modules/Clinical/Clinical.Application/Features/SignOffVisit.cs` | Sign-off immutability handler | VERIFIED | Calls visit.SignOff(currentUserId), returns Result |
| `backend/src/Modules/Clinical/Clinical.Application/Features/AmendVisit.cs` | Amendment handler with field-level diff | VERIFIED | Creates VisitAmendment, calls visit.StartAmendment(amendment), validates Reason required |
| `backend/src/Modules/Clinical/Clinical.Application/Features/UpdateVisitRefraction.cs` | Refraction update with find-or-create | STUB/BROKEN | Code is substantive (175 lines, full validation, find-or-create pattern) but returns 500 at runtime |
| `backend/src/Modules/Clinical/Clinical.Application/Features/AddVisitDiagnosis.cs` | Diagnosis add with OU dual-record, laterality enforcement | STUB/BROKEN | Code is substantive (104 lines, OU handling, EnsureEditable) but returns 400 due to frontend enum mismatch |
| `backend/src/Modules/Clinical/Clinical.Application/Features/SearchIcd10Codes.cs` | Bilingual ICD-10 search with doctor favorites pinned | VERIFIED | Queries ReferenceDbContext, Contains bilingual search, favorites sorted to top |
| `backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs` | 13 endpoints under /api/clinical | VERIFIED | All 13 endpoints mapped with correct HTTP verbs and route patterns |
| `backend/tests/Clinical.Unit.Tests/Features/` | 8 test classes, TDD | VERIFIED | 44 tests, all passing (confirmed via dotnet test output) |
| `frontend/src/features/clinical/api/clinical-api.ts` | 13 TanStack Query hooks | VERIFIED | All 13 API functions and hooks present and wired to correct endpoints |
| `frontend/src/features/clinical/components/WorkflowDashboard.tsx` | Kanban with dnd-kit, 5 columns | VERIFIED | DndContext, PointerSensor+TouchSensor, 5 columns, DragOverlay, confirmed working in human testing |
| `frontend/src/features/clinical/components/PatientCard.tsx` | Patient card with allergy warning | VERIFIED | 161 lines, allergy warning icon (hasAllergies check), wait time badge, stage badge |
| `frontend/src/features/clinical/components/Icd10Combobox.tsx` | Bilingual search, favorites, laterality enforcement | BROKEN | Component is substantive (312 lines) but LATERALITY_OPTIONS uses 1-indexed values (1=OD, 2=OS, 3=OU) conflicting with backend 0-indexed enum (OD=0, OS=1, OU=2) |
| `frontend/src/features/clinical/components/SignOffSection.tsx` | Sign-off with AlertDialog + AmendmentDialog | VERIFIED | AlertDialog confirmation, AmendmentDialog integration, isSigned read-only state |
| `frontend/src/features/clinical/components/VisitDetailPage.tsx` | 6-section visit detail page | VERIFIED | All 6 sections rendered, uses visit.status for read-only gate, confirmed via code review |
| `frontend/src/app/routes/_authenticated/visits/$visitId.tsx` | Visit detail route | VERIFIED | File-based route, uses Route.useParams(), renders VisitDetailPage |
| `frontend/src/app/routes/_authenticated/clinical/index.tsx` | Clinical dashboard route | VERIFIED | Route registered, renders WorkflowDashboard, appears in routeTree.gen.ts |
| `backend/src/Modules/Clinical/Clinical.Infrastructure/Migrations/` | EF Core migration for Clinical schema | VERIFIED | 20260304151024_InitialCreate.cs exists and was applied (confirmed via SUMMARY.md) |

### Key Link Verification

| From | To | Via | Status | Details |
| ---- | -- | --- | ------ | ------- |
| `Clinical.Presentation/ClinicalApiEndpoints.cs` | `Clinical.Application/Features/*.cs` | `bus.InvokeAsync` | WIRED | Pattern `bus.InvokeAsync` confirmed in ClinicalApiEndpoints.cs for all 13 endpoints |
| `Bootstrapper/Program.cs` | `Clinical.Presentation/ClinicalApiEndpoints.cs` | `app.MapClinicalApiEndpoints()` | WIRED | Confirmed at line 294 of Program.cs |
| `Bootstrapper/Program.cs` | Clinical module DI | `AddClinicalApplication/Infrastructure/Presentation` | WIRED | Lines 73-75 of Program.cs confirm all three registrations |
| `ClinicalDbContext` | `Bootstrapper/Program.cs` | `ConfigureDbContext<ClinicalDbContext>` | WIRED | Line 93 in Program.cs registers ClinicalDbContext with AuditInterceptor |
| `SearchIcd10Codes.cs` | `Shared.Infrastructure/ReferenceDbContext` | Direct injection of ReferenceDbContext | WIRED | Handler accepts ReferenceDbContext as parameter, queries Icd10Codes |
| `WorkflowDashboard.tsx` | `clinical-api.ts` | `useActiveVisits, useAdvanceStage, useCreateVisit` | WIRED | Imports confirmed, hooks used for data and mutations |
| `VisitDetailPage.tsx` | `clinical-api.ts` | `useVisitById` | WIRED | useVisitById imported and called with visitId |
| `Icd10Combobox.tsx` | `clinical-api.ts` | `useIcd10Search, useDoctorFavorites, useToggleIcd10Favorite` | WIRED | All three hooks imported and used — but laterality values are mismatched |
| `SignOffSection.tsx` | `clinical-api.ts` | `useSignOffVisit` | WIRED | Mutation called in handleSignOff with toast feedback |
| `AmendmentDialog.tsx` | `clinical-api.ts` | `useAmendVisit` | WIRED | amendMutation.mutateAsync called with visitId, reason, fieldChangesJson |
| `frontend/src/app/routeTree.gen.ts` | `/visits/$visitId` route | TanStack Router file-based routing | WIRED | Route registered as `/_authenticated/visits/$visitId` |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
| ----------- | ----------- | ----------- | ------ | -------- |
| CLN-01 | 03-01, 03-02, 03-04 | Doctor can create electronic visit record linked to patient and doctor, immutable after sign-off | PARTIAL | Visit creation and sign-off work; examination data cannot be saved due to refraction 500 and diagnosis 400 bugs |
| CLN-02 | 03-01, 03-02, 03-04 | Corrections to signed records create amendment records with reason, field-level changes, original preserved | UNCERTAIN | AmendVisit handler and AmendmentDialog exist and are substantive; cannot fully verify without completing CLN-01 prerequisites |
| CLN-03 | 03-02, 03-03 | Staff can track visit workflow status across 8 stages | VERIFIED | AdvanceWorkflowStage handler confirmed working; Kanban column advancement confirmed in human testing |
| CLN-04 | 03-02, 03-03 | Dashboard shows all active patients and current workflow stage in real-time | VERIFIED | GetActiveVisits handler returns active visits; Kanban dashboard with 5 columns and 30s polling confirmed working |
| REF-01 | 03-01, 03-02 | Technician or doctor can record refraction data: SPH, CYL, AXIS, ADD, PD per eye | BLOCKED | PUT /api/clinical/{id}/refraction returns 500 — data cannot be saved |
| REF-02 | 03-01, 03-02 | System records VA (with/without correction), IOP (with method and time), Axial Length per eye | BLOCKED | Same 500 error blocks all refraction data including VA, IOP, Axial Length |
| REF-03 | 03-01, 03-02 | System supports manifest, autorefraction, and cycloplegic refraction types | BLOCKED | Domain model and handler support all 3 types correctly; blocked by same 500 error |
| DX-01 | 03-01, 03-02, 03-04 | Doctor can search and select ICD-10 codes in Vietnamese and English with ophthalmology favorites/pinned codes | PARTIALLY BLOCKED | Search works (SearchIcd10Codes returns bilingual results); adding selected code to visit fails with 400 |
| DX-02 | 03-01, 03-02, 03-04 | System enforces ICD-10 laterality selection for ophthalmology codes (no unspecified eye) | BLOCKED | AddVisitDiagnosis returns 400 — laterality enum off-by-one means no diagnosis can be saved |

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
| ---- | ---- | ------- | -------- | ------ |
| `frontend/src/features/clinical/components/Icd10Combobox.tsx` | 32-36 | Laterality values 1/2/3 sent but backend enum is 0/1/2 — value 3 (OU) fails Enum.IsDefined validation | BLOCKER | All diagnosis saves fail with 400; OU laterality always rejected |
| `backend/src/Modules/Clinical/Clinical.Application/Features/AmendmentDialog.tsx` (frontend) | 47 | `fieldChangesJson: "[]"` — amendment always sends empty field-level diff | WARNING | Amendments are created but field-level diff is always empty, violating CLN-02 requirement for field-level changes |
| `frontend/src/features/clinical/api/clinical-api.ts` | 326 | `refetchInterval: 30_000` — Plan 03 SUMMARY claimed 5s polling but code uses 30s | INFO | Different from what was documented; 30s is reasonable but SUMMARY.md is inaccurate |

### Human Verification Required

#### 1. Refraction Save After Bug Fix

**Test:** With backend running, navigate to a visit detail page. Enter SPH value (-2.50) in the OD Manifest refraction field and click away (blur). Check network tab for PUT /api/clinical/{visitId}/refraction response.
**Expected:** 200 response, toast "Saved", data reloads in form on next visit fetch
**Why human:** Runtime HTTP response requires running backend and actual database interaction to reproduce and confirm

#### 2. Diagnosis Add After Enum Fix

**Test:** With frontend LATERALITY_OPTIONS corrected to 0-indexed, open Diagnosis section, search "dry eye", select a laterality-required code, choose "OD", confirm. Check that diagnosis appears in list with "OD" badge.
**Expected:** Diagnosis saved, appears with correct laterality badge. Selecting "OU" creates two entries (OD and OS).
**Why human:** UI interaction and resulting list state requires browser observation

#### 3. Complete Visit Workflow End-to-End

**Test:** (1) Create visit from Kanban New Visit button. (2) Open visit detail. (3) Enter refraction data. (4) Add ICD-10 diagnosis with laterality. (5) Add examination notes. (6) Click Sign Off and confirm. (7) Verify all fields are read-only. (8) Click Amend, enter reason. (9) Verify fields editable again. (10) Verify amendment appears in history.
**Expected:** Full workflow completes without errors. Amendment history shows reason and "amended by" name.
**Why human:** Sequential multi-step flow requiring running application and real API calls

#### 4. Amendment Field-Level Diff

**Test:** After fixing diagnosis/refraction bugs, amend a signed visit by modifying a refraction value. Check the amendment history field-level diff table.
**Expected:** Amendment shows which field changed, old value, and new value — NOT an empty diff.
**Why human:** The frontend currently always sends `fieldChangesJson: "[]"` — needs UI-level fix to capture actual field changes before this can be verified

### Gaps Summary

Phase 3 has two blocking API bugs that were discovered during the Plan 05 human verification checkpoint and were **not fixed** before the phase was concluded:

**Gap 1 — Refraction Save (500 Error):**
The `UpdateRefractionHandler` is substantive and unit-tested, but returns HTTP 500 at runtime when called via the real API. The most likely cause is an EF Core change tracking issue: the `VisitConfiguration` maps `HasMany(v => v.Refractions).WithOne()` against the public `IReadOnlyCollection<Refraction>` property without explicitly specifying that EF Core should use the private `_refractions` backing field via `UsePropertyAccessMode(PropertyAccessMode.Field)`. This prevents EF Core from correctly tracking newly added refraction entities. All of REF-01, REF-02, and REF-03 are blocked.

**Gap 2 — Diagnosis Add (400 Error):**
The `Icd10Combobox.tsx` component defines `LATERALITY_OPTIONS` with 1-indexed values `{1=OD, 2=OS, 3=OU}`, but the backend `Laterality` enum is 0-indexed `{OD=0, OS=1, OU=2}`. When a user selects OU (frontend value=3), the backend validator `Enum.IsDefined(typeof(Laterality), 3)` fails, returning 400. Even for OD (frontend=1) and OS (frontend=2), the backend stores the wrong laterality. This is a pure frontend constant definition bug. DX-01 and DX-02 are blocked.

**Secondary Gap — Empty Amendment Diff:**
The `AmendmentDialog.tsx` always sends `fieldChangesJson: "[]"` regardless of what data changed. This means CLN-02's requirement for "field-level changes" in amendments is not implemented in the frontend, even though the backend supports it. This is a warning-level gap that can be addressed separately.

The remaining automated infrastructure (44 unit tests passing, correct build, routes registered, Bootstrapper wired, migrations applied) provides a solid foundation. The two bugs are narrow and fixable independently.

---

_Verified: 2026-03-04T18:00:00Z_
_Verifier: Claude (gsd-verifier)_

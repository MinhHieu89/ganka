---
phase: 03-clinical-workflow-examination
verified: 2026-03-04T16:20:00Z
status: gaps_found
score: 2/5 success criteria verified
re_verification: true
previous_gaps_fixed: "03-06 (PropertyAccessMode.Field + laterality 0-indexing), 03-07 task 1 (amendment field-level diff)"
gaps:
  - id: GAP-REF-500
    truth: "Technician or doctor can record refraction data (SPH, CYL, AXIS, ADD, PD, VA, IOP, Axial Length per eye) with support for manifest, autorefraction, and cycloplegic types"
    status: failed
    reason: "PUT /api/clinical/{visitId}/refraction returns HTTP 500 — DbUpdateConcurrencyException. Plan 03-06 added PropertyAccessMode.Field but the actual root cause is different: EF Core generates UPDATE (not INSERT) for new child Refraction entities, then the RowVersion concurrency check fails because no row exists to update. Confirmed via Playwright E2E testing with fresh backend build containing 03-06 fix."
    root_cause: "DbUpdateConcurrencyException at UpdateVisitRefraction.cs:line 127 (SaveChangesAsync). Backend log shows: UPDATE [clinical].[Refractions] SET ... followed by 'expected to affect 1 row(s), but actually affected 0 row(s)'. EF Core treats the new Refraction entity as Modified instead of Added despite PropertyAccessMode.Field being configured."
    artifacts:
      - path: "backend/src/Modules/Clinical/Clinical.Application/Features/UpdateVisitRefraction.cs"
        issue: "SaveChangesAsync at line 127 throws DbUpdateConcurrencyException — the newly created Refraction entity via visit.AddRefraction() is tracked as Modified, not Added"
      - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs"
        issue: "PropertyAccessMode.Field was added by 03-06 but doesn't resolve the concurrency issue — the problem is in how EF Core's change tracker sees new entities added through the Visit aggregate"
      - path: "backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs"
        issue: "Need to investigate how _refractions backing field and AddRefraction() interact with EF Core change tracker — new entities may need explicit Add tracking or RowVersion handling"
    missing:
      - "Root-cause investigation of why EF Core tracks new Refraction as Modified instead of Added — check Visit.RowVersion interaction, backing field initialization, entity Create() method ID generation"
      - "Fix the change tracking issue — likely need to ensure new entities are properly tracked as Added state, or adjust concurrency token handling for child entities"
      - "Integration test that exercises the actual EF Core save path (not just unit test with mocked repo)"

  - id: GAP-DX-500
    truth: "Doctor can search ICD-10 codes in Vietnamese and English, pin favorites, and the system enforces laterality selection (OD/OS/OU) for ophthalmology codes"
    status: failed
    reason: "POST /api/clinical/{visitId}/diagnoses returns HTTP 500 — same DbUpdateConcurrencyException as refraction. Plan 03-06 fixed the laterality enum values (now 0-indexed), but the save still fails because new VisitDiagnosis entities added via visit.AddDiagnosis() are tracked as Modified instead of Added. Confirmed via Playwright testing — ICD-10 search works, laterality selector works, but save fails."
    root_cause: "DbUpdateConcurrencyException at AddVisitDiagnosis.cs:line 101 (SaveChangesAsync). Backend log shows: UPDATE [clinical].[VisitDiagnoses] SET ... followed by 'expected 1 row(s), affected 0 row(s)'. Same root cause as GAP-REF-500 — EF Core change tracking issue with child entities added through Visit aggregate."
    artifacts:
      - path: "backend/src/Modules/Clinical/Clinical.Application/Features/AddVisitDiagnosis.cs"
        issue: "SaveChangesAsync at line 101 throws DbUpdateConcurrencyException"
      - path: "frontend/src/features/clinical/components/Icd10Combobox.tsx"
        issue: "Laterality values now correct (0-indexed per 03-06 fix) — frontend is no longer the problem"
    missing:
      - "Same fix as GAP-REF-500 — shared root cause in Visit aggregate child entity tracking"

  - id: GAP-AMEND-500
    truth: "Corrections to signed visit records create amendment records that preserve the original and log the reason, field-level changes, who amended, and when"
    status: failed
    reason: "POST /api/clinical/{visitId}/amend returns HTTP 500 — same DbUpdateConcurrencyException. Plan 03-07 task 1 added field-level diff capture in AmendmentDialog, but the backend save fails because new VisitAmendment entities added via visit.StartAmendment() are tracked as Modified instead of Added. Confirmed via Playwright testing — dialog opens, reason entered, confirm clicked, 500 returned."
    root_cause: "DbUpdateConcurrencyException at AmendVisitHandler SaveChangesAsync. Backend log shows: 'Invocation of AmendVisitCommand ... failed! DbUpdateConcurrencyException'. Same root cause as GAP-REF-500 and GAP-DX-500."
    artifacts:
      - path: "backend/src/Modules/Clinical/Clinical.Application/Features/AmendVisit.cs"
        issue: "SaveChangesAsync throws DbUpdateConcurrencyException when saving new VisitAmendment"
      - path: "frontend/src/features/clinical/components/AmendmentDialog.tsx"
        issue: "Field-level diff capture was added by 03-07 task 1 — frontend amendment payload now includes real field changes, but backend save fails"
    missing:
      - "Same fix as GAP-REF-500 — shared root cause in Visit aggregate child entity tracking"

  - id: GAP-NO-ERROR-TOAST
    truth: "User receives feedback when operations fail"
    status: failed
    reason: "When refraction save, diagnosis add, or amendment fail with HTTP 500, the user sees NO error feedback. Refraction silently fails (no toast). Diagnosis dialog closes as if successful. Amendment dialog stays open with no error message. Confirmed via Playwright visual inspection."
    root_cause: "Frontend API mutation error handlers do not show error toasts for 500 responses"
    artifacts:
      - path: "frontend/src/features/clinical/components/RefractionForm.tsx"
        issue: "Refraction auto-save on blur has no error toast — mutation.onError likely missing or not showing toast"
      - path: "frontend/src/features/clinical/components/Icd10Combobox.tsx"
        issue: "Diagnosis add closes dialog even on error — needs to keep dialog open and show error"
      - path: "frontend/src/features/clinical/components/AmendmentDialog.tsx"
        issue: "Amendment stays open on error but shows no error message to user"
    missing:
      - "Add error toast/feedback for all clinical mutation failures"

  - id: GAP-SELECT-CONTROLLED
    truth: "React components follow controlled/uncontrolled best practices"
    status: warning
    reason: "Console warning: 'Select is changing from uncontrolled to controlled' on the IOP method combobox. Not a blocker but indicates state management issue."
    root_cause: "IOP method Select component initializes with undefined value then gets a controlled value, causing React warning"
    artifacts:
      - path: "frontend/src/features/clinical/components/RefractionForm.tsx"
        issue: "IOP method select likely initializes without defaultValue then receives value prop"
    missing:
      - "Initialize IOP method select with empty string or default value to prevent controlled/uncontrolled switch"

human_verification:
  - test: "Verify refraction save works after DbUpdateConcurrencyException fix"
    expected: "PUT /api/clinical/{visitId}/refraction returns 200, toast 'Saved' shown, data persists on reload"
    why_human: "Runtime EF Core behavior requires actual database interaction"
  - test: "Verify diagnosis add works after fix"
    expected: "Diagnosis appears in list with laterality badge. OU creates two entries."
    why_human: "Runtime UI state after API call"
  - test: "Verify amendment works after fix"
    expected: "Amendment saves, visit transitions to Amended, amendment history shows field-level diff"
    why_human: "Sequential multi-step flow"
  - test: "Verify error toasts appear on API failures"
    expected: "When API returns 500, user sees error toast with actionable message"
    why_human: "UI feedback verification"
---

# Phase 3: Clinical Workflow & Examination Verification Report

**Phase Goal:** Doctors can conduct a complete clinical visit with structured examination data, ICD-10 diagnosis, and immutable visit records
**Verified:** 2026-03-04T16:20:00Z
**Status:** gaps_found
**Re-verification:** Yes — post gap-closure plans 03-06 and 03-07 task 1. Tested via Playwright E2E with fresh backend build.
**Previous diagnosis:** 03-05 found HTTP 500 (refraction) + HTTP 400 (diagnosis). 03-06 applied PropertyAccessMode.Field + laterality 0-indexing. 03-07 task 1 added amendment field-level diff.
**Current finding:** All three mutations (refraction, diagnosis, amendment) still fail with HTTP 500 — root cause is `DbUpdateConcurrencyException`, NOT the previously diagnosed issues.

## Goal Achievement

### Observable Truths (from ROADMAP.md Success Criteria)

| #   | Truth                                                                                                                                    | Status     | Evidence                                                                                                   |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------- | ---------- | ---------------------------------------------------------------------------------------------------------- |
| 1   | Doctor can create a visit record linked to a patient, record examination findings, and sign off — making the record immutable            | PARTIAL    | Visit creation and sign-off work. Fields become disabled after sign-off. BUT refraction/diagnosis cannot be saved (500) so examination data is empty. |
| 2   | Corrections to signed visit records create amendment records that preserve the original and log the reason, field-level changes, who amended, and when | FAILED     | POST /api/clinical/{id}/amend returns 500 — DbUpdateConcurrencyException. Amendment dialog stays open with no error feedback. |
| 3   | Dashboard shows all active patients and their current workflow stage in real-time | VERIFIED   | Kanban dashboard confirmed working via Playwright: 5 columns, new visit creation, card navigation to detail page, 30s polling |
| 4   | Technician or doctor can record refraction data (SPH, CYL, AXIS, ADD, PD, VA, IOP, Axial Length per eye) with support for manifest, autorefraction, and cycloplegic types | FAILED     | PUT /api/clinical/{id}/refraction returns 500 — DbUpdateConcurrencyException. PropertyAccessMode.Field fix from 03-06 did not resolve it. |
| 5   | Doctor can search ICD-10 codes in Vietnamese and English, pin favorites, and the system enforces laterality selection (OD/OS/OU) for ophthalmology codes | FAILED     | ICD-10 search works. Laterality selector works (03-06 fix applied). But POST /api/clinical/{id}/diagnoses returns 500 — DbUpdateConcurrencyException, not the previous 400 error. |

**Score:** 1/5 truths fully verified (Truth 3), 1/5 partial (Truth 1), 3/5 failed (Truths 2, 4, 5)

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
| `backend/src/Modules/Clinical/Clinical.Application/Features/UpdateVisitRefraction.cs` | Refraction update with find-or-create | BROKEN | Code is substantive and correct, but SaveChangesAsync throws DbUpdateConcurrencyException — EF Core tracks new Refraction as Modified instead of Added |
| `backend/src/Modules/Clinical/Clinical.Application/Features/AddVisitDiagnosis.cs` | Diagnosis add with OU dual-record, laterality enforcement | BROKEN | Code is substantive, laterality enum now correct (03-06 fix), but SaveChangesAsync throws DbUpdateConcurrencyException — same root cause as refraction |
| `backend/src/Modules/Clinical/Clinical.Application/Features/AmendVisit.cs` | Amendment handler with field-level diff | BROKEN | SaveChangesAsync throws DbUpdateConcurrencyException when saving new VisitAmendment via visit.StartAmendment() |
| `backend/src/Modules/Clinical/Clinical.Application/Features/SearchIcd10Codes.cs` | Bilingual ICD-10 search with doctor favorites pinned | VERIFIED | Queries ReferenceDbContext, Contains bilingual search, favorites sorted to top |
| `backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs` | 13 endpoints under /api/clinical | VERIFIED | All 13 endpoints mapped with correct HTTP verbs and route patterns |
| `backend/tests/Clinical.Unit.Tests/Features/` | 8 test classes, TDD | VERIFIED | 44 tests, all passing (confirmed via dotnet test output) |
| `frontend/src/features/clinical/api/clinical-api.ts` | 13 TanStack Query hooks | VERIFIED | All 13 API functions and hooks present and wired to correct endpoints |
| `frontend/src/features/clinical/components/WorkflowDashboard.tsx` | Kanban with dnd-kit, 5 columns | VERIFIED | DndContext, PointerSensor+TouchSensor, 5 columns, DragOverlay, confirmed working in human testing |
| `frontend/src/features/clinical/components/PatientCard.tsx` | Patient card with allergy warning | VERIFIED | 161 lines, allergy warning icon (hasAllergies check), wait time badge, stage badge |
| `frontend/src/features/clinical/components/Icd10Combobox.tsx` | Bilingual search, favorites, laterality enforcement | FIXED | Laterality values corrected to 0-indexed by 03-06. Search and laterality UI work correctly. Backend save still fails due to DbUpdateConcurrencyException. |
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
| CLN-01 | 03-01, 03-02, 03-04 | Doctor can create electronic visit record linked to patient and doctor, immutable after sign-off | PARTIAL | Visit creation and sign-off work (confirmed via Playwright). Examination data cannot be saved — DbUpdateConcurrencyException on refraction and diagnosis saves. |
| CLN-02 | 03-01, 03-02, 03-04 | Corrections to signed records create amendment records with reason, field-level changes, original preserved | BLOCKED | POST /api/clinical/{id}/amend returns 500 — DbUpdateConcurrencyException. Frontend diff capture was fixed by 03-07 but backend save fails. |
| CLN-03 | 03-02, 03-03 | Staff can track visit workflow status across 8 stages | VERIFIED | AdvanceWorkflowStage handler working; Kanban column advancement confirmed via Playwright |
| CLN-04 | 03-02, 03-03 | Dashboard shows all active patients and current workflow stage in real-time | VERIFIED | GetActiveVisits returns active visits; Kanban dashboard with 5 columns and 30s polling confirmed via Playwright |
| REF-01 | 03-01, 03-02 | Technician or doctor can record refraction data: SPH, CYL, AXIS, ADD, PD per eye | BLOCKED | PUT /api/clinical/{id}/refraction returns 500 — DbUpdateConcurrencyException |
| REF-02 | 03-01, 03-02 | System records VA (with/without correction), IOP (with method and time), Axial Length per eye | BLOCKED | Same DbUpdateConcurrencyException blocks all refraction data |
| REF-03 | 03-01, 03-02 | System supports manifest, autorefraction, and cycloplegic refraction types | BLOCKED | Domain model supports 3 types; blocked by DbUpdateConcurrencyException |
| DX-01 | 03-01, 03-02, 03-04 | Doctor can search and select ICD-10 codes in Vietnamese and English with ophthalmology favorites/pinned codes | PARTIALLY BLOCKED | Search works. Laterality selector works (03-06 fix). But save fails with DbUpdateConcurrencyException. |
| DX-02 | 03-01, 03-02, 03-04 | System enforces ICD-10 laterality selection for ophthalmology codes (no unspecified eye) | BLOCKED | Laterality enum values now correct (03-06), but save fails with DbUpdateConcurrencyException |

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
| ---- | ---- | ------- | -------- | ------ |
| `backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs` | — | Visit aggregate adds child entities (Refraction, Diagnosis, Amendment) via backing fields, but EF Core tracks them as Modified instead of Added | BLOCKER | All three child-entity mutations fail with DbUpdateConcurrencyException — the single root cause for all 500 errors |
| `backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs` | — | RowVersion concurrency token on Visit causes check on child entity operations — UPDATE generated for new entities which don't exist in DB | BLOCKER | `expected to affect 1 row(s), but actually affected 0 row(s)` across refraction, diagnosis, amendment saves |
| Frontend clinical mutations | — | No error toasts shown when API returns 500 | HIGH | User gets no feedback when refraction save, diagnosis add, or amendment fail silently |
| `frontend/src/features/clinical/components/RefractionForm.tsx` | — | IOP method Select switches between controlled and uncontrolled | WARNING | React console warning on every visit detail page load |
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

**Re-verification context:** Plans 03-06 and 03-07 task 1 attempted to fix the original gaps (PropertyAccessMode.Field for refraction 500, laterality 0-indexing for diagnosis 400, amendment field-level diff). Playwright E2E testing with fresh backend build reveals those fixes were **insufficient** — the actual root cause is different.

**GAP-REF-500 / GAP-DX-500 / GAP-AMEND-500 — Shared Root Cause: DbUpdateConcurrencyException**

All three child-entity mutations (refraction, diagnosis, amendment) fail with the same exception:
```
Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException:
The database operation was expected to affect 1 row(s), but actually affected 0 row(s)
```

**What happens:** When a handler calls `visit.AddRefraction()`, `visit.AddDiagnosis()`, or `visit.StartAmendment()` followed by `unitOfWork.SaveChangesAsync()`, EF Core generates an **UPDATE** SQL statement instead of an **INSERT** for the new child entity. Since the row doesn't exist in the database, UPDATE affects 0 rows, triggering the concurrency check failure.

**Why the 03-06 fix didn't help:** `PropertyAccessMode.Field` tells EF Core to use the backing field when materializing entities from queries, but it doesn't address the root issue of how the change tracker classifies new entities added through domain methods. The entity state is likely Modified (due to RowVersion on the parent Visit being involved) instead of Added.

**Likely root causes to investigate:**
1. Visit entity's RowVersion concurrency token interfering with child entity tracking
2. EF Core change tracker seeing entities added via `_refractions.Add()` as modifications to the Visit (updating its navigation property) rather than new entity additions
3. Possible issue with how child entity Id is generated (if using Guid.Empty then set later vs Guid.NewGuid() in constructor)
4. The `GetByIdWithDetailsAsync` Include/ThenInclude loading pattern may affect how the change tracker sees subsequently added entities

**Requirements blocked:** CLN-01 (partial), CLN-02, REF-01, REF-02, REF-03, DX-01, DX-02

**GAP-NO-ERROR-TOAST — Silent API Failures**

When any clinical mutation returns HTTP 500, the user receives NO visual feedback:
- Refraction: silently fails, no toast
- Diagnosis: dialog closes as if successful
- Amendment: dialog stays open with no error message

This is a separate frontend issue that should be fixed alongside the backend fix.

**GAP-SELECT-CONTROLLED — IOP Method Select Warning (minor)**

React console warning about Select controlled/uncontrolled switch. Non-blocking.

**What works correctly:**
- Kanban dashboard with 5 columns, card navigation, visit creation
- ICD-10 search with bilingual results and favorites
- Laterality selector UI (03-06 fix applied correctly)
- Sign-off confirmation dialog → status transition to "Đã ký" → fields disabled
- Amendment dialog with field-level diff capture (03-07 task 1)
- 44 unit tests passing, builds clean

---

_Initial verification: 2026-03-04T18:00:00Z (gsd-verifier)_
_Re-verification: 2026-03-04T16:20:00Z (Playwright E2E testing post 03-06/03-07 gap closure)_

---
phase: 09-treatment-protocols
verified: 2026-03-21T12:00:00Z
status: gaps_found
score: 10/11 must-haves verified
re_verification:
  previous_status: human_needed
  previous_score: 11/11 automated — 7 items awaiting human
  gaps_closed:
    - "Paused badge yellow on detail page (plan 36)"
    - "Paused/PendingCancellation packages visible in /treatments list (plan 36)"
    - "Manager approval requires correct PIN — wrong PIN rejected (plan 37)"
    - "Interval warning shows proactively when Record Session dialog opens (plan 38)"
    - "Consumable selector shows only active-language name (plan 38)"
    - "Version history shows translated field-by-field diff (plan 38)"
    - "Create package from patient context hides patient selector (plan 38)"
    - "Back button returns to previous page not hardcoded /treatments (plan 38)"
    - "OSDI QR self-fill score captured back via SignalR (plan 39 — code only, human flow pending)"
  gaps_remaining:
    - "Clinical.Unit.Tests cannot compile: 5 SubmitOsdiQuestionnaire tests missing IOsdiNotificationService mock"
  regressions:
    - "Plan 39 changed SubmitOsdiQuestionnaireHandler.Handle() signature but did not update Clinical.Unit.Tests — 5 test methods fail CS7036"
gaps:
  - truth: "Unit test suite compiles and all tests pass"
    status: failed
    reason: "Plan 39 added IOsdiNotificationService as 4th parameter to SubmitOsdiQuestionnaireHandler.Handle() but did not update the 5 test methods in Clinical.Unit.Tests. All 5 fail with CS7036 (missing required argument). Production code builds successfully. CLAUDE.md mandates 80% test coverage — broken test compilation violates this."
    artifacts:
      - path: "backend/tests/Clinical.Unit.Tests/Features/SubmitOsdiQuestionnaireHandlerTests.cs"
        issue: "5 test methods call Handle() with 4 args (command, osdiRepo, visitRepo, unitOfWork, ct) but handler now requires 6 args — IOsdiNotificationService must be added as 4th arg, CancellationToken as 6th"
      - path: "backend/src/Modules/Clinical/Clinical.Application/Features/SubmitOsdiQuestionnaire.cs"
        issue: "Handler signature changed by plan 39: IOsdiNotificationService osdiNotificationService added before IUnitOfWork. Tests were not updated."
    missing:
      - "Add IOsdiNotificationService mock field: private readonly IOsdiNotificationService _osdiNotificationService = Substitute.For<IOsdiNotificationService>();"
      - "Pass _osdiNotificationService as 4th argument in all 5 Handle() calls in SubmitOsdiQuestionnaireHandlerTests.cs"
      - "Verify fix: dotnet build tests/Clinical.Unit.Tests/ && dotnet test tests/Clinical.Unit.Tests/"
human_verification:
  - test: "OSDI QR self-fill score capture end-to-end on two browser sessions"
    expected: "Generate QR in Record Session dialog. Open QR URL in second browser tab or mobile. Complete 12-question OSDI and submit. Score appears automatically in the clinician's session form within seconds — no manual refresh."
    why_human: "Requires two concurrent browser sessions. SignalR pipeline is fully wired but live round-trip not observed."
  - test: "Manager PIN rejection with wrong PIN"
    expected: "In /treatments/approvals, enter wrong PIN on approval — rejected. Enter correct PIN (default 123456 for admin) — approved. Requires migration AddManagerPinToUser applied."
    why_human: "BCrypt implementation verified in code. Live behavior with seeded PIN not observed."
---

# Phase 9: Treatment Protocols Verification Report (Re-verification)

**Phase Goal:** Doctors can create and manage IPL/LLLT/lid care treatment packages with session tracking, OSDI monitoring per session, and configurable business rules
**Verified:** 2026-03-21T12:00:00Z
**Status:** gaps_found
**Re-verification:** Yes — after closure of UAT gaps via plans 36-39

---

## Summary of Re-verification

The previous verification (2026-03-19) had status `human_needed` with 11/11 automated truths verified and 7 items awaiting human testing. Plans 36-39 were executed to close those human items. This re-verification confirms 9 of 9 planned gaps were addressed in code, but plan 39 introduced a regression: the `Clinical.Unit.Tests` project no longer compiles because the `SubmitOsdiQuestionnaire` handler signature was extended without updating the corresponding unit tests.

---

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|---------|
| 1 | Doctor can create IPL, LLLT, or LidCare treatment packages with 1-6 sessions and flexible pricing | VERIFIED | `TreatmentPackage.Create()` enforces 1-6; PerSession/PerPackage pricing wired to POST /api/treatments/packages |
| 2 | System tracks sessions completed and remaining per treatment course | VERIFIED | `SessionsCompleted`/`SessionsRemaining` computed properties; progress bars in list and detail views |
| 3 | System records OSDI score at each treatment session | VERIFIED | `RecordTreatmentSessionCommand` stores `OsdiScore`/`OsdiSeverity`; `SessionOsdiCapture.tsx` imports full `OsdiQuestionnaire`; QR token flow uses DB-backed Clinical module |
| 4 | System auto-marks treatment course as Completed when all sessions done | VERIFIED | `TreatmentPackage.RecordSession()` auto-transitions on `IsComplete`; 108 Treatment unit tests pass including this path; UAT test 10 passed |
| 5 | System enforces minimum interval between sessions | VERIFIED | Interval warning now shows proactively when Record Session dialog opens (plan 38); `TreatmentSessionForm.tsx` lines 292-299 compute client-side |
| 6 | Patient can have multiple active treatment courses simultaneously | VERIFIED | No blocking constraint; `PatientTreatmentsTab.tsx` shows all packages grouped by status |
| 7 | Doctor can modify treatment protocol mid-course | VERIFIED | `Modify()` creates `ProtocolVersion` snapshots; version history now shows translated field-by-field diff (plan 38); `VersionHistoryDialog.tsx` `parseAndTranslateChanges` function |
| 8 | Doctor can switch patient from one treatment type to another mid-course | VERIFIED | `MarkAsSwitched()` + `SwitchTreatmentDialog.tsx` wired; UAT test 13 passed |
| 9 | Manager can process treatment cancellation with configurable refund deduction | VERIFIED | Real PIN verification via BCrypt implemented (plan 37); Paused/PendingCancellation packages visible in list and approvals (plan 36) |
| 10 | Only users with Doctor role can create or modify treatment protocols | VERIFIED | All write endpoints use `RequirePermissions(Permissions.Treatment.Create/Update/Manage)` |
| 11 | Unit test suite compiles and all tests pass | FAILED | `Clinical.Unit.Tests` fails to compile — 5 test methods in `SubmitOsdiQuestionnaireHandlerTests.cs` call `Handle()` with old argument count; `IOsdiNotificationService` was added to handler signature by plan 39 but tests were not updated. `Treatment.Unit.Tests` unaffected: 108/108 pass. |

**Score:** 10/11 truths verified

---

## Gap Closure Verification (Plans 36-39)

### Plan 36: Paused Badge + Repository Query

| Check | Result |
|-------|--------|
| `GetActivePackagesAsync()` includes `PackageStatus.Paused` | VERIFIED — repository line 57 |
| `GetActivePackagesAsync()` includes `PackageStatus.PendingCancellation` | VERIFIED — repository line 58 |
| `TreatmentPackageDetail.tsx` Paused badge uses yellow styling | VERIFIED — `STATUS_STYLES` line 32: `border-yellow-500 text-yellow-700` |
| Badge renders with `STATUS_STYLES[pkg.status]?.className` | VERIFIED — line 249 |

### Plan 37: Real Manager PIN Verification

| Check | Result |
|-------|--------|
| `User.ManagerPinHash` nullable property added | VERIFIED — `User.cs` line 18 |
| `User.VerifyManagerPin()` uses BCrypt | VERIFIED — method in `User.cs` |
| `VerifyManagerPinHandler` injects `IUserRepository` | VERIFIED — `VerifyManagerPin.cs` line 15 |
| `VerifyManagerPinHandler` calls `userRepository.GetByIdAsync` | VERIFIED — line 22 |
| Migration `20260321091619_AddManagerPinToUser` created | VERIFIED — file exists in `Auth.Infrastructure/Migrations/` |

### Plan 38: Frontend UX Fixes (4 items)

| Check | Result |
|-------|--------|
| `TreatmentSessionForm.tsx` has `lastSessionDate` and `minIntervalDays` props | VERIFIED — lines 170-171 |
| Client-side interval warning computed in `useEffect` on dialog open | VERIFIED — lines 292-299 |
| `TreatmentPackageDetail.tsx` passes interval props to form | VERIFIED |
| `ConsumableSelector.tsx` has `getDisplayName` with `i18n.language` check | VERIFIED — lines 38-39 |
| `VersionHistoryDialog.tsx` has `parseAndTranslateChanges` function | VERIFIED — line 40 |
| Translation keys `history.fields.totalSessions` etc. in both locales | VERIFIED — `en/treatment.json` line 222; `vi/treatment.json` line 222 |
| `TreatmentPackageForm.tsx` hides patient selector when `presetPatientId` | VERIFIED — line 317: `{!presetPatientId ? (...)` |
| `TreatmentPackageDetail.tsx` back button uses `window.history.back()` | VERIFIED — line 173 |

### Plan 39: OSDI QR SignalR Score Capture

| Check | Result |
|-------|--------|
| `OsdiHub.JoinToken()` added | VERIFIED — `OsdiHub.cs` line 51 |
| `OsdiHub.LeaveToken()` added | VERIFIED — `OsdiHub.cs` line 61 |
| `IOsdiNotificationService.NotifyTokenSubmittedAsync()` interface method | VERIFIED — `IOsdiNotificationService.cs` line 23 |
| `OsdiNotificationService.NotifyTokenSubmittedAsync()` sends to token group | VERIFIED — line 40; sends to `osdi-token-{token}` |
| `SubmitOsdiQuestionnaireHandler` calls `NotifyTokenSubmittedAsync` after save | VERIFIED — line 70-71 |
| `useOsdiTokenHub` hook exported from `use-osdi-hub.ts` | VERIFIED — line 146 |
| `SessionOsdiCapture.tsx` uses `useOsdiTokenHub` with `registeredToken` | VERIFIED — line 10 (import), line 47 (usage) |
| `Clinical.Unit.Tests` updated with new `IOsdiNotificationService` mock | FAILED — tests not updated; project fails to compile |

---

### Required Artifacts

| Artifact | Purpose | Status | Details |
|----------|---------|--------|---------|
| `backend/src/Modules/Treatment/Treatment.Domain/Entities/TreatmentPackage.cs` | Aggregate root with business rules | VERIFIED | All lifecycle methods implemented |
| `backend/src/Modules/Treatment/Treatment.Infrastructure/Repositories/TreatmentPackageRepository.cs` | Broadened query for active treatments | VERIFIED | Includes Active, Paused, PendingCancellation |
| `backend/src/Modules/Auth/Auth.Domain/Entities/User.cs` | User entity with PIN hash | VERIFIED | `ManagerPinHash` + `VerifyManagerPin()` |
| `backend/src/Modules/Auth/Auth.Application/Features/VerifyManagerPin.cs` | Real PIN verification handler | VERIFIED | Queries DB, BCrypt comparison |
| `backend/src/Modules/Auth/Auth.Infrastructure/Migrations/20260321091619_AddManagerPinToUser.cs` | DB migration for PIN field | VERIFIED | Created and committed |
| `backend/src/Modules/Clinical/Clinical.Infrastructure/Hubs/OsdiHub.cs` | Token-scoped SignalR groups | VERIFIED | `JoinToken`/`LeaveToken` added |
| `backend/src/Modules/Clinical/Clinical.Infrastructure/Services/OsdiNotificationService.cs` | Token notification service | VERIFIED | `NotifyTokenSubmittedAsync` implemented |
| `backend/src/Modules/Clinical/Clinical.Application/Features/SubmitOsdiQuestionnaire.cs` | OSDI submit with notification | VERIFIED | Calls `NotifyTokenSubmittedAsync` after save |
| `backend/tests/Clinical.Unit.Tests/Features/SubmitOsdiQuestionnaireHandlerTests.cs` | Unit tests for OSDI submit | BROKEN | 5 test methods have wrong `Handle()` signature — CS7036 compile error |
| `frontend/src/features/clinical/hooks/use-osdi-hub.ts` | Token SignalR hook | VERIFIED | `useOsdiTokenHub` exported at line 146 |
| `frontend/src/features/treatment/components/SessionOsdiCapture.tsx` | Score capture listener | VERIFIED | `useOsdiTokenHub` wired with `registeredToken` |
| `frontend/src/features/treatment/components/TreatmentSessionForm.tsx` | Proactive interval warning | VERIFIED | `lastSessionDate`/`minIntervalDays` props + client-side compute |
| `frontend/src/features/treatment/components/ConsumableSelector.tsx` | Language-aware consumable names | VERIFIED | `getDisplayName` helper with `i18n.language` |
| `frontend/src/features/treatment/components/VersionHistoryDialog.tsx` | Translated version diff | VERIFIED | `parseAndTranslateChanges` with field translation |
| `frontend/src/features/treatment/components/TreatmentPackageDetail.tsx` | Yellow Paused badge + interval props | VERIFIED | `STATUS_STYLES` with yellow; interval props passed to form |
| `frontend/src/features/treatment/components/TreatmentPackageForm.tsx` | Hidden patient selector when preset | VERIFIED | Conditional render on `presetPatientId` |

---

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `TreatmentPackageRepository.GetActivePackagesAsync` | `TreatmentsPage` | GET /api/treatments/packages | WIRED | Now returns Active, Paused, PendingCancellation |
| `TreatmentPackageDetail.tsx` | `TreatmentSessionForm` interval props | `lastSessionDate` + `minIntervalDays` | WIRED | Props passed; warning computed client-side on open |
| `ConsumableSelector` | i18n language | `i18n.language === "vi"` check | WIRED | `getDisplayName` helper selects correct name |
| `SubmitOsdiQuestionnaireHandler` | `OsdiNotificationService.NotifyTokenSubmittedAsync` | Direct injection + call | WIRED | Line 70-71 calls after save |
| `OsdiHub` | Token SignalR groups | `$"osdi-token-{token}"` group name | WIRED | `JoinToken`/`LeaveToken` methods |
| `SessionOsdiCapture` | `useOsdiTokenHub` | Import + hook call | WIRED | Line 47: `useOsdiTokenHub(registeredToken, handleScoreReceived)` |
| `VerifyManagerPinHandler` | `User.VerifyManagerPin()` | `IUserRepository.GetByIdAsync` → `BCrypt.Verify` | WIRED | Lines 22, 27 |
| `Clinical.Unit.Tests` | `SubmitOsdiQuestionnaireHandler.Handle()` | Direct static method call | BROKEN | 5 test call sites have wrong argument count — CS7036 |

---

### Requirements Coverage

| Requirement | Description | Status | Evidence |
|-------------|-------------|--------|----------|
| TRT-01 | Doctor can create IPL/LLLT/lid care packages with 1-6 sessions | SATISFIED | UAT test 4 passed |
| TRT-02 | System tracks sessions completed and remaining | SATISFIED | UAT test 5 passed |
| TRT-03 | System records OSDI score at each treatment session | SATISFIED | UAT test 7 passed; QR flow wired (plan 39) |
| TRT-04 | System auto-marks course Completed when all sessions done | SATISFIED | UAT test 10 passed |
| TRT-05 | System enforces minimum interval between sessions | SATISFIED | UAT gap closed — proactive warning implemented (plan 38) |
| TRT-06 | Patient can have multiple active courses simultaneously | SATISFIED | No blocking constraint; `PatientTreatmentsTab` shows all |
| TRT-07 | Doctor can modify protocol mid-course | SATISFIED | UAT gap closed — translated version diff (plan 38) |
| TRT-08 | Doctor can switch treatment type mid-course | SATISFIED | UAT test 13 passed |
| TRT-09 | Manager can process cancellation with 10-20% deduction | SATISFIED | PIN verification real (plan 37); queue visibility fixed (plan 36) |
| TRT-10 | Only Doctor role can create/modify protocols | SATISFIED | `RequirePermissions` on all write endpoints |
| TRT-11 | Consumables recorded per session linked to pharmacy | SATISFIED | UAT test 17 passed |

All 11 TRT requirements are satisfied. The one blocking gap is a test infrastructure regression, not a business rule violation.

---

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| `backend/tests/Clinical.Unit.Tests/Features/SubmitOsdiQuestionnaireHandlerTests.cs` | 40, 65, 83, 102, 127 | `Handle()` called with wrong argument count — CS7036 compile error | Blocker | `Clinical.Unit.Tests` project cannot compile. 181 previously passing tests cannot run. Violates CLAUDE.md TDD/80% coverage policy. |
| `frontend/src/features/treatment/components/TreatmentSessionCard.tsx` | 92, 140, 229 | Uncommitted working-tree changes (localization improvements) | Warning | Functional improvements for zone/step/status localization not committed — will be lost if working tree discarded. |

**Pre-existing (not caused by phase 09):**
- `frontend/src/features/patient/api/patient-api.ts` — TS errors dating from phase 02
- `backend/tests/Optical.Unit.Tests/Features/WarrantyHandlerTests.cs` — 5 tests with wrong argument count dating from phase 08

---

### Human Verification Required

#### 1. OSDI QR self-fill score capture end-to-end

**Test:** Open Record Session dialog on an active package. Switch to "Self-Fill (QR)" tab. Click to generate QR. In a second browser window or mobile device, open the QR URL. Complete all 12 OSDI questions and submit. Observe the clinician's session form.
**Expected:** Within seconds, the OSDI score populates automatically in the session form without manual refresh. A success indicator may appear near or replacing the QR code area.
**Why human:** Requires two concurrent browser sessions (clinician + patient). The SignalR pipeline is fully wired in code — `JoinToken` group join, `OsdiTokenSubmitted` event, `handleScoreReceived` callback — but the live round-trip was not tested.

#### 2. Manager PIN rejection with wrong PIN (after migration)

**Test:** Ensure migration `AddManagerPinToUser` is applied (`dotnet ef database update --context AuthDbContext` in `backend/src/Bootstrapper/`). Navigate to `/treatments/approvals`. Approve a pending cancellation with PIN "000000". Observe error. Then approve with PIN "123456". Observe success.
**Expected:** Wrong PIN returns error and does not approve. Correct PIN completes the approval. Refund deduction amount is editable (10-20% range).
**Why human:** BCrypt implementation verified in code. Live behavior with seeded PIN and real DB record not observed during this verification.

---

### Gaps Summary

One gap blocks this phase: **plan 39 regression in `Clinical.Unit.Tests`**. The fix is a 2-minute change — add `IOsdiNotificationService` mock to the test class and pass it in all 5 `Handle()` call sites. Once fixed and all Clinical tests compile and pass, the only remaining items are 2 human verification tests that confirm live end-to-end behavior.

Additionally, uncommitted working-tree changes in `TreatmentSessionCard.tsx` (localization for IPL treatment zones, LidCare procedure steps, and session status badge) should be committed to preserve the improvements.

---

## Build Status Summary

| Project | Status | Notes |
|---------|--------|-------|
| Production code (`src/Bootstrapper`) | BUILD SUCCEEDED | All modules compile cleanly |
| `Treatment.Unit.Tests` | PASS — 108/108 | Unaffected by plan 39 |
| `Clinical.Unit.Tests` | COMPILE ERROR | 5 tests fail CS7036 — regression from plan 39 |
| `Optical.Unit.Tests` | COMPILE ERROR | 5 tests fail CS7036 — pre-existing from phase 08 |
| Frontend TypeScript (treatment files) | CLEAN | No errors in treatment module files |
| Frontend TypeScript (other modules) | Pre-existing errors | patient-api, admin-api, root — unrelated to phase 09 |

---

_Verified: 2026-03-21T12:00:00Z_
_Verifier: Claude (gsd-verifier)_
_Re-verification: Yes — after gap closure plans 09-36 through 09-39_

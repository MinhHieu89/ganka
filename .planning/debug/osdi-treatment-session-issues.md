---
status: diagnosed
trigger: "Investigate OSDI patient self-fill link error and direct input missing questionnaire"
created: 2026-03-19T00:00:00Z
updated: 2026-03-19T00:00:00Z
---

## Current Focus

hypothesis: Two separate root causes identified for both issues
test: Code trace complete
expecting: N/A - diagnosis only
next_action: Report findings

## Symptoms

expected: (1) Patient self-fill link should load OSDI questionnaire. (2) Direct input should show 12 OSDI questions with auto-calculated score.
actual: (1) Link shows "Liên kết này không hợp lệ" error. (2) Direct input only shows single "OSDI Score (0-100)" number input.
errors: "Liên kết này không hợp lệ" (This link is invalid)
reproduction: (1) Open treatment package -> Record Session -> "Bệnh nhân tự điền" tab -> generate QR -> open link. (2) Same dialog -> "Nhập trực tiếp" tab.
started: Since treatment session recording was implemented

## Eliminated

(none)

## Evidence

- timestamp: 2026-03-19
  checked: SessionOsdiCapture.tsx (treatment module) - QR generation flow
  found: Uses `useRegisterOsdiToken()` which calls `POST /api/treatments/osdi-tokens`. Stores token in in-memory `IOsdiTokenStore` (Treatment module). URL generated as `/osdi/{crypto.randomUUID()}`.
  implication: Token is stored in Treatment module's in-memory store, NOT in Clinical module's database.

- timestamp: 2026-03-19
  checked: PublicOsdiEndpoints.cs + GetOsdiByToken.cs (Clinical module) - public page backend
  found: `GET /api/public/osdi/{token}` calls `GetOsdiByTokenQuery` which uses `IOsdiSubmissionRepository.GetByTokenAsync()` to look up token in the **database** (OsdiSubmission entity). This is a completely different storage mechanism from the in-memory `IOsdiTokenStore`.
  implication: Token registered via Treatment module's in-memory store is NEVER visible to Clinical module's database-backed repository. The lookup always fails -> 404 -> "INVALID" error.

- timestamp: 2026-03-19
  checked: GenerateOsdiLink.cs (Clinical module) - the WORKING link generation
  found: The Clinical module's `GenerateOsdiLinkHandler` creates an `OsdiSubmission.CreateWithToken()` record in the **database** and returns the URL. This is used by `OsdiSection.tsx` (visit detail page) and works correctly.
  implication: There are TWO separate OSDI link generation flows - one in Clinical (database-backed, works) and one in Treatment (in-memory, broken). The Treatment flow never creates the database record that the public endpoint needs.

- timestamp: 2026-03-19
  checked: SessionOsdiCapture.tsx - direct input tab
  found: The "Nhập trực tiếp" (inline) tab renders a single `<Input type="number">` for OSDI Score (0-100). It does NOT use the `OsdiQuestionnaire` component that exists in `frontend/src/features/clinical/components/OsdiQuestionnaire.tsx`.
  implication: The full questionnaire component with 12 questions exists and works (used in OsdiSection.tsx for visit detail page). SessionOsdiCapture simply wasn't built to use it.

- timestamp: 2026-03-19
  checked: OsdiQuestionnaire.tsx (Clinical module)
  found: Complete 12-question OSDI questionnaire component with auto-score calculation, severity display, radio buttons, N/A options for questions 6-12, bilingual support. Already exported and reusable.
  implication: No new component needed - just need to import and use OsdiQuestionnaire in SessionOsdiCapture.tsx

## Resolution

root_cause: |
  **Issue 1 (Self-fill link error):** Architecture mismatch between two modules.
  - The Treatment module's `SessionOsdiCapture.tsx` generates a token via `POST /api/treatments/osdi-tokens`, which stores it in `IOsdiTokenStore` (in-memory, Treatment module).
  - The public page (`/osdi/$token`) fetches questionnaire data via `GET /api/public/osdi/{token}`, which looks up the token in `IOsdiSubmissionRepository` (database, Clinical module).
  - These are TWO COMPLETELY DIFFERENT storage mechanisms. The token registered in the in-memory store is never written to the database, so the public endpoint always returns 404 -> "INVALID".
  - The Clinical module already has a working flow: `GenerateOsdiLinkHandler` creates an `OsdiSubmission` database record with the token. This is used by `OsdiSection.tsx` on the visit detail page and works correctly.

  **Issue 2 (Direct input missing questionnaire):** Simple implementation gap.
  - `SessionOsdiCapture.tsx` inline tab only renders a single number input for manual score entry.
  - The full 12-question `OsdiQuestionnaire` component already exists in `frontend/src/features/clinical/components/OsdiQuestionnaire.tsx` but is not imported/used here.

fix: |
  **Issue 1 fix direction:** The Treatment module's `SessionOsdiCapture.tsx` should NOT use the `IOsdiTokenStore` / `POST /api/treatments/osdi-tokens` flow. Instead, it should use the Clinical module's `GenerateOsdiLink` flow (which creates a proper database-backed `OsdiSubmission` record). This requires:
  - The treatment session recording dialog needs a `visitId` to call the Clinical module's generate link endpoint, OR
  - The Treatment module's `RegisterOsdiTokenHandler` needs to be modified to ALSO create an `OsdiSubmission` record in the database (cross-module call), OR
  - Remove `IOsdiTokenStore` entirely and have the treatment flow call the Clinical module's existing `GenerateOsdiLink` command.

  **Issue 2 fix direction:** Replace the single number input in `SessionOsdiCapture.tsx`'s inline tab with the existing `OsdiQuestionnaire` component from `frontend/src/features/clinical/components/OsdiQuestionnaire.tsx`. Wire its `onSubmit` callback to set the OSDI score on the session form.

verification:
files_changed: []

---
status: diagnosed
trigger: "Investigate why the OSDI QR self-fill score is not captured back into the session form in the treatment module"
created: 2026-03-21T00:00:00Z
updated: 2026-03-21T00:00:00Z
---

## Current Focus

hypothesis: The QR self-fill score capture mechanism is NOT IMPLEMENTED for the treatment session flow. The existing SignalR-based real-time notification system only works for the clinical visit flow (visit-linked OSDI tokens), not for treatment session tokens (which have no VisitId).
test: Traced the full data flow from token creation to submission to notification
expecting: Confirmed -- no mechanism exists to push the score back to the session form
next_action: Implementation needed (not a bug fix -- this is a missing feature)

## Symptoms

expected: After patient submits OSDI via QR self-fill public page, the score should appear in the TreatmentSessionForm (SessionOsdiCapture component)
actual: Score is saved to database but never pushed back to the session form -- the form remains showing the QR code with no score update
errors: None (silent failure -- data is saved but not communicated back)
reproduction: 1) Open Record Session dialog, 2) Switch to "Self-Fill" tab, 3) Generate QR, 4) Submit OSDI on patient device, 5) Score never appears in the form
started: Always been this way -- never implemented

## Eliminated

(none -- root cause found on first hypothesis)

## Evidence

- timestamp: 2026-03-21
  checked: SessionOsdiCapture.tsx (frontend/src/features/treatment/components/SessionOsdiCapture.tsx)
  found: |
    The self-fill tab generates a QR code and registers a token via `useRegisterOsdiToken()`.
    After QR is displayed, there is NO polling, no SignalR subscription, no refetch, no
    useEffect watching for score changes. The component just shows the QR code and link
    statically. There is zero code to receive the score back.
  implication: Frontend has no mechanism to receive the score after patient submission

- timestamp: 2026-03-21
  checked: TreatmentSessionForm.tsx (lines 401-407)
  found: |
    SessionOsdiCapture receives `osdiScore` state and `setOsdiScore` callback, but
    setOsdiScore is only ever called from the inline OsdiQuestionnaire's `handleOsdiSubmit`.
    The self-fill flow never calls `onOsdiScoreChange`.
  implication: Even if data arrived, there's no wiring to update the form state from self-fill

- timestamp: 2026-03-21
  checked: RegisterOsdiToken.cs (backend handler)
  found: |
    Delegates to Clinical module via `CreateOsdiTokenForTreatmentCommand`. The Clinical
    handler creates an `OsdiSubmission` with `VisitId = null` (via `CreateWithTokenForTreatment`).
    Returns token URL and expiry. No subscription or callback mechanism is set up.
  implication: Backend token registration is fire-and-forget with no return channel

- timestamp: 2026-03-21
  checked: SubmitOsdiQuestionnaire.cs (backend submission handler)
  found: |
    When patient submits answers on the public page, the handler:
    1. Looks up submission by token
    2. Calculates OSDI score
    3. Updates OsdiSubmission record with answers/score
    4. If VisitId is present, also updates DryEyeAssessment on the visit
    5. Does NOT fire OsdiSubmittedEvent (the Wolverine cascading event)

    Even if it DID fire OsdiSubmittedEvent, that event takes a VisitId parameter,
    and treatment-session tokens have VisitId = null.
  implication: |
    TWO gaps:
    (a) SubmitOsdiQuestionnaireHandler never publishes OsdiSubmittedEvent for ANY flow
    (b) Even if it did, OsdiSubmittedEvent is visit-scoped, not token-scoped

- timestamp: 2026-03-21
  checked: Existing SignalR infrastructure (OsdiHub, OsdiNotificationService, use-osdi-hub.ts)
  found: |
    A complete SignalR notification system EXISTS for visit-based OSDI:
    - Backend: OsdiHub with JoinVisit/LeaveVisit groups, OsdiNotificationService
    - Frontend: use-osdi-hub.ts hook that joins visit group and invalidates queries
    - BUT: This is only used in VisitDetailPage.tsx (clinical module)
    - The treatment module's SessionOsdiCapture does NOT use useOsdiHub
    - The hub only supports visit groups ("visit-{visitId}"), not token groups
    - Treatment tokens have no VisitId, so they can't join a visit group
  implication: The real-time infrastructure exists but is visit-scoped only

- timestamp: 2026-03-21
  checked: Public OSDI page ($token.tsx)
  found: |
    The public page submits to POST /api/public/osdi/{token} and displays the score
    to the patient. It does NOT notify anyone else. No SignalR client on the public page.
  implication: Public page is a dead-end -- score goes to DB but no notification is sent

## Resolution

root_cause: |
  The OSDI QR self-fill score capture for treatment sessions is NOT IMPLEMENTED.
  There are THREE missing pieces:

  1. **No notification on submission (backend):** SubmitOsdiQuestionnaireHandler saves
     the score to the database but does NOT publish any event (OsdiSubmittedEvent or
     similar) that could trigger a real-time notification.

  2. **No token-scoped SignalR group (backend):** The existing OsdiHub only supports
     visit-scoped groups ("visit-{visitId}"). Treatment session tokens have VisitId=null,
     so there's no group they could join. A new group type like "osdi-token-{token}" is
     needed.

  3. **No SignalR subscription or polling (frontend):** SessionOsdiCapture.tsx displays
     the QR code but has zero code to listen for the score coming back. It needs either:
     (a) A SignalR subscription to a token-scoped group, OR
     (b) A polling mechanism that checks the token's submission status

  The existing visit-based OSDI self-fill flow (in the Clinical module) has a working
  SignalR pipeline (use-osdi-hub.ts -> OsdiHub -> OsdiNotificationService), but even
  THAT flow has a gap: SubmitOsdiQuestionnaireHandler never fires OsdiSubmittedEvent.

fix: Not applied (research-only mode)
verification: N/A
files_changed: []

### Suggested Implementation Approach

**Option A: SignalR (real-time, preferred)**
1. Add token-scoped group to OsdiHub: `JoinToken(string token)` / `LeaveToken(string token)`
2. In SubmitOsdiQuestionnaireHandler, after saving, publish an event that includes the token
3. Add notification handler that sends score to "osdi-token-{token}" group
4. Create `useOsdiTokenHub(token)` hook in frontend
5. In SessionOsdiCapture, after QR generation, subscribe to the token group
6. On "OsdiTokenSubmitted" event, call `onOsdiScoreChange(score)` to update form state

**Option B: Polling (simpler)**
1. Add GET endpoint: `/api/public/osdi/{token}/status` returning { submitted: bool, score: number | null }
2. In SessionOsdiCapture, after QR generation, poll this endpoint every 3-5 seconds
3. When submitted=true, call `onOsdiScoreChange(score)` and stop polling

**Files that need changes:**
- `backend/src/Modules/Clinical/Clinical.Application/Features/SubmitOsdiQuestionnaire.cs` -- fire event after save
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Hubs/OsdiHub.cs` -- add token group support (Option A)
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Services/OsdiNotificationService.cs` -- add token notification (Option A)
- `frontend/src/features/treatment/components/SessionOsdiCapture.tsx` -- subscribe to updates
- `frontend/src/features/clinical/hooks/use-osdi-hub.ts` or new hook -- token-scoped subscription

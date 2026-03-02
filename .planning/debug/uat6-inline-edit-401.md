---
status: diagnosed
trigger: "UAT Test 6 - Patient Inline Edit: 401 error on update, shows 'not found' page, works after refresh"
created: 2026-03-02T12:00:00Z
updated: 2026-03-02T12:00:00Z
---

## Current Focus

hypothesis: Access token expires after 15min because refresh timer is never active post-login navigation; expired token causes 401 on PUT; subsequent query refetch also fails with 401 showing error page
test: Traced full token lifecycle from login through authenticated route usage
expecting: n/a - root causes identified
next_action: report findings

## Symptoms

expected: Edit patient > Save > patient updated successfully (200 OK), stays on profile page
actual: Save returns 401, page shows "Khong tim thay benh nhan / Da xay ra loi" (patient not found / error occurred)
errors: 401 Unauthorized on PUT /api/patients/{id}
reproduction: Login > navigate to patient profile > wait some time > edit > save
started: Likely always present but only manifests when access token has expired (after ~15 minutes)

## Eliminated

- hypothesis: CORS blocks PUT requests
  evidence: Backend CORS config allows AllowAnyMethod(), AllowAnyHeader(), AllowCredentials(). PUT is not blocked.
  timestamp: 2026-03-02

- hypothesis: Antiforgery middleware blocks PUT
  evidence: No global antiforgery middleware in Program.cs. Only photo upload endpoint has DisableAntiforgery().
  timestamp: 2026-03-02

- hypothesis: Auth middleware not attaching header to PUT specifically
  evidence: openapi-fetch authMiddleware modifies Request.headers for ALL methods equally (api-client.ts line 8-14). No method-specific logic.
  timestamp: 2026-03-02

- hypothesis: Body serialization causes deserialization failure (400/500, not 401)
  evidence: Frontend correctly removes patientId from body (line 198-199 of patient-api.ts), sends JSON with Content-Type header. Backend record binding handles remaining fields.
  timestamp: 2026-03-02

## Evidence

- timestamp: 2026-03-02
  checked: JwtService.cs line 36-37 - access token lifetime
  found: Default lifetime is 15 minutes. ClockSkew is TimeSpan.Zero (Program.cs line 121), so tokens expire EXACTLY at their expiration time.
  implication: Any API call made after 15 minutes of token issuance will get 401.

- timestamp: 2026-03-02
  checked: useAuth.ts scheduleRefresh function (lines 23-57)
  found: scheduleRefresh sets a setTimeout at 80% of token lifetime (~12 min) to call refreshMutation.mutateAsync(). The timer is stored in refreshTimerRef (a useRef). This ref is LOCAL to each useAuth hook instance.
  implication: Each component that calls useAuth() gets its own independent refreshTimerRef.

- timestamp: 2026-03-02
  checked: LoginForm.tsx line 47 and useAuth.ts line 96
  found: LoginForm calls login() which internally calls scheduleRefresh(). The timer is set on LoginForm's useAuth instance's refreshTimerRef.
  implication: Timer is scheduled, but tied to LoginForm's component lifecycle.

- timestamp: 2026-03-02
  checked: LoginForm.tsx line 48
  found: After login, LoginForm navigates to /dashboard. This unmounts LoginForm (it's on the /login route).
  implication: When LoginForm unmounts, its useAuth cleanup effect (line 60-66) fires and clears the timeout. The refresh timer is DESTROYED.

- timestamp: 2026-03-02
  checked: AppShell.tsx line 12
  found: AppShell (the authenticated layout) calls useAuth() but only destructures { logout }. It never calls scheduleRefresh or login.
  implication: AppShell's useAuth instance has a fresh refreshTimerRef that is never used. NO refresh timer is active in the authenticated app.

- timestamp: 2026-03-02
  checked: _authenticated.tsx beforeLoad (lines 7-39)
  found: On page load/refresh, beforeLoad calls silentRefresh() which gets a new access token via HTTP-only cookie. It calls setAuth() to store the token. But it does NOT call scheduleRefresh().
  implication: Even after page refresh, no refresh timer is scheduled. The token will expire in 15 minutes.

- timestamp: 2026-03-02
  checked: PatientProfilePage.tsx line 59
  found: Error state renders when `patientQuery.isError || !patient`. This is the "not found" page the user sees.
  implication: The patient query entering error state triggers the "not found" page.

- timestamp: 2026-03-02
  checked: __root.tsx lines 10-17 - QueryClient default options
  found: queries have `retry: 1` and `staleTime: 5 minutes`. refetchOnWindowFocus defaults to true in TanStack Query.
  implication: When user interacts with page after mutation failure, window focus events trigger refetch of patientQuery. With expired token, refetch also fails with 401, putting query into error state.

- timestamp: 2026-03-02
  checked: authStore.ts - Zustand store (in-memory only)
  found: Auth state (accessToken, user) is stored in Zustand with no persistence. On page refresh, state is lost and must be restored via silentRefresh.
  implication: Page refresh triggers silentRefresh in _authenticated.tsx beforeLoad, getting a FRESH access token. This is why "it works after refresh" - the user gets a new valid 15-minute token.

## Resolution

root_cause: |
  PRIMARY ROOT CAUSE: Access token refresh timer is never active in the authenticated application.

  The refresh scheduling mechanism in useAuth.ts has a fundamental architectural flaw:
  1. LoginForm calls useAuth().login() which schedules a refresh timer on LoginForm's useRef
  2. LoginForm navigates away and unmounts, destroying the timer (cleanup effect fires)
  3. AppShell mounts with its own useAuth() instance but never calls scheduleRefresh()
  4. The _authenticated route's beforeLoad calls silentRefresh() but also never calls scheduleRefresh()
  5. Result: After 15 minutes, the access token expires and all API calls return 401

  SECONDARY ISSUE: No 401 response interceptor in the API client.
  When any API call returns 401, the frontend does not attempt to silently refresh the token.
  The api-client.ts middleware only has onRequest (to attach the token), but no onResponse
  to handle 401s by refreshing and retrying.

  TERTIARY ISSUE: PatientProfilePage shows misleading "patient not found" error.
  Line 59 of PatientProfilePage.tsx treats ALL query errors as "not found":
  `if (patientQuery.isError || !patient)` shows the notFound message.
  A 401 auth error is displayed as "patient not found" instead of a session expired message.

fix: |
  CONCEPTUAL FIX 1 (Token Refresh Timer):
  Move the refresh timer management out of the useAuth hook and into a persistent location.
  Options:
  a) Add an onResponse middleware to api-client.ts that detects 401 responses, calls silentRefresh(),
     updates the auth store with the new token, and retries the failed request. This is the standard
     "axios interceptor" pattern adapted for openapi-fetch.
  b) Move scheduleRefresh logic into the _authenticated route's beforeLoad (after silentRefresh)
     or into a top-level component that persists across the authenticated app lifecycle.
  c) Use a Zustand middleware/subscription that automatically schedules refresh when a new
     accessToken is set in the store.

  CONCEPTUAL FIX 2 (401 Response Interceptor):
  Add an onResponse handler to the openapi-fetch middleware that:
  - Detects 401 status
  - Calls silentRefresh() to get a new token
  - Updates the auth store
  - Retries the original request with the new token
  - If refresh also fails, clears auth and redirects to login

  CONCEPTUAL FIX 3 (Error Display):
  PatientProfilePage should differentiate between "patient not found" (404) and
  "authentication error" (401). Show appropriate messages for each case rather
  than a generic "not found" for all errors.

verification: pending
files_changed: []

---
status: diagnosed
trigger: "No global dispensing history view - only available per-patient in profile's Prescriptions tab"
created: 2026-03-11T00:00:00Z
updated: 2026-03-11T00:00:00Z
---

## Current Focus

hypothesis: The backend endpoint and frontend API client both exist and support global dispensing history (patientId is optional). The gap is purely a missing frontend route/page -- no dedicated dispensing history page exists in the pharmacy section.
test: Verified by reading all relevant files
expecting: n/a - confirmed
next_action: Build a new frontend route and page component

## Symptoms

expected: A dispensing history page accessible from the pharmacy section sidebar/navigation showing all dispensing records across all patients
actual: Dispensing history is only viewable per-patient inside PatientPrescriptionsTab component
errors: none - feature gap, not a bug
reproduction: Navigate to /pharmacy -- no dispensing history link or route exists
started: Always been this way - feature was never built

## Eliminated

- hypothesis: Backend endpoint missing for global dispensing history
  evidence: GET /api/pharmacy/dispensing/history exists in DispensingApiEndpoints.cs (line 59-65), patientId is optional in GetDispensingHistoryParams
  timestamp: 2026-03-11

- hypothesis: Frontend API client missing
  evidence: getDispensingHistory() in pharmacy-api.ts (line 491-508) and useDispensingHistory() hook in pharmacy-queries.ts (line 148-153) both exist and support optional patientId
  timestamp: 2026-03-11

## Evidence

- timestamp: 2026-03-11
  checked: DispensingApiEndpoints.cs
  found: GET /api/pharmacy/dispensing/history endpoint exists with optional patientId parameter. When patientId is null, returns ALL dispensing records (global view). Paginated with page/pageSize.
  implication: Backend fully supports global dispensing history query

- timestamp: 2026-03-11
  checked: DispensingRepository.cs GetHistoryAsync method
  found: Query filters by patientId only when patientId.HasValue is true (line 43-44). Without patientId, returns all records ordered by DispensedAt descending with pagination.
  implication: Backend repository correctly handles the global (no filter) case

- timestamp: 2026-03-11
  checked: pharmacy-api.ts getDispensingHistory function
  found: Frontend API client exists (line 491-508), accepts optional patientId, returns DispensingHistoryResult with items and totalCount
  implication: Frontend API layer is ready to use

- timestamp: 2026-03-11
  checked: pharmacy-queries.ts useDispensingHistory hook
  found: React Query hook exists (line 148-153) with optional patientId parameter
  implication: Data fetching hook is ready to use

- timestamp: 2026-03-11
  checked: PatientPrescriptionsTab.tsx
  found: This is the ONLY consumer of useDispensingHistory, always called with a specific patientId (line 23). Shows history in a patient-context tab only.
  implication: No global usage exists anywhere

- timestamp: 2026-03-11
  checked: frontend/src/app/routes/_authenticated/pharmacy/ directory
  found: Routes exist for index (inventory), drug-catalog, otc-sales, queue, stock-import, suppliers. NO dispensing history route.
  implication: The route/page was never created

- timestamp: 2026-03-11
  checked: pharmacy/index.tsx navigation links
  found: Header links go to drug-catalog, suppliers, stock-import. No link to dispensing history.
  implication: Even if a route existed, there is no navigation entry for it

## Resolution

root_cause: The frontend is missing a dedicated dispensing history page and route. The backend endpoint (GET /api/pharmacy/dispensing/history) and frontend API client (getDispensingHistory + useDispensingHistory hook) both fully support global dispensing history with optional patient filtering. The only consumer is PatientPrescriptionsTab which always passes a patientId, restricting the view to per-patient only. No route file exists at /pharmacy/dispensing-history and no navigation link points to such a page.
fix:
verification:
files_changed: []

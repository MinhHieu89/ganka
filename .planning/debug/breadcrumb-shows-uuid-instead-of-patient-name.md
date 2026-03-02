---
status: diagnosed
trigger: "the breadcrum should show name of patient instead of id string"
created: 2026-03-02T00:00:00Z
updated: 2026-03-02T00:00:00Z
---

## Current Focus

hypothesis: SiteHeader builds breadcrumbs purely from URL path segments; UUID segments are replaced with a static "Detail" label instead of the actual patient name
test: Read SiteHeader.tsx breadcrumb construction logic
expecting: confirmed - no dynamic name lookup happens
next_action: fix confirmed, return diagnosis

## Symptoms

expected: Breadcrumb on patient profile page shows patient's full name (e.g. "Patients > Nguyen Van A")
actual: Breadcrumb shows either the raw UUID or the static "Detail" label (t("sidebar.detail"))
errors: none - purely a display/logic issue
reproduction: navigate to /patients/{uuid} - breadcrumb last segment is "Detail" not the patient name
started: always

## Eliminated

- hypothesis: Router does not pass patientId to the page
  evidence: $patientId.tsx correctly extracts patientId via Route.useParams() and passes to PatientProfilePage
  timestamp: 2026-03-02T00:00:00Z

- hypothesis: Patient name is unavailable in the component tree
  evidence: PatientProfilePage already fetches patient via usePatientById(patientId) and has patient.fullName; recentPatientsStore also persists fullName indexed by id
  timestamp: 2026-03-02T00:00:00Z

## Evidence

- timestamp: 2026-03-02T00:00:00Z
  checked: SiteHeader.tsx lines 70-87
  found: |
    UUID segments (matching /^[0-9a-f]{8}-...-[0-9a-f]{12}$/i) are detected but labeled
    as t("sidebar.detail") ?? "Detail" - a static string with no patient name lookup.
    The segmentToI18nKey map has no entry for any UUID key either.
  implication: The breadcrumb never resolves UUID to a name; it always shows "Detail"

- timestamp: 2026-03-02T00:00:00Z
  checked: recentPatientsStore.ts
  found: |
    Zustand store with persist middleware stores up to 10 recent patients:
    { id, fullName, patientCode, phone }. PatientProfilePage populates it on load.
  implication: Patient name IS available in a persistent store keyed by id.
    SiteHeader can look up the name from this store without an extra API call.

- timestamp: 2026-03-02T00:00:00Z
  checked: TanStack Query cache via usePatientById
  found: |
    The query key is ["patients", patientId]. Data is cached for 5 minutes (staleTime).
    SiteHeader could call usePatientById(patientId) to get the name, but that would
    trigger an extra fetch if not already cached.
  implication: Best approach is recentPatientsStore lookup (instant, no fetch) with
    fallback to the query cache.

## Resolution

root_cause: |
  SiteHeader.tsx constructs breadcrumbs by splitting the URL path into segments and
  translating known segments via segmentToI18nKey. When it encounters a UUID segment
  (line 78: `uuidRegex.test(segment)`), it falls back to a static
  t("sidebar.detail") label instead of resolving the UUID to an entity name.
  There is no mechanism to look up a patient's name from either the Zustand store
  or the query cache at the breadcrumb level.

fix: |
  In SiteHeader.tsx, import useRecentPatientsStore. Inside the breadcrumbs mapping,
  when a UUID segment is detected AND the previous segment is "patients", look up
  the patient name from recentPatientsStore.recent by id. Use that name as the label.
  Fall back to t("sidebar.detail") if not found (e.g. on fresh page load before store
  populates - patient fetch hasn't completed yet).

verification: not applied (diagnose-only mode)
files_changed: []

---
status: diagnosed
trigger: "Investigate pause/resume issues: Paused badge not yellow + paused packages disappear from /treatments list"
created: 2026-03-21T00:00:00Z
updated: 2026-03-21T00:00:00Z
---

## Current Focus

hypothesis: Both issues confirmed with root causes identified
test: N/A - research complete
expecting: N/A
next_action: Hand off for code changes

## Symptoms

expected: (1) Paused badge should display with yellow styling. (2) Paused packages should remain visible on /treatments page.
actual: (1) Paused badge renders gray (secondary variant). (2) Paused packages vanish from list after pausing.
errors: No runtime errors - both are logic/design bugs.
reproduction: Pause any active treatment package, observe badge color on detail page and check /treatments list.
started: Since pause feature was implemented.

## Eliminated

(none - both initial hypotheses confirmed on first investigation)

## Evidence

- timestamp: 2026-03-21
  checked: TreatmentPackageDetail.tsx STATUS_VARIANT mapping (line 30-40)
  found: `Paused: "secondary"` maps to shadcn Badge "secondary" variant which renders `bg-secondary text-secondary-foreground` - a gray color, not yellow.
  implication: Issue 1 confirmed. The variant choice is wrong for a "Paused" semantic.

- timestamp: 2026-03-21
  checked: TreatmentsPage.tsx STATUS_STYLES mapping (line 37-44)
  found: TreatmentsPage uses a DIFFERENT approach - custom className `"border-yellow-500 text-yellow-700 dark:text-yellow-400"` with variant="outline". This IS yellow. So the LIST page actually has correct yellow styling for Paused, but the DETAIL page (TreatmentPackageDetail.tsx) does not.
  implication: Issue 1 is isolated to TreatmentPackageDetail.tsx only. The two files use inconsistent badge styling strategies.

- timestamp: 2026-03-21
  checked: Badge component (ui/badge.tsx)
  found: "secondary" variant = `border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80`. This is theme-dependent but typically renders gray. There is no "warning" or "yellow" variant available.
  implication: To get yellow on the detail page, either add a custom className (like TreatmentsPage does) or add a warning variant to the Badge component.

- timestamp: 2026-03-21
  checked: TreatmentsPage.tsx data source (line 58)
  found: `useActiveTreatments()` hook is the sole data source for /treatments page.
  implication: If the backend query excludes Paused packages, they will disappear from the list.

- timestamp: 2026-03-21
  checked: treatment-api.ts getActiveTreatments() (line 50-56)
  found: Calls `GET /api/treatments/packages` with no query parameters. No client-side status filtering.
  implication: The filtering must happen server-side.

- timestamp: 2026-03-21
  checked: GetActiveTreatments.cs (backend handler, line 25)
  found: Calls `packageRepository.GetActivePackagesAsync(ct)` - the method name itself implies Active-only.
  implication: Need to check the repository implementation.

- timestamp: 2026-03-21
  checked: TreatmentPackageRepository.cs GetActivePackagesAsync (line 51-59)
  found: `.Where(x => x.Status == PackageStatus.Active)` - CONFIRMED. The query explicitly filters to Active status only. Paused, PendingCancellation, and all other statuses are excluded.
  implication: Issue 2 root cause confirmed. The backend query only returns Active packages.

- timestamp: 2026-03-21
  checked: TreatmentsPage.tsx filter UI (line 68-80)
  found: The frontend provides status filter dropdowns including "Paused", "PendingCancellation", "Completed", "Cancelled", "Switched" options. But these filters are client-side only (line 90-99) and can only filter data that was already returned by the API.
  implication: The UI promises filtering by Paused/PendingCancellation/etc but the data never contains those statuses. The filter options are misleading because the backend never returns non-Active packages.

## Resolution

root_cause: |
  TWO distinct root causes:

  **Issue 1 - Paused badge not yellow (detail page only):**
  `TreatmentPackageDetail.tsx` line 35 maps `Paused: "secondary"` which renders gray via shadcn's secondary variant. The list page (TreatmentsPage.tsx) correctly uses yellow custom classes but the detail page uses a different styling approach that lacks yellow.

  **Issue 2 - Paused packages disappear from /treatments list:**
  `TreatmentPackageRepository.GetActivePackagesAsync()` (line 56) filters with `.Where(x => x.Status == PackageStatus.Active)`, excluding all non-Active packages. The /treatments page calls this endpoint as its only data source. The frontend has status filter dropdowns for Paused, PendingCancellation, etc. but these are useless since the data never includes those statuses.

fix: |
  **Issue 1:** Change TreatmentPackageDetail.tsx to use the same approach as TreatmentsPage.tsx - use variant="outline" with custom yellow className for Paused status, or switch STATUS_VARIANT to a custom className map matching STATUS_STYLES from TreatmentsPage.tsx.

  **Issue 2:** Modify `GetActivePackagesAsync` (or create a new method) to return packages with statuses that should be visible on the treatments overview: Active, Paused, and PendingCancellation at minimum. Update the `.Where()` filter to:
  `.Where(x => x.Status == PackageStatus.Active || x.Status == PackageStatus.Paused || x.Status == PackageStatus.PendingCancellation)`
  Consider whether Completed/Cancelled/Switched should also be included (the UI has filters for them).

verification: N/A - research only, no changes made
files_changed: []

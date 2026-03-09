---
status: diagnosed
trigger: "Amendment History section missing from Visit Detail page"
created: 2026-03-09T00:00:00Z
updated: 2026-03-09T00:00:00Z
---

## Current Focus

hypothesis: VisitAmendmentHistory is conditionally rendered only when amendments array is non-empty, so the section is entirely hidden when there are no amendments
test: Read VisitDetailPage.tsx line 131
expecting: Conditional render wrapping the component
next_action: Report root cause

## Symptoms

expected: Visit detail page renders 6 collapsible card sections including Amendment History always visible
actual: Amendment History section is not visible on the visit detail page
errors: none
reproduction: Open any visit detail page - Amendment History section is absent
started: Likely since component was first added with conditional rendering

## Eliminated

(none - root cause found on first investigation)

## Evidence

- timestamp: 2026-03-09T00:00:00Z
  checked: VisitDetailPage.tsx lines 131-133
  found: VisitAmendmentHistory is wrapped in `{visit.amendments.length > 0 && (...)}` conditional
  implication: The section is completely hidden when there are zero amendments, unlike all other sections which render unconditionally

- timestamp: 2026-03-09T00:00:00Z
  checked: VisitAmendmentHistory.tsx component
  found: Component exists, is properly imported (line 13), accepts amendments prop, renders inside VisitSection collapsible card
  implication: Component itself is fine; the problem is the conditional gate in the parent

- timestamp: 2026-03-09T00:00:00Z
  checked: All other sections in VisitDetailPage.tsx (lines 87-135)
  found: PatientInfoSection, RefractionSection, DryEyeSection, ExaminationNotesSection, DiagnosisSection, DrugPrescriptionSection, OpticalPrescriptionSection, MedicalImagesSection, SignOffSection all render unconditionally
  implication: Amendment History is the ONLY section with a conditional render gate - inconsistent with the pattern of other sections

## Resolution

root_cause: Line 131 in VisitDetailPage.tsx wraps VisitAmendmentHistory in a conditional `{visit.amendments.length > 0 && (...)}` which completely hides the section when there are no amendments. All other sections render unconditionally as collapsible cards regardless of whether they have data.
fix: Remove the conditional wrapper so VisitAmendmentHistory always renders, matching the pattern of other sections. The component already handles empty state gracefully via VisitSection (collapsed by default with defaultOpen={false}).
verification: (not applied - diagnosis only)
files_changed: []

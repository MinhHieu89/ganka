---
status: diagnosed
trigger: "Amendment history shows wrong field-level changes - 'pending_amendment' as new values, shows unchanged fields"
created: 2026-03-09T00:00:00Z
updated: 2026-03-09T00:02:00Z
---

## Current Focus

hypothesis: CONFIRMED - field changes are captured at amendment initiation (before user edits) with placeholder "pending_amendment" values, and there is no subsequent step to compute or store the actual diff
test: Complete workflow trace confirmed
expecting: N/A - root cause confirmed
next_action: Return diagnosis

## Symptoms

expected: After amending, Amendment History shows only the fields that were actually changed, with accurate old/new values
actual: (1) "New Value" shows "pending_amendment" instead of actual new values. (2) User edited only 1 field but 3 rows shown in the diff
errors: None (functional bug, not crash)
reproduction: Amend a signed visit, change one field, view amendment history
started: Since amendment feature was implemented

## Eliminated

## Evidence

- timestamp: 2026-03-09T00:01:00Z
  checked: AmendmentDialog.tsx buildFieldChangesSnapshot function (lines 27-84)
  found: Function snapshots ALL non-empty fields of the signed visit with "pending_amendment" as newValue. It runs at amendment initiation time (before any edits). Every field that has data gets included regardless of whether it will be changed.
  implication: This is the primary source of both bugs -- wrong newValue and too many rows

- timestamp: 2026-03-09T00:01:00Z
  checked: AmendVisit.cs backend handler
  found: Handler simply stores the fieldChangesJson string as-is from the frontend command. No server-side diff computation.
  implication: Backend is a passthrough for field changes, so the fix must address when/how the diff is computed

- timestamp: 2026-03-09T00:01:00Z
  checked: VisitAmendment.cs domain entity
  found: FieldChangesJson is a simple string property set at creation time and never updated afterward. No UpdateFieldChanges method exists.
  implication: The entity has no mechanism to update field changes after amendment edits are complete

- timestamp: 2026-03-09T00:01:00Z
  checked: VisitAmendmentHistory.tsx display component
  found: Correctly parses and displays whatever is in fieldChangesJson. The display logic itself is fine -- it faithfully renders the bad data it receives.
  implication: Display component is not the problem

- timestamp: 2026-03-09T00:02:00Z
  checked: Full amendment lifecycle (SignOffSection, Visit entity, SignOffVisit handler, AmendVisit handler)
  found: The complete workflow is: (1) Visit is signed (status=Signed), (2) User clicks "Amend" which opens AmendmentDialog, (3) Dialog calls buildFieldChangesSnapshot capturing signed state with "pending_amendment" placeholders, (4) On submit, AmendVisit backend handler stores the snapshot and sets status=Amended, (5) User edits the visit (now editable because status=Amended), (6) User re-signs the visit. There is NO step between (5) and (6) that computes an actual before/after diff. The field changes are permanently stored with wrong data from step (3).
  implication: The architectural flaw is that diff capture happens at the WRONG point in the lifecycle

- timestamp: 2026-03-09T00:02:00Z
  checked: FieldChange property name mismatch between AmendmentDialog and VisitAmendmentHistory
  found: AmendmentDialog produces objects with key "fieldName" but VisitAmendmentHistory reads key "field". This means the field name column in the UI would also be broken (showing undefined).
  implication: Secondary bug -- even if the diff data were correct, the field name column would not display properly due to property name mismatch

- timestamp: 2026-03-09T00:02:00Z
  checked: Visit.SignOff() method and Visit.StartAmendment() method
  found: SignOff() accepts Draft or Amended status and transitions to Signed. StartAmendment() only accepts Signed status and transitions to Amended. Neither method touches FieldChangesJson. There is no "FinalizeAmendment" step.
  implication: Confirms there is no mechanism anywhere in the codebase to compute the actual diff after edits

## Resolution

root_cause: |
  The amendment field-level diff is captured at the WRONG point in the lifecycle.

  PRIMARY BUG (wrong timing): buildFieldChangesSnapshot() in AmendmentDialog.tsx runs when the user
  initiates an amendment (before any edits). It snapshots every non-empty field of the signed visit
  and uses "pending_amendment" as the newValue placeholder for all of them. This snapshot is immediately
  sent to the backend and stored permanently in VisitAmendment.FieldChangesJson. The user then edits
  the visit and re-signs it, but no code ever computes or stores the actual before/after diff.

  SECONDARY BUG (shows all fields): Because the snapshot captures every non-empty field (not just changed
  ones), the amendment history always shows all data-bearing fields regardless of whether they were modified.

  TERTIARY BUG (field name mismatch): AmendmentDialog produces JSON with key "fieldName" but
  VisitAmendmentHistory reads key "field", so the field name column would show undefined.

fix:
verification:
files_changed: []

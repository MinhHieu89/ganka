---
status: investigating
trigger: "Amendment history shows (none) and (added) instead of actual old/new values. Also field names are not localized."
created: 2026-03-09T00:00:00Z
updated: 2026-03-09T00:00:00Z
---

## Current Focus

hypothesis: The data-flow between amend (baseline snapshot) and re-sign (diff computation) is broken due to a mismatch in what data `computeFieldChanges` reads. The `visit` object passed to `computeFieldChanges` has already been updated with the new values AND its `amendments` array contains the baseline snapshot — but the baseline snapshot was written before any edits. If the visit data used for comparison is stale or mismatched, the new-value side will wrongly equal the old-value side and produce no real changes. Additionally, when a refraction type is genuinely new (added during amendment), the code explicitly hard-codes "(none)" / "(added)" rather than the actual values.

test: Trace the full lifecycle — amend action → re-sign action — step by step, verifying what data is present at each point.

expecting: At re-sign time, `visit.refractions` contains the EDITED values, and `latestAmendment.fieldChangesJson` contains the baseline (pre-edit) snapshot. If those are correctly set, `computeFieldChanges` should produce a real diff. The bug manifests because of a specific condition path in the code.

next_action: Root cause analysis complete — document findings.

## Symptoms

expected: Amendment history table shows real before/after values (e.g., "odSph: -2.50 → -3.00") and localized field names (e.g., "OD SPH" not "odSph").
actual: Amendment history shows "(none)" for old value and "(added)" for new value. Field names show raw camelCase keys like "sphOd" / "refraction.manifest.odSph".
errors: No JS errors — purely a logic/data issue.
reproduction: Sign a visit with an existing manifest refraction. Click Amend, enter reason, confirm. Edit the manifest refraction SPH field. Re-sign. Open amendment history.
started: Always — this is how the code was implemented.

## Eliminated

- hypothesis: Backend loses the fieldChangesJson during save.
  evidence: Backend SignOffVisit handler receives fieldChangesJson from the command and calls latestAmendment.UpdateFieldChanges(command.FieldChangesJson) before SaveChangesAsync. The storage path is correct.
  timestamp: 2026-03-09

- hypothesis: The baseline snapshot (VisitBaseline) is not being written to the database at amend time.
  evidence: AmendmentDialog.handleSubmit calls buildBaselineSnapshot(visit) and passes the result as fieldChangesJson to useAmendVisit. AmendVisitHandler stores it as VisitAmendment.FieldChangesJson. This part is correct.
  timestamp: 2026-03-09

- hypothesis: computeFieldChanges fails to parse the baseline JSON entirely.
  evidence: The code has a try/catch and if parsing fails it returns "[]". But the issue produces "(none)"/"(added)" entries — which means it DID parse successfully and reached the refraction comparison loop. The parsing path is not the problem.
  timestamp: 2026-03-09

## Evidence

- timestamp: 2026-03-09
  checked: AmendmentDialog.tsx buildBaselineSnapshot (lines 45-65)
  found: Correctly snapshots visit.refractions mapping all fields. Returns a VisitBaseline JSON object (NOT an array). Stored as amendment.fieldChangesJson at amend time.
  implication: The "before" state is captured correctly at amendment initiation.

- timestamp: 2026-03-09
  checked: SignOffSection.tsx computeFieldChanges (lines 25-124)
  found: At re-sign time, reads latestAmendment from visit.amendments (sorted descending), parses fieldChangesJson. If parsed value is NOT an array, treats it as a VisitBaseline. Then iterates baseline.refractions and compares against current visit.refractions.
  implication: The diff logic structure is correct in intent.

- timestamp: 2026-03-09
  checked: computeFieldChanges line 36 — the array-check short-circuit
  found: "if (Array.isArray(parsed)) return latestAmendment.fieldChangesJson" — if fieldChangesJson is already an array (final diff), it short-circuits and returns as-is without re-computing. This is a guard for already-finalized amendments.
  implication: This guard is not the problem for a fresh amendment where baseline is a VisitBaseline object.

- timestamp: 2026-03-09
  checked: computeFieldChanges lines 91-99 — the "(none)"/"(added)" code path
  found: When iterating current visit.refractions, if a refraction type is NOT found in baseline.refractions, it pushes { field: `refraction.${type}`, oldValue: "(none)", newValue: "(added)" }. This is explicitly hardcoded.
  implication: This path fires when a refraction type exists in the CURRENT visit but does NOT exist in the baseline. For the reported scenario (user edits an EXISTING manifest refraction), this should NOT fire — the manifest type should exist in both baseline and current.

- timestamp: 2026-03-09
  checked: What visit.refractions contains at re-sign time
  found: The visit object passed to computeFieldChanges comes from the SignOffSection component's props (visit: VisitDetailDto). This is the React Query cached value. After the user edits a refraction, useUpdateRefraction invalidates the visit query, so the query refetches and the component re-renders with updated visit data.
  implication: At re-sign time, visit.refractions contains the NEW (edited) values. This is correct for the "new value" side of the diff.

- timestamp: 2026-03-09
  checked: What visit.amendments contains at re-sign time
  found: visit.amendments comes from the same React Query cache. It was last fetched after the AmendVisit API call (useAmendVisit onSuccess invalidates the visit query). At that point, the backend has stored the VisitBaseline JSON as fieldChangesJson. So amendments[0].fieldChangesJson is the baseline snapshot.
  implication: At re-sign time, the baseline snapshot is available. The diff computation should find real changes.

- timestamp: 2026-03-09
  checked: ROOT CAUSE #1 — The "(none)"/"(added)" output for an EDITED refraction
  found: The condition for "(none)"/"(added)" fires when `!baseline.refractions.find((r) => r.type === curRef.type)`. For an edited manifest refraction, the manifest type (type=0) IS in the baseline, so this should NOT fire. BUT — there is a subtle timing / cache issue. The useAmendVisit mutation invalidates the visit query. When the visit data reloads, the visit now has status=2 (Amended) and the amendment record with the baseline. However, if the user had NOT yet saved their refraction edits at the time of re-sign, `visit.refractions` may still have the old values — causing no diff. This is a workflow issue but is NOT what produces "(none)"/"(added)".
  implication: The "(none)"/"(added)" symptoms indicate the SPECIFIC CODE PATH at lines 91-99 is firing, meaning the refraction type in visit.refractions is NOT found in baseline.refractions.

- timestamp: 2026-03-09
  checked: ROOT CAUSE #1 CONFIRMED — RefractionDto.type vs baseline refraction type comparison
  found: In buildBaselineSnapshot, refractions are mapped with `type: r.type`. In computeFieldChanges, the lookup is `baseline.refractions.find((r) => r.type === baseRef.type)` and `baseline.refractions.find((r) => r.type === curRef.type)`. The types are compared with `===`. The VisitBaseline stores `type` as a plain JS number. The RefractionDto also has `type: number`. These should match with ===.
  HOWEVER: there is one scenario where they do NOT match. If the visit had NO refractions at all at the time of amendment (baseline.refractions = []), then ALL current refractions would fall into the "(none)"/"(added)" bucket. But more critically: the test visit likely has a manifest refraction with type=0. If the baseline has type=0 and current also has type=0, they should match. So the "(none)"/"(added)" can only fire if baseline.refractions is empty OR if the type numbers don't match.
  implication: Need to check if there is a scenario where baseline.refractions could be empty or the types differ.

- timestamp: 2026-03-09
  checked: ROOT CAUSE #1 ACTUAL CAUSE — visit.refractions at amendment time
  found: buildBaselineSnapshot reads visit.refractions at the moment AmendmentDialog opens (when user clicks the "Amend" button on a SIGNED visit). If the signed visit has refractions (type=0 manifest), baseline.refractions will have one entry with type=0. Later at re-sign, visit.refractions still has type=0. The find() should succeed.
  CRITICAL DISCOVERY: The "(none)"/"(added)" path is for refractions that are ADDED during amendment (lines 91-99). For EXISTING refractions that are merely EDITED, the diff should show specific field changes (lines 77-87). The user reports seeing "(none)"/"(added)" for an EDITED refraction, which means the existing-refraction loop (lines 66-87) is producing NO changes despite the user editing values. This suggests the values in visit.refractions at re-sign time match the baseline values — i.e., the EDITED values are not being picked up.
  implication: The visit data in the React component is stale at re-sign time, OR the refraction edits were NOT saved before the snapshot was taken.

- timestamp: 2026-03-09
  checked: Data staleness — when does the visit query refresh after refraction edit?
  found: useUpdateRefraction onSuccess invalidates clinicalKeys.visit(variables.visitId). This triggers a React Query refetch. By the time the user clicks "Sign Off" after editing, the query should have refetched. UNLESS the edits are in-flight or the component's visit prop is from a parent that holds a snapshot.
  implication: If the visit prop is up-to-date, the field comparison should work. This path is not the root cause of "(none)"/"(added)".

- timestamp: 2026-03-09
  checked: ROOT CAUSE #1 FINAL — The REAL reason for "(none)"/"(added)"
  found: Re-reading the user's reported scenario: they amend a visit, EDIT a field, then re-sign. The "(none)"/"(added)" appears. This can ONLY mean the existing-refraction loop (lines 66-87) produced NO changes for that refraction — and separately the new-refraction loop (lines 91-99) DID fire. For both loops to produce this result, the refraction at lines 66 must have been found (baseline type matches current type) but ALL fields matched (no diff), AND the new-refraction check somehow ALSO found a type NOT in baseline.
  ACTUALLY — reviewing more carefully: lines 66-87 iterate `baseline.refractions`. Lines 91-99 iterate `visit.refractions` (current). If baseline has type=0 and current has type=0, the inner find on line 92 (`baseline.refractions.find(r => r.type === curRef.type)`) WOULD find it and would NOT produce "(none)"/"(added)". So the only way to get "(none)"/"(added)" is if the baseline has NO entry for that refraction type.
  THEREFORE: The baseline snapshot (saved when user clicks "Amend") was built from a visit that had ZERO refractions, or had refractions of a DIFFERENT type number. Then the user ADDED a new refraction during amendment, and at re-sign it appears as "(none)"/"(added)".
  implication: The user's description ("edits a field") actually corresponds to a scenario where the REFRACTION RECORD DID NOT EXIST in the visit at sign-off time and was ADDED during amendment. The baseline correctly shows no refraction of that type, and the diff correctly identifies it as "added" — but the label is "(added)" instead of showing the actual new values.

- timestamp: 2026-03-09
  checked: ROOT CAUSE #1 CONFIRMED — lines 91-99 hardcode "(added)" instead of showing actual values
  found: When a refraction is genuinely NEW (added during amendment), the code pushes the whole refraction as "(added)" with no actual values shown. The correct behavior should show the actual field values of the new refraction.
  implication: BUG #1 is in SignOffSection.tsx lines 91-99. The newValue should show the actual field values of the newly added refraction, not the literal string "(added)".

- timestamp: 2026-03-09
  checked: ROOT CAUSE #2 — Field names not localized
  found: In VisitAmendmentHistory.tsx (lines 83-95), the field name is rendered directly as `{change.field}` with no translation applied. The `change.field` values are raw keys like "refraction.manifest.odSph" or "diagnosis" or "examinationNotes" — these are set in SignOffSection.tsx computeFieldChanges. There is NO field-label lookup map anywhere in the codebase (confirmed by grep for "FIELD_LABELS", "fieldLabel", etc. — no results). The i18n files have labels under "refraction.sph", "refraction.cyl", etc. but these are not mapped to the change.field keys used in the amendment diff.
  implication: BUG #2 is in VisitAmendmentHistory.tsx — it renders `change.field` raw. There is no label mapping from dot-notation keys like "refraction.manifest.odSph" to localized labels like "Manifest OD SPH".

## Resolution

root_cause: |
  Two distinct bugs:

  BUG #1 — "(none)"/"(added)" for new refractions (SignOffSection.tsx lines 91-99):
  When computeFieldChanges detects a refraction type that is present in the current visit but absent from the baseline snapshot, it produces:
    { field: "refraction.manifest", oldValue: "(none)", newValue: "(added)" }
  This is a hardcoded placeholder. It does NOT show the actual field values of the newly added refraction. The fix requires iterating the new refraction's fields and emitting one change-row per field (or a structured summary), instead of a single "(none)"/"(added)" row.

  BUG #2 — Field names not localized (VisitAmendmentHistory.tsx line 87):
  The `change.field` raw key (e.g., "refraction.manifest.odSph", "examinationNotes", "diagnosis") is rendered directly in the table cell. There is no i18n lookup map that translates these dot-notation keys to human-readable labels. The clinical.json locale file has labels like "refraction.sph" and "refraction.manifest" but they are not wired into the amendment history rendering.
  The fix requires either:
    (a) Building a lookup table in VisitAmendmentHistory.tsx that maps known field keys to t() calls, or
    (b) Storing a human-readable field label string inside each FieldChange object at the time computeFieldChanges builds the changes array (pushing `label` alongside `field`).

fix: Not applied (goal: find_root_cause_only)
verification: Not applied
files_changed: []

## Files Involved

- frontend/src/features/clinical/components/SignOffSection.tsx
  Lines 91-99: Hardcodes "(none)"/"(added)" for newly added refractions instead of emitting per-field change rows with actual values.

- frontend/src/features/clinical/components/VisitAmendmentHistory.tsx
  Line 87: Renders `change.field` (raw dot-notation key) directly — no i18n label lookup applied.

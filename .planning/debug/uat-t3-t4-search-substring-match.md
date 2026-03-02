---
status: diagnosed
trigger: "UAT Tests 3 & 4: search box does not match patient code and phone by pattern"
created: 2026-03-02T00:00:00Z
updated: 2026-03-02T00:00:00Z
---

## Current Focus

hypothesis: PatientRepository uses StartsWith for Phone and exact-match (==) for PatientCode instead of Contains for both
test: Read repository source code and verify LINQ predicates
expecting: Finding StartsWith/== operators where Contains should be used
next_action: DIAGNOSED - return findings

## Symptoms

expected: Searching '0001' should match patient code 'GK-2026-0001'; searching '6543' should match phone '0987654321'
actual: Neither substring match works - only prefix matches on Phone and exact matches on PatientCode return results
errors: No errors - just no results returned for valid substring searches
reproduction: Type '0001' in patient list search or Ctrl+K global search. No results despite patient GK-2026-0001 existing.
started: Always broken (original implementation)

## Eliminated

(none - root cause found on first hypothesis)

## Evidence

- timestamp: 2026-03-02T00:00:00Z
  checked: PatientRepository.cs lines 35-46 (SearchAsync method)
  found: |
    Phone matching uses `p.Phone.StartsWith(term)` - only matches from beginning of string.
    PatientCode matching uses `p.PatientCode == term` - requires exact full string match.
  implication: Searching '0001' cannot match 'GK-2026-0001' because == requires the full code. Searching '6543' cannot match '0987654321' because StartsWith only matches from the beginning.

- timestamp: 2026-03-02T00:00:00Z
  checked: PatientRepository.cs lines 64-71 (GetPagedAsync search filter)
  found: |
    Identical predicate logic as SearchAsync:
    `p.Phone.StartsWith(term)` and `p.PatientCode == term`
  implication: Both the Patient List search (Test 3) and Global Search (Test 4) share the same root cause since both code paths use the same matching operators.

- timestamp: 2026-03-02T00:00:00Z
  checked: SearchPatients.cs (SearchPatientsHandler) and GetPatientList.cs (GetPatientListHandler)
  found: Both handlers pass search term straight through to repository with no transformation. No application-layer filtering.
  implication: The bug is purely in the repository layer predicates, not in the application or frontend layers.

- timestamp: 2026-03-02T00:00:00Z
  checked: Frontend (usePatientSearch.ts, PatientListPage.tsx, GlobalSearch.tsx)
  found: Frontend passes search term correctly. No truncation or transformation. Both features use debounced search with minimum 2-char threshold.
  implication: Frontend is not contributing to the bug.

## Resolution

root_cause: |
  In PatientRepository.cs, both SearchAsync (line 42) and GetPagedAsync (line 70) use
  overly restrictive matching operators for PatientCode and Phone:
  - PatientCode uses `==` (exact match) instead of `Contains()` (substring match)
  - Phone uses `StartsWith()` (prefix match) instead of `Contains()` (substring match)

  This means:
  - '0001' cannot match 'GK-2026-0001' because == demands the entire string
  - '6543' cannot match '0987654321' because StartsWith only checks from index 0

fix: (not applied - diagnosis only)
verification: (not applied - diagnosis only)
files_changed: []

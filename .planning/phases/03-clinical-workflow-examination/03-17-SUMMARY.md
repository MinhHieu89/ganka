---
phase: 03-clinical-workflow-examination
plan: 17
subsystem: backend, frontend
tags: [collation, icd10, set-primary, va-decimal, gap-closure]

# Dependency graph
requires:
  - phase: 03-clinical-workflow-examination (plans 03-16)
    provides: identified remaining 3 gaps (collation, set-primary, VA decimal)
provides:
  - Latin1_General_CI_AI column-level collation for accent-insensitive Vietnamese ICD-10 search
  - SetPrimaryDiagnosis backend endpoint and frontend wiring
  - VA decimal display preservation (toFixed(2) for VA fields)
affects: [03-clinical-workflow-examination]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Column-level Latin1_General_CI_AI collation for Vietnamese diacritics stripping (ê→e, ô→o, ă→a, đ→d)"
    - "Field-aware toFormValue with toFixed(2) for VA fields"

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Application/Features/SetPrimaryDiagnosis.cs
    - backend/tests/Clinical.Unit.Tests/Features/SetPrimaryDiagnosisHandlerTests.cs
    - backend/src/Shared/Shared.Infrastructure/Migrations/Reference/20260309103241_SetVietnameseCollation.cs
    - backend/src/Shared/Shared.Infrastructure/Migrations/Reference/20260309104101_SetLatin1CollationForVietnameseSearch.cs
  modified:
    - backend/src/Shared/Shared.Infrastructure/ReferenceDbContext.cs
    - backend/src/Shared/Shared.Infrastructure/Repositories/ReferenceDataRepository.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/VisitDiagnosis.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/CreateVisitCommand.cs
    - backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
    - frontend/src/features/clinical/components/RefractionForm.tsx
    - frontend/src/features/clinical/components/DiagnosisSection.tsx
    - frontend/src/features/clinical/api/clinical-api.ts
    - frontend/public/locales/en/clinical.json
    - frontend/public/locales/vi/clinical.json
    - backend/tests/Shared.Unit.Tests/Repositories/ReferenceDataRepositoryTests.cs

key-decisions:
  - "Latin1_General_CI_AI chosen over Vietnamese_CI_AI because Vietnamese collation treats ê/ô/ă/ơ/ư/đ as distinct base letters, only stripping tone marks — Latin1 strips ALL diacritics"
  - "Column-level collation via migration instead of query-time EF.Functions.Collate for proper index usage"
  - "VA_FIELDS set (ucvaOd, ucvaOs, bcvaOd, bcvaOs) with toFixed(2) — ophthalmology VA is always decimal"

patterns-established:
  - "Use column-level collation for accent-insensitive search rather than query-time COLLATE"

requirements-completed:
  - DX-01
  - CLN-02
  - REF-02

# Metrics
duration: 45min
completed: 2026-03-09
---

# Phase 03 Plan 17: Close Final 3 Verification Gaps Summary

**Fixed ICD-10 accent-insensitive search with Latin1_General_CI_AI column collation, implemented SetPrimaryDiagnosis endpoint, and preserved VA decimal display — all human-verified at runtime**

## Performance

- **Duration:** ~45 min (including human verification)
- **Completed:** 2026-03-09
- **Tasks:** 2 (1 auto TDD + 1 human verification checkpoint)
- **Files modified:** 14

## Accomplishments

- Fixed ICD-10 accent-insensitive search: searching "viem" now matches "Viêm" entries via Latin1_General_CI_AI column-level collation
- Implemented SetPrimaryDiagnosis backend (TDD: 4 tests) + PUT endpoint + frontend mutation hook
- Fixed VA decimal display: toFormValue uses toFixed(2) for VA fields so 5.0 shows as "5.00"
- All 144 unit tests pass (128 Clinical + 16 Shared)

## Task Commits

1. **Task 1 (TDD RED):** `f22e13e` — Failing tests for SetPrimaryDiagnosis handler
2. **Task 1 (TDD GREEN):** `b5b5641` — Implement SetPrimaryDiagnosis backend handler and endpoint
3. **Task 1 (Frontend+Collation+VA):** `7cb0e1f` — Close 3 verification gaps
4. **Collation fix iteration 1:** `e777880` — Set Vietnamese_CI_AI at column level via migration
5. **Collation fix iteration 2:** `b8298a4` — Switch to Latin1_General_CI_AI for full diacritics stripping

## Human Verification Results

| # | Test | Result |
|---|------|--------|
| 1 | ICD-10 search "viem" returns accented Vietnamese entries | PASS |
| 2 | Set as Primary swaps diagnosis roles | PASS |
| 3 | VA value 5.00 preserved after page reload | PASS |
| 4 | Amendment history diagnosis labels (regression) | PASS |

## Deviations from Plan

- Plan specified Vietnamese_CI_AI collation but runtime testing revealed it only strips tone marks, not base letter modifications (ê≠e). Changed to Latin1_General_CI_AI which strips all diacritics. Required an additional migration.

## Self-Check: PASSED

All key files verified present. Commits f22e13e, b5b5641, 7cb0e1f, e777880, b8298a4 verified in git history.

---
*Phase: 03-clinical-workflow-examination*
*Completed: 2026-03-09*

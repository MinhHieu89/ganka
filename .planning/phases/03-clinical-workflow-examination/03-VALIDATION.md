---
phase: 3
slug: clinical-workflow-examination
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-04
---

# Phase 3 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.x + NSubstitute 5.x + FluentAssertions 8.x |
| **Config file** | backend/tests/Clinical.Unit.Tests/ (Wave 0 creates) |
| **Quick run command** | `dotnet test backend/tests/Clinical.Unit.Tests --no-build -x` |
| **Full suite command** | `dotnet test backend/Ganka28.slnx` |
| **Estimated runtime** | ~15 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test backend/tests/Clinical.Unit.Tests --no-build -x`
- **After every plan wave:** Run `dotnet test backend/Ganka28.slnx`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 03-01-01 | 01 | 1 | CLN-01 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~CreateVisitHandlerTests" -x` | ❌ W0 | ⬜ pending |
| 03-01-02 | 01 | 1 | CLN-01 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~SignOffVisitHandlerTests" -x` | ❌ W0 | ⬜ pending |
| 03-01-03 | 01 | 1 | CLN-02 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~AmendVisitHandlerTests" -x` | ❌ W0 | ⬜ pending |
| 03-01-04 | 01 | 1 | CLN-03 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~AdvanceWorkflowStageHandlerTests" -x` | ❌ W0 | ⬜ pending |
| 03-01-05 | 01 | 1 | CLN-04 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~GetActiveVisitsHandlerTests" -x` | ❌ W0 | ⬜ pending |
| 03-02-01 | 02 | 1 | REF-01 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~UpdateRefractionHandlerTests" -x` | ❌ W0 | ⬜ pending |
| 03-02-02 | 02 | 1 | REF-02 | unit | Covered by UpdateRefractionHandlerTests | ❌ W0 | ⬜ pending |
| 03-02-03 | 02 | 1 | REF-03 | unit | Covered by UpdateRefractionHandlerTests | ❌ W0 | ⬜ pending |
| 03-03-01 | 03 | 1 | DX-01 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~SearchIcd10CodesHandlerTests" -x` | ❌ W0 | ⬜ pending |
| 03-03-02 | 03 | 1 | DX-02 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "ClassName~AddVisitDiagnosisHandlerTests" -x` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `backend/tests/Clinical.Unit.Tests/` — create test project, add to solution
- [ ] NuGet references: xunit, NSubstitute, FluentAssertions, Bogus
- [ ] Project references: Clinical.Application, Clinical.Domain, Shared.Domain
- [ ] Test stubs for all handler tests listed above

*Existing test infrastructure in backend/tests/ covers framework setup; only Clinical-specific project needed.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Kanban drag-and-drop UX | CLN-03 | Browser interaction | Drag patient card between columns, verify stage updates |
| Refraction OD/OS side-by-side layout | REF-01 | Visual layout | Open visit, verify side-by-side eye fields render correctly |
| ICD-10 laterality inline prompt | DX-02 | UI interaction flow | Select laterality-required code, verify OD/OS/OU prompt appears |
| Visit sign-off confirmation dialog | CLN-01 | UI flow | Click sign-off, verify confirmation dialog, verify fields become read-only |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 15s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending

---
phase: 10
slug: address-all-pending-todos
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-14
---

# Phase 10 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit + NSubstitute + FluentAssertions |
| **Config file** | Each module has `*.Unit.Tests.csproj` under `backend/tests/` |
| **Quick run command** | `dotnet test backend/tests/Pharmacy.Unit.Tests/ --no-build -v q` |
| **Full suite command** | `dotnet test backend/ -v q` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test backend/tests/{ModuleName}.Unit.Tests/ --no-build -v q`
- **After every plan wave:** Run `dotnet test backend/ -v q`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 10-01-01 | 01 | 1 | Drug catalog pagination | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests/ -v q` | Extend existing | ⬜ pending |
| 10-01-02 | 01 | 1 | OTC stock validation | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests/ -v q` | New handler test | ⬜ pending |
| 10-01-03 | 01 | 1 | Drug catalog Excel import | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests/ -v q` | New handler test | ⬜ pending |
| 10-02-01 | 02 | 1 | Realtime OSDI | unit | `dotnet test backend/tests/Clinical.Unit.Tests/ -v q` | New handler test | ⬜ pending |
| 10-02-02 | 02 | 1 | OSDI answers display | unit | `dotnet test backend/tests/Clinical.Unit.Tests/ -v q` | New handler test | ⬜ pending |
| 10-02-03 | 02 | 1 | Dry eye metric history | unit | `dotnet test backend/tests/Clinical.Unit.Tests/ -v q` | New handler test | ⬜ pending |
| 10-02-04 | 02 | 1 | Batch label print | unit | `dotnet test backend/tests/Clinical.Unit.Tests/ -v q` | New test | ⬜ pending |
| 10-03-01 | 03 | 1 | Logo upload | unit | `dotnet test backend/tests/Shared.Unit.Tests/ -v q` | New handler test | ⬜ pending |
| 10-04-01 | 04 | 1 | Textarea auto-resize | manual | Visual verification | N/A | ⬜ pending |
| 10-04-02 | 04 | 1 | Patient name link | manual | Visual verification | N/A | ⬜ pending |
| 10-04-03 | 04 | 1 | DrugCombobox auto-focus | manual | Visual verification | N/A | ⬜ pending |
| 10-04-04 | 04 | 1 | Optical section defaultOpen | manual | Visual verification | N/A | ⬜ pending |
| 10-04-05 | 04 | 1 | Stock import search fix | manual | Visual verification | N/A | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Existing infrastructure covers all modules. New test files will be created for new handlers following TDD.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Textarea auto-resize | UX fix | Visual/interactive behavior | Type multi-line text, verify textarea grows |
| Patient name link | UX fix | Navigation/tab behavior | Click patient name on visit detail, verify new tab opens |
| DrugCombobox auto-focus | UX fix | Focus behavior | Open OTC sale form, verify drug field is auto-focused |
| Optical section defaultOpen | UX fix | Accordion state | Open visit with optical data, verify section expanded |
| Stock import search fix | Bug fix | Combobox filtering | Search for drug in stock import, verify results filter correctly |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending

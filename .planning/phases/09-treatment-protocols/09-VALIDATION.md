---
phase: 9
slug: treatment-protocols
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-08
---

# Phase 9 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.* + NSubstitute 5.* + FluentAssertions 8.* |
| **Config file** | `backend/tests/Treatment.Unit.Tests/Treatment.Unit.Tests.csproj` (Wave 0 creation) |
| **Quick run command** | `dotnet test backend/tests/Treatment.Unit.Tests --no-build -v q` |
| **Full suite command** | `dotnet test backend/Ganka28.slnx --no-build -v q` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test backend/tests/Treatment.Unit.Tests --no-build -v q`
- **After every plan wave:** Run `dotnet test backend/Ganka28.slnx --no-build -v q`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 09-01-01 | 01 | 1 | TRT-01 | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "CreateTreatmentPackage" -v q` | ❌ W0 | ⬜ pending |
| 09-02-01 | 02 | 1 | TRT-02 | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "SessionTracking" -v q` | ❌ W0 | ⬜ pending |
| 09-03-01 | 03 | 1 | TRT-03 | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "RecordSessionOsdi" -v q` | ❌ W0 | ⬜ pending |
| 09-04-01 | 04 | 1 | TRT-04 | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "AutoComplete" -v q` | ❌ W0 | ⬜ pending |
| 09-05-01 | 05 | 2 | TRT-05 | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "IntervalEnforcement" -v q` | ❌ W0 | ⬜ pending |
| 09-06-01 | 06 | 2 | TRT-06 | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "ConcurrentPackages" -v q` | ❌ W0 | ⬜ pending |
| 09-07-01 | 07 | 2 | TRT-07 | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "ModifyPackage" -v q` | ❌ W0 | ⬜ pending |
| 09-08-01 | 08 | 3 | TRT-08 | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "SwitchTreatment" -v q` | ❌ W0 | ⬜ pending |
| 09-09-01 | 09 | 3 | TRT-09 | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "Cancellation" -v q` | ❌ W0 | ⬜ pending |
| 09-10-01 | 10 | 1 | TRT-10 | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "Permission" -v q` | ❌ W0 | ⬜ pending |
| 09-11-01 | 11 | 3 | TRT-11 | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "ConsumableDeduction" -v q` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `backend/tests/Treatment.Unit.Tests/Treatment.Unit.Tests.csproj` — new test project with xUnit + NSubstitute + FluentAssertions references
- [ ] `backend/tests/Treatment.Unit.Tests/Features/` — test directory for handler tests
- [ ] Add Treatment.Unit.Tests to solution (Ganka28.slnx)
- [ ] Treatment.Presentation project (not scaffolded yet)
- [ ] Treatment.Application IoC.cs (register FluentValidation validators)
- [ ] Treatment.Infrastructure IoC.cs (register repositories + UnitOfWork)

*If none: "Existing infrastructure covers all phase requirements."*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| "Due Soon" section displays correctly | TRT-05 | Visual rendering/layout | Navigate to /treatments, verify sessions past minimum interval appear in "Due Soon" section |
| OSDI QR/link self-fill for treatment sessions | TRT-03 | End-to-end browser flow | Create session, generate QR, scan on mobile, verify OSDI score links to session |
| Session photo upload and preview | TRT-03 | File upload integration | Record session, upload photo via drag-drop, verify thumbnail preview displays |
| Approval queue manager workflow | TRT-09 | Multi-user interaction | Doctor requests cancellation, login as manager, verify item appears in queue, approve/reject |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending

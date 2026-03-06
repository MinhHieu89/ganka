---
phase: 7
slug: billing-finance
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-06
---

# Phase 7 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xunit + FluentAssertions + NSubstitute (backend), vitest (frontend) |
| **Config file** | backend/tests/Billing.Unit.Tests/Billing.Unit.Tests.csproj |
| **Quick run command** | `dotnet test backend/tests/Billing.Unit.Tests --no-build -q` |
| **Full suite command** | `dotnet test backend/Ganka28.slnx --no-build -q` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test backend/tests/Billing.Unit.Tests --no-build -q`
- **After every plan wave:** Run `dotnet test backend/Ganka28.slnx --no-build -q`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 07-01-01 | 01 | 1 | FIN-01 | unit | `dotnet test Billing.Unit.Tests` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `backend/tests/Billing.Unit.Tests/Billing.Unit.Tests.csproj` — test project setup
- [ ] `backend/tests/Billing.Unit.Tests/Features/` — handler test stubs

*Existing infrastructure covers framework installation. Only project scaffolding needed.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| QR code payment flow | FIN-04 | Requires VNPay/MoMo sandbox | Test with sandbox credentials |
| PDF invoice rendering | FIN-06 | Visual verification needed | Check Vietnamese diacritics, layout |
| Manager PIN override dialog | FIN-09 | UI interaction flow | Test PIN entry, wrong PIN, approval |
| Cash drawer reconciliation | FIN-07 | End-to-end workflow | Open shift, process payments, close shift |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending

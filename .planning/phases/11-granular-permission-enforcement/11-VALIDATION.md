---
phase: 11
slug: granular-permission-enforcement
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-24
---

# Phase 11 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.x (backend) / Vitest (frontend) |
| **Config file** | `backend/tests/` (backend) / `frontend/vitest.config.ts` (frontend) |
| **Quick run command** | `dotnet test backend/tests/ --filter "Category=Permission"` |
| **Full suite command** | `dotnet test backend/tests/ && cd frontend && npx vitest run` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test backend/tests/ --filter "Category=Permission"`
- **After every plan wave:** Run `dotnet test backend/tests/ && cd frontend && npx vitest run`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 11-01-01 | 01 | 1 | AUTH-04 | integration | `dotnet test --filter "Permission"` | ❌ W0 | ⬜ pending |
| 11-02-01 | 02 | 1 | AUTH-05 | integration | `npx vitest run --grep "permission guard"` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] Backend permission enforcement test stubs for AUTH-04
- [ ] Frontend route guard test stubs for AUTH-05
- [ ] Test fixtures for mock user with/without specific permissions

*If none: "Existing infrastructure covers all phase requirements."*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Sidebar hides restricted nav items | AUTH-05 | Visual verification of nav state | Log in as restricted role, verify missing nav items |
| Toast error on direct URL navigation | AUTH-05 | UI feedback verification | Navigate directly to restricted route, verify toast + redirect |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending

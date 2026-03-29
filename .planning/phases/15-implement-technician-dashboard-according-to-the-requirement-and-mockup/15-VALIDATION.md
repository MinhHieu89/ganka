---
phase: 15
slug: implement-technician-dashboard-according-to-the-requirement-and-mockup
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-29
---

# Phase 15 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit (backend) / vitest (frontend) |
| **Config file** | `backend/src/Bootstrapper/Bootstrapper.csproj` / `frontend/vitest.config.ts` |
| **Quick run command** | `dotnet test --filter "Category=Technician"` / `npx vitest run --reporter=verbose src/features/technician` |
| **Full suite command** | `dotnet test` / `npx vitest run` |
| **Estimated runtime** | ~30 seconds (backend) / ~15 seconds (frontend) |

---

## Sampling Rate

- **After every task commit:** Run quick run command for modified area
- **After every plan wave:** Run full suite command
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 45 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 15-01-01 | 01 | 1 | D-01..D-05 | unit | `dotnet test --filter "TechnicianOrder"` | ❌ W0 | ⬜ pending |
| 15-01-02 | 01 | 1 | D-06 | unit | `dotnet test --filter "WorkflowStage"` | ❌ W0 | ⬜ pending |
| 15-02-01 | 02 | 1 | D-07..D-09 | integration | `dotnet test --filter "TechnicianDashboard"` | ❌ W0 | ⬜ pending |
| 15-02-02 | 02 | 1 | D-15 | unit | `dotnet test --filter "Concurrency"` | ❌ W0 | ⬜ pending |
| 15-03-01 | 03 | 2 | D-16 | component | `npx vitest run src/features/technician` | ❌ W0 | ⬜ pending |
| 15-03-02 | 03 | 2 | D-08,D-10 | component | `npx vitest run src/features/technician` | ❌ W0 | ⬜ pending |
| 15-03-03 | 03 | 2 | D-13 | component | `npx vitest run src/features/technician` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] Backend test stubs for TechnicianOrder entity and handlers
- [ ] Frontend test stubs for technician dashboard components
- [ ] Shared test fixtures for technician test data

*Existing xUnit and vitest infrastructure covers framework needs.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Dashboard role routing | D-16 | Visual role-based rendering | Login as Technician, verify dashboard renders |
| Slide-over panel | D-13 | Visual overlay behavior | Click "Xem kết quả", verify panel slides from right |
| Row styling (dimmed/pinned) | Spec | CSS visual verification | Check "Đang đo" pinned top, "Hoàn tất" dimmed |
| Polling real-time updates | D-17 | Timing-dependent behavior | Wait 15s, verify table refreshes |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 45s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending

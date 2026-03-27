---
phase: 14
slug: implement-receptionist-role-flow
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-28
---

# Phase 14 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.x (backend) / Vitest (frontend) |
| **Config file** | `backend/tests/Directory.Build.props` / `frontend/vitest.config.ts` |
| **Quick run command** | `dotnet test --filter "Category=Unit" --no-build` / `npx vitest run --reporter=verbose` |
| **Full suite command** | `dotnet test` / `npx vitest run` |
| **Estimated runtime** | ~45 seconds (backend) / ~20 seconds (frontend) |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test --filter "Category=Unit" --no-build` + `npx vitest run`
- **After every plan wave:** Run `dotnet test` + `npx vitest run`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 60 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| TBD | TBD | TBD | TBD | TBD | TBD | TBD | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

*Will be populated after PLAN.md files are created.*

---

## Wave 0 Requirements

- [ ] Backend test stubs for new Scheduling handlers (CheckIn, MarkNoShow, CancelAppointment, BookGuestAppointment)
- [ ] Backend test stubs for new Clinical handlers (CreateWalkInVisit, CancelVisit)
- [ ] Frontend test stubs for receptionist dashboard components
- [ ] Frontend test stubs for intake form validation
- [ ] Frontend test stubs for appointment booking page

*Existing test infrastructure covers framework setup — no new framework installation needed.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Role-based dashboard rendering | D-01 | Requires authenticated session with Receptionist role | Login as receptionist, verify dashboard shows queue view not clinical view |
| Polling real-time updates | D-04 | Requires time-based observation | Open dashboard, create appointment in another tab, wait 30s for KPI refresh / 15s for table refresh |
| Responsive breakpoints | D-19 | Visual layout verification | Resize browser to 1024px, 768px, and 600px widths, verify layout adapts |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 60s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending

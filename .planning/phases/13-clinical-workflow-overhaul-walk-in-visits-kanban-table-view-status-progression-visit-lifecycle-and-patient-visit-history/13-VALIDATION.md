---
phase: 13
slug: clinical-workflow-overhaul-walk-in-visits-kanban-table-view-status-progression-visit-lifecycle-and-patient-visit-history
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-25
---

# Phase 13 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit (backend) / Vitest (frontend) |
| **Config file** | `backend/src/Bootstrapper/Bootstrapper.csproj` / `frontend/vitest.config.ts` |
| **Quick run command** | `dotnet test --filter "Category=Clinical"` / `cd frontend && npx vitest run --reporter=verbose` |
| **Full suite command** | `dotnet test` / `cd frontend && npx vitest run` |
| **Estimated runtime** | ~60 seconds |

---

## Sampling Rate

- **After every task commit:** Run quick run command for affected layer (backend or frontend)
- **After every plan wave:** Run full suite command for both layers
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 60 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| *To be populated after planning* | | | | | | | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- Existing test infrastructure covers all phase requirements — xUnit and Vitest already configured.
- [ ] Backend integration test stubs for workflow stage reversal
- [ ] Backend integration test stubs for auto-advance on sign-off
- [ ] Frontend component test stubs for kanban 8-column layout
- [ ] Frontend component test stubs for table view toggle

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Drag-and-drop between kanban columns | D-01 to D-04 | @dnd-kit interactions require browser | Drag a patient card between columns, verify stage updates |
| Horizontal scroll on narrow viewport | D-03 | Layout behavior requires visual inspection | Resize browser to < 1200px, verify horizontal scroll appears |
| Timeline click updates detail panel | D-14 | User interaction flow | Click different visits in timeline, verify right panel updates |
| localStorage view preference persistence | D-05 | Browser storage behavior | Toggle view, refresh page, verify preference persisted |

*All other behaviors have automated verification.*

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 60s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending

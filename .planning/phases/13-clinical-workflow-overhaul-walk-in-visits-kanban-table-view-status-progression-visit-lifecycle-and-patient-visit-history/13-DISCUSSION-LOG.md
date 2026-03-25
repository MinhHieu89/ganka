# Phase 13: Clinical Workflow Overhaul - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-25
**Phase:** 13-clinical-workflow-overhaul
**Areas discussed:** Kanban columns & status mapping, Status reversal & progression rules, Visit completion lifecycle, Patient visit history tab

---

## Kanban Columns & Status Mapping

| Option | Description | Selected |
|--------|-------------|----------|
| 8 columns, one per stage | Each stage gets its own column. Scrollable if needed. | ✓ |
| 8 columns with collapsible empty ones | Empty columns auto-collapse to save space | |
| Configurable grouping | Let clinic staff configure which stages to group | |

**User's choice:** 8 columns, one per stage (Recommended)
**Notes:** None

---

### Horizontal Overflow

| Option | Description | Selected |
|--------|-------------|----------|
| Horizontal scroll | All 8 columns at comfortable width, horizontal scroll | ✓ |
| Compact columns that fit screen | Squeeze all 8 columns to fit without scrolling | |
| Sticky first + last columns | Reception and Pharmacy/Optical pinned, middle scrolls | |

**User's choice:** Horizontal scroll (Recommended)
**Notes:** None

---

### Table View

| Option | Description | Selected |
|--------|-------------|----------|
| Full patient list with sortable columns | Table with columns: Patient, Doctor, Stage, Wait Time, Visit Time, Status | ✓ |
| Grouped by stage with expandable rows | Table rows grouped under stage headers (accordion) | |
| Simple flat list with stage badges | Flat sortable table, stage shown as colored badge | |

**User's choice:** Full patient list with sortable columns
**Notes:** None

---

### View Toggle

| Option | Description | Selected |
|--------|-------------|----------|
| Toggle button in toolbar | Simple icon toggle (grid/list) in dashboard header | ✓ |
| Tabs above the board | Two tabs: 'Kanban' and 'Table' | |
| You decide | Claude picks the best UX pattern | |

**User's choice:** Toggle button in toolbar (Recommended)
**Notes:** None

---

## Status Reversal & Progression Rules

### Backward Movement

| Option | Description | Selected |
|--------|-------------|----------|
| Any stage can go back one step | Any stage can move backward by one step | |
| Specific stages allow reversal | Only certain transitions allow going back | ✓ |
| Free movement in any direction | Any stage can move to any other stage | |

**User's choice:** Specific stages allow reversal (Recommended)
**Notes:** User deferred specific transition selection to Claude — "You pick what makes sense based on research in real life clinic."

---

### Role Restrictions for Reversal

| Option | Description | Selected |
|--------|-------------|----------|
| Role-based | Only Doctor and Manager roles can reverse | |
| Any staff can reverse | Anyone who can advance can also reverse | |
| You decide | Claude picks based on clinic safety | ✓ |

**User's choice:** You decide
**Notes:** None

---

### Reason Required

| Option | Description | Selected |
|--------|-------------|----------|
| Yes, require reason | Staff must enter brief reason for reversal | ✓ |
| No reason needed | Just drag backward or click a button | |
| You decide | Claude picks based on audit requirements | |

**User's choice:** Yes, require reason (Recommended)
**Notes:** None

---

## Visit Completion Lifecycle

### Done Trigger

| Option | Description | Selected |
|--------|-------------|----------|
| After reaching last stage and sign-off | Both conditions required | |
| After doctor sign-off regardless of stage | Sign-off at any stage = complete | |
| After reaching last stage only | Auto-completes at PharmacyOptical(7) | ✓ |

**User's choice:** After reaching last stage only
**Notes:** None

---

### Auto-Advance on Sign-Off

| Option | Description | Selected |
|--------|-------------|----------|
| Yes, auto-advance on sign-off | Visit auto-moves to next stage after sign-off | ✓ |
| No, keep them separate | Sign-off and stage advancement are separate actions | |
| Configurable per stage | Some stages auto-advance, others don't | |

**User's choice:** Yes, auto-advance on sign-off (Recommended)
**Notes:** None

---

### Skip Stages

| Option | Description | Selected |
|--------|-------------|----------|
| Allow skipping stages | Staff can advance past stages that aren't needed | ✓ |
| Must pass through all stages | Every visit goes through all 8 stages sequentially | |
| You decide | Claude picks based on clinical best practices | |

**User's choice:** Allow skipping stages (Recommended)
**Notes:** None

---

### Done Column Visibility

| Option | Description | Selected |
|--------|-------------|----------|
| Show in Done column for current day | Completed visits stay until end of day | ✓ |
| Disappear immediately | Removed from kanban instantly | |
| Show for configurable duration | Stay for X hours (configurable) | |

**User's choice:** Show in Done column for current day
**Notes:** None

---

## Patient Visit History Tab

### Display Format

| Option | Description | Selected |
|--------|-------------|----------|
| Timeline view | Vertical timeline with visit cards, most recent at top | ✓ |
| Table view | Sortable table with columns | |
| Both timeline and table | Toggle between views | |

**User's choice:** Timeline view (Recommended)
**Notes:** None

---

### Click Action

| Option | Description | Selected |
|--------|-------------|----------|
| Navigate to full visit detail page | Clicks to /visits/{visitId} | |
| Expand inline within timeline | Accordion-style expansion | |
| Side panel overlay | Slide-in panel without navigation | |
| 2-column layout (Other) | Timeline on left, details on right | ✓ |

**User's choice:** 2-column layout — timeline on the left, details on the right (custom input)
**Notes:** User specifically wanted a master-detail layout within the same page, not navigation to a separate page.

---

### Timeline Card Info

| Option | Description | Selected |
|--------|-------------|----------|
| Compact summary | Date, doctor name, primary diagnosis, status badge | ✓ |
| Detailed summary | Date, doctor, all diagnoses, prescription/image counts, OSDI | |
| You decide | Claude picks the right detail level | |

**User's choice:** Compact summary (Recommended)
**Notes:** None

---

### Detail Panel Content

| Option | Description | Selected |
|--------|-------------|----------|
| Same sections, read-only | All sections from VisitDetailPage in read-only mode | ✓ |
| Condensed key info only | Only refraction, diagnoses, prescriptions, OSDI | |
| You decide | Claude picks based on clinical sense | |

**User's choice:** Same sections, read-only
**Notes:** None

---

## Claude's Discretion

- Which specific backward stage transitions are allowed
- Role-based restrictions for stage reversal
- Walk-in patient creation UX improvements
- Table view filtering and column widths
- Timeline card styling and spacing
- Done column day-based filtering logic

## Deferred Ideas

None — discussion stayed within phase scope.

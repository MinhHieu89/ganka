# Phase 14: Implement Receptionist Role Flow - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-27
**Phase:** 14-implement-receptionist-role-flow
**Areas discussed:** Dashboard layout & navigation, Check-in & visit creation flow, Appointment booking UX, Action menus & status transitions, Implementation conflicts

---

## Dashboard Layout & Navigation

| Option | Description | Selected |
|--------|-------------|----------|
| Separate route (Recommended) | New /receptionist/dashboard route with its own layout | |
| Role-based view on existing dashboard | Same /dashboard route but show different content based on role | ✓ |
| You decide | Claude picks the best approach | |

**User's choice:** Same /dashboard route, role-based rendering with a completely new view for receptionist role following the mockups.

---

| Option | Description | Selected |
|--------|-------------|----------|
| Table-only (Recommended) | Matches the mockup exactly. Queue-based workflow. | ✓ |
| Table + Kanban toggle | Reuse existing kanban/table toggle | |
| You decide | Claude picks | |

**User's choice:** Table-only

---

| Option | Description | Selected |
|--------|-------------|----------|
| Polling (Recommended) | 30s KPI, 15s table. Consistent with existing pattern. | |
| WebSocket / SignalR | Instant updates, new infrastructure needed | |
| You decide | Claude picks | |

**User's choice:** Polling with manual refresh button to get latest update

---

| Option | Description | Selected |
|--------|-------------|----------|
| Keep existing sidebar, add Receptionist item | Add new nav item | |
| Role-specific sidebar | Receptionist only sees limited items | |
| You decide | Claude decides | |

**User's choice:** Use existing sidebar item for /dashboard route, but render different view for different roles

---

## Check-in & Visit Creation Flow

| Option | Description | Selected |
|--------|-------------|----------|
| Dialog modals (Recommended) | Centered popup with overlay, matches mockup | ✓ |
| Sheet side-panels | Slide-in from right | |
| You decide | Claude picks | |

**User's choice:** Dialog modals
**Notes:** User initially asked for clarification on what "walk-in existing patient popup" meant. After explanation, selected dialog modals.

---

| Option | Description | Selected |
|--------|-------------|----------|
| Full intake form (Recommended) | Complete SCR-003 form pre-filled | ✓ |
| Missing fields only | Smaller dialog with just missing fields | |
| You decide | Claude picks | |

**User's choice:** Full intake form

---

| Option | Description | Selected |
|--------|-------------|----------|
| Collapsible sections (Recommended) | Required expanded, optional collapsed | |
| Always visible | All 4 sections visible | |
| You decide | Claude picks | |

**User's choice:** Collapsible sections but all expanded by default

---

| Option | Description | Selected |
|--------|-------------|----------|
| Auto-advance to Pre-Exam | Save + create visit + move to Pre-Exam | ✓ |
| Save as Waiting, navigate to Pre-Exam view | Stay in Waiting, navigate only | |
| You decide | Claude decides | |

**User's choice:** Auto-advance to Pre-Exam

---

## Appointment Booking UX

| Option | Description | Selected |
|--------|-------------|----------|
| Large dialog modal (Recommended) | Wide centered dialog matching mockup | |
| Full page route | Navigate to dedicated page | ✓ |
| You decide | Claude picks | |

**User's choice:** Full page route at /appointment/new

---

| Option | Description | Selected |
|--------|-------------|----------|
| Use ClinicSchedule hours + hardcode 30min slots | Read hours from config, hardcode slot duration | ✓ |
| Add slot duration to ClinicSchedule config | Fully configurable | |
| You decide | Claude picks | |

**User's choice:** Hardcode 30 min for now
**Notes:** User asked about existing clinic schedule config. Confirmed ClinicSchedule entity exists with per-day operating hours but no slot duration field. User chose to hardcode slot duration.

---

| Option | Description | Selected |
|--------|-------------|----------|
| Minimal record (Recommended) | Just name + phone on appointment | |
| Full registration during booking | Open full intake form | |

**User's choice:** Save data to Appointment record only. Full intake happens at check-in when they arrive, then create patient record. Overrides doc's "tạo patient record tạm".

---

Slot capacity question:

**User's choice:** Doctor-based capacity. If a doctor is selected, check that doctor's availability (1 patient per doctor per 30-min slot). If no doctor selected, free to book anytime.

---

## Action Menus & Status Transitions

| Option | Description | Selected |
|--------|-------------|----------|
| Dropdown menu (Recommended) | Three-dot menu at end of each row | ✓ |
| Inline icon buttons | Small action buttons in row | |
| You decide | Claude picks | |

**User's choice:** Dropdown menu

**Notes:** User asked to check the spec documents for cancel/no-show reason requirements rather than asking a question. Spec clearly defines: Cancel Appointment = required dropdown reason, Cancel Visit = required dropdown reason, No-Show = optional text note. All captured from docs, not asked.

---

## Additional Decisions

| Option | Description | Selected |
|--------|-------------|----------|
| Follow earlier decision | Store on Appointment, no patient record until check-in | ✓ |
| Follow the doc (temp patient record) | Create minimal patient at booking time | |

**User's choice:** Follow earlier decision — no patient record for guest bookings

---

| Option | Description | Selected |
|--------|-------------|----------|
| Implement keyboard shortcuts + auto-save | Full UX polish | |
| Defer to later phase | Core flows first | ✓ |
| Keyboard shortcuts only | Shortcuts quick, skip auto-save | |

**User's choice:** Defer to later phase

---

| Option | Description | Selected |
|--------|-------------|----------|
| Desktop only (Recommended) | Focus on desktop experience | |
| Desktop + tablet | Include tablet breakpoint | |
| Full responsive | All 3 breakpoints | ✓ |

**User's choice:** Full responsive

---

## Claude's Discretion

- Internal component structure within features/receptionist/
- API endpoint naming and DTO shapes (following existing patterns)
- Query invalidation strategy
- Receptionist 4-status to clinical 11-stage mapping logic

## Deferred Ideas

- Keyboard shortcuts and auto-save draft — polish phase
- Notification sounds — polish phase
- Per-doctor schedule filtering — mentioned in spec as "phase sau"
- SMS/Zalo reminders — v2

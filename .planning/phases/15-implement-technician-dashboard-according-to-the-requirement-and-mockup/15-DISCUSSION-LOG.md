# Phase 15: Implement Technician Dashboard - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-29
**Phase:** 15-implement-technician-dashboard-according-to-the-requirement-and-mockup
**Areas discussed:** Scope boundary, View results panel, Backend module placement, Assignment & concurrency, Visit status management, Data model design

---

## Scope Boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Dashboard + stub navigation | Build full dashboard. Navigate to placeholder Pre-Exam page. All backend logic works. | ✓ |
| Dashboard only, no navigation | Actions change status without navigating. | |
| Dashboard + full Pre-Exam screen | Build both dashboard and Pre-Exam measurement screen. | |

**User's choice:** Dashboard + stub navigation (Recommended)
**Notes:** Pre-Exam form is a separate phase.

### "Đo bổ sung" Sub-question

| Option | Description | Selected |
|--------|-------------|----------|
| Defer to later phase | Requires doctor-side EMR integration. | ✓ |
| Include basic support | Add badge and re-queue logic, doctor trigger manual/seeded. | |

**User's choice:** Defer to later phase (Recommended)

---

## View Results Panel

| Option | Description | Selected |
|--------|-------------|----------|
| Dialog modal | Consistent with receptionist pattern. Quick open/close. | |
| Slide-over side panel | Drawer from right. Keeps table partially visible. New pattern. | ✓ |
| Navigate to patient detail | Reuse existing patient detail page. Leaves dashboard. | |

**User's choice:** Slide-over side panel
**Notes:** More room for patient data while keeping queue visible.

---

## Backend Module Placement

| Option | Description | Selected |
|--------|-------------|----------|
| Clinical module | Pre-exam is clinical activity. Keeps Scheduling focused on appointments. | ✓ |
| Scheduling module | Follow receptionist pattern. Simpler reference but blurs domain boundary. | |
| Split across both | Dashboard in Scheduling, transitions in Clinical. Clean but more coordination. | |

**User's choice:** Clinical module (Recommended)

---

## Assignment & Concurrency

| Option | Description | Selected |
|--------|-------------|----------|
| Optimistic + toast | Backend validates on accept, returns error if taken. Frontend shows toast + refreshes. | ✓ |
| Pessimistic DB locking | Row-level lock or concurrency token. Bulletproof but complex. | |
| Real-time lock with SignalR | Broadcast "patient claimed" event. Most responsive but adds WebSocket infra. | |

**User's choice:** Optimistic + toast (Recommended)

---

## Visit Status Management (User-Initiated Deep Dive)

User asked: "How do we manage status of a Visit? What are current status list and what are going to be changed?"

**Analysis presented:**
- Current system has two concepts: VisitStatus (Draft/Signed/Amended/Cancelled) for immutability and WorkflowStage (11 stages) for clinical journey
- Receptionist maps WorkflowStage to 4 display statuses
- Technician needs 4 different display statuses derived from assignment + completion + red flag fields
- Key gaps: no technician assignment tracking, no red flag fields, no pre-exam timing

**User decision:** Rename `WorkflowStage.RefractionVA` → `PreExam`

---

## Data Model Design (User-Initiated Deep Dive)

User challenged initial proposal of PreExamSession entity: "pre exam only happens once, other additional exam can be ordered multiple times by doctor, so it's not semantic to keep table name PreExamSession"

**Evolution:**
1. Initial proposal: `PreExamSession` child entity for all pre-exam rounds → rejected (semantically wrong)
2. Revised proposal: Pre-exam fields on Visit + `TechnicianOrder` for additional exams → user asked about dashboard query complexity
3. Final proposal: Everything in `TechnicianOrder` with `OrderType` enum → user approved

**User's final decision:** Single `TechnicianOrder` table with `TechnicianOrderType` enum (`PreExam` / `AdditionalExam`). No pre-exam fields on Visit entity.

**Additional decision:** User specified enum must be stored as string in DB (`HasConversion<string>()`) instead of int, for human readability.

---

## Claude's Discretion

- Internal component structure within features/technician/
- API endpoint naming and DTO shapes
- Query invalidation strategy
- Slide-over panel implementation (shadcn Sheet or custom)
- Stub Pre-Exam page design
- i18n key structure

## Deferred Ideas

- Pre-Exam measurement form — separate phase
- "Đo bổ sung" creation flow — needs doctor UI (data model ready)
- Keyboard shortcuts — polish phase
- Notification sounds — polish phase

# Phase 14: Implement Receptionist Role Flow - Context

**Gathered:** 2026-03-27
**Status:** Ready for planning

<domain>
## Phase Boundary

Implement the complete receptionist workflow: role-based dashboard with patient queue, patient intake form, appointment booking, check-in flows, and dashboard action menus. The receptionist is the front-desk operator managing patient arrivals, walk-ins, phone bookings, and queue status throughout the day.

**5 screens from spec:**
- SCR-002a: Receptionist Dashboard (role-based view on `/dashboard`)
- SCR-003: Patient Intake Form (new patient walk-in)
- SCR-004: New Appointment page (`/appointment/new`)
- SCR-005: Check-in & Create Visit popups (2 flows)
- SCR-006: Dashboard Actions (context menus per status)

</domain>

<decisions>
## Implementation Decisions

### Dashboard Layout & Navigation
- **D-01:** Same `/dashboard` route, role-based rendering — detect Receptionist role and render a completely different view. No separate route.
- **D-02:** Existing sidebar Dashboard item used — no new sidebar entries. The dashboard item renders different content based on role.
- **D-03:** Table-only view for receptionist (no kanban/table toggle). Receptionist workflow is queue-based, not stage-progression.
- **D-04:** Polling with manual refresh button for real-time updates. No WebSocket. Polling intervals per spec: 30s for KPI cards, 15s for patient table.

### Check-in & Visit Creation Flow
- **D-05:** Dialog modals for all check-in and visit creation popups (Flow A: appointment check-in, Flow B: walk-in existing patient).
- **D-06:** Full intake form (SCR-003) opens when checking in incomplete patient ("Check-in & bổ sung hồ sơ"). Pre-filled with existing data.
- **D-07:** All 4 form sections (personal, exam, history, lifestyle) are collapsible but **all expanded by default**.
- **D-08:** "Lưu & Chuyển tiền khám" button auto-advances patient to Pre-Exam stage (skips Waiting queue).

### Appointment Booking UX
- **D-09:** Full page route at `/appointments/new` for appointment booking (not a dialog). *(Updated from singular `/appointment/new` to plural per REST convention — confirmed 2026-03-28)*
- **D-10:** Hardcode 30-min slot duration. Use existing ClinicSchedule entity for operating hours per day.
- **D-11:** For new patients (phone booking): store name + phone + reason **on the Appointment record** (GuestName, GuestPhone, GuestReason fields). Do NOT create a patient record until check-in. This overrides the doc's mention of "tạo patient record tạm".
- **D-12:** Slot capacity is doctor-based: if a doctor is selected, 1 patient per doctor per 30-min slot. If no doctor selected ("BS nào trống"), allow booking in any slot freely.

### Action Menus & Status Transitions
- **D-13:** Dropdown menu (three-dot ⋯) at end of each row. Actions vary by status per spec SCR-006.
- **D-14:** Cancel Appointment and Cancel Visit: required dropdown reason (per spec). No-Show: optional text note + optional "Đặt hẹn lại" checkbox (per spec).
- **D-15:** "Đặt hẹn lại" checkbox (on No-Show and Cancel Visit popups) navigates to `/appointments/new` with patient pre-filled. Consistent with D-09.

### Deferred to Later Phase
- **D-16:** Keyboard shortcuts (N, H, /, 1-4 on dashboard; Ctrl+S, Ctrl+Enter on forms) — deferred.
- **D-17:** Auto-save draft every 30 seconds on intake form — deferred.
- **D-18:** Notification sounds — deferred.

### Responsive Design
- **D-19:** Full responsive: all 3 breakpoints (desktop >1024px, tablet 768-1024px, mobile <768px) as specified in the docs.

### Claude's Discretion
- Internal component structure and file organization within features/receptionist/
- API endpoint naming and request/response DTO shapes (following existing patterns)
- Query invalidation strategy for polling + manual refresh
- How to map receptionist 4-status model to clinical 11-stage pipeline in queries

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Receptionist Spec Documents
- `docs/dev/receiptionist/receptionist-dashboard.md` — SCR-002a: Dashboard layout, KPI cards, table columns, status system, search, filter, real-time updates, edge cases
- `docs/dev/receiptionist/patient_intake_form.md` — SCR-003: 4-section intake form, field definitions, validation rules, duplicate detection, data flow to other modules
- `docs/dev/receiptionist/receptionist-new-appointment.md` — SCR-004: 2-column booking page, existing vs new patient flows, calendar/slot grid, confirmation bar, business rules
- `docs/dev/receiptionist/receptionist-checkin.md` — SCR-005: Check-in popup (complete vs incomplete patient), walk-in visit creation popup, Intake integration from check-in
- `docs/dev/receiptionist/receptionist-dashboard-actions.md` — SCR-006: Action matrix by status, reschedule/cancel/no-show/cancel-visit popups with full field specs

### Receptionist Mockup HTML Files
- `docs/dev/receiptionist/receptionist_dashboard.html` — Dashboard visual mockup
- `docs/dev/receiptionist/receptionist-checkin_popup_existing_patient.html` — Check-in popup (complete patient)
- `docs/dev/receiptionist/receptionist-checkin_popup_new_patient_incomplete.html` — Check-in popup (incomplete patient)
- `docs/dev/receiptionist/receptionist-checkin_walkin_existing_patient_popup.html` — Walk-in existing patient popup
- `docs/dev/receiptionist/receptionist-new-appointment_existing_patient.html` — Booking page (existing patient)
- `docs/dev/receiptionist/receptionist-new-appointment_new_patient.html` — Booking page (new patient)
- `docs/dev/receiptionist/patient_intake_form.html` — Intake form mockup
- `docs/dev/receiptionist/receptionist-dashboard-actions_cancel_and_noshow_popups.html` — Cancel & no-show popup mockups
- `docs/dev/receiptionist/receptionist-dashboard-actions_cancel_visit_popup.html` — Cancel visit popup mockup
- `docs/dev/receiptionist/receptionist-dashboard-actions_context_menus_by_status.html` — Context menu mockups
- `docs/dev/receiptionist/receptionist-dashboard-actions_reschedule_appointment_popup.html` — Reschedule popup mockup

### Backend Domain Entities (Must Read Before Modifying)
- `backend/src/Modules/Scheduling/Scheduling.Domain/Entities/Appointment.cs` — Current Appointment entity (needs new fields)
- `backend/src/Modules/Scheduling/Scheduling.Domain/Enums/AppointmentStatus.cs` — Current status enum (needs NoShow)
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs` — Visit aggregate (needs source, reason, cancel fields)
- `backend/src/Modules/Clinical/Clinical.Domain/Enums/VisitStatus.cs` — Visit status enum
- `backend/src/Modules/Patient/Patient.Domain/Entities/Patient.cs` — Patient entity
- `backend/src/Modules/Scheduling/Scheduling.Domain/Entities/ClinicSchedule.cs` — Clinic operating hours config
- `backend/src/Shared/Shared.Domain/Permissions.cs` — Permission constants

### Frontend Existing Patterns (Must Read Before Building)
- `frontend/src/app/routes/_authenticated/dashboard.tsx` — Current dashboard (needs role-based branching)
- `frontend/src/features/scheduling/api/scheduling-api.ts` — Existing scheduling API client
- `frontend/src/features/patient/hooks/usePatientSearch.ts` — Patient search hook (reusable)
- `frontend/src/features/clinical/components/WorkflowDashboard.tsx` — Clinical dashboard (reference for stage mapping)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- **usePatientSearch hook** — Debounced patient search with autocomplete, returns id/name/code/phone. Reuse for dashboard search and booking page.
- **shadcn/ui components** — Dialog, DropdownMenu, Badge, Calendar, Card, Table, Tabs, Button, Input, Select, Textarea all installed. Use directly.
- **TanStack Table** — Already used in clinical dashboard. Reuse for receptionist patient queue table.
- **React Hook Form + Zod** — Established form pattern. Use for intake form and booking page.
- **ClinicSchedule API** — `useClinicSchedule()` hook exists to fetch operating hours per day.
- **useDoctors() hook** — Doctor list for appointment booking doctor selector.
- **Permission guard** — `requirePermission()` in route beforeLoad. Reuse for receptionist permission checks.

### Established Patterns
- **API client**: OpenAPI Fetch with typed endpoints in `features/{module}/api/{module}-api.ts`
- **State management**: Zustand for global state, TanStack Query for server state
- **Form validation**: React Hook Form + Zod schemas
- **Modular monolith backend**: Vertical slice features (handler + validator + request/response), Wolverine bus
- **Entity naming**: PascalCase properties (e.g., `CheckedInAt`, `NoShowAt`, `CancelledBy`)

### Integration Points
- `/dashboard` route — Add role check to render receptionist vs clinical dashboard
- `/appointment/new` route — New route file in `_authenticated/appointments/`
- Scheduling module — Extend Appointment entity with new fields + new handlers (CheckIn, MarkNoShow, etc.)
- Clinical module — Extend Visit entity with source/reason/cancel fields + new handlers
- Auth module — Add Receptionist role with appropriate permissions

</code_context>

<conflicts>
## Implementation Conflicts to Resolve

### CRITICAL: Appointment Entity Missing Fields
Appointment.cs needs: `CheckedInAt`, `NoShowAt`, `NoShowBy`, `NoShowNotes`, `Source` (phone/web), `CancelledBy`, `GuestName`, `GuestPhone`, `GuestReason`. AppointmentStatus enum needs `NoShow` value. PatientId must become nullable (for guest bookings without patient record).

### CRITICAL: Visit Entity Missing Fields
Visit.cs needs: `Source` (walk-in/appointment enum), `Reason` (chief complaint separate from appointment reason), `CancelledReason`, `CancelledBy`. Visit.Cancel() currently takes no parameters — must accept reason and user ID.

### HIGH: Receptionist Status Mapping
Receptionist uses 4 statuses (Chưa đến/Chờ khám/Đang khám/Hoàn thành) but clinical workflow uses 11 stages. Must create a query-level mapping:
- Chưa đến = Appointment exists, no visit/check-in yet
- Chờ khám = Visit created, stage = Reception
- Đang khám = Visit stage >= PreExam
- Hoàn thành = Visit completed/signed

### HIGH: BookAppointment Requires PatientId
BookAppointment handler requires PatientId. Must allow nullable PatientId for guest bookings (D-11). Guest info stored on Appointment fields instead.

### MEDIUM: No Receptionist Role/Permissions
No explicit "Receptionist" role exists. Need to seed role with permissions: Patient.Create, Patient.View, Patient.Update, Scheduling.Create, Scheduling.View, Scheduling.Update, Clinical.View (admin data only).

</conflicts>

<specifics>
## Specific Ideas

### Color System from Spec
- Chưa đến (Not Arrived): Purple — bg: #EEEDFE, text: #3C3489
- Chờ khám (Waiting): Amber — bg: #FAEEDA, text: #633806
- Đang khám (Examining): Blue — bg: #E6F1FB, text: #0C447C
- Hoàn thành (Completed): Teal — bg: #E1F5EE, text: #085041
- Reschedule popup: Purple (#534AB7)
- No-Show popup: Amber (#BA7517)
- Cancel popups: Red (#A32D2D)
- Walk-in badge: Coral
- Appointment badge: Purple

### Data Flow: Intake → Clinical
Per SCR-003 section 6.4: Lý do khám → Pre-Exam Chief Complaint. Tiền sử bệnh → EMR History. Dị ứng → EMR allergy warnings. Screen time/contact lens → Pre-Exam Block A.

</specifics>

<deferred>
## Deferred Ideas

- Keyboard shortcuts (N/H// on dashboard, Ctrl+S/Ctrl+Enter on forms) — polish phase
- Auto-save draft on intake form — needs draft storage infrastructure
- Notification sounds for new appointments / status changes — polish phase
- Per-doctor schedule filtering on booking grid (spec mentions "phase sau")
- SMS/Zalo appointment reminders — separate phase (v2)
- Appointment booking limit config (currently hardcoded 3 months max) — admin settings phase

</deferred>

---

*Phase: 14-implement-receptionist-role-flow*
*Context gathered: 2026-03-27*

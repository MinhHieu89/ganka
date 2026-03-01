# Phase 2: Patient Management & Scheduling - Context

**Gathered:** 2026-03-01
**Status:** Ready for planning

<domain>
## Phase Boundary

Staff can register patients (medical and walk-in pharmacy), manage their profiles with allergies, search for patients, and book appointments on per-doctor calendars with double-booking prevention. Patients can self-book via a public page with staff confirmation required. This phase does NOT include clinical examination, billing, treatment protocols, or notification systems.

</domain>

<decisions>
## Implementation Decisions

### Patient Registration Flow
- Claude decides best approach for medical vs walk-in pharmacy distinction (single form with toggle vs separate entry points)
- After registration, redirect to patient profile — no special toast for the generated GK-YYYY-NNNN ID
- Mandatory fields (Address, CCCD) are hardcoded: always optional at registration, always required for referrals/legal export — no admin settings page
- Allergy entry is available inline during registration (optional section)
- Allergy input uses combo approach: autocomplete from predefined list + free-text for custom entries
- Each allergy has name + severity level (mild/moderate/severe)

### Patient Search
- Live autocomplete search — results appear as you type (debounced dropdown)
- Diacritics-insensitive matching for Vietnamese names (e.g. 'Nguyen' matches 'Nguyễn')
- Phone number search uses prefix matching (e.g. '0912' matches '0912345678')
- Global search bar in site header + dedicated Patient List page
- Global search shows typed results with icons (patient icon for patients, prepared for future entity types)
- When global search bar is focused before typing, show recent patients
- Recent patients also appear as a dashboard widget

### Patient List Page
- Shows paginated DataTable of all patients by default
- Basic filters beyond search: gender, has allergies (yes/no), date range
- Uses existing generic DataTable patterns from Phase 01.2

### Patient Profile
- Tabbed layout: header with patient name/ID/photo, then tabs (Overview, Allergies, Appointments, etc.)
- Optional photo upload with initials avatar fallback
- Show both DOB and calculated age, e.g. '15/03/1985 (40 tuổi)'
- Edit via "Edit" button that switches to form mode with Save/Cancel
- Soft-delete only (deactivate) — patients are never truly deleted, medical records must be preserved, staff can reactivate

### Allergy Display & Alerts
- Claude decides allergy alert approach for downstream workflows (persistent banner vs badge — prioritize patient safety)

### Appointment Calendar
- Default weekly view per doctor
- Claude decides single-doctor-at-a-time vs side-by-side columns (consider boutique clinic with few doctors)
- Click empty time slot → dialog for quick booking (search/select patient, choose type, confirm)
- Also available: "Book Appointment" button for full form booking without calendar context
- Double-booking prevention: occupied slots visually distinct (colored, showing patient name) + server-side validation error if attempted
- Color coding: background = appointment type (New Patient blue, Follow-up green, Treatment orange, Ortho-K purple), border/badge = status (Confirmed, Pending, Cancelled, Completed)
- Drag-and-drop rescheduling with double-booking validation
- Clinic operating hours visually grayed out (Mon closed, Tue-Fri 13-20h, Sat-Sun 8-12h)
- Appointment cancellation requires mandatory reason (Patient no-show, Patient request, Doctor unavailable, Other)
- Appointment durations configurable by type (defaults: new 30min, follow-up 20min, treatment 30-45min, Ortho-K 60-90min)

### Patient Self-Booking
- Separate public URL (e.g. /book) — no login required, shareable as link or QR code
- Zalo integration is link sharing only (no Zalo API/Mini App) — patients open booking link in browser
- Claude decides self-booking form fields (balance friction vs usefulness)
- Claude decides staff confirmation workflow (notification + approve/reject vs auto-confirm)
- Claude decides whether patients can choose a specific doctor or just time slots
- Claude decides whether to include a status check page (reference number URL)
- Bilingual: Vietnamese + English
- Fully branded with Ganka28 logo, clinic colors, address, phone number
- Confirmation page shown after submission (no SMS/Zalo notifications in this phase)
- Anti-spam: rate limit to max pending bookings per phone number

### Claude's Discretion
- Medical vs walk-in form distinction approach
- Allergy alert design for downstream workflows (prioritize patient safety)
- Calendar single-doctor vs multi-doctor column layout
- Self-booking form fields
- Staff confirmation workflow for self-bookings
- Doctor selection on public booking page
- Booking status check page
- Whether to include a Notes tab on patient profile
- Appointment history display on profile (past + upcoming vs separate tabs)

</decisions>

<specifics>
## Specific Ideas

- Global search bar should show recent patients when focused (before typing) — both on dashboard widget and in search dropdown
- Existing DataTable component from Phase 01.2 should be reused for patient list
- Existing dashboard-01 layout and design system should be followed
- Vietnamese diacritics support from Phase 01.2-07 should be leveraged for search

</specifics>

<deferred>
## Deferred Ideas

- Insurance information (provider, policy number) — Phase 7: Billing & Finance
- Contact preferences (Phone/Zalo/Email for reminders) — future notification feature
- SMS/Zalo appointment notifications — future notification feature
- Zalo Mini App integration — future enhancement
- Patient notes/comments tab — Claude may include if scope fits, otherwise defer

</deferred>

---

*Phase: 02-patient-management-scheduling*
*Context gathered: 2026-03-01*

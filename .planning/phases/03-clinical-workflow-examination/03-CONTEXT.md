# Phase 3: Clinical Workflow & Examination - Context

**Gathered:** 2026-03-04
**Status:** Ready for planning

<domain>
## Phase Boundary

Doctors can conduct a complete clinical visit with structured examination data, ICD-10 diagnosis, and immutable visit records. This includes: visit creation from appointment check-in or walk-in, workflow tracking via Kanban dashboard (patient journey through clinic stages), refraction recording (SPH, CYL, AXIS, ADD, PD, VA, IOP, Axial Length per eye with manifest/autorefraction/cycloplegic types), ICD-10 diagnosis with laterality enforcement, visit sign-off (immutable records), and amendment workflow for corrections.

This phase does NOT include: prescriptions (Phase 5), Dry Eye template (Phase 4), medical imaging (Phase 4), treatment protocols (Phase 9), billing (Phase 7), or pharmacy (Phase 6).

</domain>

<decisions>
## Implementation Decisions

### Workflow Dashboard
- Kanban board layout with columns for each workflow stage
- Patient cards show: name, appointment time, assigned doctor, wait time, allergy warning icon
- Click card to open visit details
- Drag-and-drop on desktop + action button on each card for advancing stages
- Claude's discretion on whether to show all 8 stages or group into 4-5 (pick what works best for a small boutique clinic with 2 doctors)

### Visit Creation & Lifecycle
- Visit created from appointment check-in: staff clicks "Check in" on confirmed appointment, creates Visit record, moves patient to Reception on Kanban
- Walk-in visits: staff creates manually via "New Visit" button (select existing patient or register new)
- Visit detail page is a single scrollable page with collapsible card sections (not tabbed), like a medical chart
- Sections: Patient Info, Refraction, Examination Notes (free-text), Diagnosis, Sign-off

### Visit Sign-off & Immutability
- "Sign Off Visit" button at bottom of visit page
- Confirmation dialog explains consequences: "This will lock the record. Corrections will require an amendment."
- Doctor confirms to finalize — no digital signature required
- After sign-off, all fields become read-only

### Amendment Flow
- Doctor clicks "Amend" on a signed visit
- Fields become editable again
- Must enter mandatory reason for amendment before saving
- System auto-captures field-level diff (old/new values), who amended, and when
- Original record preserved via amendment chain

### Refraction Data Entry
- Side-by-side OD (right eye) / OS (left eye) layout — standard ophthalmology convention
- Tabs per refraction type: Manifest | Autorefraction | Cycloplegic
- Each tab has its own OD/OS side-by-side form
- Only filled tabs get saved — indicator (*) on tabs that have data
- Fields per eye: SPH, CYL, AXIS, ADD, PD
- Visual Acuity: decimal notation (0.1 to 2.0), Vietnamese standard
- Both UCVA (uncorrected) and BCVA (best-corrected) recorded per eye
- IOP per eye with method notation
- Axial Length per eye

### ICD-10 Diagnosis Interface
- Combobox with search (Popover/Command pattern, same as allergy selector)
- Search by code or description in Vietnamese/English
- Show code + bilingual description in results
- Multiple diagnoses per visit allowed
- Per-doctor favorites: star icon on each result to toggle, pinned codes appear at top of search results
- Laterality enforcement: when selecting a code with RequiresLaterality=true, auto-prompt inline selector for OD/OS/OU. If OU selected, system adds two records (.1 right + .2 left). Block unspecified eye (.9)
- Primary/secondary diagnosis designation: first diagnosis added marked "Primary", subsequent ones "Secondary". Doctor can reorder or change primary designation

### Claude's Discretion
- Kanban stage grouping (all 8 vs grouped 4-5)
- Examination notes section structure (free-text vs semi-structured)
- Exact refraction input validation ranges
- IOP measurement method options (Goldmann, Non-contact, etc.)
- Loading states and error handling
- Kanban real-time update mechanism

</decisions>

<specifics>
## Specific Ideas

- ICD-10 codes already seeded in Phase 1 with bilingual support (English + Vietnamese) and RequiresLaterality flag — reuse ReferenceDbContext
- Workflow stages match the clinic's actual patient journey: reception, refraction/VA, doctor exam, diagnostics, doctor reads, Rx, cashier, pharmacy/optical
- Allergy warning icon on Kanban cards is critical for patient safety — doctor must see at a glance if patient has allergies before examination
- Visit detail page should feel like a medical chart — single scrollable document, not a fragmented tabbed UI

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- **PatientProfilePage**: Tab-based detail page pattern — adapt for scrollable visit page
- **Allergy Combobox**: Popover + Command pattern for category-grouped search — reuse for ICD-10 diagnosis selector
- **handleServerValidationError**: RFC 7807 error handler — ready for all new forms
- **ServerValidationAlert**: Non-field error display component
- **DataTable**: Generic table component — could use for visit history lists
- **DatePicker**: Date/time selection — for visit timestamps
- **Field/FieldLabel/FieldError**: Form field layout components
- **Card**: shadcn/ui Card component — for collapsible visit sections and Kanban patient cards
- **Skeleton**: Loading state components

### Established Patterns
- **Aggregate Root + IAuditable**: All new entities follow Entity/AggregateRoot base with audit tracking
- **Wolverine handlers**: Static handler classes with FluentValidation, Result<T> return
- **Repository + UnitOfWork**: Per-module DbContext with schema isolation
- **Minimal API endpoints**: MapGroup with RequireAuthorization, bus.InvokeAsync pattern
- **IoC.cs per layer**: Static extension methods for DI registration
- **React Hook Form + Zod**: Form validation with Controller pattern
- **TanStack Query**: Query hooks with queryKey factories, mutation with cache invalidation
- **openapi-fetch**: Typed API client with auth middleware
- **Enum normalization**: Backend int enums <-> frontend string maps

### Integration Points
- **Patient module**: Visit references PatientId (Guid). Denormalize patient name on Visit entity (same as Appointment pattern)
- **Scheduling module**: Visit links to AppointmentId. Check-in on appointment creates visit
- **ICD-10 reference data**: Shared ReferenceDbContext with Icd10Code entity (Code, DescriptionEn, DescriptionVi, RequiresLaterality, IsFavorite)
- **Audit system**: AuditInterceptor auto-tracks changes on IAuditable entities
- **Auth system**: Doctor/Technician roles determine who can record refraction vs sign off visits
- **Clinical module scaffold**: ClinicalDbContext already registered in Program.cs, empty module structure exists

</code_context>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 03-clinical-workflow-examination*
*Context gathered: 2026-03-04*

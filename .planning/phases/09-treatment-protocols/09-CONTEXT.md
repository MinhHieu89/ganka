# Phase 9: Treatment Protocols - Context

**Gathered:** 2026-03-05
**Status:** Ready for planning

<domain>
## Phase Boundary

Doctors can create and manage IPL/LLLT/lid care treatment packages with session tracking, OSDI monitoring per session, and configurable business rules. This includes: global protocol templates with per-patient customization, structured device parameters per treatment type, session recording with OSDI and clinical observations, configurable minimum interval enforcement, multiple concurrent courses per patient, mid-course modification with version history, treatment type switching, and cancellation with manager-approved refund deductions. Consumables used per session are tracked and auto-deducted from the consumables warehouse.

This phase does NOT include: billing/payment collection (Phase 7 — invoicing and 50/50 split enforcement live there), pharmacy drug dispensing (Phase 6), optical orders (Phase 8), or Myopia Control template (post-launch).

</domain>

<decisions>
## Implementation Decisions

### Package Creation & Protocol Templates
- Global protocol templates: doctors configure reusable templates (e.g., "Standard IPL 4-session", "LLLT 6-session dry eye") with default parameters and pricing
- When assigning to a patient, doctor can customize session count, parameters, and pricing — template is starting point, not rigid
- Packages can be created from both the visit detail page (post-exam) and the patient profile Treatments tab — flexibility for different workflows
- Template defines default package price and per-session price, overridable per patient when creating the package
- Only users with Doctor role can create or modify protocol templates and treatment packages (TRT-10)

### Treatment-Specific Structured Parameters
- Each treatment type has its own defined fields captured at the protocol/template level and per session:
  - **IPL**: energy (J/cm²), pulse count, spot size, treatment zones
  - **LLLT**: wavelength (nm), power (mW), duration (min), treatment area
  - **Lid care**: procedure steps/checklist, products used, duration
- Parameters on the template serve as defaults; actual values recorded per session can differ (doctor adjusts based on patient response)
- Structured fields enable cross-session comparison and treatment effectiveness analysis

### Session Documentation
- Treatment sessions are standalone by default (own record in Treatment module, not a Visit child entity)
- Optional visit link: if the patient also has a clinical exam that day, the session can be linked to that visit's VisitId
- Per session records: device parameters used (structured per type), OSDI score, clinical observations (freeform text), optional photos, consumables used
- Either doctor or technician can record sessions — system logs who performed it (PerformedBy)
- OSDI capture: both inline recording during session AND patient self-fill via QR/link (reuses Phase 4 public OSDI page pattern)
- Session is immutable after completion (similar to visit sign-off pattern)

### Treatment Monitoring UI
- Dedicated `/treatments` page showing all active packages across patients — staff landing page for treatment management
- Patient profile gets a "Treatments" tab showing that patient's packages (active, completed, cancelled)
- "Due Soon" section at top of `/treatments` page: surfaces sessions whose minimum interval has passed — proactive scheduling reminders
- Treatments page list/table style: Claude's discretion (DataTable with filters or Kanban by status)
- Package detail progress visualization: Claude's discretion (timeline with OSDI trend, card grid per session, or combination)

### Mid-course Changes & Cancellation
- Version history for protocol modifications: each change creates a version snapshot with what changed, who changed it, when, and reason — doctor can review full modification trail
- Treatment type switching (TRT-08): Claude's discretion on approach (close old package as "Switched" + create new, or convert in-place) — pick what fits data model and billing best
- Cancellation workflow: doctor/staff requests cancellation → status changes to "Pending Cancellation" → appears in approval queue → manager approves/rejects
- Manager approval via dedicated approval queue page (not inline) — provides clear audit trail and separation of duties
- Cancellation deduction percentage (10-20%): Claude's discretion (per treatment type or global setting)

### Interval Enforcement
- Configurable minimum intervals per treatment type: IPL 2-4 weeks, LLLT 1-2 weeks, lid care 1-2 weeks (per TRT-05)
- Intervals set on the protocol template, adjustable per patient package
- System warns but allows override when scheduling a session before minimum interval — warning + confirmation with reason logged
- "Due Soon" on treatments page uses these intervals to surface eligible sessions

### Auto-completion & Status Lifecycle
- Package statuses: Active, Paused, Pending Cancellation, Cancelled, Switched, Completed
- Auto-marks as "Completed" when all sessions are done (TRT-04)
- Multiple concurrent active packages per patient supported (TRT-06)

### Claude's Discretion
- Treatments page layout (DataTable vs Kanban)
- Package detail progress visualization (timeline vs cards vs hybrid)
- Treatment type switching data model approach (close+create vs convert in-place)
- Cancellation deduction scope (per-type vs global)
- Consumable selection UI during session recording
- Session photo upload UX (reuse Phase 4 image upload pattern or simpler inline)
- Protocol template management page layout
- Default interval values and configuration UI
- Loading states and error handling

</decisions>

<specifics>
## Specific Ideas

- IPL/LLLT/lid care are the clinic's core differentiator — structured data tracking enables treatment effectiveness reporting (OSDI trends across sessions)
- Protocol templates save doctors time since most patients get the same standard protocols; customization handles exceptions
- "Due Soon" reminders are important because treatment intervals are clinically significant — too short risks tissue damage, too long reduces efficacy
- Version history for modifications matters because mid-course changes affect billing (remaining sessions, pricing adjustments) and clinical accountability
- Approval queue for cancellations matches the discount/refund approval pattern decided in Phase 7 — consistent manager oversight workflows
- Session recording by technicians is practical because in a small clinic, the doctor may be seeing other patients while the technician performs the treatment

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- **TreatmentDbContext**: Scaffolded with "treatment" schema, empty — ready for entities
- **PermissionModule.Treatment**: Already exists in Auth with all 6 actions seeded (View, Create, Update, Delete, Export, Manage). Doctor role has Treatment.Manage
- **Sidebar nav**: "Treatments" entry exists (`disabled: true`, icon: IconHeartbeat, route: /treatments). i18n keys in both EN/VI locales
- **OsdiSubmission + GetOsdiHistoryQuery**: Phase 4 OSDI entities and contracts fully implemented — reuse for treatment session OSDI recording
- **Public OSDI page**: Patient self-fill via QR/token — reuse pattern for treatment session OSDI capture
- **VisitSection**: Collapsible card wrapper — use for treatment section on visit detail page
- **DataTable**: Generic table component — use for treatment package list, session list, approval queue
- **QuestPDF**: Already integrated for document generation — available for treatment progress reports
- **handleServerValidationError**: RFC 7807 error handler — reuse for treatment forms
- **Patient photo upload**: FormData + raw fetch pattern — reuse for session photos

### Established Patterns
- **Per-feature vertical slices**: Command/Handler with FluentValidation, Result<T> return
- **Minimal API endpoints**: MapGroup with RequireAuthorization, bus.InvokeAsync pattern
- **Domain events + Wolverine FX**: For cross-module communication (TreatmentSessionCompletedEvent → Pharmacy deducts consumables, → Billing adds charge)
- **Transactional outbox**: Wolverine FX with SQL Server transport — no lost messages on crash
- **Visit aggregate child pattern**: EnsureEditable() guard, private backing fields, UsePropertyAccessMode(Field) — but treatment sessions are NOT Visit children (independent entities with optional VisitId reference)
- **Cross-module queries**: Via Contracts project (e.g., Treatment queries Clinical.Contracts for OSDI history)
- **Public API pattern**: /api/public/ routes without RequireAuthorization, with RequireRateLimiting (for OSDI self-fill)
- **React Hook Form + Zod**: zodResolver with validation, Controller pattern
- **TanStack Query**: queryKey factories, mutation with cache invalidation
- **onError toast pattern**: All React Query mutations must have onError with toast.error

### Integration Points
- **Treatment.Presentation**: Needs to be created (not scaffolded yet) and wired into Bootstrapper + Program.cs
- **Clinical module**: Query OSDI history via Clinical.Contracts GetOsdiHistoryQuery for trend charts on treatment packages
- **Pharmacy module**: Domain event TreatmentSessionCompletedEvent consumed by Pharmacy to auto-deduct consumables (Phase 6 must implement handler)
- **Billing module**: Domain event for treatment charges; billing enforces 50/50 split payment rule (Phase 7)
- **Patient profile page**: Add "Treatments" tab to patient detail page
- **Visit detail page**: Add treatment session section (for linked sessions)
- **Sidebar navigation**: Change `disabled: true` to `false` on treatments nav item, add permission guard
- **Permission system**: PermissionModule.Treatment already seeded — use Treatment.View, Treatment.Create, Treatment.Manage in endpoint authorization
- **i18n**: New treatment.json translation files (EN/VI) for treatment-specific labels
- **Approval queue**: New pattern — reusable for Phase 7's discount/refund approval (or shared component if Phase 7 implements it first)

</code_context>

<deferred>
## Deferred Ideas

- Myopia Control treatment protocols — post-launch (same template engine, different structured fields)
- Ortho-K follow-up auto-scheduling — v2 notification feature (NTF-03)
- Treatment effectiveness reporting dashboards (OSDI improvement trends across patient cohorts) — v2 (RPT-06)
- Per-patient treatment progress report export — v2 (RPT-07)
- Zalo OA treatment reminders — v2 (NTF-03)

</deferred>

---

*Phase: 09-treatment-protocols*
*Context gathered: 2026-03-05*

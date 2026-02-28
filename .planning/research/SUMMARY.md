# Project Research Summary

**Project:** Ganka28 — Ophthalmology Clinic Management System
**Domain:** Boutique HIS + Pharmacy + Optical Center + Chronic Disease Tracking (Vietnam)
**Researched:** 2026-02-28
**Confidence:** HIGH (stack and features), HIGH/MEDIUM (architecture), MEDIUM-HIGH (pitfalls)

## Executive Summary

Ganka28 is a specialized hospital information system for a boutique ophthalmology clinic in Vietnam, covering patient management, clinical workflows, pharmacy, optical center, billing, and chronic disease tracking (starting with Dry Eye). Unlike generic clinic software, the primary differentiator is structured, queryable, longitudinal tracking of ophthalmic measurements — OSDI scores, TBUT, Schirmer tests, and Meibomian gland grading — which enables visit-over-visit trend analysis and treatment effectiveness reporting. This is not a side feature; it is the reason the system exists. Every architectural and domain modeling decision must protect the integrity of this structured clinical data.

The recommended approach is a modular monolith on .NET 9 + ASP.NET Core, with WolverineFx as the all-in-one mediator, message bus, and transactional outbox, backed by SQL Server with schema-per-module isolation. The frontend is TanStack Start (React 19 + Vite) with shadcn/ui components. This architecture gives clean module boundaries that could evolve into microservices, reliable cross-module messaging without external infrastructure, and a straightforward deployment profile (single Azure App Service) appropriate for the clinic's scale of ~8 concurrent staff users. The entire stack was user-decided with verified versions; no major technology choices remain open.

The critical risks are all data-model level and must be addressed in Phase 1, before any clinical data is entered. First, ophthalmic data must be stored in typed, structured domain value objects — not generic key-value tables — or the trend analysis and reporting features become impossible without a rewrite. Second, ICD-10 coding must enforce laterality (OD/OS/OU) from Day 1 to satisfy the So Y Te regulatory connection deadline of 31 December 2026. Third, medical records must be truly immutable using an amendment-chain pattern (not soft-deletes), as retrofitting this later requires a rewrite-level effort estimated at 6-8 weeks. These three pitfalls share a common theme: the cost to fix after launch is extreme, but the cost to build correctly from the start is low.

## Key Findings

### Recommended Stack

The backend is .NET 9 / ASP.NET Core 9 with WolverineFx 5.16.4 as the core message bus and mediator, EF Core 9 with schema-per-module isolation on SQL Server 2022, and Azure Blob Storage for medical images. WolverineFx replaces the combination of MediatR + MassTransit + a separate outbox library, providing transactional outbox/inbox, saga support, and SignalR transport in a single dependency. The frontend is TanStack Start 1.163.2 (RC stage but API-stable), React 19, and shadcn/ui with Tailwind CSS v4. Key supporting libraries are Riok.Mapperly (source-generated, replaces now-commercial AutoMapper), QuestPDF for document generation, and react-i18next for bilingual Vietnamese/English UI.

The stack has important "avoid" items: Swashbuckle is deprecated (use Microsoft.AspNetCore.OpenApi + Scalar instead), AutoMapper is now commercial (use Mapperly), the old @tanstack/start npm package is deprecated (use @tanstack/react-start), and Moq must be pinned to >=4.20.2 to avoid the SponsorLink telemetry controversy. Version alignment is non-negotiable for EF Core (all Microsoft.EntityFrameworkCore.* packages must match exactly) and WolverineFx core packages (all must be 5.16.4).

**Core technologies:**
- **.NET 9 / ASP.NET Core 9:** Runtime and Web API — user-decided, STS release, EF Core 9 compatible
- **WolverineFx 5.16.4:** All-in-one mediator + message bus + transactional outbox + saga engine — replaces MediatR + MassTransit, actively maintained
- **EF Core 9.0.12 + SQL Server 2022:** ORM and primary database — schema-per-module isolation via `HasDefaultSchema()`
- **TanStack Start 1.163.2:** Full-stack React framework with SSR + SPA, type-safe routing — user-decided
- **shadcn/ui + Tailwind CSS v4:** Component library (copy-paste model, not npm dependency) — user-decided (Maia style, Stone base, Green theme)
- **Azure Blob Storage (Azure.Storage.Blobs 12.27.0):** Medical image storage with SAS token access
- **Zalo OA API v3 (direct REST, no SDK):** Patient notifications — user-decided for Vietnamese market
- **QuestPDF 2026.2.2:** Prescription and invoice PDF generation — free community license for <$1M revenue
- **Riok.Mapperly 4.3.1:** DTO/entity mapping via compile-time source generation — replaces AutoMapper

### Expected Features

The clinic cannot open without 18 core features covering authentication/RBAC, patient registration, appointment scheduling, clinical visit workflow, Dry Eye exam template (OSDI/TBUT/Schirmer/Meibomian), refraction data recording, medical image upload, drug and glasses prescription writing, ICD-10 diagnosis lookup, pharmacy inventory + dispensing, optical frame inventory + order tracking, unified billing with multiple payment methods, and document printing.

The primary differentiators (not available in generic Vietnam HIS or generic international EHR) are structured Dry Eye tracking with visit-over-visit metric comparison, medical image side-by-side comparison across visits, IPL/LLLT treatment packages with per-session OSDI tracking and minimum interval enforcement, and a VIP membership program with family linking. These features are what justify building a custom system rather than buying an off-the-shelf solution.

**Must have (table stakes — P1):**
- Authentication + RBAC (6 roles: Doctor, Technician, Nurse, Cashier, Optical Staff, Manager)
- Patient registration with two-tier model (full clinical vs. light pharmacy walk-in)
- Appointment scheduling with no-double-booking enforcement
- Visit/encounter management with state-machine workflow status tracking
- Dry Eye exam template (OSDI questionnaire + TBUT + Schirmer + Meibomian scoring per eye)
- Refraction data recording (SPH/CYL/AXIS/ADD/PD/VA/IOP/Axial Length, per eye, typed)
- Medical image upload and viewing with structured metadata
- Drug and glasses prescription writing
- ICD-10 diagnosis lookup with laterality enforcement
- Pharmacy inventory management (batch tracking, expiry, minimum stock alerts)
- Pharmacy prescription dispensing linked to HIS prescriptions
- Optical frame inventory with barcode; glasses order tracking (full lifecycle)
- Unified billing with QR/cash/card payment methods; invoice printing
- Prescription and invoice printing (Circular 26/2025 compliant formats)
- Field-level audit trail for all medical records
- Bilingual UI (Vietnamese primary, English secondary)

**Should have (competitive differentiators — P2):**
- Visit-over-visit metric comparison (OSDI/TBUT/Schirmer trends)
- Medical image side-by-side comparison across visits
- IPL/LLLT treatment packages with session tracking and per-session OSDI
- Minimum interval enforcement between treatment sessions
- VIP membership program with tier-based auto-upgrade and family linking
- Zalo OA appointment reminders and post-visit summaries
- Revenue dashboard by department

**Defer (v2+):**
- Myopia Control exam template (uses same template engine — build after Dry Eye is validated)
- Axial length growth charts (age-adjusted normative data)
- Ortho-K follow-up protocol auto-generation
- Treatment effectiveness cohort reporting (needs months of data first)
- Device integration (DICOM, slit lamp auto-import) — architecture must be DICOM-ready
- MISA API auto-sync (manual Excel export is Phase 1; API in Phase 2)
- BHYT insurance integration (only if clinic decides to participate)
- Online patient portal / self-booking

### Architecture Approach

The system is a modular monolith with 6 business modules (HIS, Treatment Protocols, Pharmacy, Optical Center, Finance, Reporting) plus shared kernel and cross-cutting services (Auth, Notifications). Each module follows Clean Architecture with 4 projects (Domain, Application, Infrastructure, Contracts) where only the `.Contracts` project crosses module boundaries. HIS is the upstream context from which all other modules receive integration events via Wolverine's transactional outbox. Modules must never JOIN across schema boundaries, never call each other's domain/application layers, and never share mutable state. The Reporting module is a pure CQRS projection layer with no domain logic.

**Major components:**
1. **HIS (Core upstream context)** — Patient registration, visits, medical records, appointments, refraction, ICD-10 diagnoses, medical images, allergy tracking
2. **Treatment Protocols** — IPL/LLLT packages, session tracking, OSDI per session, minimum interval enforcement, Wolverine sagas for package lifecycle
3. **Pharmacy** — Drug catalog, inventory, batch/expiry tracking, prescription dispensing, walk-in OTC sales
4. **Optical Center** — Frame/lens catalog with barcode, glasses orders (state machine), contact lens, warranty, combo pricing
5. **Finance** — Unified billing, payment processing (VNPay/MoMo/ZaloPay), VIP membership, discounts, MISA export
6. **Reporting** — Full CQRS projections, revenue dashboards, OSDI trend reports, Excel exports
7. **Shared Kernel** — Value objects: `Money`, `PatientId`, `RefractionReading`, `OSDIScore`, common enums
8. **Auth (cross-cutting)** — ASP.NET Identity, JWT, granular RBAC permissions per role
9. **Notifications (cross-cutting)** — Zalo OA REST client, outbox-backed reliable delivery, delivery status tracking

Key patterns: Wolverine handler-per-message (no service layer), integration events with transactional outbox for all cross-module communication, Wolverine sagas for multi-step workflows (treatment packages, glasses order lifecycle), mixed CQRS (light for operational modules, full CQRS only for Reporting).

### Critical Pitfalls

1. **Generic data model for ophthalmic data** — Store all ophthalmic measurements (SPH, CYL, TBUT, OSDI, IOP, etc.) as typed domain value objects with numeric ranges and laterality enforcement from Day 1. A generic `ExamField(Name, Value)` table makes trend analysis and comparison features impossible without a full rewrite.

2. **ICD-10 laterality failures** — Force the doctor to select OD/OS/OU before accepting any ICD-10 code that requires laterality (the 6th character in ophthalmology ICD-10 codes). Never allow unspecified variants when laterality is known. This is data readiness for the So Y Te deadline of 31/12/2026.

3. **Immutable records implemented as soft-delete** — Medical records must use an amendment-chain pattern where the original is sealed on doctor sign-off and corrections create new `AmendmentRecord` entities referencing the original. No UPDATE statements on medical record tables. Build this in Phase 1; retrofitting costs 6-8 weeks.

4. **Vietnamese regulatory compliance deferred** — Prescription data model must include Circular 26/2025 required fields (patient CCCD, INN generic drug names, digital signature placeholder) from Day 1. Contact So Y Te for API specs early; the 31/12/2026 deadline means testing must complete by October 2026.

5. **Appointment double-booking from race conditions** — A unique database constraint on `(DoctorId, SlotDate, SlotTime)` is the only reliable prevention of concurrent booking conflicts. Application-level checks are insufficient. Include this in the initial migration; adding it later risks data conflicts.

6. **Chronic disease tracking with inconsistent measurement protocols** — OSDI must be computed from the 12-question validated questionnaire (stored as individual responses with version), not entered as a free-form number. Enforce units and method on TBUT (seconds) and Schirmer (mm, 5-minute standard). This is the clinic's primary differentiator and must be built with data integrity as the top priority.

7. **Medical images as generic file attachments** — Model `MedicalImage` as a first-class entity with typed metadata: `ImageType` enum, `Eye` (Right/Left), `AcquisitionDate`, `VisitId`, `PatientId`. Use thumbnail generation on upload. Generate short-lived SAS tokens (15-30 min) for full-resolution access. The visit-over-visit comparison feature depends entirely on this structure.

## Implications for Roadmap

Based on the dependency analysis across all four research files, the architecture's build order recommendation, and the critical pitfalls requiring early prevention, 5 phases are recommended:

### Phase 1: Foundation and Infrastructure

**Rationale:** Auth, SharedKernel, and the modular monolith infrastructure scaffolding are prerequisites for every other phase. The database schema-per-module design, Wolverine configuration, and HIS domain model (especially typed ophthalmic value objects and amendment-chain pattern) must be locked in before any clinical data enters the system. Three of the seven critical pitfalls (generic data model, immutable records, ICD-10 laterality) are fully preventable in this phase and extremely expensive to fix after launch.

**Delivers:** Deployable skeleton with staff login, patient registration, and appointment scheduling. Staff can log in, create patients, and book appointments. All module DbContexts scaffolded with correct schemas.

**Addresses features:** Authentication + RBAC, Patient Registration, Appointment Scheduling, ICD-10 lookup infrastructure

**Avoids pitfalls:**
- Generic data model: typed value objects for all ophthalmic measurements baked into domain layer
- Immutable records: amendment-chain pattern established on Visit and VisitRecord aggregates
- ICD-10 laterality: laterality enforcement rules built into ICD-10 bounded context service
- Appointment race condition: unique DB constraint on slot in initial migration
- Vietnamese text: SQL Server Vietnamese collation (Vietnamese_CI_AS) set at database creation

**Needs research-phase:** No — infrastructure patterns are well-documented (Wolverine official docs, kgrzybek modular monolith reference implementation)

---

### Phase 2: HIS Clinical Core

**Rationale:** With the foundation in place, the core clinical workflow can be built. This is the highest-value phase for clinical staff: visit workflow management, the Dry Eye exam template, refraction recording, medical image upload, and allergy tracking. The Dry Eye template is the reason the system exists; building it second (immediately after foundation) ensures clinical staff are unblocked early. Image metadata structure established here is required by the side-by-side comparison feature in Phase 4.

**Delivers:** Fully functional clinical workflow. Doctor can examine a patient, record a structured Dry Eye assessment (OSDI/TBUT/Schirmer), capture refraction data, upload medical images, record ICD-10 diagnoses, and sign the visit record (making it immutable).

**Addresses features:** Visit/Encounter Management with state machine workflow, Dry Eye Exam Template (OSDI questionnaire with 12-item scoring, TBUT, Schirmer, Meibomian grading), Refraction Data Recording, Medical Image Upload and Viewing, Drug and Glasses Prescription Writing, Allergy Tracking with alerts

**Avoids pitfalls:**
- Chronic disease tracking inconsistency: OSDI stored as structured 12-question responses, not a number
- Medical image as generic attachment: `MedicalImage` entity with typed metadata and thumbnail pipeline
- Appointment race condition: tested concurrently during this phase

**Needs research-phase:** Possibly — Dry Eye template data model is specialized. The `DryEyeAssessment` aggregate design (what fields, what validation ranges, how OSDI version is tracked) may benefit from a focused research-phase to consult clinical literature before implementation.

---

### Phase 3: Pharmacy and Finance Core

**Rationale:** Pharmacy and Finance can be developed in parallel because they share a well-defined event-driven interface (DispensingCompleted -> Finance). Building these together ensures the first complete patient journey (visit -> prescription -> dispensing -> invoice -> payment) is working end-to-end. This is when the system becomes operationally useful, enabling the clinic to transition off paper-based billing.

**Delivers:** End-to-end patient visit with billing. Pharmacist can view pending prescriptions, dispense drugs with batch/expiry tracking, and the unified invoice is automatically generated for the cashier to collect payment (cash/QR/card). Invoice and prescription printing complete.

**Addresses features:** Pharmacy Inventory Management (batch/expiry/min-stock), Pharmacy Prescription Dispensing, Walk-in OTC Sales, Unified Billing, Multiple Payment Methods, Invoice Printing, Discount Management, Refund Workflow

**Avoids pitfalls:**
- Regulatory compliance: prescription entity verified to contain all Circular 26/2025 required fields before this phase ships
- VIP discount logic: VIP exclusion rules for prescription drugs verified in integration tests
- MISA export: denormalized invoice export view built as CQRS read model, not complex joins at query time

**Needs research-phase:** Possibly for payment gateway integration — VNPay/MoMo webhook handling patterns, idempotency requirements, and Vietnamese banking-specific constraints may benefit from focused research before implementation.

---

### Phase 4: Optical Center and Treatment Protocols

**Rationale:** Optical Center and Treatment Protocols are independent of each other (both downstream from HIS) and can be developed in parallel. Both require HIS to be complete (RefractionRxIssued and TreatmentPlanCreated events). This phase introduces Wolverine sagas (treatment package lifecycle, glasses order lifecycle) and the visit-over-visit comparison features that leverage the image metadata and clinical data established in Phase 2.

**Delivers:** Full optical workflow with barcode-based frame inventory, glasses order tracking (ordered -> processing -> ready -> delivered), and Zalo notification when glasses are ready. Full treatment protocol management: IPL/LLLT packages, session recording with per-session OSDI, minimum interval enforcement, and visit-over-visit metric comparison for doctors.

**Addresses features:** Optical Frame/Lens Catalog + Barcode, Glasses Order Tracking with Zalo notification, Contact Lens Management, Combo Pricing + Warranty, IPL/LLLT Treatment Packages, Session Tracking + Per-Session OSDI, Minimum Interval Enforcement, Visit-over-Visit Metric Comparison, Image Side-by-Side Comparison

**Avoids pitfalls:**
- Treatment package session tracking: saga pattern enforces correct state transitions
- Image comparison: uses structured `MedicalImage` metadata established in Phase 2

**Needs research-phase:** No for Optical (standard inventory/order patterns). Possibly for Treatment Protocols — the saga design for multi-step treatment workflows may benefit from targeted research on Wolverine saga patterns before coding.

---

### Phase 5: Reporting, Notifications, VIP, and Polish

**Rationale:** Reporting is last by necessity — it requires real data from all event-producing modules to be meaningful. Zalo OA notifications are an enhancement layer that the core workflow does not depend on. VIP membership requires an established patient base and billing data. This phase also includes bilingual UI completion, SignalR real-time staff dashboard, revenue dashboards, and all remaining document templates (referral letters, consent forms, pharmacy labels).

**Delivers:** Management dashboards with revenue by department, OSDI trend reports, Excel exports. Staff dashboard with real-time patient queue updates via SignalR. Zalo OA appointment reminders, post-visit summaries, and treatment session reminders. VIP membership with auto-tier upgrade. Bilingual UI with complete translation coverage. MISA Excel export for accounting.

**Addresses features:** Revenue Dashboard by Department, Gross Margin Analysis, Treatment Effectiveness Reporting (OSDI trends), Data Export (Excel/CSV), Zalo OA Notifications (appointment reminders, glasses-ready, post-visit summaries, session reminders), VIP Membership Program, Real-time Staff Dashboard (SignalR), Referral Letter and Consent Form Printing, Pharmacy Labels

**Avoids pitfalls:**
- Zalo token management: access token stored in database with automated refresh job (never in appsettings)
- Zalo delivery failure: delivery status log with staff UI for visibility into non-delivery
- Reporting read model staleness: Wolverine event processing lag monitoring and reconciliation checks

**Needs research-phase:** Yes for Zalo OA — the ZBS Template Message format (effective Jan 2026) is newly unified, and the exact API surface for template approval and sending needs verification against current Zalo developer documentation before building the notification module.

---

### Phase Ordering Rationale

- **HIS is the upstream context** for all other modules. Its domain model must be correct before downstream modules can consume its events. Building it first is mandatory.
- **Pharmacy and Finance together** in Phase 3 enables the first complete patient journey (visit -> billing -> payment), which makes the system operationally viable for clinic launch.
- **Optical and Treatment in parallel** in Phase 4 exploits independence: neither depends on the other, both depend on HIS being complete.
- **Reporting last** is architecturally required: it is a pure projection module that subscribes to events from all other modules. It needs those modules to exist and emit real data before it can be tested meaningfully.
- **All three high-cost pitfalls** (generic data model, immutable records, ICD-10 laterality) are preventable in Phase 1 at low cost. The research unanimously places these as Phase 1 concerns with recovery costs of 4-8 weeks each if missed.

### Research Flags

**Phases needing deeper research during planning:**

- **Phase 2 — Dry Eye Template Design:** The `DryEyeAssessment` aggregate is the core differentiator and is not well-documented in generic EHR literature. A focused research-phase on OSDI versioning, standardized dry eye parameter definitions, and the clinical workflow for Meibomian gland grading would reduce rework risk. Sources: published PMC research on standardized dry eye documentation.

- **Phase 3 — Payment Gateway Integration (Vietnam):** VNPay, MoMo, and ZaloPay each have distinct webhook formats, idempotency requirements, and Vietnamese banking constraints. A focused research-phase before implementation will prevent repeated rework on payment callback handling.

- **Phase 5 — Zalo OA ZBS Template Integration:** The ZBS Template Message format (replacing ZNS, effective Jan 2026) is newly unified and the current API surface needs verification. Template approval workflow timelines (can take 2-4 weeks) must also be understood to avoid blocking this phase.

**Phases with well-documented patterns (skip research-phase):**

- **Phase 1 — Infrastructure/Modular Monolith:** WolverineFx has official documentation and Jeremy Miller's reference implementations. The kgrzybek modular-monolith-with-ddd GitHub repo (11k+ stars) covers the patterns directly. Standard setup.
- **Phase 4 — Optical Center:** Frame/lens inventory, barcode scanning, and order tracking follow standard retail/inventory patterns. Well-documented in general software development literature.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | All package versions verified on NuGet and npm. User decisions on core choices (Wolverine, TanStack Start, SQL Server, shadcn/ui) eliminate technology selection risk. One caveat: TanStack Start is RC stage (MEDIUM for that specific component). |
| Features | HIGH | Domain well-understood from PROJECT.md and validated against industry standards (competitor analysis across 5+ systems). Ophthalmology-specific features cross-referenced with peer-reviewed PMC literature. |
| Architecture | HIGH (core patterns) / MEDIUM (Wolverine-specific) | Modular monolith patterns validated against official Wolverine docs, Jeremy Miller's blog, and kgrzybek reference implementation. Wolverine-SignalR transport and multi-DbContext saga patterns have less community-written documentation. |
| Pitfalls | MEDIUM-HIGH | Domain-specific pitfalls (ophthalmic data model, Dry Eye tracking) supported by peer-reviewed PMC research. Vietnamese regulatory pitfalls (Circular 26/2025) sourced from secondary English-language sources; primary Vietnamese regulation text not directly reviewed. |

**Overall confidence:** HIGH

### Gaps to Address

- **So Y Te API technical specification:** The actual API format for the national data connection (due 31/12/2026) is not publicly documented in English. The clinic must contact So Y Te Hanoi or their licensing authority directly before Phase 3-4 to obtain the current technical spec. Do not build the integration against assumptions.

- **Circular 26/2025 exact field requirements:** The prescription fields required by the Ministry of Health's National e-Prescription Portal need verification against the primary Vietnamese regulation text (not secondary English sources). A native Vietnamese-language source review is recommended before finalizing the prescription domain model in Phase 1.

- **TanStack Start RC stability risk:** TanStack Start is at RC stage (v1.163.2). While the API is considered stable per RC announcement and receives daily releases, a v1.0 stable release is imminent. Pin versions carefully (`^1.163.x`), monitor release notes, and plan a version upgrade task when v1.0 ships.

- **Barcode scanner hardware integration:** The optical center requires barcode scanning for frame inventory (currently no barcode system exists). The specific scanner hardware the clinic will purchase may affect the frontend integration approach. This needs a hardware decision before Phase 4 planning.

- **Zalo OA account creation timeline:** Zalo OA account creation and ZNS template approval can take 2-4 weeks. This process should start during Phase 2-3 development to ensure the Zalo integration is unblocked when Phase 5 begins.

## Sources

### Primary (HIGH confidence)
- [WolverineFx Official Documentation](https://wolverinefx.net/) — modular monolith patterns, SQL Server transport, EF Core outbox/inbox, saga patterns, SignalR transport
- [NuGet.org](https://www.nuget.org/) — all .NET package versions verified
- [npm registry](https://www.npmjs.com/) — all frontend package versions verified
- [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd) — modular monolith reference implementation (11k+ stars)
- [AAO ICD-10-CM for Ophthalmology](https://www.aao.org/practice-management/coding/icd-10-cm) — laterality coding rules
- [PMC: Long-Term Financial and Clinical Impact of EHR on Ophthalmology Practice](https://pmc.ncbi.nlm.nih.gov/articles/PMC4354962/) — workflow impact data
- [PMC: Dry Eye Module Software](https://pmc.ncbi.nlm.nih.gov/articles/PMC10276731/) — standardized dry eye documentation
- [Microsoft Azure Blob Storage Security Recommendations](https://learn.microsoft.com/en-us/azure/storage/blobs/security-recommendations) — SAS token patterns
- [EF Core 9 What's New](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/whatsnew) — schema-per-module, migrations

### Secondary (MEDIUM confidence)
- [Jeremy D. Miller Blog](https://jeremydmiller.com/) — Wolverine + EF Core + SQL Server integration patterns, transactional outbox
- [Kamil Grzybek: Modular Monolith Integration Styles](https://www.kamilgrzybek.com/blog/posts/modular-monolith-integration-styles) — DDD context mapping
- [Zalo OA API Documentation](https://developers.zalo.me/) — API structure, ZBS Template Message format
- [TanStack Start v1 RC Announcement](https://tanstack.com/blog/announcing-tanstack-start-v1) — framework stability status
- [shadcn/ui February 2026 Changelog](https://ui.shadcn.com/docs/changelog) — unified radix-ui package migration
- [Circular 26/2025/TT-BYT secondary source](https://lts.com.vn/) — Vietnamese electronic prescription requirements
- [HCMC Prescription Linkage Statistics](https://www.vietnam.vn/) — private clinic compliance rates
- [PMC: Web-based Longitudinal Dry Eye Assessment](https://pubmed.ncbi.nlm.nih.gov/29409963/) — longitudinal tracking challenges

### Tertiary (LOW confidence)
- Ophthalmology EHR marketing sources (Nextech, ModMed, SightView) — competitor feature sets (cross-referenced for consistency, not relied upon individually)
- Vietnamese HIS market overview (SotaERP blog) — local market context

---
*Research completed: 2026-02-28*
*Ready for roadmap: yes*

# Ganka28 Clinic Management System

## What This Is

A comprehensive clinic management system for Ganka28, a boutique ophthalmology clinic in Hanoi, Vietnam. The system manages the full clinical workflow — from patient registration and eye examinations through chronic disease tracking, treatment protocols, pharmacy dispensing, optical center operations, and financial reporting. Built as a modular monolith to support single-location launch with future multi-branch expansion.

## Core Value

Doctors can manage chronic eye disease patients (Dry Eye, Myopia Control) with structured data tracking, image comparison across visits, and treatment progress reporting — the clinical workflow that differentiates Ganka28 from generic clinics.

## Requirements

### Validated

(None yet — ship to validate)

### Active

- [ ] HIS — electronic medical records with customizable templates per disease group
- [ ] HIS — chronic disease management (Dry Eye template with OSDI, TBUT, Schirmer tracking)
- [ ] HIS — medical image management (Fluorescein, Meibography, OCT, side-by-side comparison)
- [ ] HIS — ICD-10 code lookup in diagnosis workflow
- [ ] HIS — appointment scheduling (walk-in + pre-booked, no double booking)
- [ ] HIS — refraction data (SPH, CYL, AXIS, ADD, PD, VA, IOP, Axial Length)
- [ ] Treatment Protocols — IPL/LLLT/lid care packages (1-6 sessions, flexible pricing)
- [ ] Treatment Protocols — session tracking, OSDI per session, auto-complete
- [ ] Treatment Protocols — minimum interval enforcement between sessions
- [ ] Pharmacy — inventory management (import/export/stock, expiry alerts, min stock)
- [ ] Pharmacy — prescription dispensing linked to HIS + walk-in sales
- [ ] Pharmacy — prescription validity (7 days) with expiry warnings
- [ ] Optical Center — frame management with barcode, lens by prescription, contact lenses
- [ ] Optical Center — glasses order tracking (ordered → processing → ready → delivered)
- [ ] Optical Center — combo pricing (frame + lens), warranty management
- [ ] Reporting — revenue dashboard by department (medical, optical, pharmacy, treatment)
- [ ] Reporting — gross margin analysis, doctor performance, treatment effectiveness (OSDI trends)
- [ ] Reporting — data export (Excel/CSV) for research, anonymized
- [ ] Finance — unified billing (single invoice for mixed services), payment methods (cash, bank, QR, card)
- [ ] Finance — VIP membership program (tier-based, auto-upgrade, configurable discounts)
- [ ] Finance — treatment package payments (full upfront or 50/50 split)
- [ ] Auth — role-based access (Doctor, Technician, Nurse, Cashier, Optical Staff, Manager) with granular configurable permissions
- [ ] Notifications — Zalo OA integration (appointment reminders, post-visit summary, treatment reminders, glasses ready)
- [ ] Printing — prescriptions, optical Rx, invoices, referral letters, consent forms, pharmacy labels
- [ ] Clinical Safety — allergy tracking with prescribing alerts
- [ ] Audit — full audit trail (field-level) for all medical record changes
- [ ] Bilingual UI — Vietnamese and English

### Out of Scope

- Online sales / e-commerce — deferred to Phase 2 (not needed for launch)
- BHYT (health insurance) integration — future, when clinic decides to participate
- Multi-branch support — architecture supports it, but not built for v1 (single location)
- Electronic queue/number display — boutique model, staff calls patients by name
- SMS notifications — Zalo only for v1
- Drug interaction checking — only allergy alerts for v1
- Electronic doctor signature — doctor name on prescription sufficient for v1
- Myopia Control template — deferred post-launch (Dry Eye template is priority)
- Glaucoma, Keratoconus, Corneal transplant templates — expandable architecture, built post-launch
- Device integration (slit lamp, OCT, etc.) — API-ready architecture, integrate per-device post-launch
- MISA auto-sync — Phase 1 is manual export, Phase 2 considers API integration

## Context

**Clinical Model:** Ganka28 is a boutique ophthalmology clinic specializing in chronic eye disease (Dry Eye, Myopia Control). The clinic follows a structured patient journey: reception → refraction/VA testing (technician) → doctor exam → diagnostics → doctor reads results → prescription (drugs/glasses) → cashier → pharmacy/optical → glasses processing → pickup/delivery.

**Staff (launch):** 2 doctors, 2 technicians, 1 nurse, 1 cashier, 1 optical staff, 1 manager (~8 people).

**Operating Hours:** Tue-Fri 13:00-20:00, Sat-Sun 08:00-12:00 (possible afternoon expansion), closed Monday.

**Appointment Slots:** 1 patient per doctor per slot. Durations: new patient 30min, follow-up 20min, treatment (IPL/LLLT) 30-45min, Ortho-K fitting 60-90min.

**Patient ID Format:** GK-YYYY-NNNN (e.g., GK-2026-0001). Required fields for medical patients: name, phone, DOB, gender. Walk-in pharmacy customers: name + phone only.

**Pharmacy:** Tủ thuốc (cabinet pharmacy, part of clinic, not separate legal entity). Sells to patients (by prescription) and walk-in customers (OTC). Multiple suppliers, no controlled substances.

**Optical Center:** Strategic revenue driver. Suppliers: Essilor, Hoya, Việt Pháp. Barcode system needed (none exists). Lenses processed in-house or outsourced (1-3 days). Warranty: 12 months frame + lens. Contact lenses (Ortho-K) prescribed via HIS.

**Ortho-K Protocol:** Doctor prescribes → order from supplier → trial lens → fit → follow-up protocol (1 day → 1 week → 1 month → 3 months → 6 months). System auto-generates reminder schedule.

**Payments:** Single consolidated invoice per visit (all services combined). Internal revenue tracking per department. Payment methods: cash, bank transfer, QR (VNPay/MoMo/ZaloPay), card (Visa/MC). Discounts: VIP auto-applied, manual discounts need manager approval. Refunds: manager/owner approval only.

**VIP Program:** Membership tiers based on 12-month spending or treatment completion. Benefits: 10% off follow-up visits, family discount (up to 2 people), 5-7% off services (configurable). 12-month validity. Not applied to prescription drugs or fixed-price items.

**E-invoicing:** MISA. Phase 1: manual export from system → import to MISA.

**Compliance Deadlines:** Sở Y tế data connection before 31/12/2026. ICD-10 coding from Day 1 to ensure data readiness.

**Data:** No legacy data migration — starting fresh. 10-year medical record retention required.

**Zalo OA:** Not yet created. Will be set up during implementation. Reminders sent 1 day before appointment.

## Constraints

- **Tech Stack**: .NET 9 backend, TanStack Start frontend, SQL Server database — chosen, non-negotiable
- **Architecture**: Modular Monolith with DDD — each subsystem is a bounded context
- **Testing**: TDD with Moq/xUnit, all 4 layers (Domain, Application, Integration, E2E/Playwright) from Day 1
- **Messaging**: Wolverine FX for commands, queries, domain events, and Outbox pattern — not MediatR
- **CQRS**: Mixed — Full CQRS for Reporting module (denormalized read models), Light CQRS for all other modules
- **Frontend**: shadcn/ui with Maia style, Stone base color, Green theme, Tabler icons, Inter font, no border radius
- **Frontend Mode**: SSR for public-facing pages (booking, results), SPA for staff dashboard
- **Auth**: ASP.NET Identity + JWT + RBAC with granular configurable permissions
- **Storage**: Azure Blob Storage for medical images (Fluorescein, Meibography, OCT, etc.)
- **Deployment**: Azure App Service (Basic/Standard tier, ~$50-80/month), Azure SQL, Azure Blob
- **CI/CD**: Azure DevOps pipelines
- **Notifications**: Direct Zalo OA API (no third-party aggregator)
- **Security**: TLS + Azure TDE (Transparent Data Encryption) at rest
- **Medical Records**: Immutable visit records — corrections create amendment records with full audit trail
- **Regulatory**: Must support Sở Y tế connection before 31/12/2026
- **Language**: Bilingual Vietnamese-English UI
- **Budget**: Start minimal (~$50-80/month infra), scale as clinic grows
- **Cross-module Communication**: Domain Events + Outbox via Wolverine FX with SQL Server transport

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Modular Monolith over Microservices | Single team, single location, simpler ops. Modules can be extracted later if needed | — Pending |
| Wolverine FX over MediatR | Built-in Outbox, better async support, modern API. Handles commands + queries + events | — Pending |
| SQL Server over PostgreSQL | Native .NET integration, best EF Core support, Azure SQL available, team familiarity | — Pending |
| Schema-per-module over DB-per-module | Clean boundaries with shared transaction support when needed. Simpler cross-module queries | — Pending |
| Mixed CQRS over uniform approach | Full CQRS only where read complexity justifies it (Reporting). Light CQRS elsewhere avoids unnecessary code | — Pending |
| Immutable visits over editable records | Medical compliance standard. Amendment records preserve full history. Required for audit | — Pending |
| ICD-10 from Day 1 | Data readiness for Sở Y tế connection (31/12/2026). Avoids costly backfill | — Pending |
| Azure Blob over SQL FILESTREAM | Cost-effective for large medical images. SAS tokens for secure access. Easier to scale | — Pending |
| TanStack Start over Next.js | Pairs with .NET backend (no Node.js SSR server needed for staff SPA). SSR for public pages | — Pending |
| Zalo OA direct API over aggregator | Lower cost, more control over templates. Single channel sufficient for v1 | — Pending |
| Dry Eye template first | Clinic's primary differentiator. Myopia Control and other templates follow post-launch | — Pending |
| Online sales deferred | Not critical for clinic launch. Reduces v1 scope significantly | — Pending |
| MISA manual export first | Auto-sync adds complexity. Manual export proves the data model before investing in API integration | — Pending |

---
*Last updated: 2026-02-28 after initialization*

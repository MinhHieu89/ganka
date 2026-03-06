# Roadmap: Ganka28 Clinic Management System

## Overview

This roadmap delivers a complete ophthalmology clinic management system across 9 phases, progressing from infrastructure scaffolding through clinical workflows, inventory management, financial operations, and the clinic's core differentiator -- structured chronic disease tracking with treatment protocols. Each phase delivers a coherent, verifiable capability that builds on the previous. The build order follows the domain's natural dependency chain: authentication and architecture foundations first, then patient management, clinical workflows, and finally the downstream modules (pharmacy, finance, optical, treatment) that consume clinical data through domain events.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [ ] **Phase 1: Foundation & Infrastructure** - Modular monolith skeleton with auth, audit, i18n, and multi-tenant scaffolding
- [x] **Phase 2: Patient Management & Scheduling** - Patient registration and appointment booking with calendar (completed 2026-03-02)
- [x] **Phase 3: Clinical Workflow & Examination** - Visit lifecycle, refraction recording, and ICD-10 diagnosis (completed 2026-03-05)
- [ ] **Phase 4: Dry Eye Template & Medical Imaging** - Structured dry eye assessment and medical image management with comparison
- [ ] **Phase 5: Prescriptions & Document Printing** - Drug and optical prescription writing with all printable documents
- [ ] **Phase 6: Pharmacy & Consumables** - Drug inventory, dispensing, and consumables warehouse
- [ ] **Phase 7: Billing & Finance** - Unified invoicing, payment processing, and shift management
- [ ] **Phase 8: Optical Center** - Frame/lens inventory, glasses order tracking, warranty, and stocktaking
- [ ] **Phase 9: Treatment Protocols** - IPL/LLLT/lid care packages with session tracking and OSDI monitoring

## Phase Details

### Phase 1: Foundation & Infrastructure
**Goal**: Staff can log in with role-based access to a deployed modular monolith with audit logging, bilingual UI, and all architectural foundations locked in
**Depends on**: Nothing (first phase)
**Requirements**: AUTH-01, AUTH-02, AUTH-03, AUTH-04, AUTH-05, AUD-01, AUD-02, AUD-03, AUD-04, UI-01, UI-02, ARC-01, ARC-02, ARC-03, ARC-04, ARC-05, ARC-06
**Success Criteria** (what must be TRUE):
  1. Staff member can log in with email/password and receive a session that persists across browser refreshes, times out after inactivity, and supports logout
  2. Admin can assign roles (Doctor, Technician, Nurse, Cashier, Optical Staff, Manager, Accountant) to users and configure granular permissions per role
  3. All login attempts and record access are logged immutably and retained for 10+ years
  4. User can switch between Vietnamese and English UI with all labels, menus, and system text translated
  5. System is deployed with schema-per-module database, Azure Blob Storage configured, daily backups enabled, and all module DbContexts scaffolded with BranchId on aggregate roots
**Plans**: 8 plans in 5 waves

Plans:
- [x] 01-01-PLAN.md — Backend scaffolding: .NET 10 solution, shared kernel, all module .csproj files, Bootstrapper host
- [x] 01-02-PLAN.md — Frontend scaffolding: TanStack Start SPA, shadcn/ui, i18next bilingual, app shell layout
- [x] 01-03-PLAN.md — Auth module: domain entities, JWT auth, login/refresh endpoints, RBAC, data seeding
- [x] 01-04-PLAN.md — Audit module + architecture foundations: audit interceptor, access logging, Azure Blob, ACL adapters, ICD-10 seeding
- [x] 01-05-PLAN.md — Auth UI: login page, session management, user/role admin pages, permission matrix
- [x] 01-06-PLAN.md — Audit UI + architecture tests: audit log viewer with filters/export, NetArchTest rules
- [ ] 01-07-PLAN.md — End-to-end verification checkpoint

### Phase 01.2: Refactor frontend to shadcn/ui with TanStack Start file-based routing (INSERTED)

**Goal:** Frontend uses shadcn/ui comprehensively with dashboard-01 layout, login-04 login page, wrapper component pattern for safe upgrades, Field+Controller forms, generic DataTable, and AlertDialog for session warning
**Depends on:** Phase 1
**Requirements:** UI-01, UI-02, AUTH-01, AUTH-03, AUTH-04, AUD-01, AUD-02
**Plans:** 8/8 plans complete

Plans:
- [x] 01.2-01a-PLAN.md -- Install/upgrade all 20 shadcn/ui primitives via CLI
- [x] 01.2-01b-PLAN.md -- Create 20 wrapper components, replace all direct ui/ imports
- [x] 01.2-02-PLAN.md -- Dashboard layout: SiteHeader with breadcrumbs, AppSidebar with placeholder nav items
- [x] 01.2-03-PLAN.md -- Login page with login-04 layout, Field+Controller forms, AlertDialog session warning
- [x] 01.2-04-PLAN.md -- Generic DataTable component, rebuild all tables, forms, and filters with shadcn patterns
- [x] 01.2-05-PLAN.md -- Automated verification + visual verification checkpoint
- [ ] 01.2-06-PLAN.md -- Dashboard layout fix: match dashboard-01 reference exactly (inset variant, offcanvas, CSS variables)
- [ ] 01.2-07-PLAN.md -- Vietnamese diacritics: rewrite all vi/*.json translation files with proper accented text

### Phase 01.1: Change the current code structure of the backend (INSERTED)

**Goal:** Backend Application layer uses vertical slice feature-based organization with business logic in handlers, thin repository interfaces for data access, and unit tests for all feature handlers. Shared Presentation extensions, per-layer DI registration, and Central Package Management.
**Depends on:** Phase 1
**Plans:** 9/9 plans complete

Plans:
- [x] 01.1-01-PLAN.md — Foundation: Presentation projects, repository interfaces per aggregate root, UnitOfWork, Infrastructure implementations, Bootstrapper DI wiring
- [x] 01.1-02-PLAN.md — Audit module restructuring: 3 feature files (GetAuditLogs, ExportAuditLogs, GetAccessLogs), Minimal API endpoints, rename IAuditReadContext to IAuditReadRepository
- [x] 01.1-03-PLAN.md — Auth features (auth flow): Login, Logout, RefreshToken, GetCurrentUser, UpdateLanguage vertical slice migration with Minimal API endpoints
- [x] 01.1-04-PLAN.md — Auth features (admin flow): GetUsers, CreateUser, UpdateUser, AssignRoles, GetRoles, CreateRole, UpdateRolePermissions, GetPermissions with Minimal API endpoints
- [x] 01.1-05-PLAN.md — Unit tests: Auth.Unit.Tests project + tests for all 13 Auth and 3 Audit feature handlers
- [ ] 01.1-06-PLAN.md — Shared.Presentation project with ResultExtensions (ToHttpResult) and HttpContextExtensions (TryGetUserId)
- [ ] 01.1-07-PLAN.md — Presentation layer cleanup: route groups, ResultExtensions, HttpContextExtensions for all endpoints
- [ ] 01.1-08-PLAN.md — Per-layer IoC.cs DI registration for Auth, Audit, and Shared modules
- [ ] 01.1-09-PLAN.md — Central Package Management: Directory.Packages.props for all NuGet packages

### Phase 2: Patient Management & Scheduling
**Goal**: Staff can register patients, manage their profiles, and book appointments with no double-booking
**Depends on**: Phase 1
**Requirements**: PAT-01, PAT-02, PAT-03, PAT-04, PAT-05, SCH-01, SCH-02, SCH-03, SCH-04, SCH-05, SCH-06
**Success Criteria** (what must be TRUE):
  1. Staff can register a medical patient (name, phone, DOB, gender) and the system auto-generates a GK-YYYY-NNNN patient ID, or register a walk-in pharmacy customer with name and phone only
  2. Staff can search for patients by name, phone, or patient ID and get results in under 3 seconds
  3. Staff can record allergies on a patient profile, and those allergies are visible as alerts during downstream workflows
  4. Staff can book appointments on a per-doctor calendar with configurable durations, and the system prevents double-booking with a database-level constraint
  5. Patients can self-book via a public-facing page, with staff confirmation required before the appointment is finalized
**Plans**: 14 plans (6 original + 4 gap closure + 4 UAT gap closure)

Plans:
- [x] 02-01-PLAN.md -- Patient module backend: domain entities, EF Core infrastructure, application features, Minimal API endpoints
- [x] 02-02-PLAN.md -- Scheduling module backend: appointments, self-booking, clinic schedule, double-booking prevention, public endpoints
- [x] 02-03-PLAN.md -- Frontend dependencies + shared components: FullCalendar, cmdk, shadcn wrappers, GlobalSearch, i18n translations
- [x] 02-04-PLAN.md -- Patient frontend: registration form, patient list, profile page with tabs, allergy management
- [x] 02-05-PLAN.md -- Scheduling frontend: FullCalendar calendar, booking dialogs, public self-booking page, pending bookings panel
- [x] 02-06-PLAN.md -- Integration verification checkpoint: API tests + browser verification of all flows
- [x] 02-07-PLAN.md -- [GAP] Fix patient registration 404, inline edit 500, pagination visibility, breadcrumb display
- [x] 02-08-PLAN.md -- [GAP] Fix doctor dropdown, datetime picker, textarea corners, dialog styling, booking form polish
- [x] 02-09-PLAN.md -- [GAP] Rewrite allergy form autocomplete with free-text support and Vietnamese categories
- [x] 02-10-PLAN.md -- [GAP] PAT-03 field validation infrastructure + BookingForm placeholder cleanup
- [x] 02-11-PLAN.md -- [UAT GAP] Backend: substring search, UTC-to-Vietnam timezone, structured validation errors
- [x] 02-12-PLAN.md -- [UAT GAP] Frontend infra: replace calendar.tsx, fix dialog spacing, 401 interceptor, registration redirect, allergy severity
- [x] 02-13-PLAN.md -- [UAT GAP] Fix staff booking form (DoctorSelector, pre-populate, slot alignment) and replace allergy autocomplete
- [x] 02-14-PLAN.md -- [UAT GAP] Reusable server validation UI, patient profile header redesign, full UAT verification

### Phase 02.1: I notice some bugs in frontend that I want to resolve (INSERTED)

**Goal:** Fix session persistence via HTTP-only cookie refresh tokens, reset all pages to shadcn/ui default neutral theme with rounded corners
**Depends on:** Phase 02
**Requirements:** AUTH-04, UI-01, UI-02
**Plans:** 14/14 plans complete

Plans:
- [x] 02.1-01-PLAN.md -- Backend HTTP-only cookie: TDD for Set-Cookie on login/refresh/logout, RememberMe entity column, environment-aware Secure flag
- [x] 02.1-02-PLAN.md -- Frontend auth flow: credentials:include, remove refreshToken from JS, silent refresh on page load via cookie
- [x] 02.1-03-PLAN.md -- Theme reset: replace Stone+Green with shadcn/ui neutral defaults, fix hsl(oklch) bugs, replace hardcoded stone-* classes
- [x] 02.1-04-PLAN.md -- Verification checkpoint: automated checks + human verification of session persistence and visual alignment

### Phase 3: Clinical Workflow & Examination
**Goal**: Doctors can conduct a complete clinical visit with structured examination data, ICD-10 diagnosis, and immutable visit records
**Depends on**: Phase 2
**Requirements**: CLN-01, CLN-02, CLN-03, CLN-04, REF-01, REF-02, REF-03, DX-01, DX-02
**Success Criteria** (what must be TRUE):
  1. Doctor can create a visit record linked to a patient, record examination findings, and sign off the visit -- making the record immutable
  2. Corrections to signed visit records create amendment records that preserve the original and log the reason, field-level changes, who amended, and when
  3. Dashboard shows all active patients and their current workflow stage (reception, refraction/VA, doctor exam, diagnostics, doctor reads, Rx, cashier, pharmacy/optical) in real-time
  4. Technician or doctor can record refraction data (SPH, CYL, AXIS, ADD, PD, VA, IOP, Axial Length per eye) with support for manifest, autorefraction, and cycloplegic types
  5. Doctor can search ICD-10 codes in Vietnamese and English, pin favorites, and the system enforces laterality selection (OD/OS/OU) for ophthalmology codes
**Plans**: 10 plans (5 original + 5 gap closure)

Plans:
- [x] 03-01-PLAN.md -- Clinical domain entities, contracts DTOs, EF Core infrastructure, test project scaffold
- [x] 03-02-PLAN.md -- Clinical application handlers (TDD), presentation endpoints, Bootstrapper wiring
- [x] 03-03-PLAN.md -- Frontend shared components, API hooks, Kanban workflow dashboard with @dnd-kit
- [x] 03-04-PLAN.md -- Frontend visit detail page: refraction, diagnosis, sign-off, amendment
- [x] 03-05-PLAN.md -- End-to-end verification checkpoint
- [x] 03-06-PLAN.md -- [GAP] Fix refraction save 500 (EF Core backing field config) and diagnosis add 400 (laterality enum mismatch)
- [x] 03-07-PLAN.md -- [GAP] Fix amendment empty field-level diff + human verification of all gap fixes
- [x] 03-08-PLAN.md -- [GAP] Fix DbUpdateConcurrencyException: explicit child-entity registration with EF Core change tracker
- [x] 03-09-PLAN.md -- [GAP] Frontend error toasts + IOP Select warning fix + human verification of complete workflow
- [ ] 03-10-PLAN.md -- [UAT GAP] Fix refraction data reload (type vs refractionType mismatch) and IOP Select warning

### Phase 03.1: Create Vietnamese user stories documentation for all implemented features (INSERTED)

**Goal:** Comprehensive Vietnamese user stories documentation covering all 35 completed requirements from Phases 1, 2, and 3, organized by workflow area with standard user story format, full acceptance criteria, and requirement traceability
**Depends on:** Phase 3
**Requirements:** AUTH-01, AUTH-02, AUTH-03, AUTH-04, AUTH-05, PAT-01, PAT-02, PAT-03, PAT-04, PAT-05, SCH-01, SCH-02, SCH-03, SCH-04, SCH-05, SCH-06, CLN-01, CLN-02, CLN-03, CLN-04, REF-01, REF-02, REF-03, DX-01, DX-02, AUD-01, AUD-02, AUD-03, AUD-04, UI-01, UI-02, ARC-01, ARC-02, ARC-03, ARC-04, ARC-05, ARC-06
**Plans:** 3/3 plans complete

Plans:
- [ ] 03.1-01-PLAN.md -- Authentication & authorization + patient management user stories (AUTH-01..05, PAT-01..05)
- [ ] 03.1-02-PLAN.md -- Scheduling & booking + clinical workflow user stories (SCH-01..06, CLN-01..04, REF-01..03, DX-01..02)
- [ ] 03.1-03-PLAN.md -- Audit & compliance + UI & system user stories (AUD-01..04, UI-01..02, ARC-01..06)

### Phase 4: Dry Eye Template & Medical Imaging
**Goal**: Doctors can perform structured Dry Eye assessments with OSDI scoring and compare clinical data and images across visits
**Depends on**: Phase 3
**Requirements**: DRY-01, DRY-02, DRY-03, DRY-04, IMG-01, IMG-02, IMG-03, IMG-04
**Success Criteria** (what must be TRUE):
  1. Doctor can record a complete Dry Eye assessment per eye (OSDI from 12-question questionnaire, TBUT, Schirmer, Meibomian gland grading, Tear meniscus, Staining score) with the system calculating and color-coding OSDI severity
  2. Doctor can view an OSDI trend chart across all visits for a patient, showing disease progression or improvement over time
  3. Doctor can compare Dry Eye metrics (TBUT, Schirmer, etc.) between any two visits side-by-side
  4. Staff can upload medical images and videos (Fluorescein, Meibography, OCT, lacrimal duct video) associated with specific visits, and doctors can view them in a lightbox with zoom
  5. Doctor can compare images of the same type side-by-side across two visits for the same patient
**Plans**: 8 plans in 5 waves

Plans:
- [x] 04-01a-PLAN.md -- Backend domain entities, enums, contracts DTOs
- [x] 04-01b-PLAN.md -- EF Core configs, repositories, migration
- [x] 04-02-PLAN.md -- Dry Eye + OSDI handlers (TDD): assessment CRUD, OSDI calculation, history, comparison
- [x] 04-03-PLAN.md -- Medical image handlers (TDD) + API endpoints + public OSDI endpoints + Bootstrapper wiring
- [x] 04-04-PLAN.md -- Frontend dry eye form, OSDI questionnaire, trend chart, comparison panel
- [x] 04-05-PLAN.md -- Frontend image upload, gallery, lightbox, comparison overlay, public OSDI page
- [x] 04-06-PLAN.md -- End-to-end verification checkpoint
- [ ] 04-07-PLAN.md -- Vietnamese user stories documentation (DOC-01)

### Phase 5: Prescriptions & Document Printing
**Goal**: Doctors can write drug and optical prescriptions that comply with MOH regulations, and all clinical documents can be printed
**Depends on**: Phase 3
**Requirements**: RX-01, RX-02, RX-03, RX-04, RX-05, PRT-01, PRT-02, PRT-03, PRT-04, PRT-05, PRT-06
**Success Criteria** (what must be TRUE):
  1. Doctor can write a drug prescription by selecting from the pharmacy catalog or adding off-catalog drugs, and catalog-linked items are flagged for auto stock deduction upon dispensing
  2. Doctor can write an optical prescription (glasses Rx) with full refraction parameters
  3. System warns the doctor when prescribing a drug the patient is allergic to (using allergy data from patient profile)
  4. All prescriptions comply with MOH format requirements (required fields, dosage format per Bo Y te regulations)
  5. Staff can print drug prescriptions, optical Rx, invoices/receipts, referral letters, consent forms, and pharmacy labels -- all with correct clinic branding and formatting
**Plans**: 25 plans in 9 waves

Plans:
- [ ] 05-01-PLAN.md -- Pharmacy drug catalog domain entities, enums, contracts DTOs
- [ ] 05-02-PLAN.md -- Pharmacy EF Core config, repository, drug catalog seeder (~60-80 ophthalmic drugs)
- [ ] 05-03-PLAN.md -- Pharmacy application handlers: search, create, update drug catalog items
- [ ] 05-04-PLAN.md -- Clinical prescription domain entities: DrugPrescription, PrescriptionItem, OpticalPrescription, Visit update
- [ ] 05-05a-PLAN.md -- Clinical prescription EF Core configs + VisitConfiguration update
- [ ] 05-05b-PLAN.md -- ClinicalDbContext update + contract DTOs
- [ ] 05-06-PLAN.md -- [TDD] Drug prescription handlers: add, update, remove, allergy check
- [ ] 05-07-PLAN.md -- [TDD] Optical prescription handlers: add, update
- [ ] 05-08-PLAN.md -- Pharmacy Presentation layer: API endpoints, IoC registration
- [ ] 05-09-PLAN.md -- Clinic settings entity, service, EF config, DI registration for configurable document headers
- [ ] 05-09b-PLAN.md -- Clinic settings HTTP API endpoints (GET/PUT /api/settings/clinic)
- [ ] 05-10-PLAN.md -- Integration: prescription endpoints in ClinicalAPI, Bootstrapper wiring, settings endpoint wiring, NuGet packages, migration
- [ ] 05-11-PLAN.md -- QuestPDF infrastructure: font manager, clinic header component, drug prescription PDF (A5)
- [ ] 05-12a-PLAN.md -- PDF documents: optical Rx, referral letter, consent form, pharmacy label
- [ ] 05-12b-PLAN.md -- Complete DocumentService + print API endpoints
- [ ] 05-13-PLAN.md -- Frontend drug catalog admin page: table, form dialog, API hooks, route file
- [ ] 05-14-PLAN.md -- Frontend prescription components: DrugCombobox, DrugAllergyWarning, DrugPrescriptionForm, API hooks
- [ ] 05-15-PLAN.md -- Frontend DrugPrescriptionSection + VisitDetailPage integration
- [ ] 05-16-PLAN.md -- Frontend OpticalPrescriptionSection + form with auto-populate from refraction
- [ ] 05-17a-PLAN.md -- Frontend PrintButton component, document API, section integrations
- [ ] 05-17b-PLAN.md -- i18n translations (EN + VI) for prescription and pharmacy
- [ ] 05-18-PLAN.md -- Frontend clinic settings admin page, Vietnamese pharmacy translations, sidebar navigation
- [ ] 05-19-PLAN.md -- [TDD] Pharmacy.Unit.Tests project: search and CRUD handler tests
- [ ] 05-20-PLAN.md -- Vietnamese user stories documentation (DOC-01)
- [ ] 05-21-PLAN.md -- End-to-end verification checkpoint

### Phase 05.1: Fix architecture test failures from prior phases (INSERTED)

**Goal:** All 55 NetArchTest architecture rules pass with zero failures, restoring clean architecture boundaries across Patient and Clinical modules
**Depends on:** Phase 5
**Requirements**: ARCH-01, ARCH-02, ARCH-03, ARCH-04, ARCH-05
**Plans:** 8 plans in 4 waves

Plans:
- [ ] 05.1-01-PLAN.md -- Move FieldChange record to Clinical.Domain.ValueObjects + remove stale Clinical.Contracts Domain reference
- [ ] 05.1-02-PLAN.md -- Move PatientFieldValidationResult to Patient.Contracts + clean Presentation Domain imports
- [ ] 05.1-03a-PLAN.md -- Create Patient.Contracts.Enums (Gender, PatientType, AllergySeverity)
- [ ] 05.1-03b-PLAN.md -- Remove Patient.Domain reference from Patient.Contracts.csproj + update 5 DTO using directives
- [ ] 05.1-04-PLAN.md -- Create IReferenceDataRepository interface + implementation + IoC registration
- [ ] 05.1-05-PLAN.md -- Create enum mappers + update Patient.Application handlers with boundary conversion
- [ ] 05.1-06-PLAN.md -- Update Clinical.Application handlers to use IReferenceDataRepository + update unit tests
- [ ] 05.1-07-PLAN.md -- Full verification: all 55 architecture tests + full test suite regression check

### Phase 6: Pharmacy & Consumables
**Goal**: Pharmacist can manage drug inventory with batch/expiry tracking and dispense against prescriptions, with a separate consumables warehouse for treatment supplies
**Depends on**: Phase 5
**Requirements**: PHR-01, PHR-02, PHR-03, PHR-04, PHR-05, PHR-06, PHR-07, CON-01, CON-02, CON-03
**Success Criteria** (what must be TRUE):
  1. Staff can manage drug inventory with batch tracking, multiple suppliers, and import stock via supplier invoice or Excel bulk import
  2. System alerts when drugs approach expiry (configurable 30/60/90 day thresholds) or fall below minimum stock levels
  3. Pharmacist can view pending prescriptions from HIS, dispense drugs with auto stock deduction per batch (FEFO), and the system enforces 7-day prescription validity
  4. Staff can process walk-in OTC sales without a prescription
  5. System maintains a separate consumables warehouse (IPL gel, eye shields, etc.) with stock levels, alerts, and auto-deduction when consumables are used in treatment sessions
**Plans**: 27 plans in 9 waves

Plans:
- [ ] 06-01-PLAN.md -- Pharmacy stock domain entities: Supplier, DrugBatch, SupplierDrugPrice, DrugCatalogItem pricing extension
- [ ] 06-02-PLAN.md -- Stock import and adjustment domain entities: StockImport, StockImportLine, StockAdjustment
- [ ] 06-03-PLAN.md -- Dispensing domain entities: DispensingRecord, DispensingLine, BatchDeduction
- [ ] 06-04-PLAN.md -- OTC sale and consumables domain entities: OtcSale, ConsumableItem, ConsumableBatch
- [ ] 06-05a-PLAN.md -- EF Core configs: Supplier, DrugBatch, StockImport, DrugCatalogItem update
- [ ] 06-05b-PLAN.md -- EF Core configs: DispensingRecord, OtcSale, ConsumableItem, StockAdjustment
- [ ] 06-06-PLAN.md -- PharmacyDbContext update + all Contracts DTOs
- [ ] 06-07-PLAN.md -- Repository interfaces: Supplier, DrugBatch, StockImport, Dispensing, OtcSale, Consumable
- [ ] 06-08-PLAN.md -- Repository implementations: Supplier, DrugBatch (FEFO), StockImport, Dispensing, OtcSale
- [ ] 06-09-PLAN.md -- ConsumableRepository, ConsumableCatalogSeeder, Pharmacy IoC, DrugCatalogItemRepository update
- [ ] 06-10-PLAN.md -- [TDD] FEFO Allocator domain service + Pharmacy.Unit.Tests project
- [ ] 06-11-PLAN.md -- [TDD] Supplier CRUD handlers
- [ ] 06-12-PLAN.md -- [TDD] Stock import handlers (supplier invoice + Excel bulk) + MiniExcel
- [ ] 06-13-PLAN.md -- [TDD] Dispensing handlers with FEFO + 7-day expiry + cross-module query
- [ ] 06-14-PLAN.md -- [TDD] OTC sale and inventory management handlers
- [ ] 06-15-PLAN.md -- [TDD] Alert handlers (expiry + low stock) + ConsumableItem CRUD handlers
- [ ] 06-16-PLAN.md -- [TDD] Consumable stock management handlers (add/adjust/alerts)
- [ ] 06-17-PLAN.md -- Pharmacy.Presentation project + API endpoints (suppliers, inventory, stock import, alerts)
- [ ] 06-18-PLAN.md -- Dispensing/OTC/Consumables API endpoints + cross-module handler + Bootstrapper wiring
- [ ] 06-19-PLAN.md -- Frontend API layer: pharmacy + consumables API functions and TanStack Query hooks
- [ ] 06-20-PLAN.md -- Frontend drug inventory page: table, batch details, alert banners
- [ ] 06-21-PLAN.md -- Frontend supplier management + stock import pages
- [ ] 06-22-PLAN.md -- Frontend dispensing queue page + dispensing dialog
- [ ] 06-23-PLAN.md -- Frontend OTC sales page + stock adjustment dialog
- [ ] 06-24-PLAN.md -- Frontend consumables warehouse page: table, forms, alerts
- [ ] 06-25-PLAN.md -- Sidebar navigation update + i18n translations (EN/VI)
- [ ] 06-26-PLAN.md -- Vietnamese user stories documentation (DOC-01)
- [ ] 06-27-PLAN.md -- End-to-end verification checkpoint

### Phase 7: Billing & Finance
**Goal**: Cashier can generate unified invoices across all departments, collect payments via multiple methods, and manage shifts with cash reconciliation
**Depends on**: Phase 6
**Requirements**: FIN-01, FIN-02, FIN-03, FIN-04, FIN-05, FIN-06, FIN-07, FIN-08, FIN-09, FIN-10
**Success Criteria** (what must be TRUE):
  1. System generates a single consolidated invoice per visit combining charges from medical, optical, pharmacy, and treatment departments with internal revenue allocation per line item
  2. Cashier can collect payment via cash, bank transfer, QR (VNPay/MoMo/ZaloPay), or card (Visa/MC)
  3. System generates e-invoice (hoa don dien tu) compliant with Vietnamese tax law for MISA export
  4. System supports treatment package payments (full upfront or 50/50 split) and enforces the rule that the 2nd payment must be received before the mid-course session
  5. Manual discounts require manager approval, refunds require manager/owner approval with full audit trail, and all price changes are logged with who/when/old/new values
**Plans**: 26 plans in 12 waves

Plans:
- [ ] 07-01-PLAN.md -- Invoice aggregate + InvoiceLineItem domain entities, enums, domain event (includes _payments/_discounts backing fields + RecordPayment/ApplyDiscount stubs)
- [ ] 07-02-PLAN.md -- Payment, Discount, Refund domain entities + enums
- [ ] 07-03-PLAN.md -- CashierShift aggregate + ShiftTemplate entity + ShiftStatus enum
- [ ] 07-04-PLAN.md -- EF Core configs: Invoice, InvoiceLineItem, Payment, Discount, Refund
- [ ] 07-05-PLAN.md -- EF Core configs: CashierShift, ShiftTemplate + BillingDbContext update + ShiftTemplateSeeder
- [ ] 07-06-PLAN.md -- Billing.Contracts DTOs (Invoice, Payment, Shift, EInvoice) + cross-module queries
- [ ] 07-07-PLAN.md -- Repository interfaces + Invoice/Payment repository implementations
- [ ] 07-08-PLAN.md -- CashierShift repository + UnitOfWork + IoC registration + IBillingDocumentService
- [ ] 07-09-PLAN.md -- [TDD] Invoice CRUD handlers: CreateInvoice, AddLineItem, FinalizeInvoice
- [ ] 07-10-PLAN.md -- [TDD] Payment handlers: RecordPayment with all methods, split payment (FIN-06 partial: records data, enforcement deferred to Phase 9)
- [ ] 07-11-PLAN.md -- [TDD] Discount + Refund handlers: ApplyDiscount, ApproveDiscount, RequestRefund
- [ ] 07-12-PLAN.md -- [TDD] Shift handlers: OpenShift, CloseShift, GetCurrentShift, GetShiftReport
- [ ] 07-13-PLAN.md -- Billing.Presentation project + all API endpoints
- [ ] 07-14-PLAN.md -- Bootstrapper wiring, solution update, migration
- [ ] 07-15-PLAN.md -- QuestPDF documents: Invoice, Receipt, EInvoice, ShiftReport
- [ ] 07-16-PLAN.md -- BillingDocumentService + EInvoiceExportService + print/export endpoints
- [ ] 07-17-PLAN.md -- Frontend API layer: billing-api.ts, shift-api.ts, VND formatter
- [ ] 07-18-PLAN.md -- Frontend cashier dashboard + invoice detail page + components
- [ ] 07-19-PLAN.md -- Frontend PaymentForm + PaymentMethodSelector + InvoiceView integration
- [ ] 07-20-PLAN.md -- Frontend DiscountDialog, RefundDialog, ApprovalPinDialog, EInvoiceExportButton
- [ ] 07-21-PLAN.md -- Frontend shift management page + ShiftOpen/Close/Report components
- [ ] 07-22-PLAN.md -- i18n translations (EN/VI) + sidebar navigation + InvoiceView integration
- [ ] 07-23-PLAN.md -- Vietnamese user stories documentation (DOC-01)
- [ ] 07-24-PLAN.md -- End-to-end verification checkpoint
- [ ] 07-25-PLAN.md -- Supplementary invoice/shift handlers: RemoveInvoiceLineItem, GetInvoiceById, GetInvoicesByVisit, GetShiftTemplates
- [ ] 07-26-PLAN.md -- Supplementary discount/refund handlers: RejectDiscount, ApproveRefund, ProcessRefund

### Phase 8: Optical Center
**Goal**: Staff can manage frame/lens inventory with barcodes, track glasses orders through their full lifecycle, and handle warranty claims
**Depends on**: Phase 5, Phase 7
**Requirements**: OPT-01, OPT-02, OPT-03, OPT-04, OPT-05, OPT-06, OPT-07, OPT-08, OPT-09
**Success Criteria** (what must be TRUE):
  1. Staff can manage frame inventory with barcode scanning (brand, model, color, size, price, stock) and order lenses by prescription from suppliers
  2. System tracks glasses order lifecycle (Ordered -> Processing -> Received -> Ready -> Delivered) and blocks processing until full payment is confirmed
  3. Staff can create preset and custom combo pricing (frame + lens combinations) and manage warranty claims with supporting documents (replace/repair/discount)
  4. System stores lens prescription history per patient with year-over-year comparison and lens replacement history
  5. Staff can perform barcode-based stocktaking with physical count entry and a discrepancy report comparing physical vs. system inventory
**Plans**: TBD

Plans:
- [ ] 08-01: TBD
- [ ] 08-02: TBD
- [ ] 08-03: TBD

### Phase 9: Treatment Protocols
**Goal**: Doctors can create and manage IPL/LLLT/lid care treatment packages with session tracking, OSDI monitoring per session, and configurable business rules
**Depends on**: Phase 4, Phase 6, Phase 7
**Requirements**: TRT-01, TRT-02, TRT-03, TRT-04, TRT-05, TRT-06, TRT-07, TRT-08, TRT-09, TRT-10, TRT-11
**Success Criteria** (what must be TRUE):
  1. Doctor can create IPL, LLLT, or lid care treatment packages with 1-6 sessions and flexible pricing (per-session or per-package), and only users with Doctor role can create or modify protocols
  2. System tracks sessions completed and remaining per course, records OSDI score at each session, and auto-marks the course as "Completed" when all sessions are done
  3. System enforces configurable minimum intervals between sessions (IPL 2-4 weeks, LLLT 1-2 weeks, lid care 1-2 weeks) and supports multiple concurrent treatment courses per patient
  4. Doctor can modify a treatment protocol mid-course (add/remove sessions, change parameters) or switch a patient to a different treatment type, both requiring doctor approval
  5. Manager can process treatment cancellation with configurable refund deduction (10-20% fee), and consumables used per session are tracked and auto-deducted from the consumables warehouse

**Plans**: TBD

Plans:
- [ ] 09-01: TBD
- [ ] 09-02: TBD
- [ ] 09-03: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 8 -> 9

Note: Phase 4 and Phase 5 both depend only on Phase 3 and can potentially run in parallel. Phase 8 depends on both Phase 5 and Phase 7.

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation & Infrastructure | 6/7 | In Progress | - |
| 1.1 Backend Restructuring | 5/9 | In Progress | - |
| 1.2 Frontend shadcn/ui Refactoring | 6/8 | In Progress | - |
| 2. Patient Management & Scheduling | 14/14 | Complete    | 2026-03-02 |
| 2.1 Frontend Bug Fixes + Auth Security | 4/4 | Complete | 2026-03-02 |
| 3. Clinical Workflow & Examination | 9/10 | In Progress | - |
| 3.1 Vietnamese User Stories Documentation | 0/3 | Not started | - |
| 4. Dry Eye Template & Medical Imaging | 7/8 | In Progress | - |
| 5. Prescriptions & Document Printing | 23/25 | In Progress|  |
| 5.1 Fix Architecture Test Failures | 0/8 | Not started | - |
| 6. Pharmacy & Consumables | 0/27 | Not started | - |
| 7. Billing & Finance | 0/26 | Not started | - |
| 8. Optical Center | 0/3 | Not started | - |
| 9. Treatment Protocols | 0/3 | Not started | - |

# Requirements: Ganka28 Clinic Management System

**Defined:** 2026-02-28
**Core Value:** Doctors can manage chronic eye disease patients (Dry Eye, Myopia Control) with structured data tracking, image comparison across visits, and treatment progress reporting

## v1 Requirements

Requirements for initial release. Each maps to roadmap phases.

### Authentication & Security

- [x] **AUTH-01**: Staff can log in with credentials and receive JWT token with role-based claims
- [x] **AUTH-02**: System supports roles: Doctor, Technician, Nurse, Cashier, Optical Staff, Manager, Accountant
- [x] **AUTH-03**: Admin can configure granular permissions per role (CRUD per entity/action)
- [x] **AUTH-04**: User session persists with token refresh, times out after inactivity, supports logout
- [x] **AUTH-05**: System logs all login attempts, record access, and data views (access logging)

### Patient Management

- [ ] **PAT-01**: Staff can register medical patient with name, phone, DOB, gender; system auto-generates GK-YYYY-NNNN ID
- [ ] **PAT-02**: Staff can register walk-in pharmacy customer with name + phone only (lightweight record, no full medical profile)
- [ ] **PAT-03**: System supports configurable mandatory fields (Address, CCCD) that become required for referrals, legal export, or So Y te reporting
- [x] **PAT-04**: Staff can search patients by name, phone, or patient ID with results in <=3 seconds
- [ ] **PAT-05**: System stores structured allergy list per patient and displays allergy alerts during prescribing

### Clinical Workflow

- [ ] **CLN-01**: Doctor can create electronic visit record (benh an) linked to patient and doctor, immutable after sign-off
- [ ] **CLN-02**: Corrections to signed visit records create amendment records with reason, field-level changes, and original preserved
- [ ] **CLN-03**: Staff can track visit workflow status (reception -> refraction/VA -> doctor exam -> diagnostics -> doctor reads -> Rx -> cashier -> pharmacy/optical)
- [ ] **CLN-04**: Dashboard shows all active patients and their current workflow stage in real-time

### Dry Eye Template

- [ ] **DRY-01**: Doctor can record Dry Eye exam with structured fields: OSDI score, TBUT, Schirmer, Meibomian gland grading, Tear meniscus, Staining score -- all per eye (left/right)
- [ ] **DRY-02**: System calculates and displays OSDI severity classification (Normal 0-12, Mild 13-22, Moderate 23-32, Severe 33-100) with color coding
- [ ] **DRY-03**: Doctor can view OSDI trend chart across visits for a patient
- [ ] **DRY-04**: Doctor can compare TBUT, Schirmer, and other metrics between visits side-by-side

### Refraction & Examination

- [ ] **REF-01**: Technician or doctor can record refraction data: SPH, CYL, AXIS, ADD, PD per eye
- [ ] **REF-02**: System records VA (with/without correction), IOP (with method and time), Axial Length per eye
- [ ] **REF-03**: System supports manifest, autorefraction, and cycloplegic refraction types

### Medical Imaging

- [ ] **IMG-01**: Staff can upload medical images (Fluorescein, Meibography, Specular microscopy, Topography, OCT) and associate them with the correct visit
- [ ] **IMG-02**: Staff can upload video files (e.g., lacrimal duct procedures) associated with visits
- [ ] **IMG-03**: Doctor can view images in a lightbox with zoom capability
- [ ] **IMG-04**: Doctor can compare images side-by-side across two visits (same image type, same patient)

### Diagnosis & Prescription

- [ ] **DX-01**: Doctor can search and select ICD-10 codes in Vietnamese and English with ophthalmology favorites/pinned codes
- [ ] **DX-02**: System enforces ICD-10 laterality selection for ophthalmology codes (no unspecified eye)
- [ ] **RX-01**: Doctor can write drug prescription by selecting from pharmacy catalog or adding off-catalog drugs
- [ ] **RX-02**: Catalog-linked prescriptions auto-deduct stock when dispensed; off-catalog drugs flagged as manual
- [ ] **RX-03**: Doctor can write glasses prescription (optical Rx) with full refraction parameters
- [ ] **RX-04**: Prescriptions comply with MOH format requirements (required fields, dosage format per Bo Y te)
- [ ] **RX-05**: System warns when prescribing drugs the patient is allergic to

### Appointment Scheduling

- [ ] **SCH-01**: Staff can book appointments for patients (walk-in registration + pre-booked slots)
- [ ] **SCH-02**: Patients can self-book via public website/Zalo with staff confirmation workflow
- [ ] **SCH-03**: System enforces no double-booking (1 patient per doctor per time slot)
- [ ] **SCH-04**: System displays calendar view per doctor, color-coded by appointment type
- [ ] **SCH-05**: Appointment durations configurable by type (default: new 30min, follow-up 20min, treatment 30-45min, Ortho-K 60-90min)
- [ ] **SCH-06**: System respects clinic operating hours (Tue-Fri 13-20h, Sat-Sun 8-12h, closed Monday) as configurable schedule

### Treatment Protocols

- [ ] **TRT-01**: Doctor can create IPL, LLLT, or lid care treatment packages with 1-6 sessions and flexible pricing (per-session or per-package)
- [ ] **TRT-02**: System tracks sessions completed and remaining per treatment course
- [ ] **TRT-03**: System records OSDI score at each treatment session
- [ ] **TRT-04**: System auto-marks treatment course as "Completed" when all sessions are done
- [ ] **TRT-05**: System enforces minimum interval between sessions (configurable per type: IPL 2-4wk, LLLT 1-2wk, lid care 1-2wk)
- [ ] **TRT-06**: Patient can have multiple active treatment courses simultaneously (e.g., IPL + LLLT)
- [ ] **TRT-07**: Doctor can modify treatment protocol mid-course (add/remove sessions, change parameters)
- [ ] **TRT-08**: Doctor can switch patient from one treatment type to another mid-course (requires doctor approval)
- [ ] **TRT-09**: Manager can process treatment cancellation with configurable refund deduction (10-20% fee)
- [ ] **TRT-10**: Only users with Doctor role can create or modify treatment protocols
- [ ] **TRT-11**: System records consumables used per treatment session (linked to consumables warehouse)

### Pharmacy

- [ ] **PHR-01**: Staff can manage drug inventory with batch tracking and multiple suppliers
- [ ] **PHR-02**: Staff can import stock via supplier invoice (manual entry) or Excel bulk import
- [ ] **PHR-03**: System tracks expiry dates and alerts at configurable thresholds (30/60/90 days before expiry)
- [ ] **PHR-04**: System alerts when drug stock falls below configurable minimum level per drug
- [ ] **PHR-05**: Pharmacist can dispense drugs against HIS prescription with auto stock deduction
- [ ] **PHR-06**: Staff can process walk-in OTC sales without prescription
- [ ] **PHR-07**: System enforces 7-day prescription validity and warns on expired prescriptions

### Consumables Warehouse

- [ ] **CON-01**: System maintains separate consumables warehouse independent from pharmacy stock
- [ ] **CON-02**: Staff can manage treatment supplies inventory (IPL gel, eye shields, etc.) with stock levels and alerts
- [ ] **CON-03**: Consumable usage per treatment session auto-deducts from consumables warehouse

### Optical Center

- [ ] **OPT-01**: Staff can manage frame inventory with barcode scanning (brand, model, color, size, price, stock)
- [ ] **OPT-02**: Staff can order lenses by prescription from suppliers (Essilor, Hoya, Viet Phap)
- [ ] **OPT-03**: System tracks glasses order lifecycle: Ordered -> Processing -> Received -> Ready -> Delivered
- [ ] **OPT-04**: System blocks lens processing until full payment is received (no deposit model)
- [ ] **OPT-05**: Contact lenses (Ortho-K, soft) are prescribed via HIS, not sold through optical counter
- [ ] **OPT-06**: Staff can create combo pricing (preset combos + custom frame+lens combinations)
- [ ] **OPT-07**: System tracks warranty per sale (12 months frame + lens) with claim workflow (replace/repair/discount, case assessment with documents)
- [ ] **OPT-08**: System stores lens prescription history per patient with year-over-year comparison and lens replacement history
- [ ] **OPT-09**: Staff can perform barcode-based stocktaking (physical count vs. system, discrepancy report)

### Billing & Finance

- [ ] **FIN-01**: System generates single consolidated invoice per visit with charges from all departments (medical, optical, pharmacy, treatment)
- [ ] **FIN-02**: System tracks internal revenue allocation per department on each invoice line item
- [ ] **FIN-03**: System supports payment methods: cash, bank transfer, QR (VNPay/MoMo/ZaloPay), card (Visa/MC)
- [ ] **FIN-04**: System generates e-invoice (hoa don dien tu) per Vietnamese tax law
- [ ] **FIN-05**: System supports treatment package payments: full upfront or 50/50 split
- [ ] **FIN-06**: System enforces 50/50 split rule: 2nd payment due before mid-course session (5-session -> before session 3, 3-session -> before session 2)
- [ ] **FIN-07**: Manual discounts require manager approval before applying
- [ ] **FIN-08**: Refund processing requires manager/owner approval with full audit trail
- [ ] **FIN-09**: System maintains price change audit log (who changed, when, old/new values)
- [ ] **FIN-10**: System supports shift management: define shifts, assign staff, track revenue per shift, cash reconciliation at shift end

### Printing & Documents

- [ ] **PRT-01**: System prints drug prescriptions with clinic header, doctor name, patient info, drug list with dosage
- [ ] **PRT-02**: System prints glasses prescriptions (optical Rx) with refraction parameters
- [ ] **PRT-03**: System prints invoices/receipts with itemized charges and payment method
- [ ] **PRT-04**: System prints referral letters (giay chuyen vien) with patient info, diagnosis, reason
- [ ] **PRT-05**: System prints treatment consent forms with patient name, procedure type, date
- [ ] **PRT-06**: System prints pharmacy labels with patient name, drug name, dose, frequency, expiry

### Audit & Compliance

- [x] **AUD-01**: System records field-level audit trail for all medical record changes (who, when, what changed, old/new values)
- [x] **AUD-02**: System records access log for all user logins, logouts, and medical record views
- [x] **AUD-03**: Audit logs are immutable and retained for minimum 10 years
- [x] **AUD-04**: System supports ICD-10 coding from Day 1 for So Y te data readiness (deadline 31/12/2026)

### User Interface

- [x] **UI-01**: All UI text, labels, menus, and reports available in Vietnamese and English (Vietnamese primary)
- [x] **UI-02**: Staff can switch language preference per user session

### Architecture & Infrastructure

- [x] **ARC-01**: All external system integrations use ACL adapter pattern (domain ports + infrastructure adapters)
- [x] **ARC-02**: All aggregate roots include BranchId for future multi-branch support (EF Core global query filters)
- [x] **ARC-03**: Template engine supports adding new disease templates without application code changes (config/plugin-driven)
- [x] **ARC-04**: Azure SQL automatic daily backup with point-in-time recovery (35 days retention)
- [x] **ARC-05**: Azure Blob Storage with soft delete and versioning for medical images
- [x] **ARC-06**: Full data export capability ensuring data ownership by Ganka28 (no vendor lock-in)

## v2 Requirements

Deferred to post-launch (1-3 months after go-live). Tracked but not in current roadmap.

### Notifications

- **NTF-01**: Appointment reminder via Zalo OA (1 day before)
- **NTF-02**: Post-visit summary / thank-you message via Zalo OA
- **NTF-03**: Treatment session reminder via Zalo OA (when minimum interval has passed)
- **NTF-04**: Glasses ready notification via Zalo OA (triggered on order status "Ready")

### VIP Membership

- **VIP-01**: Tier-based membership program (spending threshold or treatment completion triggers upgrade)
- **VIP-02**: Auto-calculate tier upgrade with 12-month rolling window
- **VIP-03**: VIP discounts auto-applied: 10% off follow-up visits, 5-7% off services (configurable per service category)
- **VIP-04**: Family linkage (up to 2 members) with VIP benefits
- **VIP-05**: Tier change history tracking
- **VIP-06**: VIP not applied to prescription drugs or fixed-price items

### Reporting & Dashboards

- **RPT-01**: Revenue dashboard by department (medical, optical, pharmacy, treatment) with daily/weekly/monthly views
- **RPT-02**: Revenue breakdown by individual employee (all staff, not just doctors)
- **RPT-03**: Revenue breakdown by product brand
- **RPT-04**: Gross margin analysis per product and per segment
- **RPT-05**: Doctor performance reporting (revenue, patient count per doctor)
- **RPT-06**: Treatment effectiveness reporting (OSDI improvement trends across patients)
- **RPT-07**: Per-patient treatment progress report export (for patient or referring doctor)
- **RPT-08**: Research data export (Excel/CSV): exam data, treatment outcomes, refraction progression -- anonymized, >=1000 patients without error

### UI Enhancements

- **UIX-01**: Mobile-responsive design for tablet use during clinical exams
- **UIX-02**: Open API with documentation for third-party and device integration

### Optical Enhancements

- **OPX-01**: Trial lens inventory management for Ortho-K fitting (tracked separately, reusable)

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Online sales / e-commerce | Deferred to Phase 2 milestone. Not needed for clinic launch |
| BHYT (health insurance) integration | Future, when clinic decides to participate. ICD-10 from Day 1 provides data foundation |
| Multi-branch UI and management | Architecture supports it (BranchId), but no UI/logic for v1. Single location |
| Electronic queue / number display | Boutique model (~8 staff). Staff calls patients by name |
| SMS notifications | Zalo penetration sufficient in Vietnam. SMS is 3-5x more expensive. Add if Zalo non-adoption shown |
| Drug interaction checking | Only allergy alerts for v1. Pharmacist expertise handles interactions. Consider curated ophtho list in v2 |
| Electronic doctor signature | Doctor name on prescription sufficient for private clinic. Revisit when regulations mandate |
| Myopia Control template | Dry Eye is priority. Axial Length captured in refraction from Day 1 for data readiness. Same template engine, second template post-launch |
| Glaucoma, Keratoconus, Corneal transplant templates | Extensible architecture (ARC-03). Build post-launch per clinical demand |
| Device auto-import (slit lamp, OCT, etc.) | API-ready architecture (ARC-01). Integrate per-device post-launch. Manual upload for v1 |
| MISA auto-sync | Phase 1: manual export. Phase 2: consider API integration after proving data model |
| DICOM integration | v1 uses JPEG/PNG. DICOM deferred until specific device integration |
| AI image analysis | Doctors perform analysis. No training data or regulatory readiness |
| Real-time chat / patient portal | Massive scope. Zalo OA for patient communication |
| Birthday / holiday greetings | Not selected for v1 or v2 |
| Multi-language medical records | Medical content in Vietnamese (legal language). UI is bilingual |
| Tissue bank integration | Future expansion |
| Clinical research module | Partially addressed by research data export (RPT-08) |
| Eye specialty chain management | Future expansion, enabled by multi-tenancy (ARC-02) |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| AUTH-01 | Phase 1 | Complete |
| AUTH-02 | Phase 1 | Complete |
| AUTH-03 | Phase 1 | Complete |
| AUTH-04 | Phase 1 | Complete |
| AUTH-05 | Phase 1 | Complete |
| PAT-01 | Phase 2 | Pending |
| PAT-02 | Phase 2 | Pending |
| PAT-03 | Phase 2 | Pending |
| PAT-04 | Phase 2 | Complete |
| PAT-05 | Phase 2 | Pending |
| CLN-01 | Phase 3 | Pending |
| CLN-02 | Phase 3 | Pending |
| CLN-03 | Phase 3 | Pending |
| CLN-04 | Phase 3 | Pending |
| DRY-01 | Phase 4 | Pending |
| DRY-02 | Phase 4 | Pending |
| DRY-03 | Phase 4 | Pending |
| DRY-04 | Phase 4 | Pending |
| REF-01 | Phase 3 | Pending |
| REF-02 | Phase 3 | Pending |
| REF-03 | Phase 3 | Pending |
| IMG-01 | Phase 4 | Pending |
| IMG-02 | Phase 4 | Pending |
| IMG-03 | Phase 4 | Pending |
| IMG-04 | Phase 4 | Pending |
| DX-01 | Phase 3 | Pending |
| DX-02 | Phase 3 | Pending |
| RX-01 | Phase 5 | Pending |
| RX-02 | Phase 5 | Pending |
| RX-03 | Phase 5 | Pending |
| RX-04 | Phase 5 | Pending |
| RX-05 | Phase 5 | Pending |
| SCH-01 | Phase 2 | Pending |
| SCH-02 | Phase 2 | Pending |
| SCH-03 | Phase 2 | Pending |
| SCH-04 | Phase 2 | Pending |
| SCH-05 | Phase 2 | Pending |
| SCH-06 | Phase 2 | Pending |
| TRT-01 | Phase 9 | Pending |
| TRT-02 | Phase 9 | Pending |
| TRT-03 | Phase 9 | Pending |
| TRT-04 | Phase 9 | Pending |
| TRT-05 | Phase 9 | Pending |
| TRT-06 | Phase 9 | Pending |
| TRT-07 | Phase 9 | Pending |
| TRT-08 | Phase 9 | Pending |
| TRT-09 | Phase 9 | Pending |
| TRT-10 | Phase 9 | Pending |
| TRT-11 | Phase 9 | Pending |
| PHR-01 | Phase 6 | Pending |
| PHR-02 | Phase 6 | Pending |
| PHR-03 | Phase 6 | Pending |
| PHR-04 | Phase 6 | Pending |
| PHR-05 | Phase 6 | Pending |
| PHR-06 | Phase 6 | Pending |
| PHR-07 | Phase 6 | Pending |
| CON-01 | Phase 6 | Pending |
| CON-02 | Phase 6 | Pending |
| CON-03 | Phase 6 | Pending |
| OPT-01 | Phase 8 | Pending |
| OPT-02 | Phase 8 | Pending |
| OPT-03 | Phase 8 | Pending |
| OPT-04 | Phase 8 | Pending |
| OPT-05 | Phase 8 | Pending |
| OPT-06 | Phase 8 | Pending |
| OPT-07 | Phase 8 | Pending |
| OPT-08 | Phase 8 | Pending |
| OPT-09 | Phase 8 | Pending |
| FIN-01 | Phase 7 | Pending |
| FIN-02 | Phase 7 | Pending |
| FIN-03 | Phase 7 | Pending |
| FIN-04 | Phase 7 | Pending |
| FIN-05 | Phase 7 | Pending |
| FIN-06 | Phase 7 | Pending |
| FIN-07 | Phase 7 | Pending |
| FIN-08 | Phase 7 | Pending |
| FIN-09 | Phase 7 | Pending |
| FIN-10 | Phase 7 | Pending |
| PRT-01 | Phase 5 | Pending |
| PRT-02 | Phase 5 | Pending |
| PRT-03 | Phase 5 | Pending |
| PRT-04 | Phase 5 | Pending |
| PRT-05 | Phase 5 | Pending |
| PRT-06 | Phase 5 | Pending |
| AUD-01 | Phase 1 | Complete |
| AUD-02 | Phase 1 | Complete |
| AUD-03 | Phase 1 | Complete |
| AUD-04 | Phase 1 | Complete |
| UI-01 | Phase 1 | Complete |
| UI-02 | Phase 1 | Complete |
| ARC-01 | Phase 1 | Complete |
| ARC-02 | Phase 1 | Complete |
| ARC-03 | Phase 1 | Complete |
| ARC-04 | Phase 1 | Complete |
| ARC-05 | Phase 1 | Complete |
| ARC-06 | Phase 1 | Complete |

**Coverage:**
- v1 requirements: 96 total
- Mapped to phases: 96
- Unmapped: 0

---
*Requirements defined: 2026-02-28*
*Last updated: 2026-02-28 after roadmap creation (traceability updated)*

# Feature Research

**Domain:** Ophthalmology Clinic Management System (Boutique, Chronic Disease Focus, Vietnam)
**Researched:** 2026-02-28
**Confidence:** HIGH (domain well-understood from PROJECT.md, validated against industry standards)

## Feature Landscape

### Table Stakes (Users Expect These)

Features clinic staff assume exist. Missing these = system feels broken, staff revert to paper.

#### A. Patient Registration and Records

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Patient registration (name, phone, DOB, gender, ID format GK-YYYY-NNNN) | Every clinic system starts here. No patient record = nothing works downstream | LOW | Walk-in pharmacy customers need lighter registration (name + phone only). Two-tier registration model |
| Electronic medical records with visit history | Core of any HIS. Doctors need to see past visits at a glance | MEDIUM | Immutable visit records with amendment trail per PROJECT.md. This is the right approach -- industry standard for compliance |
| Patient search and lookup | Staff searches by name, phone, or patient ID dozens of times per day | LOW | Must be fast. Support partial name search, phone search, ID search |
| Allergy tracking with prescribing alerts | Patient safety non-negotiable. Allergy data must surface during prescribing | MEDIUM | Cross-module: allergy data in Patient module, alerts trigger in Pharmacy and Prescription modules |
| ICD-10 diagnosis coding | Required for So Y Te data connection (deadline 31/12/2026). Also industry standard | MEDIUM | Searchable ICD-10 lookup in both Vietnamese and English. Ophthalmology-relevant codes should be favorited/pinned for speed |

#### B. Clinical Workflow

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Customizable exam templates per disease group | Ophthalmology exams have structured data (unlike general practice free-text). Dry Eye template is priority | HIGH | Template engine must support: structured fields, scoring (OSDI), measurements (TBUT, Schirmer), left/right eye separation, and free-text sections. Architecture must allow adding new disease templates post-launch |
| Refraction data recording (SPH, CYL, AXIS, ADD, PD, VA, IOP, Axial Length) | Every eye clinic records refraction. This is the bread and butter of ophthalmology | MEDIUM | Must support: manifest refraction, autorefraction, cycloplegic refraction. Left/right eye with best-corrected VA. IOP with method (Goldmann, Tonopen, iCare). Time of IOP measurement |
| Medical image upload and viewing | Ophthalmology is image-heavy. Fluorescein staining, meibography, OCT scans | MEDIUM | Azure Blob Storage per PROJECT.md. Need: upload, tag by type, associate with visit, view in lightbox. DICOM support deferred (device integration is out of scope for v1) |
| Visit workflow status tracking | Staff needs to know where each patient is in the journey: reception -> tech -> doctor -> diagnostics -> doctor reads -> Rx -> cashier -> pharmacy/optical | MEDIUM | State machine for visit status. Visible dashboard showing all active patients and their current stage |
| Prescription writing (drugs and glasses) | Core clinical output. Doctor writes prescriptions, pharmacy/optical dispenses | MEDIUM | Drug prescriptions linked to pharmacy inventory. Glasses prescriptions linked to optical center. Both print-ready |

#### C. Appointment Scheduling

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Appointment booking (walk-in + pre-booked) | Clinics need to manage patient flow. Both walk-ins and pre-booked patients coexist | MEDIUM | Slot-based: 1 patient per doctor per slot. Durations vary by type (new 30min, follow-up 20min, treatment 30-45min, Ortho-K fitting 60-90min) |
| No double-booking enforcement | Scheduling integrity. Double-booking causes chaos in a small clinic | LOW | Hard constraint on the slot model. Reject conflicting bookings |
| Appointment calendar view | Staff needs visual overview of the day/week schedule | MEDIUM | Per-doctor calendar. Color-coded by appointment type. Show gaps clearly |

#### D. Pharmacy Management

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Drug inventory management (import/export/stock levels) | Pharmacy cannot operate without knowing what is in stock | MEDIUM | Batch tracking (required for expiry). Multiple suppliers. Import/export records for audit |
| Expiry date tracking with alerts | Patient safety and regulatory compliance. Dispensing expired drugs is a liability | MEDIUM | Alert when stock approaches expiry (configurable threshold, e.g., 30/60/90 days). FIFO dispensing logic |
| Minimum stock alerts | Prevent stockouts of commonly used drugs | LOW | Configurable per-drug minimum. Dashboard showing items below threshold |
| Prescription dispensing linked to HIS | Prescriptions from doctors flow directly to pharmacy. No re-entry | MEDIUM | Prescription validity enforcement (7 days per PROJECT.md). Link dispensing record to visit and prescription |
| Walk-in OTC sales | Pharmacy also serves non-patients buying OTC drugs | LOW | Lighter workflow: no prescription required. Still tracked for inventory and revenue |

#### E. Optical Center

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Frame inventory with barcode | Optical center needs to track frames (brand, model, color, size, price, stock) | MEDIUM | Barcode scanning for receiving and selling. Per PROJECT.md: no barcode system exists currently, must build from scratch |
| Lens ordering by prescription | Doctor writes glasses Rx, optical staff matches to lens products | MEDIUM | Lens database (Essilor, Hoya, Viet Phap suppliers). Match SPH/CYL/ADD to available lens types |
| Glasses order tracking (ordered -> processing -> ready -> delivered) | Orders take 1-3 days for processing. Staff and patients need status visibility | MEDIUM | State machine for order lifecycle. Trigger "glasses ready" Zalo notification at ready state |
| Contact lens management | Ortho-K and other contact lenses are prescribed via HIS | MEDIUM | Tied to clinical workflow (doctor prescribes, order from supplier, trial lens, fit, follow-up protocol) |
| Combo pricing (frame + lens) | Standard optical retail practice. Customers expect package deals | LOW | Configurable combo rules. Price = frame + lens + optional coatings, with potential bundle discount |
| Warranty management (12 months frame + lens) | Industry standard. Customers expect warranty tracking | LOW | Link warranty to sale record. Track warranty claims and replacements |

#### F. Billing and Payments

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Unified billing (single invoice per visit) | Patients expect one bill for everything, not separate bills from each department | MEDIUM | Aggregate charges from medical, optical, pharmacy, treatment into single invoice. Internal revenue attribution per department |
| Multiple payment methods (cash, bank transfer, QR, card) | Vietnam payment landscape demands this. Cash alone is insufficient | MEDIUM | VNPay/MoMo/ZaloPay QR codes, bank transfer, Visa/MC. VietQR is the national standard |
| Invoice printing | Tangible receipt is expected in Vietnam clinic context | LOW | Print-ready invoice with clinic branding, itemized charges, payment method |
| Discount management (manual + VIP auto-apply) | Clinics offer discounts. Manual discounts need manager approval | LOW | VIP discounts auto-applied. Manual discounts flagged for manager approval. Not applied to fixed-price items per PROJECT.md |
| Refund workflow | Refunds happen. Need proper tracking and approval | LOW | Manager/owner approval required. Full audit trail |

#### G. Reporting

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Revenue dashboard by department | Management needs to know which departments are profitable | MEDIUM | Medical, optical, pharmacy, treatment revenue breakdown. Daily/weekly/monthly/custom range |
| Data export (Excel/CSV) | Universal expectation for any business system. Needed for MISA manual export | LOW | Export patient lists, financial reports, prescription data. Anonymization option for research exports |

#### H. Authentication and Security

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Role-based access control (RBAC) | Different staff have different needs and permissions. Cashier should not edit medical records | MEDIUM | 6 roles per PROJECT.md: Doctor, Technician, Nurse, Cashier, Optical Staff, Manager. Granular configurable permissions (CRUD per entity type) |
| Audit trail for medical records | Regulatory requirement. All changes to medical records must be traceable | MEDIUM | Field-level audit: who changed what, when, old value, new value. Immutable audit log |
| Session management and authentication | Basic security. JWT + ASP.NET Identity per PROJECT.md | MEDIUM | Login, logout, password reset, session timeout. No patient-facing auth needed for v1 |

#### I. Printing and Documents

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Prescription printing (drug + glasses Rx) | Paper prescriptions are still standard in Vietnam clinics | MEDIUM | Formatted templates with clinic header, doctor name, patient info, drug list with dosage, or optical Rx with measurements |
| Invoice printing | Tangible receipt required | LOW | See billing section above |
| Referral letter printing | Doctors refer patients to hospitals. Need standardized referral form | LOW | Template with patient info, diagnosis, reason for referral, doctor signature line |
| Consent form printing | Required for procedures (IPL, LLLT). Patient must sign before treatment | LOW | Pre-formatted consent forms. Fill patient name, procedure type, date. Print for signature |
| Pharmacy labels | Labels for dispensed medications with patient name, drug, dosage instructions | LOW | Small label format. Patient name, drug name, dose, frequency, expiry |

#### J. Notifications

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Appointment reminders (Zalo OA) | Reduces no-shows by up to 40% per industry data. Standard practice | MEDIUM | 1 day before appointment per PROJECT.md. Requires Zalo OA setup and ZNS template approval. Pre-approved templates with dynamic parameters |
| Glasses ready notification (Zalo OA) | Patients are waiting for their glasses. Notification saves them calling to check | LOW | Triggered when order status changes to "ready" |

#### K. Bilingual UI

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Vietnamese and English interface | Clinic may serve foreign patients. Staff may prefer one language. International standard | MEDIUM | All UI text, form labels, reports in both languages. Vietnamese primary, English secondary. Drug names may be in English/Latin |

### Differentiators (Competitive Advantage)

Features that set Ganka28 apart. Not expected in generic clinic software, but core to the boutique chronic disease management model. These align with the clinic's stated Core Value.

#### A. Chronic Disease Tracking (PRIMARY DIFFERENTIATOR)

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Dry Eye template with structured scoring (OSDI, TBUT, Schirmer, Meibomian gland scoring) | The entire reason this system exists. Generic EHRs do not track these metrics in structured, queryable format. This enables treatment effectiveness analysis | HIGH | OSDI is a 12-question validated questionnaire (score 0-100). TBUT in seconds. Schirmer in mm. Meibomian gland expressibility scoring. All per-eye. All trackable over time |
| Visit-over-visit comparison for clinical metrics | Doctors need to see "Is this patient getting better?" at a glance. Side-by-side comparison of OSDI scores, TBUT, Schirmer across visits | HIGH | Line charts or comparison tables showing metric trends. Critical for treatment protocol decisions. No commercial system does this well for Dry Eye specifically |
| Medical image side-by-side comparison across visits | Fluorescein staining photos, meibography images compared visit-over-visit. Visual proof of treatment progress | HIGH | Image viewer with side-by-side or overlay mode. Date-labeled. Must handle multiple image types (Fluorescein, Meibography, OCT). This is what makes the system invaluable for chronic management |
| Treatment effectiveness reporting (OSDI trends) | Aggregate analysis: "How effective are our IPL treatments across all patients?" Data-driven clinical decisions | HIGH | Cohort analysis capability. OSDI reduction after N sessions. Average improvement by treatment type. This is research-grade functionality that boutique clinics crave |

#### B. Treatment Protocol Management

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| IPL/LLLT/lid care treatment packages (1-6 sessions) | Structured treatment packages with session tracking. Not a one-off visit model | MEDIUM | Package definition: treatment type, number of sessions, pricing (flexible per PROJECT.md). Each session records OSDI, images, notes |
| Session tracking with per-session OSDI | Every treatment session captures outcome data. Enables per-session effectiveness analysis | MEDIUM | Auto-prompt OSDI questionnaire at each session. Track pre/post or per-session scores. Auto-complete package when all sessions done |
| Minimum interval enforcement between sessions | Clinical safety: IPL sessions need minimum spacing (typically 2-4 weeks). System prevents scheduling too early | LOW | Configurable per treatment type. Soft warning (allow override with reason) vs hard block (configurable) |
| Treatment package payment flexibility (full upfront or 50/50 split) | Patient convenience. Some patients prefer paying upfront for discount, others split payments | LOW | Payment plans linked to treatment package. Track outstanding balances. Reminder for second payment |

#### C. VIP Membership Program

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Tier-based VIP membership (spending-based auto-upgrade) | Patient retention and loyalty. Industry data shows 15-25% revenue boost from VIP programs | MEDIUM | 12-month rolling window for tier calculation. Spending thresholds configurable. Auto-upgrade notifications |
| Family discount linkage (up to 2 family members) | Differentiator for family eye care. Parents bring children for myopia control | LOW | Link family member accounts. Apply VIP benefits to linked accounts. Cap at 2 per PROJECT.md |
| Configurable VIP benefits (% discounts per service category) | Flexibility to adjust program. Not a rigid one-size-fits-all discount | LOW | Per-tier, per-service-category discount rules. Exclude prescription drugs and fixed-price items |

#### D. Ortho-K Follow-up Protocol (Future, Post-Launch)

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Automated follow-up schedule generation | Ortho-K has mandatory follow-up protocol (1d -> 1w -> 1m -> 3m -> 6m). System auto-creates appointment schedule | MEDIUM | Template-driven schedule generation. Triggered on Ortho-K fitting completion. Auto-send Zalo reminders. Deferred per PROJECT.md but architecture should support it |
| Axial length growth tracking and charting | Myopia control effectiveness measured by axial length over time. Visual charts for parents | HIGH | Age-adjusted normative data comparison (like Doctora/mEYE Suite). Progression analysis. Deferred to post-launch but data model should capture axial length from Day 1 |

#### E. Clinical Decision Support (Lightweight)

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| OSDI severity classification | Auto-classify OSDI score: Normal (0-12), Mild (13-22), Moderate (23-32), Severe (33-100) | LOW | Simple calculation but surfaces the right information at the right time. Color-coded severity indicator on patient dashboard |
| Treatment protocol suggestions based on severity | "Patient OSDI is Severe + poor Meibomian gland function -> consider IPL" | MEDIUM | Rule-based, not AI. Configurable by clinic doctors. Suggestions, not mandates. Start simple, expand rules over time |

#### F. Post-Visit Patient Communication

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Post-visit summary via Zalo OA | Patient receives a summary: diagnosis, medications prescribed, next appointment, care instructions | MEDIUM | Template-based ZNS message. Personalizing with visit data. Builds trust and patient engagement. Reduces "what did the doctor say?" phone calls |
| Treatment session reminders via Zalo OA | "Your next IPL session is due. Please book your appointment" | LOW | Triggered when minimum interval has passed since last session. Drives treatment protocol compliance |

### Anti-Features (Commonly Requested, Often Problematic)

Features that seem good but create problems for this specific clinic context.

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| Full DICOM integration / device auto-import | "Auto-import images from slit lamp, OCT, keratograph" saves time | Every device has different integration protocols. Massive development effort for v1. Device APIs are proprietary and poorly documented. PROJECT.md explicitly defers this | Manual image upload for v1. API-ready architecture so device integration can be added per-device post-launch. One device at a time, not all at once |
| AI-powered image analysis | ModMed and others offer AI for retinal screening. Seems cutting-edge | Enormous complexity. Requires training data, regulatory considerations, ongoing maintenance. A boutique clinic does not need automated screening -- the doctors ARE the screening | Structured data templates that help doctors document their findings efficiently. Leave AI analysis to specialized device software |
| Drug interaction checking | "System should warn about drug interactions, not just allergies" | Requires a comprehensive drug interaction database (maintained externally). Significant ongoing cost and liability. False positives cause alert fatigue | Allergy alerts for v1 (per PROJECT.md). Pharmacist expertise handles interactions. Consider a curated ophthalmology-specific interaction list in v2 if demand exists |
| Online patient portal / e-commerce | "Patients should book online, view their records, buy products" | Massive scope expansion. Security surface area increases dramatically. Patient-facing features need different UX standards. PROJECT.md explicitly defers online sales | Zalo OA for patient communication. Staff-assisted booking. Online portal is a v2+ consideration after core system is stable |
| Electronic queue / number display | Standard in large hospitals. Patients take a number, wait | Boutique model with 8 staff and 1 patient per doctor per slot. Staff calls patients by name. Electronic queue adds complexity without value at this scale | Visit workflow dashboard for staff shows patient status. Patients are managed by name. Revisit if clinic scales to multiple locations |
| SMS notifications | "Not everyone uses Zalo" | In Vietnam, Zalo penetration is 70M+ users. SMS is 3-5x more expensive per message. Maintaining two channels doubles notification complexity | Zalo OA only for v1. If a patient does not use Zalo, staff makes a phone call. Add SMS in v2 if data shows significant Zalo non-adoption |
| Multi-language medical records | "Records should be in both Vietnamese and English for international patients" | Medical records should be in one language for consistency and legal clarity. UI can be bilingual, but clinical data in two languages creates confusion | Bilingual UI (labels, menus, buttons). Medical record content in Vietnamese (the legal language). Referral letters can be generated in English on request |
| BHYT (health insurance) integration | "We might participate in insurance later" | BHYT integration is complex, requires specific data formats, claim submission workflows, and ongoing regulatory compliance. Building it when you might not use it wastes months | Architecture should not prevent BHYT integration, but do not build it. When the clinic decides to participate, research and build specifically. ICD-10 from Day 1 provides the data foundation |
| Electronic doctor signature | "Digital signature on prescriptions for legal compliance" | Vietnamese regulations for electronic medical signatures are still evolving. Implementation requires digital certificate infrastructure. Doctor name on prescription is sufficient for private clinic practice | Print doctor name on prescriptions. Physical signature where required. Revisit when regulations mandate e-signatures |
| Real-time everything (WebSockets for all updates) | "Staff should see updates instantly without refreshing" | Adds significant infrastructure complexity. For an 8-person clinic, polling every 30-60 seconds provides a near-real-time experience at a fraction of the cost | Polling-based updates for most screens (30-60 second refresh). Consider WebSockets only for the visit workflow dashboard if staff demands it |
| Myopia Control template in v1 | "We treat myopia too, should be in v1" | Dry Eye is the primary differentiator and launch focus. Building two complex disease templates doubles the clinical template work. Myopia Control requires axial length tracking, growth charts, and different protocol logic | Capture axial length in refraction data from Day 1 (data readiness). Build Myopia Control template post-launch when Dry Eye template is validated and stable. Same template engine, second template |

## Feature Dependencies

```
[Patient Registration]
    |
    +--requires--> [Visit / Encounter Management]
    |                   |
    |                   +--requires--> [Clinical Exam Templates (Dry Eye)]
    |                   |                   |
    |                   |                   +--enhances--> [Visit-over-Visit Comparison]
    |                   |                   +--enhances--> [Treatment Effectiveness Reporting]
    |                   |
    |                   +--requires--> [Refraction Data Recording]
    |                   |                   |
    |                   |                   +--requires--> [Glasses Prescription]
    |                   |                                       |
    |                   |                                       +--requires--> [Optical Center: Lens Ordering]
    |                   |                                       +--requires--> [Optical Center: Order Tracking]
    |                   |
    |                   +--requires--> [Drug Prescription Writing]
    |                   |                   |
    |                   |                   +--requires--> [Pharmacy: Prescription Dispensing]
    |                   |
    |                   +--requires--> [Medical Image Upload]
    |                   |                   |
    |                   |                   +--enhances--> [Image Side-by-Side Comparison]
    |                   |
    |                   +--requires--> [ICD-10 Diagnosis]
    |                   |
    |                   +--requires--> [Visit Workflow Status Tracking]
    |                   |
    |                   +--enhances--> [Billing: Charge Aggregation]
    |
    +--requires--> [Allergy Tracking]
    |                   |
    |                   +--enhances--> [Pharmacy: Allergy Alerts during Dispensing]
    |
    +--enhances--> [VIP Membership Program]
    |                   |
    |                   +--requires--> [Billing: Discount Auto-Application]
    |
    +--enhances--> [Zalo OA Notifications]

[Pharmacy: Inventory Management]
    |
    +--independent (can be built before clinical workflow)
    +--requires--> [Pharmacy: Expiry Tracking]
    +--requires--> [Pharmacy: Min Stock Alerts]
    +--enhances--> [Pharmacy: Prescription Dispensing]

[Optical Center: Frame Inventory + Barcode]
    |
    +--independent (can be built before clinical workflow)
    +--enhances--> [Optical Center: Lens Ordering]
    +--enhances--> [Optical Center: Combo Pricing]
    +--requires--> [Optical Center: Warranty Management]

[Treatment Package Definition]
    |
    +--requires--> [Visit / Encounter Management]
    +--requires--> [Treatment Session Tracking]
    |                   |
    |                   +--enhances--> [Per-Session OSDI Tracking]
    |                   +--requires--> [Minimum Interval Enforcement]
    +--requires--> [Treatment Package Payments]

[RBAC / Authentication]
    |
    +--foundational (must be built first, everything depends on it)

[Appointment Scheduling]
    |
    +--requires--> [Patient Registration]
    +--enhances--> [Zalo OA: Appointment Reminders]
    +--enhances--> [Visit Workflow]

[Billing / Payments]
    |
    +--requires--> [Patient Registration]
    +--requires--> [Visit / Encounter Management]
    +--enhances--> [Revenue Dashboard]
    +--requires--> [Payment Methods Integration (QR, card)]

[Reporting / Dashboards]
    |
    +--requires--> [Billing data]
    +--requires--> [Clinical data]
    +--requires--> [Pharmacy data]
    +--requires--> [Optical data]
    +--note: Full CQRS with denormalized read models per PROJECT.md
```

### Dependency Notes

- **RBAC is foundational:** Every screen, every action depends on knowing who the user is and what they can do. Build first.
- **Patient Registration is the root entity:** Nothing clinical happens without a patient record. Build immediately after auth.
- **Visit/Encounter is the core hub:** Medical records, prescriptions, billing charges, images all hang off visits. This is the most important entity to get right.
- **Pharmacy and Optical inventories are independent:** They can be built and stocked before the clinical workflow connects to them. Good candidates for parallel development.
- **Reporting requires everything else:** Dashboards need data from all departments. Build last, after data is flowing.
- **Zalo OA integration is an enhancement layer:** The core system works without notifications. Zalo can be added after core workflows are stable.
- **Treatment packages require visits + clinical templates:** Cannot track IPL sessions without the ability to record an encounter with OSDI data.

## MVP Definition

### Launch With (v1)

Minimum viable system for clinic operations from Day 1.

- [ ] **Authentication + RBAC** -- Staff cannot use the system without logging in. Permissions prevent data access violations
- [ ] **Patient Registration** -- The root entity. Every workflow starts here
- [ ] **Appointment Scheduling** -- Clinic needs to manage daily patient flow (walk-in + pre-booked)
- [ ] **Visit/Encounter Management with Workflow Status** -- Core clinical workflow: patient arrives, moves through stages, completes visit
- [ ] **Dry Eye Exam Template (OSDI, TBUT, Schirmer, Meibomian scoring)** -- The primary differentiator. Structured chronic disease data capture
- [ ] **Refraction Data Recording** -- Every eye exam records refraction. Table stakes
- [ ] **Medical Image Upload and Viewing** -- Doctors need to store and review images
- [ ] **Drug Prescription Writing** -- Clinical output: doctor prescribes medications
- [ ] **Glasses Prescription Writing** -- Clinical output: doctor prescribes glasses
- [ ] **ICD-10 Diagnosis Lookup** -- Regulatory data readiness (So Y Te deadline 2026)
- [ ] **Pharmacy Inventory Management** -- Pharmacy must track stock, batches, expiry
- [ ] **Pharmacy Prescription Dispensing** -- Link prescriptions to dispensing
- [ ] **Optical Center Frame Inventory + Barcode** -- Optical needs to track frames
- [ ] **Glasses Order Tracking** -- Multi-day processing workflow needs status tracking
- [ ] **Unified Billing + Multiple Payment Methods** -- Single invoice per visit, QR/cash/card
- [ ] **Prescription and Invoice Printing** -- Paper output still required in Vietnam
- [ ] **Audit Trail** -- Medical record compliance from Day 1
- [ ] **Bilingual UI (Vietnamese + English)** -- Per PROJECT.md requirements

### Add After Validation (v1.x)

Features to add once core is working and staff is comfortable with the system.

- [ ] **Visit-over-visit metric comparison** -- Add when doctors start asking "how is this patient trending?" (likely within weeks of launch)
- [ ] **Image side-by-side comparison** -- Add when enough visit images accumulate to compare
- [ ] **IPL/LLLT Treatment Packages + Session Tracking** -- Add when clinic starts booking treatment sessions. Requires encounter system to be stable
- [ ] **Per-session OSDI tracking** -- Add with treatment packages
- [ ] **VIP Membership Program** -- Add when patient base grows enough to justify tiers. Not needed at clinic opening
- [ ] **Zalo OA Appointment Reminders** -- Add after Zalo OA account is created and ZNS templates approved
- [ ] **Post-visit Summary via Zalo** -- Add after basic Zalo integration is working
- [ ] **Treatment Session Reminders via Zalo** -- Add after treatment packages and Zalo are both working
- [ ] **Revenue Dashboard by Department** -- Add once billing data accumulates (meaningful after 1-2 months)
- [ ] **Gross Margin Analysis** -- Add after cost data is consistently captured in pharmacy/optical
- [ ] **Referral Letter and Consent Form Printing** -- Add when doctors request specific templates
- [ ] **Pharmacy Labels** -- Add when pharmacy staff requests. Low effort

### Future Consideration (v2+)

Features to defer until core system is validated and clinic operations are stable.

- [ ] **Myopia Control Template** -- Build on the proven template engine. Deferred per PROJECT.md
- [ ] **Axial Length Growth Charts (age-adjusted)** -- Requires Myopia Control template. Data captured from Day 1 via refraction
- [ ] **Ortho-K Follow-up Protocol Auto-generation** -- Build after Myopia Control template
- [ ] **Treatment Effectiveness Reporting (cohort analysis)** -- Requires months of treatment data. Build when data volume justifies it
- [ ] **Doctor Performance Reporting** -- Sensitive. Add after management processes are established
- [ ] **Device Integration (slit lamp, OCT, keratograph)** -- Per-device, API-ready architecture. Add one at a time post-launch
- [ ] **MISA API Auto-sync** -- Phase 1 is manual export. Auto-sync in Phase 2
- [ ] **BHYT Insurance Integration** -- Only if clinic decides to participate
- [ ] **Online Patient Booking** -- Patient-facing feature, deferred
- [ ] **Glaucoma, Keratoconus, Corneal Transplant Templates** -- Same template engine, new templates
- [ ] **Drug Interaction Checking** -- Curated ophthalmology-specific list if demand exists

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| Authentication + RBAC | HIGH | MEDIUM | P1 |
| Patient Registration | HIGH | LOW | P1 |
| Appointment Scheduling | HIGH | MEDIUM | P1 |
| Visit/Encounter Management + Workflow | HIGH | HIGH | P1 |
| Dry Eye Exam Template | HIGH | HIGH | P1 |
| Refraction Data Recording | HIGH | MEDIUM | P1 |
| Medical Image Upload/View | HIGH | MEDIUM | P1 |
| Drug Prescription Writing | HIGH | MEDIUM | P1 |
| Glasses Prescription Writing | HIGH | MEDIUM | P1 |
| ICD-10 Diagnosis Lookup | HIGH | MEDIUM | P1 |
| Pharmacy Inventory Management | HIGH | MEDIUM | P1 |
| Pharmacy Dispensing | HIGH | MEDIUM | P1 |
| Optical Frame Inventory + Barcode | HIGH | MEDIUM | P1 |
| Glasses Order Tracking | HIGH | MEDIUM | P1 |
| Unified Billing + Payments | HIGH | HIGH | P1 |
| Prescription/Invoice Printing | HIGH | MEDIUM | P1 |
| Audit Trail | HIGH | MEDIUM | P1 |
| Bilingual UI | MEDIUM | MEDIUM | P1 |
| Visit-over-Visit Comparison | HIGH | MEDIUM | P2 |
| Image Side-by-Side Comparison | HIGH | MEDIUM | P2 |
| Treatment Packages (IPL/LLLT) | HIGH | MEDIUM | P2 |
| Session Tracking + Per-Session OSDI | HIGH | MEDIUM | P2 |
| VIP Membership Program | MEDIUM | MEDIUM | P2 |
| Zalo OA Notifications (all types) | MEDIUM | MEDIUM | P2 |
| Revenue Dashboard | MEDIUM | MEDIUM | P2 |
| Gross Margin Analysis | MEDIUM | MEDIUM | P2 |
| Referral/Consent Form Printing | LOW | LOW | P2 |
| Pharmacy Labels | LOW | LOW | P2 |
| Myopia Control Template | MEDIUM | HIGH | P3 |
| Axial Length Growth Charts | MEDIUM | HIGH | P3 |
| Ortho-K Follow-up Protocol | MEDIUM | MEDIUM | P3 |
| Treatment Effectiveness Reporting | MEDIUM | HIGH | P3 |
| Device Integration | MEDIUM | HIGH | P3 |
| MISA API Auto-sync | LOW | MEDIUM | P3 |

**Priority key:**
- P1: Must have for launch -- clinic cannot open without it
- P2: Should have within first 1-3 months -- enhances clinical value and operations
- P3: Nice to have -- future consideration based on clinic needs

## Competitor Feature Analysis

| Feature | Generic Vietnam HIS (eHospital, VNPT HIS) | International Ophthalmology EHR (ModMed, Nextech) | Myopia-Specific (Doctora, mEYE Suite) | Ganka28 Approach |
|---------|---------------------------------------------|---------------------------------------------------|---------------------------------------|-------------------|
| Patient Registration | Basic, insurance-focused | Comprehensive, insurance-driven | Minimal (child/parent focus) | Two-tier: full clinical + light pharmacy walk-in |
| Exam Templates | Generic, not ophthalmology-specific | Ophthalmology-specific, extensive | Myopia-only | Ophthalmology-specific, disease-group templates starting with Dry Eye |
| Dry Eye Tracking | Not available | Basic (unstructured notes) | Not available | STRUCTURED: OSDI, TBUT, Schirmer, Meibomian scoring with trending |
| Image Management | Basic upload | DICOM integration, device auto-import | Topography images | Upload + view + visit-over-visit comparison. DICOM deferred |
| Treatment Packages | Not a concept | CPT-based procedure tracking | Atropine/Ortho-K protocols | Session-based packages with OSDI per session and interval enforcement |
| Optical Center | Not integrated | Some have optical modules | Not available | Fully integrated: frame inventory, barcode, lens ordering, order tracking, warranty |
| Pharmacy | Basic dispensing | e-Prescribing (US-focused) | Not available | Full inventory + batch/expiry + prescription link + OTC sales |
| VIP/Loyalty | Not available | Not available | Not available | Tier-based spending program with family linkage |
| Notifications | SMS (expensive) | Patient portal, email | Email reports | Zalo OA (cost-effective for Vietnam market) |
| Billing | BHYT insurance claims | US insurance billing | Not applicable | Cash/QR/card, unified invoice, department revenue tracking |
| Compliance | So Y Te reporting | HIPAA/US regulatory | Minimal | ICD-10 from Day 1, So Y Te data readiness, immutable records + audit trail |
| Language | Vietnamese only | English only | English only | Bilingual Vietnamese + English |

## Sources

- [Revival Health: Choosing the Best Ophthalmology Software 2025](https://www.revivalhealth.io/blog/choosing-the-best-ophthalmology-software-top-contenders-for-2025) -- Competitor landscape overview
- [ClinikEHR: Clinic Management Software for Eye Clinics](https://clinikehr.com/blog/clinic-management-software-for-eye-clinics) -- Feature checklist for eye clinics
- [Nextech Ophthalmology EHR](https://www.nextech.com/ophthalmology/ehr-pm-software) -- Market leader feature set
- [ModMed Ophthalmology](https://www.modmed.com/specialties/ophthalmology/) -- AI features, clinical workflow
- [SotaERP: Top 5 Clinic Management Software in Vietnam](https://erp.sota-solutions.com/blog/business-management-4/top-5-best-clinic-management-software-in-vietnam-6) -- Vietnam market context
- [Doctora Myopia Management Software](https://doctora.io/myopia-management-software) -- Myopia-specific features, axial length tracking
- [Myopia Profile: mEYE Suite](https://www.myopiaprofile.com/product/mEYE-Suite) -- Myopia management instrument/software ecosystem
- [Zalo Notification Service API](https://developers.zalo.me/docs/zalo-notification-service/bat-dau/gioi-thieu-zalo-notification-service-api) -- ZNS documentation
- [PMC: Digital Health Policy Vietnam](https://pmc.ncbi.nlm.nih.gov/articles/PMC8867296/) -- Vietnam HIS requirements and compliance
- [JMIR: Status of Digital Health Technology in Vietnamese Hospitals](https://formative.jmir.org/2025/1/e53483) -- Vietnam healthcare IT adoption
- [Optometry Times: Meibography for Dry Eye](https://www.optometrytimes.com/view/why-meibography-can-be-game-changer-treating-dry-eye) -- Clinical imaging standards
- [EyeWiki: IPL Therapy](https://eyewiki.org/Intense_Pulsed_Light_(IPL)_Therapy) -- IPL treatment protocol reference
- [Langate: Pharmacy Inventory Management Features](https://langate.com/news-and-blog/10-main-features-of-a-pharmacy-inventory-management-system/) -- Pharmacy module feature standards
- [OptoSoft: Optical Software](https://www.opto-soft.com/) -- Optical POS and inventory features
- [Frier Levitt: EMR Audit Trails](https://www.frierlevitt.com/articles/understanding-emr-audit-trails-importance-and-implications-for-medical-record-alteration/) -- Audit trail compliance requirements

---
*Feature research for: Ophthalmology Clinic Management System (Boutique, Chronic Disease Focus, Vietnam)*
*Researched: 2026-02-28*

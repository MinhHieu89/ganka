# Phase 5: Prescriptions & Document Printing - Context

**Gathered:** 2026-03-05
**Status:** Ready for planning

<domain>
## Phase Boundary

Doctors can write drug and optical prescriptions during clinical visits with MOH-compliant formatting, system checks drug allergies during prescribing, and all clinical documents can be printed as PDFs. This includes: drug prescription writing from pharmacy catalog + off-catalog free-text, optical prescription (glasses Rx) with refraction parameters, drug-allergy warnings, and PDF generation for drug Rx, optical Rx, referral letters, consent forms, and pharmacy labels.

This phase does NOT include: invoice/receipt printing (Phase 7 — depends on billing system), drug inventory/dispensing (Phase 6), treatment protocols (Phase 9), or pharmacy stock management (Phase 6).

</domain>

<decisions>
## Implementation Decisions

### Drug Prescription Flow
- Hybrid dosage entry: structured fields (dose, frequency, duration, route) auto-generate instruction text, PLUS free-text override field for doctor customization
- Add-then-edit pattern per drug line: doctor clicks "Add Drug" → fills form in dialog/inline row → clicks "Add" to commit. Edit/remove available per line. Same interaction pattern as Diagnosis section
- Separate "Drug Prescription" VisitSection on visit detail page (not combined with optical Rx)
- Drug Rx and Optical Rx are fully independent — doctor can write either, both, or neither per visit
- Prescriptions become read-only after visit sign-off (follows existing immutability pattern)

### Allergy Warning System
- Inline red warning banner appears immediately below drug field when allergy match detected during drug selection
- Blocking AlertDialog confirmation when doctor tries to save/finalize a prescription with allergy conflict — doctor must explicitly confirm or cancel
- Both mechanisms combined: awareness during selection + hard stop before commit

### Optical Rx Design
- Separate "Optical Prescription" VisitSection on visit detail page
- Auto-populate SPH/CYL/AXIS/ADD/PD from current visit's manifest refraction, doctor can edit values before finalizing
- Distance Rx / Near Rx sections for bifocal/progressive prescriptions
- Far PD / Near PD fields (interpupillary distance for distance and near)
- Lens type recommendation field (single vision / bifocal / progressive / reading)
- Fully independent from drug Rx — can exist without drug prescription and vice versa

### Document Layout & Print Approach
- Backend PDF generation via QuestPDF (.NET library, free/MIT) for all document types
- Frontend gets PDF URL, opens in new browser tab for print/download
- A5 paper size (148 x 210mm) for drug prescriptions — standard Vietnamese clinical convention
- A4 for other documents (referral letters, consent forms, optical Rx) — Claude's discretion on per-document sizing
- Configurable clinic header template: admin can set logo image, clinic name, address, phone, fax, license number, tagline — stored as clinic settings, all documents pull from this config
- Invoice/receipt printing (PRT-03) deferred to Phase 7 when billing system exists

### Drug Catalog Structure
- Drug catalog entity lives in Pharmacy module (already scaffolded, empty) — Clinical module references via Contracts/cross-module query
- Seeded with ~50-100 common ophthalmic drugs (eye drops, antibiotics, anti-inflammatories, etc.)
- All Vietnamese seed data must use proper diacritics (consistent with Phase 01.2 decision)
- Essential fields per drug: name (EN/VI), generic name, form (drops/tablet/ointment/injection), strength (e.g., 0.5%), route (topical/oral/IM), unit (bottle/tube/box), default dosage template
- Phase 6 adds supplier, price, batch tracking, stock levels to existing catalog entries
- Admin UI for drug catalog management (add/edit drugs, Excel import for bulk additions)
- Off-catalog drugs: doctor types drug name + dosage manually as free-text entry, flagged as "off-catalog" (no stock deduction link in Phase 6)

### Claude's Discretion
- Copy-from-previous-visit shortcut for optical Rx (whether to include and how)
- Exact A4 vs A5 sizing per non-prescription document type
- Staining score / grading display on printed documents
- Pharmacy label layout and label paper size
- Referral letter and consent form content templates
- Drug catalog seed data selection (specific drugs to include)
- Loading states and error handling
- Exact MOH required field placement on prescription PDF

</decisions>

<specifics>
## Specific Ideas

- Drug prescriptions on A5 paper is the Vietnamese clinic standard per MOH conventions — doctors and pharmacists expect this format
- The hybrid dosage entry (structured + free-text override) matches how Vietnamese doctors work: they often use standard dosage patterns but need flexibility for special cases
- Allergy warning uses the existing patient allergy system (Phase 2) — no new allergy data model needed, just cross-reference patient allergies against selected drug during prescribing
- All Vietnamese text in seed data must use proper diacritics (e.g., "Thuốc nhỏ mắt" not "Thuoc nho mat")
- Configurable clinic header supports future multi-branch expansion (each branch has own header config)

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- **VisitSection**: Collapsible card wrapper with `headerExtra` slot — use for Drug Rx and Optical Rx sections
- **Icd10Combobox**: Popover + Command search pattern — adapt for DrugCombobox (search drug catalog)
- **DiagnosisSection**: Add-then-edit interaction pattern (dialog/inline, per-item save) — mirror for prescription drug lines
- **RefractionForm**: OD/OS side-by-side grid — reference for optical Rx layout
- **AllergyAlert**: Banner component with full/compact modes — adapt for drug-allergy warning inline banner
- **Patient allergy system**: Complete with catalog, add/remove features, repositories — query for allergy checking
- **handleServerValidationError**: RFC 7807 error handler — reuse for prescription form validation
- **AllergyCatalogSeeder**: IHostedService seeding pattern — mirror for DrugCatalogSeeder
- **IAzureBlobService**: Blob storage for clinic logo upload in header config

### Established Patterns
- **Visit aggregate child entities**: Backing field collections, `EnsureEditable()` guard, `Add___()` domain methods
- **Per-feature files**: Command/Handler with FluentValidation, `Result<T>` return
- **Minimal API endpoints**: MapGroup with RequireAuthorization, `bus.InvokeAsync` pattern
- **React Hook Form + Zod**: zodResolver with validation, Controller pattern
- **TanStack Query**: queryKey factories, mutation with cache invalidation
- **openapi-fetch**: Typed API client with auth middleware
- **Enum normalization**: Backend int enums <-> frontend string maps
- **onError toast pattern**: All React Query mutations must have onError with toast.error

### Integration Points
- **Visit entity**: Add `_prescriptions` and `_opticalPrescriptions` backing field collections + domain methods
- **ClinicalDbContext**: Add `DbSet<DrugPrescription>`, `DbSet<OpticalPrescription>`, create migration
- **VisitDetailPage**: Insert `<DrugPrescriptionSection>` and `<OpticalPrescriptionSection>` between DiagnosisSection and SignOffSection
- **Pharmacy module**: Drug catalog entities, DrugCatalogSeeder, admin management endpoints
- **Patient module**: Cross-module query for patient allergies (allergy checking during prescribing)
- **WorkflowStage.Rx**: Already defined as stage 5 — prescription writing naturally maps here
- **Permission system**: `PermissionModule.Pharmacy` already exists — add prescribing permissions
- **i18n**: Add keys to `en/clinical.json`, `vi/clinical.json`, new `en/pharmacy.json`, `vi/pharmacy.json`
- **Clinic settings**: New settings entity/table for configurable clinic header (logo, name, address, etc.)

</code_context>

<deferred>
## Deferred Ideas

- Invoice/receipt printing (PRT-03) — deferred to Phase 7 when billing system is built
- Full MOH drug database import — Vietnamese Dược điển is too large for boutique clinic needs; seed ~50-100 common drugs instead
- Drug interaction checking (beyond allergy warnings) — explicitly out of scope for v1 per PROJECT.md

</deferred>

---

*Phase: 05-prescriptions-document-printing*
*Context gathered: 2026-03-05*

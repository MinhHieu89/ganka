# Phase 6: Pharmacy & Consumables - Context

**Gathered:** 2026-03-05
**Status:** Ready for planning

<domain>
## Phase Boundary

Pharmacist can manage drug inventory with batch/expiry tracking, import stock via supplier invoice or Excel, dispense drugs against HIS prescriptions with auto FEFO batch selection, process walk-in OTC sales, and manage a separate consumables warehouse for treatment supplies. This phase builds on the drug catalog created in Phase 5.

This phase does NOT include: billing/payment collection (Phase 7), treatment session auto-deduction of consumables (Phase 9), invoice/receipt generation (Phase 7), or optical inventory (Phase 8).

</domain>

<decisions>
## Implementation Decisions

### Stock Import & Batch Management
- Both import methods: supplier invoice form for day-to-day, Excel bulk import for large orders and initial stock load
- Purchase price tracked per batch — enables cost-of-goods-sold and margin analysis in Phase 7
- Selling price is per drug (single selling price on catalog), not per batch — simpler for cashier and invoicing
- Supplier entity with name, contact info, plus default purchase price per drug per supplier — auto-fills price during stock import
- Batch fields: batch number, expiry date, quantity, purchase price, supplier reference
- Phase 5 drug catalog already has: name (EN/VI), generic name, form, strength, route, unit, default dosage template — Phase 6 adds: selling price, min stock level, supplier-drug pricing

### Dispensing Workflow
- Both queue + patient lookup: dedicated pharmacy queue page as primary (pending prescriptions sorted by time), plus accessible from patient profile
- Queue page shows count badge in sidebar navigation for pending prescriptions
- Auto FEFO (First Expiry, First Out) with manual override — system suggests earliest-expiry batches, pharmacist can select different batches if needed
- All-or-nothing per drug line — no partial dispensing. Each drug line must be fully dispensed or skipped entirely
- 7-day prescription expiry: warn but allow override — warning banner + confirmation dialog with reason logged, pharmacist can still dispense expired prescriptions
- Dispensing creates a dispensing record linking prescription line → batch(es) used → quantities deducted

### Walk-in OTC Sales
- Claude's discretion on implementation approach (quick sale form vs mini-prescription) — pick what fits the data model best
- Customer linkage is optional — staff can link to existing patient/walk-in customer or process anonymous sale
- No payment collection in Phase 6 — only records the sale and deducts stock. Phase 7 billing handles payment
- No receipt/invoice generation in Phase 6 — deferred to Phase 7 unified billing. Pharmacist gives manual receipt if needed
- OTC sales still auto-deduct stock via same batch/FEFO mechanism as prescription dispensing

### Alerts & Thresholds
- Expiry alerts at configurable thresholds (30/60/90 days) per PHR-03
- Min stock alerts when drug falls below configurable minimum level per drug per PHR-04
- Alert presentation: Claude's discretion (dashboard notifications, sidebar badges, in-context warnings)

### Consumables Warehouse
- Fully separate section — own sidebar nav item and pages, independent from pharmacy
- Configurable per item: each consumable marked as "expiry-tracked" (batch model with batch number, expiry, FEFO) or "simple stock" (quantity-only tracking)
- Seeded with ~10-15 core IPL/LLLT supplies: IPL gel, eye shields, LLLT disposable tips, lid care pads, sterile wipes, anesthetic drops, etc.
- Manual stock management only in Phase 6 (add/remove/adjust) — auto-deduction from treatment sessions deferred entirely to Phase 9
- Same min stock alert pattern as pharmacy drugs

### Claude's Discretion
- OTC sale data model approach (quick sale form vs mini-prescription pipeline)
- Alert presentation design (dashboard widget, toast notifications, sidebar badges)
- Excel import template format and validation rules
- Pharmacy queue page layout and filtering options
- Consumables seed data selection (specific items to include)
- Stock adjustment workflow (manual corrections, write-offs)
- Dispensing confirmation UI details
- Loading states and error handling

</decisions>

<specifics>
## Specific Ideas

- Pharmacy is a "tu thuoc" (cabinet pharmacy, part of clinic) — not a separate legal entity. Staff are clinic employees
- Multiple suppliers per drug is normal — same drug sourced from different suppliers at different prices
- FEFO is the standard pharmaceutical dispensing practice in Vietnam — regulators expect this
- Vietnamese pharmacy labels (PRT-06) were created in Phase 5 — dispensing in Phase 6 can link to label printing
- Walk-in customers (PAT-02) already exist as lightweight patient records (name + phone only) from Phase 2
- All Vietnamese seed data must use proper diacritics (established pattern from Phase 01.2 and Phase 5)

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- **PharmacyDbContext**: Scaffolded with "pharmacy" schema, empty — ready for entities
- **AllergyCatalogSeeder**: IHostedService seeding pattern — mirror for DrugCatalogSeeder (Phase 5) and ConsumableCatalogSeeder
- **DataTable**: Generic table component — use for drug inventory list, batch list, dispensing queue, consumables list
- **AllergyAlert**: Banner component — adapt for expiry/low-stock warning banners
- **handleServerValidationError**: RFC 7807 error handler — reuse for all pharmacy forms
- **Drug catalog entities**: Created in Phase 5 in Pharmacy module — extend with selling price, min stock, supplier pricing

### Established Patterns
- **Per-feature vertical slices**: Command/Handler with FluentValidation, Result<T> return
- **Minimal API endpoints**: MapGroup with RequireAuthorization, bus.InvokeAsync pattern
- **IHostedService seeders**: Idempotent seeding (skip if data exists) for catalog data
- **Cross-module queries**: Via Contracts project (e.g., Clinical module queries Pharmacy.Contracts for drug catalog)
- **React Hook Form + Zod**: zodResolver with validation, Controller pattern
- **TanStack Query**: queryKey factories, mutation with cache invalidation
- **Vietnamese_CI_AI collation**: For drug name search (accent-insensitive, case-insensitive)
- **onError toast pattern**: All React Query mutations must have onError with toast.error

### Integration Points
- **Pharmacy module**: Extend drug catalog with inventory fields, add batch/supplier/dispensing entities
- **Sidebar navigation**: Add Pharmacy and Consumables nav items with pending count badge
- **Patient module**: Cross-module query for walk-in customer records (PAT-02)
- **Clinical module**: Cross-module query for pending prescriptions (dispensing queue)
- **Permission system**: PermissionModule.Pharmacy already exists — add dispensing, stock management, OTC sale permissions
- **i18n**: Add/extend pharmacy.json translation files (EN/VI)
- **Phase 5 drug catalog**: Phase 6 extends catalog entities with selling price, min stock level, supplier-drug relationships
- **Phase 5 prescription printing**: Dispensing can trigger pharmacy label printing (PRT-06)

</code_context>

<deferred>
## Deferred Ideas

- Auto-deduction of consumables from treatment sessions — deferred to Phase 9 (Treatment Protocols)
- Payment collection for OTC sales — deferred to Phase 7 (Billing & Finance)
- Receipt/invoice generation for any pharmacy transaction — deferred to Phase 7
- Drug interaction checking (beyond allergy warnings) — explicitly out of scope for v1 per PROJECT.md
- Controlled substance tracking — not needed (clinic has no controlled substances per PROJECT.md)

</deferred>

---

*Phase: 06-pharmacy-consumables*
*Context gathered: 2026-03-05*

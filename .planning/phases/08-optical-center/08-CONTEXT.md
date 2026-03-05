# Phase 8: Optical Center - Context

**Gathered:** 2026-03-05
**Status:** Ready for planning

<domain>
## Phase Boundary

Staff can manage frame/lens inventory with barcode scanning, track glasses orders through their full lifecycle (Ordered → Processing → Received → Ready → Delivered), create preset and custom combo pricing, handle warranty claims with supporting documents, store lens prescription history per patient with year-over-year comparison, and perform barcode-based stocktaking with discrepancy reports. Contact lenses (Ortho-K) are prescribed via HIS, not sold through optical counter.

This phase does NOT include: billing/payment collection (Phase 7 — already handles payment enforcement before processing), treatment protocols (Phase 9), patient notifications via Zalo OA (v2 — NTF-04), or drug/consumable inventory (Phases 5-6).

</domain>

<decisions>
## Implementation Decisions

### Barcode System & Scanning
- EAN-13 barcode format for all frames
- Mixed barcode source: scan manufacturer barcodes when frames already have them, generate + print labels for untagged frames using clinic prefix
- Dual scanning support: USB barcode scanner as primary (keyboard input to focused field), phone/tablet camera as fallback (web-based scanner for mobile stocktaking)
- Barcode label printing: Claude's discretion on approach — support both thermal label printer and A4 sticker sheet output

### Frame Catalog Structure
- Full attribute set per frame: brand, model, color, size (lens width/bridge/temple as separate fields), material (metal/plastic/titanium), gender (M/F/unisex), frame type (full-rim/semi-rimless/rimless), selling price, cost price
- Barcode field (EAN-13) — either scanned from manufacturer or system-generated
- Stock quantity tracked per frame SKU (brand + model + color + size combination)

### Lens Catalog & Ordering
- Hybrid model: bulk stock for common lens powers + custom orders per prescription for unusual parameters
- Per-piece quantity tracking for stocked lenses — system deducts when assigned to glasses order, low-stock alerts
- Custom lens orders placed with suppliers (Essilor, Hoya, Viet Phap) per patient prescription, tracked individually
- Lens catalog attributes: brand, type (single vision/bifocal/progressive/reading), material (CR-39/polycarbonate/hi-index), coating options, power range

### Supplier Management
- Shared supplier entity with Pharmacy module (Phase 6) — tag suppliers by type (drug/optical/both)
- Essilor, Hoya, Viet Phap pre-configured as optical suppliers
- Cross-module supplier query via Contracts project (same pattern as pharmacy)

### Glasses Order Lifecycle
- Orders created from HIS optical Rx (Phase 5) — optical staff selects frame + lens from catalog, links order to prescription
- No walk-in/external Rx order path for v1 — all orders originate from clinic optical prescriptions
- Processing types: in-house (simple lenses) vs outsourced to supplier lab (complex lenses like progressives) — tracked per order
- Configurable estimated delivery: in-house same-day to 1 day, outsourced 1-3 business days. Estimated date set at order creation, system alerts when overdue
- Status transitions: Ordered → Processing → Received → Ready → Delivered
- Payment enforcement: system blocks Processing status until full payment confirmed (OPT-04 — uses Phase 7 billing integration)
- Patient notification: manual for v1 (staff contacts from "Ready for pickup" list), Zalo OA auto-notification deferred to v2 (NTF-04)

### Combo Pricing
- Preset combo packages: admin creates named packages (frame + lens combination) with a fixed combo price
- Custom combos: optical staff can create ad-hoc frame + lens combinations at order time with manual price override/discount
- Combo pricing applied at order creation, flows to Phase 7 billing as a single optical line item

### Warranty Management
- 12-month warranty on frame + lens per sale, starting from delivery date (status = Delivered)
- Three resolution types: Replace, Repair, Discount
- Manager approval required for replacements only — repairs and discounts handled by optical staff directly
- Warranty claim record includes: claim date, resolution type, case assessment notes, supporting photos/documents (uploaded to Azure Blob)
- Full audit trail on warranty claims

### Lens Prescription History (OPT-08)
- System stores complete lens prescription per patient per glasses order (linked to optical Rx)
- Year-over-year comparison view showing prescription changes over time
- Lens replacement history: when a patient returns for new lenses, previous prescriptions visible for reference

### Contact Lenses / Ortho-K (OPT-05)
- Contact lenses prescribed via HIS clinical workflow (doctor writes Rx), not sold through optical counter
- No contact lens inventory management in optical center — tracked separately if needed

### Stocktaking (OPT-09)
- Barcode-based stocktaking: scan frame barcodes during physical count
- Enter physical count per scanned item, system compares against expected inventory
- Discrepancy report: items with count mismatch (over/under), items not scanned (missing), items scanned but not in system
- Phone camera scanning for mobile stocktaking on the floor (fallback scanning method)

### Claude's Discretion
- Barcode label layout and paper sizing (thermal vs A4 sheet)
- Web-based barcode scanner library selection (quagga.js, html5-qrcode, or similar)
- Barcode generation approach (EAN-13 prefix allocation for clinic-generated barcodes)
- Lens catalog seed data (common power ranges to stock)
- Frame catalog admin page layout and filtering
- Glasses order detail page layout
- Stocktaking session workflow (start/pause/complete)
- Overdue order alert presentation
- Loading states and error handling

</decisions>

<specifics>
## Specific Ideas

- Optical center is described as a "strategic revenue driver" in PROJECT.md — UI should support efficient workflow for high volume
- Suppliers (Essilor, Hoya, Viet Phap) are known — seed these as optical suppliers in shared supplier entity
- Frame size notation uses standard optical format: lens width - bridge width - temple length (e.g., 52-18-140)
- USB barcode scanner acts like a keyboard — no special browser API needed, just focus the input field
- In-house processing for simple lenses, outsourced for progressive/high-Rx — common pattern in Vietnamese optical shops
- All Vietnamese text in seed data must use proper diacritics (established pattern)

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- **OpticalDbContext**: Scaffolded with "optical" schema, empty — ready for entities
- **PermissionModule.Optical**: Already defined in Auth module — add optical-specific permissions
- **WorkflowStage.PharmacyOptical**: Stage 7 — glasses order naturally maps here
- **DataTable**: Generic table component — use for frame inventory, lens catalog, order list, stocktaking
- **handleServerValidationError**: RFC 7807 error handler — reuse for all optical forms
- **IAzureBlobService**: Blob storage for warranty claim supporting documents
- **QuestPDF**: Already integrated — use for barcode labels, stocktaking reports
- **AllergyCatalogSeeder/DrugCatalogSeeder**: IHostedService seeding pattern — mirror for optical supplier seeding
- **Supplier entity**: Created in Phase 6 (Pharmacy) — extend with supplier type tag for optical

### Established Patterns
- **Per-feature vertical slices**: Command/Handler with FluentValidation, Result<T> return
- **Minimal API endpoints**: MapGroup with RequireAuthorization, bus.InvokeAsync pattern
- **Cross-module queries**: Via Contracts project (Optical queries Clinical.Contracts for optical Rx, Pharmacy.Contracts for suppliers)
- **Domain events + Wolverine FX**: For order status change events (e.g., payment confirmed -> unblock processing)
- **React Hook Form + Zod**: zodResolver with validation, Controller pattern
- **TanStack Query**: queryKey factories, mutation with cache invalidation
- **onError toast pattern**: All React Query mutations must have onError with toast.error
- **Audit interceptor**: Captures field-level changes — tracks warranty claims, price changes, stock adjustments

### Integration Points
- **Optical module**: Build domain entities, handlers, endpoints in scaffolded module
- **Clinical module**: Cross-module query for optical Rx data (create glasses order from prescription)
- **Pharmacy module**: Shared supplier entity (extend with optical supplier type)
- **Billing module (Phase 7)**: Cross-module query for payment confirmation (OPT-04 enforcement)
- **Sidebar navigation**: Add Optical Center nav item with sub-sections (inventory, orders, stocktaking)
- **Permission system**: PermissionModule.Optical — add frame/lens CRUD, order management, warranty, stocktaking permissions
- **i18n**: New optical.json translation files (EN/VI)

</code_context>

<deferred>
## Deferred Ideas

- Zalo OA "Glasses ready" notification (NTF-04) — deferred to v2 notifications
- Walk-in orders from external prescriptions — not supported for v1, all orders from HIS optical Rx
- Trial lens inventory for Ortho-K fitting (OPX-01) — deferred to v2 optical enhancements
- Contact lens inventory management — tracked separately if needed post-launch

</deferred>

---

*Phase: 08-optical-center*
*Context gathered: 2026-03-05*

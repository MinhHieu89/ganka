# Phase 7: Billing & Finance - Context

**Gathered:** 2026-03-05
**Status:** Ready for planning

<domain>
## Phase Boundary

Cashier can generate unified invoices across all departments (medical, optical, pharmacy, treatment), collect payments via multiple methods (cash, bank transfer, QR, card), generate e-invoices for MISA export, handle treatment package split payments, process discounts with manager approval and refunds with manager/owner approval, and manage shifts with cash reconciliation. Invoices are built progressively during the visit and printed as PDF receipts.

This phase does NOT include: VIP membership/auto-discounts (v2), revenue reporting/dashboards (v2), MISA API auto-sync (v2 — manual export for v1), or payment gateway API integration (manual confirmation for v1).

</domain>

<decisions>
## Implementation Decisions

### Invoice Structure & Consolidation
- Progressively built invoice: invoice starts when visit begins, charges added as each service is performed (refraction, exam, drugs dispensed, optical orders, etc.). Cashier sees accumulated total at checkout
- Line items grouped by department on printed invoice: Kham benh, Duoc pham, Kinh, Dieu tri — patient sees which charges come from where (standard Vietnamese clinic convention)
- Internal revenue allocation tracked per department on each line item (not shown to patient, used for reporting)
- OTC pharmacy sales invoicing: Claude's discretion on data model (nullable VisitId or separate invoice type) — pick what keeps the model cleanest
- E-invoice output: both PDF (hoa don dien tu format for printing/filing) AND structured data export (JSON/XML for MISA import). Phase 1 approach = manual export, no MISA API
- Invoice numbering: Claude's discretion on format and sequence

### Payment Methods & Processing
- All payment methods use manual confirmation — no API integration with VNPay/MoMo/ZaloPay or POS terminals for v1
- QR payment: cashier selects method, patient scans clinic's static QR, cashier manually confirms receipt
- Card payment: Claude's discretion — same manual entry pattern (external POS terminal, record card type + last 4 digits)
- Multi-method split payment supported: cashier can split payment across methods (e.g., 500k cash + 300k QR). Each payment recorded separately against the invoice
- Treatment package 50/50 split payments: Claude's discretion on approach (two linked invoices vs single invoice with two payments) — pick what fits the invoice/payment data model best
- System enforces 50/50 rule: 2nd payment must be received before mid-course session (5-session -> before session 3, 3-session -> before session 2)

### Discount & Refund Approval
- Both percentage and fixed-amount discounts supported
- Discounts can apply per line item OR per invoice total — cashier has both options
- Manager approval for discounts: Claude's discretion on workflow (in-app request/approve queue vs manager PIN override). Consider clinic's small team (~8 staff, manager often present)
- Refund approval requires manager/owner approval with full audit trail
- Refund scope: Claude's discretion (partial line-item refunds vs full invoice refunds) — consider Vietnamese e-invoice regulations
- Price change audit log: all price changes tracked with who, when, old/new values (FIN-09)

### Shift Management & Reconciliation
- Shift definition: pre-configured templates as defaults (Morning, Afternoon matching clinic hours), cashier can adjust start/end times when opening
- Concurrent shifts: Claude's discretion — consider clinic has ~1 cashier but data model should be multi-branch ready (BranchId)
- Cash reconciliation at shift close: cashier enters physical cash count, system compares against expected cash (opening balance + cash received - cash refunds), shows discrepancy with manager note field
- Printable PDF shift report: revenue by payment method, transaction count, cash discrepancy, notes — uses QuestPDF (already integrated from Phase 5)

### Claude's Discretion
- OTC sale invoice data model (nullable VisitId vs separate invoice type)
- Invoice numbering format and sequence
- Card payment recording details
- Treatment package payment tracking approach (two invoices vs two payments on one invoice)
- Manager approval workflow style (in-app queue vs PIN override)
- Refund scope (partial vs full)
- Concurrent shift support
- Opening cash balance entry workflow
- Tax calculation display (if applicable for Vietnamese clinic invoices)
- Loading states and error handling
- Cashier dashboard layout and navigation

</decisions>

<specifics>
## Specific Ideas

- Vietnamese clinic invoices group charges by department — this is the expected format patients and accountants recognize
- MISA is the most common accounting software in Vietnam — manual export is standard practice for small clinics before API integration
- Most Vietnamese clinics use static QR codes (clinic's bank account) rather than dynamic QR per transaction — manual confirmation is the norm
- The clinic operates Tue-Fri 13:00-20:00, Sat-Sun 08:00-12:00 — shift templates should match these hours
- Single cashier operation currently (~8 staff total) but data model should support multi-branch future
- All prices in VND (Vietnamese Dong) — no multi-currency needed
- E-invoice (hoa don dien tu) is legally required per Vietnamese tax law — must generate compliant format

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- **BillingDbContext**: Scaffolded with "billing" schema, empty — ready for entities
- **QuestPDF**: Already integrated in Phase 5 for document generation — reuse for invoices, receipts, shift reports, e-invoices
- **Clinic header template**: Configurable clinic branding from Phase 5 (logo, name, address, phone, license) — invoices pull from same config
- **DataTable**: Generic table component — use for invoice list, payment history, shift list
- **handleServerValidationError**: RFC 7807 error handler — reuse for billing forms
- **PermissionModule.Billing**: Already defined in Auth module — add billing-specific permissions

### Established Patterns
- **Per-feature vertical slices**: Command/Handler with FluentValidation, Result<T> return
- **Minimal API endpoints**: MapGroup with RequireAuthorization, bus.InvokeAsync pattern
- **Cross-module queries**: Via Contracts project — Billing queries Clinical, Pharmacy, Optical for charges
- **Domain events + Wolverine FX**: For charge creation events (drug dispensed -> add invoice line)
- **React Hook Form + Zod**: zodResolver with validation, Controller pattern
- **TanStack Query**: queryKey factories, mutation with cache invalidation
- **onError toast pattern**: All React Query mutations must have onError with toast.error
- **Audit interceptor**: Already captures field-level changes — will track price changes, discounts, refunds

### Integration Points
- **Clinical module**: Query visit services/charges for invoice line items
- **Pharmacy module**: Query dispensing records (Phase 6) and OTC sales for pharmacy charges; drug selling prices from catalog
- **Optical module**: Query optical orders (Phase 8) for optical charges — may need stub/interface for Phase 7 if Optical is Phase 8
- **Treatment module**: Query treatment packages (Phase 9) for treatment charges and split payment enforcement
- **Patient module**: Query patient info for invoice header
- **Sidebar navigation**: Add Billing/Finance nav items
- **Permission system**: PermissionModule.Billing — add invoice, payment, discount, refund, shift permissions
- **i18n**: New billing.json translation files (EN/VI)
- **Workflow stages**: Cashier stage (stage 6) already exists — billing naturally maps here

</code_context>

<deferred>
## Deferred Ideas

- VIP membership auto-discounts — deferred to v2 (VIP-01 through VIP-06)
- Revenue dashboards and reporting — deferred to v2 (RPT-01 through RPT-08)
- MISA API auto-sync — v1 is manual export, v2 considers API integration
- Payment gateway API integration (VNPay/MoMo/ZaloPay) — v1 uses manual confirmation
- POS terminal integration for card payments — v1 uses manual entry

</deferred>

---

*Phase: 07-billing-finance*
*Context gathered: 2026-03-05*

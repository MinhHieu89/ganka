---
phase: 07-billing-finance
plan: 16
subsystem: documents
tags: [questpdf, pdf, e-invoice, misa, json, xml, vnd, billing-documents]

# Dependency graph
requires:
  - phase: 07-13
    provides: "Billing API endpoints with Wolverine bus dispatch and route groups"
  - phase: 07-14
    provides: "Billing.Presentation project, EF migration, bootstrapper integration"
  - phase: 07-15
    provides: "QuestPDF document classes (InvoiceDocument, ReceiptDocument, EInvoiceDocument, ShiftReportDocument) with BillingDocumentDataRecords and VndFormatter"
provides:
  - "BillingDocumentService generating PDFs for invoice, receipt, e-invoice, and shift report"
  - "EInvoiceExportService exporting structured JSON and XML for MISA import"
  - "6 print/export API endpoints under /api/billing/print and /api/billing/export"
  - "IBillingDocumentService registered in IoC as scoped service"
affects: [07-billing-finance, frontend-billing]

# Tech tracking
tech-stack:
  added: []
  patterns: [direct-service-injection-for-documents, cross-schema-raw-sql-for-patient-code, static-export-service-pattern]

key-files:
  created:
    - backend/src/Modules/Billing/Billing.Infrastructure/Services/BillingDocumentService.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Services/EInvoiceExportService.cs
  modified:
    - backend/src/Modules/Billing/Billing.Presentation/BillingApiEndpoints.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/IoC.cs

key-decisions:
  - "Made EInvoiceExportService static since it operates purely on pre-assembled EInvoiceData records with no DI dependencies"
  - "Used raw SQL cross-schema query (patient.Patients) for PatientCode lookup to avoid cross-module project references"
  - "Default unit-of-measure inferred from Department enum (Medical/Treatment=Lan, Pharmacy=Hop, Optical=Cai) since InvoiceLineItem domain entity lacks Unit field"
  - "Tax rate hardcoded at 8% GTGT -- standard Vietnamese VAT for medical services"

patterns-established:
  - "Direct IBillingDocumentService injection in endpoints (not via Wolverine) for PDF generation -- mirrors Clinical module pattern"
  - "Static export service for stateless data transformation (JSON/XML serialization)"
  - "BillingDocumentService delegates to EInvoiceExportService for export methods to maintain single implementation"

requirements-completed: [FIN-04]

# Metrics
duration: 4min
completed: 2026-03-06
---

# Phase 07 Plan 16: Billing Document Services Summary

**BillingDocumentService generating 4 PDF document types via QuestPDF and EInvoiceExportService producing JSON/XML for MISA e-invoice import with 8% GTGT tax calculation**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-06T14:49:46Z
- **Completed:** 2026-03-06T14:53:57Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- BillingDocumentService implements all 6 IBillingDocumentService methods: GenerateInvoicePdfAsync, GenerateReceiptPdfAsync, GenerateEInvoicePdfAsync, GenerateShiftReportPdfAsync, ExportEInvoiceJsonAsync, ExportEInvoiceXmlAsync
- EInvoiceExportService produces Vietnamese-friendly JSON (UnsafeRelaxedJsonEscaping for Unicode) and XML (XDocument/XElement) with all Decree 123/2020 mandatory fields
- 6 new API endpoints: 3 print (invoice/receipt/e-invoice PDF), 2 export (JSON/XML), 1 shift report PDF
- Full tax calculation: 8% GTGT rate applied to pre-tax total, amount-in-words via VndFormatter

## Task Commits

Each task was committed atomically:

1. **Task 1: Implement BillingDocumentService and EInvoiceExportService** - `a1b1dc9` (feat)
2. **Task 2: Add print/export endpoints and update IoC** - `2d77e68` (feat)

**Plan metadata:** (pending)

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Infrastructure/Services/BillingDocumentService.cs` - IBillingDocumentService implementation with PDF generation for all 4 document types and e-invoice export delegation
- `backend/src/Modules/Billing/Billing.Infrastructure/Services/EInvoiceExportService.cs` - Static JSON/XML export service with Vietnamese Unicode support and MISA-compatible format
- `backend/src/Modules/Billing/Billing.Presentation/BillingApiEndpoints.cs` - 6 new print/export endpoints with direct IBillingDocumentService injection
- `backend/src/Modules/Billing/Billing.Infrastructure/IoC.cs` - AddScoped IBillingDocumentService registration

## Decisions Made
- **EInvoiceExportService as static class** -- The service only transforms pre-assembled EInvoiceData records into JSON/XML strings with no database or DI dependencies. Static methods are simpler and more testable for pure data transformation.
- **Cross-schema raw SQL for PatientCode** -- Following the Clinical module pattern (DocumentService.GetPatientInfoAsync), used BillingDbContext.Database.SqlQuery to query patient.Patients table directly. This avoids cross-module project references while enabling patient code display on billing documents.
- **Default unit-of-measure by department** -- InvoiceLineItem domain entity does not have a "Unit" field (e.g., "Lan", "Hop"). Since this would require a schema migration and is a display concern, we infer sensible defaults from Department: Medical/Treatment = "Lan" (time), Pharmacy = "Hop" (box), Optical = "Cai" (piece).
- **BillingDocumentService delegates export to EInvoiceExportService** -- ExportEInvoiceJsonAsync and ExportEInvoiceXmlAsync in BillingDocumentService load the invoice and build EInvoiceData, then call the static EInvoiceExportService methods. This keeps the interface unified while separating PDF generation from data serialization concerns.

## Deviations from Plan

None - plan executed exactly as written. Both tasks compiled cleanly on first build. Full solution build passes with 0 errors.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All billing document generation and export services are operational
- Frontend can call /api/billing/print/* and /api/billing/export/* endpoints for document download
- E-invoice JSON/XML export ready for MISA manual import workflow
- Future enhancement: add Unit field to InvoiceLineItem domain entity for more accurate document display

## Self-Check: PASSED

(verified below)

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*

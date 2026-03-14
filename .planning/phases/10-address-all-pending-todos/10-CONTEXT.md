# Phase 10: Address All Pending Todos - Context

**Gathered:** 2026-03-14
**Status:** Ready for planning

<domain>
## Phase Boundary

Fix bugs, improve UX, and add missing features captured as todos across Clinical, Pharmacy, Dry Eye, and Optical modules. 13 todos covering: quick UX fixes (auto-expand, auto-focus, patient link, textarea resize), bug fixes (stock import search, OTC validation), feature enhancements (dry eye trend charts, OSDI answers view, realtime OSDI, batch label printing, drug catalog Excel import), backend work (clinic logo upload), and performance (server-side pagination for pharmacy).

</domain>

<decisions>
## Implementation Decisions

### Dry Eye Metric Trend Charts
- Separate charts per metric (TBUT, Schirmer, Meibomian, Tear Meniscus, Staining) — individual small charts stacked vertically
- OD/OS differentiation per chart
- Time range selector with options: Last 3 months, 6 months, 1 year, All
- Placed on patient detail page Dry Eye tab

### OSDI Question Answers Display
- Expandable inline section on visit detail page
- "View Details" button next to OSDI score expands to show all 12 questions with individual scores
- Grouped by category: Vision, Eye Symptoms, Environmental Triggers

### Realtime OSDI Score Updates
- SignalR push approach — reuse existing SignalR infrastructure from Phase 7.1 (BillingHub pattern)
- Backend broadcasts OsdiSubmitted event to visit detail page subscribers
- Frontend useOsdiHub() hook invalidates React Query cache for instant UI update

### Drug Catalog Excel Import
- Inline table preview after file upload — valid rows marked green, invalid rows highlighted red with specific error per cell
- User confirms to import only valid rows, can fix and re-upload rejected rows
- Provide downloadable Excel template with expected columns

### Pharmacy Server-Side Pagination
- Full server-side pagination AND search for drug catalog page
- Backend endpoint accepts page/pageSize/search parameters, returns paginated results with total count
- Consistent with existing server-side pagination patterns in the app

### OTC Sale Stock Validation
- Check available stock on BOTH drug selection AND quantity change
- Inline warning under the row: "Only 5 in stock" or "Out of stock"
- Disable submit button if any row exceeds available stock

### Batch Pharmacy Label Printing
- Single continuous thermal print output with all labels
- User cuts labels manually after printing
- All labels generated in one PDF/print action from a "Print All Labels" button at prescription level

### Clinic Logo Upload
- Store in Azure Blob Storage (consistent with existing medical image infrastructure)
- POST /api/settings/clinic/logo endpoint
- Update ClinicSettings.LogoBlobUrl with blob URL

### Textarea Auto-Expand
- Global AutoResizeTextarea wrapper component
- Replace all Textarea usages across the app
- Auto-resize height to scrollHeight on input

### Patient Name Link
- Clickable patient name on visit detail page opens patient detail in new browser tab (target="_blank")

### Claude's Discretion
- Logo upload file size and format constraints (reasonable web defaults)
- Auto-focus implementation approach for DrugCombobox
- Optical Prescription section defaultOpen logic
- Stock import search fix (debug and fix the combobox filtering)
- Exact chart library and styling for dry eye metric charts
- Textarea auto-resize implementation details (JS vs CSS approach)

</decisions>

<specifics>
## Specific Ideas

- Thermal printer for pharmacy labels — continuous output, user cuts manually
- Dry eye charts should be similar in style to existing OSDI trend chart but one chart per metric
- SignalR hub for OSDI can follow the same pattern as BillingHub from Phase 7.1

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- BillingHub (SignalR): Pattern for creating OsdiHub for realtime OSDI updates
- QuestPDF infrastructure: Extend for batch label PDF generation
- DataTable component: Already supports server-side pagination pattern
- DrugCombobox: Needs auto-focus fix, already exists
- OpticalPrescriptionSection: Simple defaultOpen prop change
- OSDI trend chart: Existing chart component to reference for dry eye metric charts

### Established Patterns
- SignalR hub + React Query invalidation (Phase 7.1)
- Server-side paginated API endpoints (page/pageSize/search params)
- Azure Blob Storage upload (medical images infrastructure)
- Excel import with MiniExcel (stock import in Phase 6)

### Integration Points
- Visit detail page: OSDI answers expandable, realtime OSDI, patient name link
- Dry Eye tab on patient detail: metric trend charts
- Drug catalog page: Excel import, server-side pagination
- OTC sales page: inline stock validation
- Prescription section: batch label print button
- Settings page: logo upload
- All textareas app-wide: auto-resize wrapper

</code_context>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 10-address-all-pending-todos*
*Context gathered: 2026-03-14*

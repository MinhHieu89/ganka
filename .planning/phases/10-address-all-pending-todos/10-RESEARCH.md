# Phase 10: Address All Pending Todos - Research

**Researched:** 2026-03-14
**Domain:** Cross-module UX improvements, bug fixes, feature enhancements (Clinical, Pharmacy, Dry Eye, Optical)
**Confidence:** HIGH

## Summary

Phase 10 addresses 13 discrete todos spanning quick UX fixes, bug fixes, and feature enhancements across the entire application. The work is heterogeneous -- ranging from a single-line `defaultOpen` prop change to full-stack features like dry eye metric trend charts and drug catalog Excel import. The key finding is that nearly all work can leverage existing infrastructure: recharts is already installed for charting, SignalR/BillingHub provides the pattern for OsdiHub, MiniExcel is already used for stock import, QuestPDF generates pharmacy labels, IAzureBlobService handles blob uploads, and server-side pagination patterns exist across Optical and Patient modules.

The primary risk areas are: (1) the textarea auto-resize replacement being a global change touching many components, (2) the drug catalog Excel import needing a new preview UI with inline validation display, and (3) the dry eye metric trend charts requiring a new backend endpoint to return per-metric time-series data with OD/OS differentiation and time range filtering.

**Primary recommendation:** Group work into 5-6 sub-plans by complexity and dependency: quick UX fixes first (auto-expand, auto-focus, patient link, defaultOpen), then bug fixes (stock import search, OTC validation), then medium features (OSDI answers, textarea wrapper, logo upload, batch labels), then large features (dry eye charts, server-side pagination, realtime OSDI, drug catalog Excel import).

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Dry Eye Metric Trend Charts: Separate charts per metric (TBUT, Schirmer, Meibomian, Tear Meniscus, Staining) -- individual small charts stacked vertically, OD/OS differentiation per chart, time range selector (Last 3 months, 6 months, 1 year, All), placed on patient detail page Dry Eye tab
- OSDI Question Answers Display: Expandable inline section on visit detail page, "View Details" button next to OSDI score expands to show all 12 questions with individual scores, grouped by category (Vision, Eye Symptoms, Environmental Triggers)
- Realtime OSDI Score Updates: SignalR push approach -- reuse existing SignalR infrastructure from Phase 7.1 (BillingHub pattern), backend broadcasts OsdiSubmitted event, frontend useOsdiHub() hook invalidates React Query cache
- Drug Catalog Excel Import: Inline table preview after file upload -- valid rows green, invalid rows red with specific error per cell, user confirms to import only valid rows, provide downloadable Excel template
- Pharmacy Server-Side Pagination: Full server-side pagination AND search for drug catalog page, backend endpoint accepts page/pageSize/search parameters
- OTC Sale Stock Validation: Check available stock on BOTH drug selection AND quantity change, inline warning under the row, disable submit button if any row exceeds available stock
- Batch Pharmacy Label Printing: Single continuous thermal print output with all labels, user cuts manually, all labels generated in one PDF/print action from "Print All Labels" button at prescription level
- Clinic Logo Upload: Store in Azure Blob Storage, POST /api/settings/clinic/logo endpoint, update ClinicSettings.LogoBlobUrl
- Textarea Auto-Expand: Global AutoResizeTextarea wrapper component, replace all Textarea usages across the app, auto-resize height to scrollHeight on input
- Patient Name Link: Clickable patient name on visit detail page opens patient detail in new browser tab (target="_blank")

### Claude's Discretion
- Logo upload file size and format constraints (reasonable web defaults)
- Auto-focus implementation approach for DrugCombobox
- Optical Prescription section defaultOpen logic
- Stock import search fix (debug and fix the combobox filtering)
- Exact chart library and styling for dry eye metric charts
- Textarea auto-resize implementation details (JS vs CSS approach)

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope
</user_constraints>

## Standard Stack

### Core (Already Installed)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| recharts | ^3.7.0 | Dry eye metric trend charts | Already used for OSDI trend chart |
| @microsoft/signalr | ^10.0.0 | Realtime OSDI push | Already used for BillingHub |
| MiniExcel | (installed) | Drug catalog Excel import parsing | Already used for stock import |
| QuestPDF | (installed) | Batch label PDF generation | Already used for pharmacy labels |
| Azure.Storage.Blobs | (installed) | Clinic logo upload | Already used via IAzureBlobService |
| date-fns | ^4.1.0 | Date formatting for charts | Already installed |

### No New Dependencies Required

All 13 todos can be implemented using the existing stack. No new npm or NuGet packages needed.

## Architecture Patterns

### Recommended Grouping by Complexity

```
Phase 10 Todos:
├── Quick UX Fixes (1-2 hours each)
│   ├── OpticalPrescriptionSection defaultOpen
│   ├── DrugCombobox auto-focus
│   └── Patient name link (target="_blank")
├── Bug Fixes (2-4 hours each)
│   ├── Stock import search combobox filtering
│   └── OTC sale stock validation
├── Medium Features (4-8 hours each)
│   ├── OSDI answers expandable display
│   ├── AutoResizeTextarea wrapper + global replacement
│   ├── Clinic logo upload endpoint
│   └── Batch pharmacy label printing
└── Large Features (8-16 hours each)
    ├── Dry eye metric trend charts (new backend endpoint + 5 charts)
    ├── Drug catalog server-side pagination
    ├── Drug catalog Excel import (new endpoint + preview UI)
    └── Realtime OSDI via SignalR (new hub + hook)
```

### Pattern 1: SignalR Hub (for Realtime OSDI)
**What:** Create OsdiHub following BillingHub pattern
**When to use:** Realtime OSDI score push to visit detail page

Existing pattern from `backend/src/Modules/Billing/Billing.Infrastructure/Hubs/BillingHub.cs`:
```csharp
[Authorize]
public class OsdiHub : Hub
{
    public async Task JoinVisit(string visitId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"visit-{visitId}");
    }
    public async Task LeaveVisit(string visitId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"visit-{visitId}");
    }
}
```

Frontend hook follows `frontend/src/features/billing/hooks/use-billing-hub.ts` pattern:
- `useOsdiHub(visitId)` joins visit-specific group
- On "OsdiSubmitted" event, invalidates visit query cache
- Returns connection status

### Pattern 2: Server-Side Pagination (for Drug Catalog)
**What:** Backend endpoint with page/pageSize/search params returning `{ items, totalCount, page, pageSize, totalPages }`
**When to use:** Drug catalog page (currently client-side filtered)

Existing pattern from `Pharmacy.Application.Features.StockImport.GetStockImports`:
```csharp
var page = Math.Clamp(query.Page, 1, int.MaxValue);
var pageSize = Math.Clamp(query.PageSize, 1, 100);
var (items, totalCount) = await repository.GetAllAsync(page, pageSize, ct);
var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);
```

Frontend uses `manualPagination: true` on TanStack Table (see `PatientTable.tsx`, `AuditLogTable.tsx`).

### Pattern 3: Excel Import with Preview (for Drug Catalog Import)
**What:** Upload Excel -> parse with MiniExcel -> return preview with valid/invalid rows -> user confirms -> persist valid rows
**When to use:** Drug catalog Excel import

Existing pattern from `Pharmacy.Application.Features.StockImport.ImportStockFromExcel`:
- Parse with `MiniExcel.Query<T>(stream, hasHeader: true)`
- Validate each row, collect errors per row/column
- Return `ExcelImportPreview(ValidLines, Errors)` for user review
- Separate confirm endpoint to persist

### Pattern 4: Azure Blob Upload (for Clinic Logo)
**What:** Upload file to Azure Blob Storage via `IAzureBlobService.UploadAsync`
**When to use:** Clinic logo upload

Existing interface at `Shared.Application.Services.IAzureBlobService`:
```csharp
Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType);
```

### Pattern 5: QuestPDF Batch Document (for Batch Labels)
**What:** Generate multi-page PDF with one label per page for continuous thermal printing
**When to use:** Batch pharmacy label printing

Existing `PharmacyLabelDocument` generates single label (70x35mm). Batch version creates multiple pages in one document.

### Anti-Patterns to Avoid
- **Client-side filtering large datasets:** Drug catalog page currently does client-side search. Must convert to server-side for scalability.
- **Creating separate SignalR connections per component:** Reuse a single hub connection per page, not per component.
- **Hardcoding OSDI question text in frontend:** Questions already exist in backend DTOs (OsdiQuestionDto with TextEn/TextVi). Fetch from backend.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Chart rendering | Custom SVG charts | recharts (already installed) | Complex axis, tooltips, responsive |
| Excel parsing | Custom CSV/XLSX parser | MiniExcel (already installed) | Handles encoding, types, streaming |
| Realtime push | Polling or SSE | SignalR (already installed) | Connection management, reconnection |
| PDF generation | HTML-to-PDF | QuestPDF (already installed) | Precise layout, thermal printer sizing |
| Blob storage | Custom file storage | IAzureBlobService (already exists) | SAS URLs, soft delete, versioning |
| Auto-resize textarea | CSS-only field-sizing | JS scrollHeight approach | field-sizing: content has poor browser support |

## Common Pitfalls

### Pitfall 1: Textarea Auto-Resize Global Replacement Breaks Forms
**What goes wrong:** Replacing `Textarea` import across the app breaks form validation or styling in some components.
**Why it happens:** Some components may use Textarea ref forwarding, controlled value, or specific className overrides.
**How to avoid:** Create `AutoResizeTextarea` as a wrapper that extends the existing `Textarea` component. Use the same prop interface. Find all Textarea imports with grep and test each replacement.
**Warning signs:** Form submissions that previously worked now fail, or visual layout shifts.

### Pitfall 2: Dry Eye Chart Data Endpoint Performance
**What goes wrong:** N+1 query pattern -- existing `GetOsdiHistory` fetches each visit individually in a loop.
**Why it happens:** Current code calls `GetByIdAsync` for each assessment to get visit date.
**How to avoid:** Create a new repository method that joins DryEyeAssessments with Visits in a single query, returning all metrics with visit dates. Include time range filtering at the SQL level.
**Warning signs:** Slow page load on patients with many visits.

### Pitfall 3: OTC Stock Validation Race Condition
**What goes wrong:** Stock appears available when user selects drug, but is depleted by another user before submission.
**Why it happens:** Check is done at selection time but stock changes between check and submission.
**How to avoid:** The inline warning is advisory (check at selection + quantity change). The server-side validation in `CreateOtcSale` handler already checks available batches via FEFO. Keep both: frontend warning for UX + backend enforcement for correctness.
**Warning signs:** "Insufficient stock" errors on submission despite green UI.

### Pitfall 4: SignalR Group Membership Lost on Reconnect
**What goes wrong:** After network interruption, OSDI updates stop arriving.
**Why it happens:** SignalR group membership is server-side and cleared on disconnect.
**How to avoid:** In the `onreconnected` handler, re-invoke `JoinVisit(visitId)`. This pattern already exists in `useBillingHub`.
**Warning signs:** Realtime updates work initially but stop after brief network disruption.

### Pitfall 5: Drug Catalog Excel Import Column Mismatch
**What goes wrong:** Users upload Excel files with slightly different column names and import fails silently.
**Why it happens:** MiniExcel maps columns to property names case-insensitively but requires exact name match.
**How to avoid:** Provide a downloadable template. Show clear error message if expected columns are missing. Consider mapping by column position as fallback.
**Warning signs:** Valid-looking Excel files produce zero valid rows.

## Code Examples

### AutoResizeTextarea Component
```typescript
// frontend/src/shared/components/AutoResizeTextarea.tsx
import { useCallback, useRef, useEffect } from "react"
import { Textarea } from "@/shared/components/Textarea"

export const AutoResizeTextarea = React.forwardRef<
  HTMLTextAreaElement,
  React.ComponentProps<typeof Textarea>
>(({ onChange, value, ...props }, ref) => {
  const innerRef = useRef<HTMLTextAreaElement | null>(null)

  const resize = useCallback(() => {
    const el = innerRef.current
    if (!el) return
    el.style.height = "auto"
    el.style.height = `${el.scrollHeight}px`
  }, [])

  useEffect(() => { resize() }, [value, resize])

  return (
    <Textarea
      ref={(node) => {
        innerRef.current = node
        if (typeof ref === "function") ref(node)
        else if (ref) ref.current = node
      }}
      value={value}
      onChange={(e) => {
        onChange?.(e)
        resize()
      }}
      {...props}
    />
  )
})
```

### Dry Eye Metric Chart (single metric)
```typescript
// Follows existing OsdiTrendChart pattern from frontend/src/features/patient/components/OsdiTrendChart.tsx
// Uses recharts LineChart with two Line components (OD/OS)
<LineChart data={chartData}>
  <XAxis dataKey="visitDate" />
  <YAxis />
  <Tooltip />
  <Line dataKey="od" stroke="hsl(var(--chart-1))" name="OD" />
  <Line dataKey="os" stroke="hsl(var(--chart-2))" name="OS" />
</LineChart>
```

### Batch Label PDF
```csharp
// Extends existing PharmacyLabelDocument pattern
// One document with multiple pages, each page = one label
container.Page(page => {
    page.Size(70, 35, Unit.Millimetre);
    // ... label content for each drug in prescription
});
// Loop creates multiple pages in single IDocument
```

### OTC Stock Check Endpoint
```csharp
// New query: GET /api/pharmacy/drugs/{id}/available-stock
// Returns total available quantity across all valid batches
public static async Task<int> Handle(
    GetDrugAvailableStockQuery query,
    IDrugBatchRepository drugBatchRepository,
    CancellationToken ct)
{
    var batches = await drugBatchRepository.GetAvailableBatchesFEFOAsync(query.DrugCatalogItemId, ct);
    return batches.Sum(b => b.AvailableQuantity);
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Client-side table filtering | Server-side pagination with search | Standard practice | Drug catalog must adopt for scalability |
| Polling for updates | SignalR push | Already in app | OSDI follows same pattern |
| Individual label printing | Batch PDF generation | Phase 10 | Single print action for all labels |
| Static OSDI score display | Realtime push updates | Phase 10 | Instant feedback when patient submits |

## Open Questions

1. **Drug Catalog Import: What columns?**
   - What we know: Stock import uses DrugName, BatchNumber, ExpiryDate, Quantity, PurchasePrice
   - What's unclear: Drug catalog import needs different columns (Name, NameVi, GenericName, Form, Route, Strength, Unit, SellingPrice, MinStockLevel, etc.)
   - Recommendation: Define column mapping based on CreateDrugCatalogItem command fields. Provide Excel template with all expected columns.

2. **OSDI Answers: Are individual answers stored?**
   - What we know: `OsdiSubmission.AnswersJson` stores a JSON array of 12 answers (0-4 or null)
   - What's unclear: Do we need to parse the JSON and display alongside question text?
   - Recommendation: Yes -- backend already stores answers as JSON. Frontend parses and maps to the 12 OSDI questions (already defined in `OsdiQuestionDto`). A new endpoint `GET /api/clinical/visits/{visitId}/osdi-answers` can return structured data.

3. **Dry Eye Metrics: New backend endpoint needed**
   - What we know: `GetDryEyeAssessmentsByPatientAsync` returns `List<DryEyeAssessment>` with all metrics but no visit date joined
   - What's unclear: Need new query that returns metrics + visit dates + supports time range filter
   - Recommendation: Create `GetDryEyeMetricHistory` handler that joins assessments with visits, returns time-series data for each metric with OD/OS values.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit + NSubstitute + FluentAssertions |
| Config file | Each module has `*.Unit.Tests.csproj` under `backend/tests/` |
| Quick run command | `dotnet test backend/tests/Pharmacy.Unit.Tests/ --no-build -v q` |
| Full suite command | `dotnet test backend/ -v q` |

### Phase Requirements -> Test Map
| Todo | Behavior | Test Type | Automated Command | File Exists? |
|------|----------|-----------|-------------------|-------------|
| Drug catalog pagination | Server-side page/search returns correct results | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests/ -v q` | Extend existing |
| OTC stock validation | Available stock check endpoint returns correct sum | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests/ -v q` | New handler test |
| Drug catalog Excel import | Parse/validate Excel, return preview | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests/ -v q` | New handler test |
| Realtime OSDI | OsdiSubmitted event handler triggers notification | unit | `dotnet test backend/tests/Clinical.Unit.Tests/ -v q` | New handler test |
| OSDI answers | Return structured answers with questions | unit | `dotnet test backend/tests/Clinical.Unit.Tests/ -v q` | New handler test |
| Dry eye metric history | Return time-series data per metric | unit | `dotnet test backend/tests/Clinical.Unit.Tests/ -v q` | New handler test |
| Logo upload | Upload blob, update settings | unit | `dotnet test backend/tests/Shared.Unit.Tests/ -v q` | New handler test |
| Batch label print | Generate multi-page label PDF | unit | `dotnet test backend/tests/Clinical.Unit.Tests/ -v q` | New test |

### Sampling Rate
- **Per task commit:** `dotnet test backend/tests/{ModuleName}.Unit.Tests/ --no-build -v q`
- **Per wave merge:** `dotnet test backend/ -v q`
- **Phase gate:** Full suite green before verification

### Wave 0 Gaps
None -- existing test infrastructure covers all modules. New test files will be created for new handlers following TDD.

## Sources

### Primary (HIGH confidence)
- Codebase inspection: BillingHub.cs, use-billing-hub.ts, ImportStockFromExcel.cs, PharmacyLabelDocument.cs, OsdiTrendChart.tsx, IAzureBlobService.cs, IClinicSettingsService.cs, DataTable.tsx
- Codebase inspection: DryEyeAssessmentDto.cs, OsdiSubmission.cs (AnswersJson field confirms individual answers stored)
- Codebase inspection: PatientTable.tsx, AuditLogTable.tsx (server-side pagination with manualPagination pattern)
- Codebase inspection: GetStockImports.cs (page/pageSize/totalCount pagination response pattern)

### Secondary (MEDIUM confidence)
- recharts 3.x documentation -- LineChart with multiple Line components for OD/OS differentiation
- SignalR group-per-entity pattern for visit-scoped notifications

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - all libraries already installed and used in the project
- Architecture: HIGH - all patterns already exist in the codebase, this phase extends them
- Pitfalls: HIGH - identified from actual codebase patterns and existing code issues
- UX fixes: HIGH - simple prop changes and link additions

**Research date:** 2026-03-14
**Valid until:** 2026-04-14 (stable -- all patterns are existing code, not external APIs)

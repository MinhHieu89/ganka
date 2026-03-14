---
phase: 10-address-all-pending-todos
verified: 2026-03-14T09:30:00Z
status: human_needed
score: 13/13 must-haves verified
human_verification:
  - test: "AutoResizeTextarea live resizing"
    expected: "Typing multiple lines in any textarea causes the field to expand in height without a scrollbar"
    why_human: "Requires interactive input to verify scrollHeight-based resize fires correctly in a real browser"
  - test: "OpticalPrescriptionSection expanded by default"
    expected: "Opening a visit detail page shows the Optical Prescription collapsible section already expanded"
    why_human: "Visual state of accordion cannot be confirmed programmatically without rendering"
  - test: "DrugCombobox auto-focuses search on open"
    expected: "Clicking the drug selection combobox in a prescription form immediately places cursor in the search input"
    why_human: "Focus state requires actual browser rendering and interaction"
  - test: "Patient name link opens patient profile in new tab"
    expected: "Clicking the patient name on the visit detail page opens the patient profile page in a new browser tab"
    why_human: "Navigation and tab behavior requires live browser interaction"
  - test: "Drug catalog Excel import preview shows green/red rows"
    expected: "Uploading an Excel file with mixed valid/invalid rows shows a table where valid rows have green indicators and invalid rows show red error messages per cell"
    why_human: "Visual color-coding and error display in the table requires visual inspection"
  - test: "OTC sale inline stock warning and disabled submit"
    expected: "Entering a quantity greater than available stock on an OTC sale line shows an inline warning and the Submit button becomes disabled"
    why_human: "Dynamic button state and inline warning require real API responses and interactive quantity change"
  - test: "Dry eye metric charts display OD/OS lines"
    expected: "On a patient with dry eye assessments in the Dry Eye tab, 5 stacked charts appear (TBUT, Schirmer, Meibomian, TearMeniscus, Staining) each with two lines (OD and OS) and a time range selector"
    why_human: "Chart rendering with real data requires visual verification"
  - test: "OSDI answers expand by category"
    expected: "On a visit with OSDI data, a View Details button appears next to the OSDI score; clicking it expands a section showing 12 questions grouped into Ocular Symptoms, Vision-Related Function, and Environmental Triggers"
    why_human: "Expand/collapse behavior and correct category grouping require visual and interactive testing"
  - test: "Batch labels PDF download"
    expected: "Clicking Print All Labels on a prescription with drugs downloads or opens a PDF containing one 70x35mm thermal label per drug"
    why_human: "PDF generation and browser download require live testing against the running backend"
  - test: "Clinic logo upload with preview"
    expected: "On the Clinic Settings page, selecting an image file previews it before upload, and clicking Upload persists it"
    why_human: "File selection, preview rendering, and actual Azure Blob upload require a running backend with valid Azure credentials"
  - test: "Stock import drug search combobox filters correctly"
    expected: "Typing a drug name in the drug search combobox on the Stock Import page filters the dropdown to matching drugs"
    why_human: "Combobox filtering UX requires interactive browser testing"
  - test: "Realtime OSDI score update via SignalR"
    expected: "When a patient submits the OSDI questionnaire via QR code while the doctor has the visit detail page open, the OSDI score updates without a page refresh"
    why_human: "Realtime push via SignalR requires two simultaneous sessions and live network behavior"
---

# Phase 10: Address All Pending Todos - Verification Report

**Phase Goal:** Address all pending todo items from earlier phases (13 todos across Clinical, Pharmacy, Dry Eye modules)
**Verified:** 2026-03-14T09:30:00Z
**Status:** human_needed
**Re-verification:** No — initial verification

---

## Goal Achievement

All 13 TODO items have been implemented across 10 sub-plans (10-01 through 10-10). Automated code inspection confirms all artifacts exist, are substantive, and are wired into the application. An end-to-end Playwright verification was performed in plan 07 and all features passed. Remaining human verification items are for live visual/interactive behaviors.

### Observable Truths (TODO Requirements Coverage)

| # | TODO | Truth | Status | Evidence |
|---|------|-------|--------|----------|
| 1 | TODO-01 | All textareas auto-resize to fit content | VERIFIED | `AutoResizeTextarea.tsx` uses scrollHeight resize on mount and value change; 32+ files replaced |
| 2 | TODO-02 | Optical prescription section expanded by default | VERIFIED | `sectionOpen` state initialized to `true`, `open={sectionOpen}` wired in VisitSection |
| 3 | TODO-03 | DrugCombobox auto-focuses search input when opened | VERIFIED | `autoFocus` attribute present on Input elements in DrugCombobox; `onOpenAutoFocus` prevention removed |
| 4 | TODO-04 | Clinic logo can be uploaded and stored in Azure Blob | VERIFIED | `UploadClinicLogo.cs` uses `IAzureBlobService`, persists URL via `UpdateLogoUrlAsync`; UI in `ClinicSettingsPage.tsx` |
| 5 | TODO-05 | Drug catalog uses server-side pagination with page/pageSize/search | VERIFIED | `PaginatedDrugCatalog.cs` returns paginated results; `DrugCatalogPage.tsx` uses `useSearchDrugCatalog` with debounced search |
| 6 | TODO-06 | Patient name on visit detail is a clickable link in new tab | VERIFIED | `PatientInfoSection.tsx` uses `Link` with `target="_blank"` at line 48 |
| 7 | TODO-07 | Dry eye metric history shows per-metric time-series with OD/OS and time range filter | VERIFIED | `GetDryEyeMetricHistory.cs` returns 5 metric series; `DryEyeMetricCharts.tsx` (185 lines) integrated in `PatientDryEyeTab.tsx` |
| 8 | TODO-08 | Batch pharmacy label PDF generates one label per drug | VERIFIED | `BatchPharmacyLabelDocument.cs` uses 70x35mm per-page pattern; `PrintBatchLabels.cs` wired; Print All Labels button in `DrugPrescriptionSection.tsx` |
| 9 | TODO-09 | Stock import drug search combobox filters correctly | VERIFIED | `StockImportForm.tsx` fixed: removed `shouldFilter={false}`, uses drug name as value; cmdk built-in filtering active |
| 10 | TODO-10 | OSDI answers displayed grouped by category | VERIFIED | `OsdiAnswersSection.tsx` (103 lines) fetches `/osdi-answers`, displays by category; wired into `OsdiSection.tsx` |
| 11 | TODO-11 | Excel import for drug catalog with preview validation | VERIFIED | `ImportDrugCatalogFromExcel.cs` uses MiniExcel; `DrugCatalogImportDialog.tsx` (331 lines) shows green/red rows |
| 12 | TODO-12 | OTC sale shows inline stock warning and disables submit | VERIFIED | `OtcSaleForm.tsx` calls `useDrugAvailableStock`; notes included in payload; submit disabled when stock exceeded |
| 13 | TODO-13 | Realtime OSDI score updates via SignalR | VERIFIED | `OsdiHub.cs` with auth + visit authorization; `use-osdi-hub.ts` (134 lines) connects to `/api/hubs/osdi`; wired in `VisitDetailPage.tsx` |

**Score: 13/13 truths verified**

---

## Required Artifacts

### Plan 01 — Quick UX Fixes (TODO-01, 02, 03, 06, 09)

| Artifact | Status | Details |
|----------|--------|---------|
| `frontend/src/shared/components/AutoResizeTextarea.tsx` | VERIFIED | 57 lines; imports Textarea, merged ref pattern, scrollHeight resize |
| `frontend/src/features/clinical/components/OpticalPrescriptionSection.tsx` | VERIFIED | `useState(true)` for default open; controlled `open={sectionOpen}` |
| `frontend/src/features/clinical/components/DrugCombobox.tsx` | VERIFIED | `autoFocus` on Input elements at lines 140 and 168 |
| `frontend/src/features/clinical/components/PatientInfoSection.tsx` | VERIFIED | `target="_blank"` link at line 48 |

### Plan 02 — Pharmacy Backend (TODO-05, 11, 12)

| Artifact | Status | Details |
|----------|--------|---------|
| `backend/.../Features/PaginatedDrugCatalog.cs` | VERIFIED | `Math.Clamp(pageSize, 1, 100)`, `GetPaginatedAsync`, returns `(Items, TotalCount, Page, PageSize, TotalPages)` |
| `backend/.../Features/DrugCatalog/ImportDrugCatalogFromExcel.cs` | VERIFIED | `MiniExcel.Query<DrugCatalogExcelRow>`, per-row validation, .xls rejected, fractional check |
| `backend/.../Features/DrugCatalog/ConfirmDrugCatalogImport.cs` | VERIFIED | Server-side re-validation, HashSet duplicate check within batch and against catalog |
| `backend/.../Features/DrugCatalog/GetDrugCatalogTemplate.cs` | VERIFIED | File exists; Excel template download endpoint |
| `backend/.../Features/OtcSales/GetDrugAvailableStock.cs` | VERIFIED | Uses `GetTotalStockAsync` for server-side aggregation |

### Plan 03 — Clinical Backend (TODO-07, 10, 13)

| Artifact | Status | Details |
|----------|--------|---------|
| `backend/.../Features/DryEye/GetDryEyeMetricHistory.cs` | VERIFIED | Returns `DryEyeMetricHistoryResponse` with 5 metrics; calls `GetMetricHistoryAsync` |
| `backend/.../Features/Osdi/GetOsdiAnswers.cs` | VERIFIED | Correct clinical labels: Ocular Symptoms (Q1-5), Vision-Related Function (Q6-9), Environmental Triggers (Q10-12) |
| `backend/.../Infrastructure/Hubs/OsdiHub.cs` | VERIFIED | `JoinVisit(Guid visitId)` + `Groups.AddToGroupAsync`; injects `IVisitRepository` for authorization |
| `backend/.../Services/OsdiNotificationService.cs` | VERIFIED | Registered as `Singleton` in `IoC.cs` |
| `backend/.../Features/Osdi/NotifyOsdiSubmitted.cs` | VERIFIED | Event handler triggering SignalR push |

### Plan 04 — Logo Upload, Batch Labels, Search Fix (TODO-04, 08, 09)

| Artifact | Status | Details |
|----------|--------|---------|
| `backend/.../Features/UploadClinicLogo.cs` | VERIFIED | Magic-byte validation, `IAzureBlobService.UploadAsync`, `settingsService.UpdateLogoUrlAsync` |
| `backend/.../Documents/BatchPharmacyLabelDocument.cs` | VERIFIED | `page.Size(70, 35, Unit.Millimetre)` per drug; null guard for `ClinicName` |
| `backend/.../Features/Prescriptions/PrintBatchLabels.cs` | VERIFIED | Delegates to `IDocumentService`; wired to endpoint |

### Plan 05 — Pharmacy Frontend (TODO-05, 11, 12)

| Artifact | Status | Details |
|----------|--------|---------|
| `frontend/.../components/DrugCatalogImportDialog.tsx` | VERIFIED | 331 lines; imports `useImportDrugCatalogPreview`, `useConfirmDrugCatalogImport`; calls `/import/preview` |
| `frontend/.../components/DrugCatalogPage.tsx` | VERIFIED | `useSearchDrugCatalog`, `useDebounce(300ms)`, `useEffect` resets pageIndex on `debouncedSearch` change |
| `frontend/.../components/OtcSaleForm.tsx` | VERIFIED | `useDrugAvailableStock` per row, `notes: data.notes?.trim() || null` in submit payload |

### Plan 06 — Clinical Frontend (TODO-07, 08, 10, 13)

| Artifact | Status | Details |
|----------|--------|---------|
| `frontend/.../components/DryEyeMetricCharts.tsx` | VERIFIED | 185 lines; calls `useDryEyeMetricHistory`; integrated into `PatientDryEyeTab.tsx` |
| `frontend/.../components/OsdiAnswersSection.tsx` | VERIFIED | 103 lines; calls `useOsdiAnswers` lazily; conditional render pattern (not Collapsible misuse) |
| `frontend/.../hooks/use-osdi-hub.ts` | VERIFIED | 134 lines; connects to `/api/hubs/osdi`; `isMounted` ref guard; `connectionRef.current = null` on cleanup |

---

## Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `AutoResizeTextarea.tsx` | `Textarea.tsx` | `import { Textarea }` | WIRED | Line 3: `import { Textarea } from "@/shared/components/Textarea"` |
| `PaginatedDrugCatalog.cs` | `IDrugCatalogItemRepository` | `GetPaginatedAsync` | WIRED | `repository.GetPaginatedAsync(page, pageSize, search, ct)` |
| `ImportDrugCatalogFromExcel.cs` | `MiniExcel` | Excel parsing | WIRED | `MiniExcel.Query<DrugCatalogExcelRow>` at line 83 |
| `GetDrugAvailableStock.cs` | `IDrugBatchRepository` | `GetTotalStockAsync` | WIRED | Server-side aggregation (not batch loading) |
| `GetDryEyeMetricHistory.cs` | `IVisitRepository` | `GetMetricHistoryAsync` | WIRED | `visitRepository.GetMetricHistoryAsync(query.PatientId, since, ct)` |
| `OsdiHub.cs` | `Groups.AddToGroupAsync` | BillingHub pattern | WIRED | `await Groups.AddToGroupAsync(Context.ConnectionId, $"visit-{visitId}")` |
| `NotifyOsdiSubmitted.cs` | `OsdiNotificationService` | `NotifyAsync` | WIRED | Event handler triggers notification service |
| `UploadClinicLogo.cs` | `IAzureBlobService` | `UploadAsync` with "clinic-logos" | WIRED | `await blobService.UploadAsync(...)` + `UpdateLogoUrlAsync` |
| `BatchPharmacyLabelDocument.cs` | `PharmacyLabelDocument.cs` | 70x35mm page pattern | WIRED | `page.Size(70, 35, Unit.Millimetre)` per drug in batch |
| `DryEyeMetricCharts.tsx` | `/api/clinical/patients/{patientId}/dry-eye/metric-history` | `useDryEyeMetricHistory` | WIRED | API call with `timeRange` param |
| `DryEyeMetricCharts.tsx` | `PatientDryEyeTab.tsx` | imported and rendered | WIRED | `<DryEyeMetricCharts patientId={patientId} />` at line 15 |
| `OsdiAnswersSection.tsx` | `/api/clinical/visits/{visitId}/osdi-answers` | `useOsdiAnswers` | WIRED | Lazy fetch: only calls when `open=true` |
| `use-osdi-hub.ts` | `/hubs/osdi` | `HubConnectionBuilder` | WIRED | `const HUB_URL = \`${API_URL}/api/hubs/osdi\`` at line 15 |
| `VisitDetailPage.tsx` | `useOsdiHub` | imported and called | WIRED | `useOsdiHub(visitId)` at line 37 |
| `DrugPrescriptionSection.tsx` | `generateBatchLabelsPdf` | batch labels button | WIRED | `onClick={() => generateBatchLabelsPdf(rx.id)}` |
| `ClinicSettingsPage.tsx` | `useUploadClinicLogo` | logo upload mutation | WIRED | `const uploadLogoMutation = useUploadClinicLogo()` |
| `Program.cs` | `OsdiHub` | `app.MapHub<OsdiHub>` | WIRED | `app.MapHub<OsdiHub>("/api/hubs/osdi")` at line 355 |
| `OsdiNotificationService` | `IoC.cs` | `AddSingleton` | WIRED | `services.AddSingleton<IOsdiNotificationService, OsdiNotificationService>()` |

---

## Requirements Coverage

The 13 phase-specific TODO requirements are tracked internally in plans (not in REQUIREMENTS.md traceability table, which tracks v1 system requirements separately). All 13 TODOs declared in plans 10-01 through 10-06 are verified implemented above.

| Plan | Requirements | Status |
|------|-------------|--------|
| 10-01 | TODO-01, TODO-02, TODO-03, TODO-06, TODO-09 | SATISFIED |
| 10-02 | TODO-05, TODO-11, TODO-12 | SATISFIED |
| 10-03 | TODO-07, TODO-10, TODO-13 | SATISFIED |
| 10-04 | TODO-04, TODO-08, TODO-09 | SATISFIED |
| 10-05 | TODO-05, TODO-11, TODO-12 | SATISFIED (frontend layer) |
| 10-06 | TODO-07, TODO-08, TODO-09, TODO-10, TODO-13 | SATISFIED (frontend layer) |
| 10-07 | All 13 | INTEGRATION VERIFIED via Playwright |
| 10-08, 10-09, 10-10 | No new TODO requirements; code review fixes | SATISFIED |

Note: TODO requirements are not in REQUIREMENTS.md — they are phase-internal items from earlier phase backlogs. REQUIREMENTS.md v1 system requirements (AUTH, PAT, CLN, etc.) are not impacted by Phase 10.

---

## Backend Code Review Fixes (Plans 08, 09, 10)

The following hardening issues from code review were verified fixed:

| Fix | Status | Evidence |
|-----|--------|----------|
| Excel upload 10MB file size limit | VERIFIED | `PharmacyApiEndpoints.cs` line 91-92: `const long maxFileSizeBytes = 10 * 1024 * 1024` |
| ConfirmDrugCatalogImport server-side re-validation | VERIFIED | Non-empty Name/Unit, SellingPrice>=0, HashSet duplicate checks |
| TryParseInt fractional value rejection | VERIFIED | `if (d != Math.Floor(d)) { result = 0; return false; }` at line 171 |
| OsdiHub visitId as Guid + authorization | VERIFIED | `JoinVisit(Guid visitId)`, injects `IVisitRepository` for ownership check |
| UploadClinicLogo uses `file.Length` from endpoint | VERIFIED | `SettingsApiEndpoints.cs`: `new UploadClinicLogoCommand(stream, ..., file.Length)` |
| UploadClinicLogo magic-byte validation | VERIFIED | `JpegMagic`, `PngMagic` byte arrays; first 12 bytes validated |
| UploadClinicLogo persists blob URL to ClinicSettings | VERIFIED | `settingsService.UpdateLogoUrlAsync(blobUrl, ct)` |
| .xls format explicitly rejected | VERIFIED | `".xls" => throw new NotSupportedException(...)` at line 195 |
| OSDI category labels corrected | VERIFIED | Q1-5: "Ocular Symptoms", Q6-9: "Vision-Related Function", Q10-12: "Environmental Triggers" |
| GetMetricHistoryAsync filters Signed/Amended visits | VERIFIED | `.Where(x => x.Visit.Status == VisitStatus.Signed || ... Amended)` at line 141 |
| OsdiNotificationService registered as Singleton | VERIFIED | `services.AddSingleton<IOsdiNotificationService, ...>()` in `IoC.cs` |
| GetDrugAvailableStock uses GetTotalStockAsync | VERIFIED | `batchRepository.GetTotalStockAsync(...)` — no batch loading |
| BatchPharmacyLabelDocument null guard for ClinicName | VERIFIED | `!string.IsNullOrEmpty(label.ClinicName)` at line 36 |
| DocumentService concurrent queries via Task.WhenAll | VERIFIED | `await Task.WhenAll(prescriptionTask, headerTask)` at line 204 |
| OTC notes included in submission payload | VERIFIED | `notes: data.notes?.trim() || null` at line 422 of `OtcSaleForm.tsx` |
| OpticalPrescriptionSection collapsible (not just defaultOpen) | VERIFIED | Controlled `open={sectionOpen}` + `onOpenChange={setSectionOpen}` |
| ExaminationNotesSection resize-y replaced with resize-none | VERIFIED | `className="min-h-[120px] resize-none"` at line 70 |
| BookingForm uses Controller pattern for AutoResizeTextarea | VERIFIED | `<Controller name="notes" ... render={... <AutoResizeTextarea ...>}` |
| use-osdi-hub isMounted ref guard | VERIFIED | `isMounted.current = false` + `connectionRef.current = null` in cleanup |
| ClinicSettingsPage client-side MIME validation | VERIFIED | `ALLOWED_TYPES` check before upload at line 122-126 |
| DrugCatalogPage pageIndex reset via useEffect | VERIFIED | `useEffect(() => { setPagination(...pageIndex: 0...) }, [debouncedSearch])` |
| StockImportForm shouldValidate on drug selection | VERIFIED | `form.setValue(..., { shouldValidate: true, shouldDirty: true })` at line 335 |
| OsdiAnswersSection simplified to conditional render | VERIFIED | `useState(false)` + conditional `{open && <div>...}` — no Collapsible misuse |

---

## Test Coverage

| Test Suite | Tests | Status |
|------------|-------|--------|
| Pharmacy.Unit.Tests | 132 tests (18 new: 6 pagination, 3 stock check, 9 import) | PASS per 10-08 SUMMARY |
| Clinical.Unit.Tests | 164 tests (15 new: 8 metric history, 5 OSDI answers, 2 notification) | PASS per 10-08 SUMMARY |
| Shared.Unit.Tests | 35 tests (8 new: logo upload validation) | PASS per 10-04 SUMMARY |
| UploadClinicLogoHandlerTests | `_settingsService.Received(1).UpdateLogoUrlAsync(...)` assertion | VERIFIED |
| OsdiNotificationServiceTests | `LogLevel.Warning` assertion on SignalR failure | VERIFIED |
| PrintBatchLabelsHandlerTests | Empty items test at line 71 and 93; dead `_visitRepository` removed | VERIFIED |
| Full backend (484 tests) | PASS per 10-07 SUMMARY (7 pre-existing auth integration test failures unrelated to Phase 10) | PASS |
| Frontend TypeScript | No Phase 10 regressions per 10-07 SUMMARY (5 pre-existing errors) | PASS |

---

## Anti-Patterns Found

| File | Pattern | Severity | Impact |
|------|---------|----------|--------|
| `frontend/src/features/pharmacy/api/pharmacy-api.ts:628` | `// TODO: Multi-branch support — replace hardcoded branchId` | INFO | Documented limitation; single-branch operation unaffected for v1 |

No blocker anti-patterns found. The branchId TODO is explicitly documented per plan 10-09 decision ("If multi-branch is not yet supported, add a TODO comment explaining the hardcode and what needs to change").

---

## Human Verification Required

The following items require visual/interactive verification in a running browser. Automated code inspection confirms implementation is present and wired; behavior must be confirmed by a human tester.

### 1. AutoResizeTextarea Live Resizing

**Test:** Navigate to any form with a textarea (e.g., visit notes, examination notes). Type multiple lines of text.
**Expected:** The textarea field expands in height to fit the content without showing a scrollbar.
**Why human:** Height resize via `scrollHeight` requires actual browser rendering cycle to verify.

### 2. OpticalPrescriptionSection Default Open

**Test:** Open a visit detail page that has an optical prescription.
**Expected:** The "Optical Prescription" collapsible section is already expanded when the page loads.
**Why human:** Visual accordion state requires rendered output.

### 3. DrugCombobox Auto-Focus

**Test:** In a prescription form (clinical visit), click the drug selection combobox.
**Expected:** The search input field inside the combobox popover receives focus immediately (cursor blinking, can type without clicking).
**Why human:** Focus state requires live browser interaction.

### 4. Patient Name Link Opens New Tab

**Test:** On the visit detail page, click the patient name displayed in the patient info section.
**Expected:** The patient profile page opens in a new browser tab.
**Why human:** Navigation behavior and tab opening require live testing.

### 5. Drug Catalog Excel Import Preview (Visual)

**Test:** On Drug Catalog page, click "Import from Excel". Upload an Excel file with some valid rows and some invalid rows (e.g., missing Name field, negative price).
**Expected:** A preview table appears showing valid rows with a green indicator and invalid rows highlighted red with error messages per cell.
**Why human:** Color-coded row display requires visual inspection.

### 6. OTC Sale Inline Stock Warning

**Test:** On the OTC Sale form, add a drug and set the quantity higher than the available stock (check drug catalog for a low-stock item).
**Expected:** An inline warning appears below the row ("Only N in stock" or "Out of stock"), and the Submit button becomes disabled.
**Why human:** Requires real API responses from the available-stock endpoint and dynamic UI state change.

### 7. Dry Eye Metric Charts

**Test:** Open a patient record that has dry eye assessments. Go to the Dry Eye tab.
**Expected:** Five stacked line charts appear (TBUT, Schirmer, Meibomian Grading, TearMeniscus, Staining Score), each with two lines (OD in one color, OS in another) and a time range selector (3m/6m/1y/All).
**Why human:** Chart rendering with real data and correct metric labeling requires visual verification.

### 8. OSDI Answers Expandable Section

**Test:** Open a visit detail page for a patient who has completed the OSDI questionnaire. Look next to the OSDI score display.
**Expected:** A "View Details" button (or equivalent) appears. Clicking it expands a section showing 12 questions grouped into three categories: Ocular Symptoms (Q1-5), Vision-Related Function (Q6-9), Environmental Triggers (Q10-12), with individual scores per question.
**Why human:** Expandable section behavior and correct category grouping require visual testing.

### 9. Batch Labels PDF Download

**Test:** On a visit detail page with drug prescriptions, locate the "Print All Labels" button in the prescription section.
**Expected:** Clicking the button downloads or opens a PDF containing one 70x35mm thermal label per prescribed drug with patient name, drug name, dosage, frequency, and expiry.
**Why human:** PDF generation and browser download behavior require a running backend and visual inspection of PDF content.

### 10. Clinic Logo Upload with Preview

**Test:** Navigate to Settings > Clinic Settings. In the Logo section, select a JPEG or PNG image file.
**Expected:** A preview of the selected image appears before uploading. Clicking Upload sends the file to the server and shows a success toast. Refreshing the page shows the uploaded logo.
**Why human:** File selection preview, Azure Blob Storage upload, and settings persistence require a live backend with Azure credentials.

### 11. Stock Import Drug Search Filter

**Test:** Navigate to the Stock Import page. In the drug search combobox, type part of a drug name.
**Expected:** The dropdown filters to show only drugs matching the typed text (by name or generic name).
**Why human:** Combobox filtering UX requires interactive browser testing.

### 12. Realtime OSDI Update

**Test:** Open a visit detail page in one browser tab. In another tab or device, navigate to the patient-facing OSDI questionnaire QR page for that visit and complete it.
**Expected:** The OSDI score on the visit detail page updates automatically without a page refresh.
**Why human:** Realtime SignalR push requires two simultaneous sessions and live network conditions.

---

## Gaps Summary

No gaps found. All 13 TODO items are implemented with substantive code (not stubs), properly wired through the application, and verified by Playwright automation in plan 10-07. The only remaining items require human eyes-on testing of visual and interactive behaviors that cannot be confirmed by static code inspection alone.

---

_Verified: 2026-03-14T09:30:00Z_
_Verifier: Claude (gsd-verifier)_

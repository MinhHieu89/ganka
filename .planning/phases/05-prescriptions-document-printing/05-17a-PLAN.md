---
phase: 05-prescriptions-document-printing
plan: 17a
type: execute
wave: 8
depends_on: ["05-12b", "05-15", "05-16"]
files_modified:
  - frontend/src/features/clinical/components/PrintButton.tsx
  - frontend/src/features/clinical/api/document-api.ts
  - frontend/src/features/clinical/components/DrugPrescriptionSection.tsx
  - frontend/src/features/clinical/components/OpticalPrescriptionSection.tsx
autonomous: true
requirements:
  - PRT-01
  - PRT-02
  - PRT-04
  - PRT-05
  - PRT-06
must_haves:
  truths:
    - "Print buttons appear on DrugPrescriptionSection and OpticalPrescriptionSection"
    - "Clicking print fetches PDF blob and opens in new browser tab"
    - "Print buttons disabled when no data exists for that document type"
    - "Document API functions handle auth and blob response correctly"
  artifacts:
    - path: "frontend/src/features/clinical/components/PrintButton.tsx"
      provides: "Generic print button that opens PDF in new tab"
      contains: "PrintButton"
    - path: "frontend/src/features/clinical/api/document-api.ts"
      provides: "API functions for PDF generation endpoints"
      contains: "generateDrugPrescriptionPdf|generateOpticalPrescriptionPdf"
  key_links:
    - from: "PrintButton.tsx"
      to: "document-api.ts"
      via: "Fetch PDF blob and open in new tab"
      pattern: "window\\.open|URL\\.createObjectURL"
    - from: "DrugPrescriptionSection.tsx"
      to: "PrintButton.tsx"
      via: "Print Drug Rx button in headerExtra"
      pattern: "PrintButton|printDrugRx"
    - from: "OpticalPrescriptionSection.tsx"
      to: "PrintButton.tsx"
      via: "Print Optical Rx button"
      pattern: "PrintButton|printOpticalRx"
---

<objective>
Add print button component, document API functions, and integrate print buttons into prescription sections.

Purpose: Connects the frontend to the PDF generation backend. PrintButton fetches PDF bytes and opens in a new browser tab for printing/downloading. Integrates print buttons into DrugPrescriptionSection and OpticalPrescriptionSection.

Output: PrintButton component, document API functions, updated prescription sections with print buttons
</objective>

<execution_context>
@C:/Users/minhh/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/minhh/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/phases/05-prescriptions-document-printing/05-CONTEXT.md
@.planning/phases/05-prescriptions-document-printing/05-12b-SUMMARY.md
@.planning/phases/05-prescriptions-document-printing/05-15-SUMMARY.md
@.planning/phases/05-prescriptions-document-printing/05-16-SUMMARY.md

@frontend/src/features/clinical/components/DrugPrescriptionSection.tsx
@frontend/src/features/clinical/components/OpticalPrescriptionSection.tsx

<interfaces>
Backend print endpoints (from 05-12b):
```
GET /api/clinical/{visitId}/print/drug-rx -> PDF file
GET /api/clinical/{visitId}/print/optical-rx -> PDF file
GET /api/clinical/{visitId}/print/referral-letter?reason=...&to=... -> PDF file
GET /api/clinical/{visitId}/print/consent-form?procedureType=... -> PDF file
GET /api/clinical/prescription-items/{itemId}/print/label -> PDF file
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Create PrintButton component and document API functions</name>
  <files>
    frontend/src/features/clinical/components/PrintButton.tsx,
    frontend/src/features/clinical/api/document-api.ts
  </files>
  <action>
**document-api.ts**: API functions for PDF generation:
```typescript
// Not TanStack Query hooks -- these are one-shot fetch functions for PDF blobs

export async function generateDrugPrescriptionPdf(visitId: string): Promise<Blob> {
  const token = useAuthStore.getState().accessToken
  const res = await fetch(`${API_URL}/api/clinical/${visitId}/print/drug-rx`, {
    headers: { Authorization: `Bearer ${token}` },
    credentials: 'include'
  })
  if (!res.ok) throw new Error('Failed to generate PDF')
  return res.blob()
}

export async function generateOpticalPrescriptionPdf(visitId: string): Promise<Blob> { ... }
export async function generateReferralLetterPdf(visitId: string, reason: string, to: string): Promise<Blob> { ... }
export async function generateConsentFormPdf(visitId: string, procedureType: string): Promise<Blob> { ... }
export async function generatePharmacyLabelPdf(prescriptionItemId: string): Promise<Blob> { ... }
```

Use native fetch (not openapi-fetch) since we need blob response handling. Include auth token from store. Include credentials: 'include' for cookie-based auth.

**PrintButton.tsx**: Generic print button:
```typescript
interface PrintButtonProps {
  onClick: () => Promise<Blob>
  label: string
  disabled?: boolean
  icon?: React.ReactNode
  variant?: "outline" | "ghost" | "default"
  size?: "sm" | "default"
}

export function PrintButton({ onClick, label, disabled, icon, variant = "outline", size = "sm" }: PrintButtonProps) {
  const [isPrinting, setIsPrinting] = useState(false)
  const handlePrint = async () => {
    setIsPrinting(true)
    try {
      const blob = await onClick()
      const url = URL.createObjectURL(blob)
      window.open(url, '_blank')
      // Clean up blob URL after a delay
      setTimeout(() => URL.revokeObjectURL(url), 30000)
    } catch (err) {
      toast.error(t("common:errors.printFailed"))
    } finally {
      setIsPrinting(false)
    }
  }
  return <Button variant={variant} size={size} disabled={disabled || isPrinting} onClick={handlePrint}>
    {isPrinting ? <IconLoader2 className="h-4 w-4 animate-spin" /> : icon}
    {label}
  </Button>
}
```
  </action>
  <verify>
    <automated>cd D:/projects/ganka/frontend && npx tsc --noEmit 2>&1 | head -30</automated>
  </verify>
  <done>PrintButton component created. Document API functions handle auth and blob responses for all 5 document types.</done>
</task>

<task type="auto">
  <name>Task 2: Integrate print buttons into prescription sections</name>
  <files>
    frontend/src/features/clinical/components/DrugPrescriptionSection.tsx,
    frontend/src/features/clinical/components/OpticalPrescriptionSection.tsx
  </files>
  <action>
**DrugPrescriptionSection.tsx** -- add print button:
- Import PrintButton and generateDrugPrescriptionPdf
- Add "In don thuoc" (Print Drug Rx) PrintButton in headerExtra area (next to "Add Drug" button)
- Only show when prescription exists and has items
- Also add per-item "In nhan" (Print Label) button for each drug line item (calls generatePharmacyLabelPdf with item ID)

**OpticalPrescriptionSection.tsx** -- add print button:
- Import PrintButton and generateOpticalPrescriptionPdf
- Add "In don kinh" (Print Optical Rx) PrintButton in headerExtra area
- Only show when optical prescription exists

Both sections should also support print buttons for referral letter and consent form, either as:
- Additional buttons in a "Print Actions" area at the bottom of the visit page, or
- Dropdown menu on the print button with multiple document options

Use the simpler approach: individual PrintButton per document type where it makes sense contextually.
  </action>
  <verify>
    <automated>cd D:/projects/ganka/frontend && npx tsc --noEmit 2>&1 | head -30</automated>
  </verify>
  <done>Print buttons integrated into DrugPrescriptionSection and OpticalPrescriptionSection. Drug Rx, Optical Rx, and pharmacy label print buttons all wired.</done>
</task>

</tasks>

<verification>
- `cd frontend && npx tsc --noEmit` passes
- PrintButton opens PDF in new tab with proper auth
- Print buttons visible on prescription sections when data exists
- Print buttons disabled when no prescription data
</verification>

<success_criteria>
Print functionality wired into prescription UI. Users can print drug prescriptions, optical prescriptions, and pharmacy labels directly from the visit detail page.
</success_criteria>

<output>
After completion, create `.planning/phases/05-prescriptions-document-printing/05-17a-SUMMARY.md`
</output>

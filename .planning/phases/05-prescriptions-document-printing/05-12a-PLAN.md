---
phase: 05-prescriptions-document-printing
plan: 12a
type: execute
wave: 6
depends_on: ["05-11"]
files_modified:
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/OpticalPrescriptionDocument.cs
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/ReferralLetterDocument.cs
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/ConsentFormDocument.cs
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/PharmacyLabelDocument.cs
autonomous: true
requirements:
  - PRT-02
  - PRT-04
  - PRT-05
  - PRT-06
must_haves:
  truths:
    - "Optical Rx prints on A4 with OD/OS refraction grid and lens type"
    - "Referral letter prints on A4 with patient info, diagnosis, and referral reason"
    - "Consent form prints on A4 with procedure type and patient signature line"
    - "Pharmacy label prints on small label format with drug name, dose, frequency"
    - "All documents use ClinicHeaderComponent for consistent branding"
  artifacts:
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/OpticalPrescriptionDocument.cs"
      provides: "A4 optical prescription PDF"
      contains: "class OpticalPrescriptionDocument : IDocument"
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/ReferralLetterDocument.cs"
      provides: "A4 referral letter PDF"
      contains: "class ReferralLetterDocument : IDocument"
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/ConsentFormDocument.cs"
      provides: "A4 consent form PDF"
      contains: "class ConsentFormDocument : IDocument"
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/PharmacyLabelDocument.cs"
      provides: "Small-format pharmacy label PDF"
      contains: "class PharmacyLabelDocument : IDocument"
  key_links:
    - from: "All 4 document classes"
      to: "ClinicHeaderComponent"
      via: "Shared header rendering"
      pattern: "ClinicHeaderComponent"
---

<objective>
Implement remaining 4 printable document types (QuestPDF IDocument classes).

Purpose: Creates the OpticalPrescriptionDocument, ReferralLetterDocument, ConsentFormDocument, and PharmacyLabelDocument. Each uses the shared ClinicHeaderComponent and Noto Sans Vietnamese font from Plan 11.

Output: 4 document IDocument implementations
</objective>

<execution_context>
@C:/Users/minhh/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/minhh/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/phases/05-prescriptions-document-printing/05-CONTEXT.md
@.planning/phases/05-prescriptions-document-printing/05-RESEARCH.md
@.planning/phases/05-prescriptions-document-printing/05-11-SUMMARY.md

@backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/DrugPrescriptionDocument.cs

<interfaces>
From 05-11:
```csharp
// ClinicHeaderComponent -- reusable QuestPDF header block
// DocumentFontManager.RegisterFonts() -- Noto Sans registration
// DrugPrescriptionDocument as IDocument pattern reference
```

From 05-05b (DTOs):
```csharp
public sealed record OpticalPrescriptionDto(Guid Id, Guid VisitId, decimal? OdSph, ...);
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Create OpticalPrescriptionDocument and ReferralLetterDocument</name>
  <files>
    backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/OpticalPrescriptionDocument.cs,
    backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/ReferralLetterDocument.cs
  </files>
  <action>
**OpticalPrescriptionDocument.cs**: QuestPDF IDocument
- A4 page, 15mm margins, Noto Sans font
- Layout:
  - ClinicHeaderComponent
  - Title: "DON KINH" (Glasses Prescription) centered, bold 14pt
  - Patient info: Ho ten, Ngay sinh, Gioi tinh
  - **Distance Rx (Kinh nhin xa)**: OD/OS grid with SPH, CYL, AXIS, ADD columns
  - **Near Rx (Kinh nhin gan)**: OD/OS grid (if near overrides present)
  - **PD**: Far PD (PD xa) and Near PD (PD gan)
  - Lens type recommendation (Loai trong kinh): SingleVision/Bifocal/Progressive/Reading
  - Doctor notes
  - Footer: Date + Doctor name + signature line

**ReferralLetterDocument.cs**: QuestPDF IDocument
- A4 page, 20mm margins
- Layout:
  - ClinicHeaderComponent
  - Title: "GIAY CHUYEN VIEN" centered, bold
  - "Kinh gui" (To): referral destination field
  - Patient info: full block (name, DOB, gender, address, CCCD)
  - "Chan doan" (Diagnosis): from visit diagnoses
  - "Ly do chuyen vien" (Reason for referral): provided text
  - "Tom tat benh an" (Clinical summary): examination notes
  - Footer: Date + Referring doctor name + stamp line
  - Space for hospital receiving stamp

All Vietnamese text must use proper diacritics.
  </action>
  <verify>
    <automated>dotnet build backend/src/Modules/Clinical/Clinical.Infrastructure/Clinical.Infrastructure.csproj</automated>
  </verify>
  <done>OpticalPrescriptionDocument renders A4 OD/OS refraction grid. ReferralLetterDocument renders A4 formal referral with diagnosis and reason.</done>
</task>

<task type="auto">
  <name>Task 2: Create ConsentFormDocument and PharmacyLabelDocument</name>
  <files>
    backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/ConsentFormDocument.cs,
    backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/PharmacyLabelDocument.cs
  </files>
  <action>
**ConsentFormDocument.cs**: QuestPDF IDocument
- A4 page, 20mm margins
- Layout:
  - ClinicHeaderComponent
  - Title: "PHIEU DONG Y THU THUAT / DIEU TRI" centered, bold
  - Patient info block
  - Procedure description section with explanation of risks and benefits
  - "Toi da duoc giai thich va dong y thuc hien thu thuat/dieu tri noi tren" (consent statement)
  - Patient signature line with date
  - Doctor/witness signature line with date
  - Space for fingerprint if patient cannot sign

**PharmacyLabelDocument.cs**: QuestPDF IDocument
- Small label: ~70 x 35mm (custom PageSize)
- Compact layout, 3mm margins, font size 7-8pt:
  - Clinic name (abbreviated if long)
  - Patient name
  - Drug name + strength
  - Dosage/usage instructions (DosageOverride or Dosage)
  - Quantity
  - Date dispensed
- Designed for adhesive label printers (standard pharmacy label stock)

All Vietnamese text must use proper diacritics.
  </action>
  <verify>
    <automated>dotnet build backend/src/Modules/Clinical/Clinical.Infrastructure/Clinical.Infrastructure.csproj</automated>
  </verify>
  <done>ConsentFormDocument renders A4 consent with signature lines. PharmacyLabelDocument renders small-format label.</done>
</task>

</tasks>

<verification>
- `dotnet build backend/src/Modules/Clinical/Clinical.Infrastructure/Clinical.Infrastructure.csproj` passes
- All 4 document types compile and implement IDocument
</verification>

<success_criteria>
All remaining printable document types implemented: optical Rx (A4), referral letter (A4), consent form (A4), pharmacy label (~70x35mm). All use ClinicHeaderComponent and Vietnamese-compatible Noto Sans font.
</success_criteria>

<output>
After completion, create `.planning/phases/05-prescriptions-document-printing/05-12a-SUMMARY.md`
</output>

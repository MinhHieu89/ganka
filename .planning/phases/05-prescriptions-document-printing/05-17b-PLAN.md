---
phase: 05-prescriptions-document-printing
plan: 17b
type: execute
wave: 8
depends_on: ["05-12b", "05-15", "05-16"]
files_modified:
  - frontend/public/locales/en/clinical.json
  - frontend/public/locales/vi/clinical.json
  - frontend/public/locales/en/pharmacy.json
autonomous: true
requirements:
  - RX-01
  - RX-03
  - PRT-01
  - PRT-02
must_haves:
  truths:
    - "i18n translations added for all prescription UI labels in English and Vietnamese"
    - "Drug form and route enum translations added"
    - "Pharmacy catalog translations added"
    - "All Vietnamese text uses proper diacritics"
  artifacts:
    - path: "frontend/public/locales/en/clinical.json"
      provides: "English translations for prescription UI"
      contains: "prescription"
    - path: "frontend/public/locales/vi/clinical.json"
      provides: "Vietnamese translations for prescription UI"
      contains: "prescription"
    - path: "frontend/public/locales/en/pharmacy.json"
      provides: "English translations for pharmacy module"
      contains: "catalog"
  key_links: []
---

<objective>
Add i18n translations for prescription and pharmacy UI labels.

Purpose: Adds all necessary i18n translations for the prescription and pharmacy UI in both English and Vietnamese. These translations are used by DrugPrescriptionSection, OpticalPrescriptionSection, DrugCatalogPage, and PrintButton components.

Output: Updated clinical.json (EN/VI), new pharmacy.json (EN)
</objective>

<execution_context>
@C:/Users/minhh/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/minhh/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/phases/05-prescriptions-document-printing/05-CONTEXT.md

@frontend/public/locales/en/clinical.json
@frontend/public/locales/vi/clinical.json
</context>

<tasks>

<task type="auto">
  <name>Task 1: Add prescription and pharmacy i18n translations</name>
  <files>
    frontend/public/locales/en/clinical.json,
    frontend/public/locales/vi/clinical.json,
    frontend/public/locales/en/pharmacy.json
  </files>
  <action>
**clinical.json (EN)** -- add prescription keys under existing structure:
```json
{
  "prescription": {
    "drugRx": "Drug Prescription",
    "opticalRx": "Optical Prescription",
    "addDrug": "Add Drug",
    "editDrug": "Edit Drug",
    "removeDrug": "Remove Drug",
    "noPrescriptions": "No prescriptions yet",
    "offCatalog": "Off-catalog",
    "allergyWarning": "Allergy Warning",
    "allergyConfirmTitle": "Allergy Warning - Confirm Prescription",
    "allergyConfirmMessage": "This prescription contains drugs the patient may be allergic to. Are you sure you want to proceed?",
    "confirmPrescribe": "Confirm Prescription",
    "drugName": "Drug Name",
    "genericName": "Generic Name",
    "strength": "Strength",
    "form": "Form",
    "route": "Route",
    "dosage": "Dosage",
    "dosageOverride": "Custom Dosage Instructions",
    "quantity": "Quantity",
    "unit": "Unit",
    "frequency": "Frequency",
    "duration": "Duration (days)",
    "doctorAdvice": "Doctor's Advice",
    "writeOpticalRx": "Write Optical Rx",
    "autoFillRefraction": "Auto-fill from Refraction",
    "distanceRx": "Distance Rx",
    "nearRx": "Near Rx",
    "farPd": "Far PD",
    "nearPd": "Near PD",
    "lensType": "Lens Type",
    "singleVision": "Single Vision",
    "bifocal": "Bifocal",
    "progressive": "Progressive",
    "reading": "Reading",
    "printDrugRx": "Print Drug Rx",
    "printOpticalRx": "Print Optical Rx",
    "printReferral": "Print Referral Letter",
    "printConsent": "Print Consent Form",
    "printLabel": "Print Label",
    "referralReason": "Referral Reason",
    "referralTo": "Refer To",
    "procedureType": "Procedure Type"
  },
  "drugForm": {
    "eyeDrops": "Eye Drops",
    "tablet": "Tablet",
    "capsule": "Capsule",
    "ointment": "Ointment",
    "injection": "Injection",
    "gel": "Gel",
    "solution": "Solution",
    "suspension": "Suspension",
    "cream": "Cream",
    "spray": "Spray"
  },
  "drugRoute": {
    "topical": "Topical",
    "oral": "Oral",
    "intramuscular": "Intramuscular",
    "intravenous": "Intravenous",
    "subconjunctival": "Subconjunctival",
    "intravitreal": "Intravitreal",
    "periocular": "Periocular"
  }
}
```

**clinical.json (VI)** -- add Vietnamese translations with proper diacritics:
All Vietnamese text MUST use proper diacritics. No unaccented Vietnamese.

**pharmacy.json (EN)** -- new file for pharmacy module:
```json
{
  "catalog": {
    "title": "Drug Catalog",
    "addDrug": "Add Drug",
    "editDrug": "Edit Drug",
    "search": "Search drugs...",
    "name": "Name",
    "nameVi": "Vietnamese Name",
    "genericName": "Generic Name",
    "form": "Form",
    "strength": "Strength",
    "route": "Route",
    "unit": "Unit",
    "defaultDosage": "Default Dosage",
    "active": "Active",
    "inactive": "Inactive",
    "empty": "No drugs in catalog"
  }
}
```
  </action>
  <verify>
    <automated>cd D:/projects/ganka/frontend && npx tsc --noEmit 2>&1 | head -30</automated>
  </verify>
  <done>i18n translations added for EN and VI covering all prescription UI labels, drug forms, drug routes, and pharmacy catalog. All Vietnamese uses proper diacritics.</done>
</task>

</tasks>

<verification>
- `cd frontend && npx tsc --noEmit` passes
- All UI labels use i18n keys (no hardcoded strings)
- Vietnamese translations use proper diacritics
- pharmacy.json file created for EN
</verification>

<success_criteria>
i18n translations cover all new prescription and pharmacy labels in both English and Vietnamese.
</success_criteria>

<output>
After completion, create `.planning/phases/05-prescriptions-document-printing/05-17b-SUMMARY.md`
</output>

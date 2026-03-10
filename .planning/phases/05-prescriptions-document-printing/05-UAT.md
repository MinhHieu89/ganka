---
status: diagnosed
phase: 05-prescriptions-document-printing
source: 05-01-SUMMARY.md, 05-02-SUMMARY.md, 05-03-SUMMARY.md, 05-04-SUMMARY.md, 05-05a-SUMMARY.md, 05-05b-SUMMARY.md, 05-06-SUMMARY.md, 05-07-SUMMARY.md, 05-08-SUMMARY.md, 05-09-SUMMARY.md, 05-09b-SUMMARY.md, 05-10-SUMMARY.md, 05-11-SUMMARY.md, 05-12a-SUMMARY.md, 05-12b-SUMMARY.md, 05-13-SUMMARY.md, 05-14-SUMMARY.md, 05-15-SUMMARY.md, 05-16-SUMMARY.md, 05-17a-SUMMARY.md, 05-17b-SUMMARY.md, 05-18-SUMMARY.md, 05-19-SUMMARY.md, 05-20-SUMMARY.md
started: 2026-03-10T00:00:00Z
updated: 2026-03-10T09:00:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Cold Start Smoke Test
expected: Kill any running server/service. Start the application from scratch. Server boots without errors, migrations complete, drug catalog seed (78 drugs) populates, and homepage loads with live data.
result: pass

### 2. Pharmacy Sidebar Link
expected: AppSidebar shows "Pharmacy" link in operations section. Clicking it navigates to /pharmacy page.
result: pass

### 3. Drug Catalog List
expected: /pharmacy page displays DataTable with all active drugs showing sortable columns: name, form, route, generic name, unit.
result: pass

### 4. Add Drug to Catalog
expected: Clicking "Add Drug" on /pharmacy opens form. Fill name, generic name, form (EyeDrops), route (Topical), unit, save. Drug appears in catalog table.
result: issue
reported: "UI not accessible. DrugCatalogPage component exists but is not wired to any route. /pharmacy shows inventory view with only Stock Import button."
severity: major

### 5. Edit Drug in Catalog
expected: Clicking Edit on a drug row opens form with pre-filled data. Modify dosage template, save. Changes persist and reflect in table.
result: issue
reported: "UI not accessible. Same root cause as test 4 - DrugCatalogPage not routed."
severity: major

### 6. Drug Catalog Search
expected: Searching "paracetamol" or partial drug name in catalog filters/returns matching drugs with form and route displayed.
result: issue
reported: "UI not accessible. Same root cause as test 4 - DrugCatalogPage not routed."
severity: major

### 7. Prescribe Drug from Catalog
expected: In a visit's DrugPrescriptionSection, click "Add Drug", search "Tobramycin" in DrugCombobox, select from dropdown, specify dose/frequency/duration, save. Prescription appears with drug name, form, route, and generated dosage text.
result: issue
reported: "UI not accessible. DrugPrescriptionSection not integrated into visit detail page."
severity: major

### 8. Prescribe Off-Catalog Drug
expected: In DrugCombobox, switch to "Off-Catalog" mode, type custom drug name, fill prescription details, save. System accepts and saves the off-catalog prescription.
result: issue
reported: "UI not accessible. Same root cause as test 7 - DrugPrescriptionSection not integrated."
severity: major

### 9. Hybrid Dosage Entry
expected: Enter Dose=1, Frequency="Twice daily", Duration=7 days — system auto-generates "1 twice daily for 7 days". Enter free-text override — override text displays instead of generated text.
result: skipped
reason: Dependent on test 7 (drug prescription UI not accessible)

### 10. Edit Drug Prescription
expected: Click Edit on saved prescription item. Form opens with pre-filled data (drug, dosage, form/route). Modify dosage, save. Updated prescription replaces old one.
result: skipped
reason: Dependent on test 7 (drug prescription UI not accessible)

### 11. Remove Drug Prescription
expected: Click Delete on prescription item, confirm. Item removed from DrugPrescriptionSection and database.
result: skipped
reason: Dependent on test 7 (drug prescription UI not accessible)

### 12. Prescription Notes (Loi Dan)
expected: Fill "Loi Dan" textarea in DrugPrescriptionSection, save. Instructions text persists and displays alongside prescription list.
result: skipped
reason: Dependent on test 7 (drug prescription UI not accessible)

### 13. Allergy Warning on Drug Prescription
expected: Prescribe drug for patient with matching allergy record. Red DrugAllergyWarning banner appears showing allergy name and severity. Save requires explicit confirmation via AlertDialog with "Cancel" and "Proceed" buttons.
result: skipped
reason: Dependent on test 7 (drug prescription UI not accessible)

### 14. Write Optical Prescription
expected: Click "Write Optical Rx" in OpticalPrescriptionSection. Form opens with OD/OS distance refraction grids (SPH/CYL/AXIS/ADD). Fill OD SPH=-2.00, OS SPH=-1.50, select LensType (Progressive), save. Prescription appears in section with OD/OS values.
result: skipped
reason: OpticalPrescriptionSection likely not integrated into visit detail page (same pattern as drug prescription)

### 15. Auto-Populate Optical Rx from Manifest Refraction
expected: When visit has manifest refraction data and doctor clicks "Write Optical Rx", form pre-populates OD/OS distance values from manifest. PD auto-fills with average of OD+OS PD values if both present.
result: skipped
reason: Dependent on test 14

### 16. Near Rx and PD Fields
expected: Expand "Near Rx" section in OpticalPrescriptionForm, enter NearOD values (+2.00). Enter FarPD=62, NearPD=58. Values persist after save.
result: skipped
reason: Dependent on test 14

### 17. Edit Optical Prescription
expected: Click Edit on saved optical prescription. Form opens with current values. Modify SPH values, save. Prescription updates (one per visit enforced).
result: skipped
reason: Dependent on test 14

### 18. Clinic Settings Page
expected: Navigate to /admin/clinic-settings. Page displays current clinic settings: name, address, phone, fax, license number, tagline, and clinic logo image.
result: skipped
reason: User requested skip all remaining tests

### 19. Update Clinic Settings
expected: Fill clinic name="Phòng Khám Mắt GANKA", address, phone, fax, license fields, click Save. Settings persist and validate non-empty required fields.
result: skipped
reason: User requested skip all remaining tests

### 20. Upload Clinic Logo
expected: Click logo upload area, select PNG/JPG file. Preview displays. Save persists logo for use in document headers.
result: skipped
reason: User requested skip all remaining tests

### 21. Print Drug Prescription PDF (A5)
expected: Click "Print Rx" in DrugPrescriptionSection. Browser opens new tab with A5 PDF containing: clinic header (logo, name, address), patient info, diagnosis, drug table (name, form, route, dosage), Loi Dan, doctor signature space.
result: skipped
reason: Dependent on drug prescription UI (test 7)

### 22. Print Optical Prescription PDF (A4)
expected: Click "Print Optical Rx" in OpticalPrescriptionSection. Browser opens new tab with A4 PDF showing clinic header, patient info, OD/OS refraction grids, PD, lens type, and notes.
result: skipped
reason: Dependent on optical prescription UI (test 14)

### 23. Print Pharmacy Label (70x35mm)
expected: Click label icon next to drug item in DrugPrescriptionSection. Browser opens new tab with compact 70x35mm PDF containing clinic name, patient name, drug name, dosage, quantity, and date.
result: skipped
reason: Dependent on drug prescription UI (test 7)

### 24. Print Referral Letter PDF (A4)
expected: Click "Print Referral" button. Browser opens PDF with clinic header, patient info, diagnosis, referral reason, clinical summary, and dual signature areas.
result: skipped
reason: User requested skip all remaining tests

### 25. Print Consent Form PDF (A4)
expected: Click "Print Consent". Browser opens PDF with clinic header, procedure explanation, risks/benefits, consent statement, signature lines for patient/doctor/witness, and fingerprint space.
result: skipped
reason: User requested skip all remaining tests

### 26. Vietnamese Diacritics in Documents
expected: Print any document with Vietnamese text (e.g. clinic name "Phòng Khám Mắt GANKA"). All Vietnamese diacritics (ă, â, ê, ô, ơ, ư, etc.) display correctly in PDF.
result: skipped
reason: Dependent on document printing tests

### 27. Clinic Settings Reflected in Documents
expected: After updating clinic name in /admin/clinic-settings, newly printed documents display updated name in header (not hardcoded defaults).
result: skipped
reason: Dependent on clinic settings and document printing tests

### 28. Vietnamese UI Labels
expected: When browser locale is VI, drug prescription labels display in Vietnamese with proper diacritics: "Đơn Thuốc", "Thêm Thuốc", "Liều Lượng", etc.
result: skipped
reason: User requested skip all remaining tests

### 29. Drug Form and Route Enum Labels
expected: Drug form enums show translated labels (EyeDrops="Eye Drops"/"Nhỏ Mắt", Tablet="Viên Nén"). Drug route enums show labels (Topical="Ngoài Da", Oral="Đường Uống").
result: skipped
reason: User requested skip all remaining tests

## Summary

total: 29
passed: 3
issues: 5
pending: 0
skipped: 21

## Gaps

- truth: "Drug catalog CRUD accessible from /pharmacy page (Add/Edit/Search)"
  status: failed
  reason: "User reported: DrugCatalogPage component exists but is not wired to any route. /pharmacy shows inventory view with only Stock Import button."
  severity: major
  test: 4,5,6
  root_cause: "Phase 06-20 overwrote pharmacy/index.tsx, replacing DrugCatalogPage with PharmacyInventoryPage. No separate route was created to preserve drug catalog management access. Three components orphaned: DrugCatalogPage, DrugCatalogTable, DrugFormDialog."
  artifacts:
    - path: "frontend/src/app/routes/_authenticated/pharmacy/index.tsx"
      issue: "DrugCatalogPage import replaced by PharmacyInventoryPage in phase 06-20"
    - path: "frontend/src/features/pharmacy/components/DrugCatalogPage.tsx"
      issue: "Orphaned - zero imports across codebase"
    - path: "frontend/src/features/pharmacy/components/DrugCatalogTable.tsx"
      issue: "Orphaned - has search/filter and edit button"
    - path: "frontend/src/features/pharmacy/components/DrugFormDialog.tsx"
      issue: "Orphaned - has create/edit modes"
  missing:
    - "Create route file frontend/src/app/routes/_authenticated/pharmacy/drug-catalog.tsx"
    - "Add navigation link from pharmacy index page to drug catalog"
  debug_session: ".planning/debug/drug-catalog-page-unreachable.md"

- truth: "Drug prescription section accessible in visit detail page"
  status: resolved
  reason: "User reported: DrugPrescriptionSection not integrated into visit detail page. Components exist but are not rendered."
  severity: false_positive
  test: 7,8
  root_cause: "NO BUG - Both DrugPrescriptionSection and OpticalPrescriptionSection are fully integrated in VisitDetailPage.tsx (lines 115-127). OpticalPrescriptionSection renders collapsed by default (defaultOpen={false}). User needs to verify by scrolling visit detail page."
  artifacts:
    - path: "frontend/src/features/clinical/components/VisitDetailPage.tsx"
      issue: "Both sections imported and rendered - working as expected"
  missing: []
  debug_session: ".planning/debug/prescription-sections-not-visible.md"

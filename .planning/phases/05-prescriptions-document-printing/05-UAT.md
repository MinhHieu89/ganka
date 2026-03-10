---
status: testing
phase: 05-prescriptions-document-printing
source: 05-01-SUMMARY.md, 05-02-SUMMARY.md, 05-03-SUMMARY.md, 05-04-SUMMARY.md, 05-05a-SUMMARY.md, 05-05b-SUMMARY.md, 05-06-SUMMARY.md, 05-07-SUMMARY.md, 05-08-SUMMARY.md, 05-09-SUMMARY.md, 05-09b-SUMMARY.md, 05-10-SUMMARY.md, 05-11-SUMMARY.md, 05-12a-SUMMARY.md, 05-12b-SUMMARY.md, 05-13-SUMMARY.md, 05-14-SUMMARY.md, 05-15-SUMMARY.md, 05-16-SUMMARY.md, 05-17a-SUMMARY.md, 05-17b-SUMMARY.md, 05-18-SUMMARY.md, 05-19-SUMMARY.md, 05-20-SUMMARY.md
started: 2026-03-10T00:00:00Z
updated: 2026-03-10T09:30:00Z
---

## Current Test

number: complete
name: All tests finished
status: done

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
expected: Clicking "Add Drug" on /pharmacy/drug-catalog opens form. Fill name, generic name, form (EyeDrops), route (Topical), unit, save. Drug appears in catalog table.
result: pass (fixed - created /pharmacy/drug-catalog route)

### 5. Edit Drug in Catalog
expected: Clicking Edit on a drug row opens form with pre-filled data. Modify dosage template, save. Changes persist and reflect in table.
result: pass (fixed - same route fix)

### 6. Drug Catalog Search
expected: Searching "paracetamol" or partial drug name in catalog filters/returns matching drugs with form and route displayed.
result: pass (fixed - same route fix)

### 7. Prescribe Drug from Catalog
expected: In a visit's DrugPrescriptionSection, click "Add Drug", search "Tobramycin" in DrugCombobox, select from dropdown, specify dose/frequency/duration, save. Prescription appears with drug name, form, route, and generated dosage text.
result: pass (fixed - dosage duplicate line removed, duration label fixed)

### 8. Prescribe Off-Catalog Drug
expected: In DrugCombobox, switch to "Off-Catalog" mode, type custom drug name, fill prescription details, save. System accepts and saves the off-catalog prescription.
result: pass

### 9. Hybrid Dosage Entry
expected: Enter Dose=1, Frequency="Twice daily", Duration=7 days — system auto-generates "1 twice daily for 7 days". Enter free-text override — override text displays instead of generated text.
result: pass

### 10. Edit Drug Prescription
expected: Click Edit on saved prescription item. Form opens with pre-filled data (drug, dosage, form/route). Modify dosage, save. Updated prescription replaces old one.
result: pass (fixed - doseAmount now round-trips via PrescriptionItemInput)

### 11. Remove Drug Prescription
expected: Click Delete on prescription item, confirm in dialog. Item removed from DrugPrescriptionSection and database.
result: pass (fixed - added delete confirmation AlertDialog)

### 12. Prescription Notes (Loi Dan)
expected: Fill "Loi Dan" textarea in DrugPrescriptionSection, save. Instructions text persists and displays alongside prescription list.
result: pass (fixed - stale closure bug in handleSavePrescription, used notesRef)

### 13. Allergy Warning on Drug Prescription
expected: Prescribe drug for patient with matching allergy record. Red DrugAllergyWarning banner appears showing allergy name and severity. Save requires explicit confirmation via AlertDialog with "Cancel" and "Proceed" buttons.
result: pass

### 14. Write Optical Prescription
expected: Click "Write Optical Rx" in OpticalPrescriptionSection. Form opens with OD/OS distance refraction grids (SPH/CYL/AXIS/ADD). Fill OD SPH=-2.00, OS SPH=-1.50, select LensType (Progressive), save. Prescription appears in section with OD/OS values.
result: pass (fixed - cancel/save button localization actions->buttons, section auto-expand)

### 15. Auto-Populate Optical Rx from Manifest Refraction
expected: Click "Write Optical Rx", section expands, click "Auto-fill from Refraction" button. OD/OS values populate from manifest refraction. PD auto-fills with average if both present. Near PD is manual entry (not in refraction data).
result: pass

### 16. Near Rx and PD Fields
expected: Expand "Near Rx" section in OpticalPrescriptionForm, enter NearOD values (+2.00). Enter FarPD=62, NearPD=58. Values persist after save.
result: pass

### 17. Edit Optical Prescription
expected: Click Edit on saved optical prescription. Form opens with current values. Modify SPH values, save. Prescription updates (one per visit enforced).
result: pass

### 18. Clinic Settings Page
expected: Navigate to /admin/clinic-settings. Page displays current clinic settings: name, address, phone, fax, license number, tagline, and clinic logo image.
result: pass

### 19. Update Clinic Settings
expected: Fill clinic name="Phòng Khám Mắt GANKA", address, phone, fax, license fields, click Save. Settings persist and validate non-empty required fields.
result: pass

### 20. Upload Clinic Logo
expected: Click logo upload area, select PNG/JPG file. Preview displays. Save persists logo for use in document headers.
result: skipped (backend endpoint missing — captured as todo)

### 21. Print Drug Prescription PDF (A5)
expected: Click "Print Rx" in DrugPrescriptionSection. Browser opens new tab with A5 PDF containing: clinic header (logo, name, address), patient info, diagnosis, drug table (name, form, route, dosage), Loi Dan, doctor signature space.
result: pass

### 22. Print Optical Prescription PDF (A4)
expected: Click "Print Optical Rx" in OpticalPrescriptionSection. Browser opens new tab with A4 PDF showing clinic header, patient info, OD/OS refraction grids, PD, lens type, and notes.
result: pass (fixed — PatientCode added to all documents, patient info row alignment fixed)

### 23. Print Pharmacy Label (70x35mm)
expected: Click label icon next to drug item in DrugPrescriptionSection. Browser opens new tab with compact 70x35mm PDF containing clinic name, patient name, drug name, dosage, quantity, and date.
result: pass

### 24. Print Referral Letter PDF (A4)
expected: Click "Print Referral" button. Browser opens PDF with clinic header, patient info, diagnosis, referral reason, clinical summary, and dual signature areas.
result: pass (fixed — created DocumentActionsSection with referral dialog, added to VisitDetailPage)

### 25. Print Consent Form PDF (A4)
expected: Click "Print Consent". Browser opens PDF with clinic header, procedure explanation, risks/benefits, consent statement, signature lines for patient/doctor/witness, and fingerprint space.
result: pass

### 26. Vietnamese Diacritics in Documents
expected: Print any document with Vietnamese text (e.g. clinic name "Phòng Khám Mắt GANKA"). All Vietnamese diacritics (ă, â, ê, ô, ơ, ư, etc.) display correctly in PDF.
result: pass

### 27. Clinic Settings Reflected in Documents
expected: After updating clinic name in /admin/clinic-settings, newly printed documents display updated name in header (not hardcoded defaults).
result: pass

### 28. Vietnamese UI Labels
expected: When browser locale is VI, drug prescription labels display in Vietnamese with proper diacritics: "Đơn Thuốc", "Thêm Thuốc", "Liều Lượng", etc.
result: pass

### 29. Drug Form and Route Enum Labels
expected: Drug form enums show translated labels (EyeDrops="Eye Drops"/"Nhỏ Mắt", Tablet="Viên Nén"). Drug route enums show labels (Topical="Ngoài Da", Oral="Đường Uống").
result: pass

## Summary

total: 29
passed: 27
issues: 0
pending: 0
skipped: 1

## Gaps

- truth: "Drug catalog CRUD accessible from /pharmacy page (Add/Edit/Search)"
  status: fixed
  reason: "Phase 06-20 overwrote pharmacy/index.tsx. Fixed by creating /pharmacy/drug-catalog route and sidebar link."
  severity: major
  test: 4,5,6
  root_cause: "Phase 06-20 overwrote pharmacy/index.tsx, replacing DrugCatalogPage with PharmacyInventoryPage."
  artifacts:
    - path: "frontend/src/app/routes/_authenticated/pharmacy/drug-catalog.tsx"
      issue: "Created new route file"
    - path: "frontend/src/app/routes/_authenticated/pharmacy/index.tsx"
      issue: "Added Drug Catalog nav link"
    - path: "frontend/src/shared/components/AppSidebar.tsx"
      issue: "Added sidebar entry"
  missing: []
  debug_session: ".planning/debug/drug-catalog-page-unreachable.md"

- truth: "PatientCode displayed on all printed documents"
  status: fixed
  reason: "PatientCode was missing from all document data records and PDF templates."
  severity: minor
  test: 22
  root_cause: "PatientCode not included in PatientBasicInfo SQL query or data records."
  artifacts:
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/Shared/DocumentDataRecords.cs"
      issue: "Added PatientCode to all 4 data records"
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Services/DocumentService.cs"
      issue: "Added PatientCode to SQL query and all data mappings"
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/*.cs"
      issue: "Added Mã BN label to all 4 document templates"

- truth: "Patient info rows aligned consistently in all PDF documents"
  status: fixed
  reason: "Ngày sinh/Giới tính row used 50/50 split while Họ tên/Mã BN row used 66/33 split."
  severity: minor
  test: 22
  root_cause: "Inconsistent RelativeItem() proportions between patient info rows."
  artifacts:
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/OpticalPrescriptionDocument.cs"
      issue: "Changed Ngày sinh row to RelativeItem(2)"
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/DrugPrescriptionDocument.cs"
      issue: "Changed Ngày sinh row to RelativeItem(2)"
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/ConsentFormDocument.cs"
      issue: "Changed Ngày sinh row to RelativeItem(2)"
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Documents/ReferralLetterDocument.cs"
      issue: "Changed Ngày sinh row to RelativeItem(2)"

- truth: "Referral Letter and Consent Form print buttons accessible from visit page"
  status: fixed
  reason: "No UI existed for printing referral letters or consent forms."
  severity: major
  test: 24,25
  root_cause: "DocumentActionsSection component was never created during phase execution."
  artifacts:
    - path: "frontend/src/features/clinical/components/DocumentActionsSection.tsx"
      issue: "Created new component with dialogs for referral and consent"
    - path: "frontend/src/features/clinical/components/VisitDetailPage.tsx"
      issue: "Added DocumentActionsSection to visit page"

---
status: investigating
trigger: "Investigate missing Vietnamese localization across the treatment module frontend"
created: 2026-03-19T00:00:00Z
updated: 2026-03-19T00:00:00Z
---

## Current Focus

hypothesis: CONFIRMED - Components use hardcoded English strings instead of `useTranslation()` / `t()` calls
test: Read all 14 treatment components and compared i18n usage
expecting: Hardcoded strings in components
next_action: Document root cause and report findings

## Symptoms

expected: All treatment module UI labels should display in Vietnamese when VN locale is active
actual: Protocol template CRUD dialog, package detail page, and record session dialog show English labels
errors: N/A (functional but not localized)
reproduction: Switch to Vietnamese locale and navigate to treatment module pages
started: Likely since these components were created

## Eliminated

## Evidence

- timestamp: 2026-03-19
  checked: EN and VI treatment.json translation files
  found: Both files have IDENTICAL structure with all 186 lines and same keys. VI file is fully translated.
  implication: The translation files are NOT the problem. All needed keys exist in both languages.

- timestamp: 2026-03-19
  checked: useTranslation usage across all 14 treatment components
  found: Only 3 of 14 components use useTranslation - CancellationApprovalQueue, CancellationRequestDialog, ProtocolTemplateList
  implication: 11 components never call useTranslation and use hardcoded strings instead

- timestamp: 2026-03-19
  checked: ProtocolTemplateForm.tsx (Test 3 issue)
  found: Zero i18n imports. All labels hardcoded in English: "Name", "Treatment Type", "Default Session Count", "Pricing Mode", "Package Price (VND)", "Session Price (VND)", "Min Interval (days)", "Max Interval (days)", "Cancellation Deduction %", "Description", "Default Parameters", "Energy (J/cm2)", "Pulse Count", "Spot Size", "Treatment Zones", "Add Zone", "Add Step", "Add Product", "Cancel", "Create"/"Update", dialog titles, select options ("Per Session"/"Per Package", "Lid Care"), toast messages, validation messages
  implication: Entire component needs i18n wiring

- timestamp: 2026-03-19
  checked: TreatmentPackageDetail.tsx (Test 6 issue)
  found: Zero i18n imports. Hardcoded English: "Sessions Progress", "Package price:", "Per session:", "Created:", "Last session:", "Next due:", "Patient:", "Record Session", "Modify", "Pause"/"Resume", "Switch Type", "Request Cancellation", "View History", "Sessions", "No sessions recorded yet", "Treatment package not found", "Back"
  implication: Entire component needs i18n wiring

- timestamp: 2026-03-19
  checked: TreatmentSessionForm.tsx (Test 7 issue) - IplParameterFields, LlltParameterFields, LidCareParameterFields sub-components
  found: Zero i18n imports. Hardcoded English in sub-components: "Energy (J/cm2)", "Pulse Count", "Spot Size", "Treatment Zones", "Wavelength (nm)", "Power (mW)", "Duration (min)", "Treatment Area", "Procedure Steps", "Products Used", "Add". Main form sections partially in Vietnamese (hardcoded) but device param labels all English.
  implication: Sub-components within TreatmentSessionForm need i18n

- timestamp: 2026-03-19
  checked: OsdiTrendChart.tsx
  found: Zero i18n imports. Hardcoded English: "OSDI Score Trend", "No OSDI scores recorded yet", "More sessions needed for trend chart", "Session", severity labels ("Normal", "Mild", "Moderate", "Severe")
  implication: Needs i18n

- timestamp: 2026-03-19
  checked: TreatmentSessionCard.tsx
  found: Zero i18n imports. Hardcoded English: "Session #", "Energy:", "Pulses:", "Spot Size:", "Zones:", "Wavelength:", "Power:", "Duration:", "Area:", "Steps:", "Products:", "Consumables:", "Interval override:", severity labels
  implication: Needs i18n

- timestamp: 2026-03-19
  checked: VersionHistoryDialog.tsx
  found: Zero i18n imports. Hardcoded Vietnamese WITHOUT diacritics: "Lich su thay doi", "Dang tai...", "Chua co thay doi nao duoc ghi nhan", "Phien ban", "Ly do:", "Noi dung thay doi:", "Chi tiet ky thuat", "Truoc", "Sau"
  implication: Needs i18n AND diacritics fix

- timestamp: 2026-03-19
  checked: ConsumableSelector.tsx
  found: Zero i18n imports. One English label "Stock:" hardcoded
  implication: Minor - needs i18n for "Stock:" label

- timestamp: 2026-03-19
  checked: SessionOsdiCapture.tsx
  found: Zero i18n imports. Mixed hardcoded: "OSDI Score (0-100)" in English, severity guide in English, some Vietnamese hardcoded
  implication: Needs i18n

## Resolution

root_cause: |
  The treatment module has comprehensive VI translation files (both EN and VI treatment.json are complete with identical key structures), but the vast majority of components (11 out of 14) do NOT use the i18n system at all. They have hardcoded strings - some in English, some in Vietnamese, and some (VersionHistoryDialog) in Vietnamese without proper diacritics. Only 3 components (CancellationApprovalQueue, CancellationRequestDialog, ProtocolTemplateList) properly use `useTranslation("treatment")` with `t()` calls.

  This is a classic "i18n infrastructure exists but wasn't wired up" pattern. The translation keys were created but the components were built with inline strings instead of referencing those keys.

fix:
verification:
files_changed: []

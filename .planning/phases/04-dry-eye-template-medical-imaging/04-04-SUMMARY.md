---
phase: 04-dry-eye-template-medical-imaging
plan: 04
subsystem: ui
tags: [dry-eye, osdi, recharts, qrcode-react, react-hook-form, auto-save, severity-badge, trend-chart, comparison, clinical]

# Dependency graph
requires:
  - phase: 04-dry-eye-template-medical-imaging
    plan: 02
    provides: Dry eye & OSDI backend handlers (UpdateDryEye, GetOsdiHistory, GetDryEyeComparison, GenerateOsdiLink)
  - phase: 04-dry-eye-template-medical-imaging
    plan: 03
    provides: Medical image backend handlers and clinical API endpoints
provides:
  - DryEyeForm with OD/OS grid mirroring RefractionForm pattern (auto-save on blur 500ms)
  - OsdiSection with color-coded severity badge and QR code generation via qrcode.react
  - OsdiQuestionnaire with 12 bilingual questions, live score preview, radio options
  - DryEyeSection wrapper integrating form + OSDI into VisitDetailPage
  - OsdiTrendChart with recharts LineChart and 4 severity background bands
  - DryEyeComparisonPanel with dual visit selectors and delta indicators
  - PatientDryEyeTab composing trend chart + comparison in patient profile
  - All dry eye and OSDI i18n keys in English and Vietnamese with proper diacritics
affects: [04-06, 04-07]

# Tech tracking
tech-stack:
  added: [recharts, qrcode.react]
  patterns:
    - "DryEyeForm mirrors RefractionForm: OD/OS grid, debounced auto-save on blur, read-only when signed"
    - "SEVERITY_CONFIG pattern: 4-tier color-coded badge (green/yellow/orange/red) for OSDI classification"
    - "OsdiQuestionnaire: inline collapsible section with bilingual questions and live score calculation"
    - "Delta indicators: green arrow up for improvement, red arrow down for deterioration with per-metric direction"
    - "ReferenceArea severity bands: recharts background bands for visual severity context in trend chart"

key-files:
  created:
    - frontend/src/features/clinical/components/DryEyeForm.tsx
    - frontend/src/features/clinical/components/DryEyeSection.tsx
    - frontend/src/features/clinical/components/OsdiSection.tsx
    - frontend/src/features/clinical/components/OsdiQuestionnaire.tsx
    - frontend/src/features/patient/components/OsdiTrendChart.tsx
    - frontend/src/features/patient/components/DryEyeComparisonPanel.tsx
    - frontend/src/features/patient/components/PatientDryEyeTab.tsx
    - frontend/src/shared/components/RadioGroup.tsx
    - frontend/src/shared/components/ui/radio-group.tsx
  modified:
    - frontend/src/features/clinical/api/clinical-api.ts
    - frontend/src/features/clinical/components/VisitDetailPage.tsx
    - frontend/src/features/patient/components/PatientProfilePage.tsx
    - frontend/public/locales/en/clinical.json
    - frontend/public/locales/vi/clinical.json
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/VisitDetailDto.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetVisitById.cs

key-decisions:
  - "DryEyeAssessmentDto added to VisitDetailDto for frontend data access (backend deviation)"
  - "RadioGroup shadcn component added with wrapper for OSDI questionnaire radio options"
  - "OSDI history reused for comparison visit selectors instead of separate visits endpoint"
  - "Inline collapsible section for OSDI questionnaire (not modal) following VisitSection pattern"

patterns-established:
  - "Severity badge: SEVERITY_CONFIG with CSS class mapping for 4-tier color-coded classification"
  - "Delta indicator: per-metric improvement direction (higherIsBetter flag) for clinical metric comparison"
  - "Trend chart: recharts ReferenceArea bands for background severity visualization"

requirements-completed: [DRY-01, DRY-02, DRY-03, DRY-04]

# Metrics
duration: 13min
completed: 2026-03-05
---

# Phase 04 Plan 04: Dry Eye Frontend UI Summary

**DryEyeForm with OD/OS auto-save grid, OsdiSection with severity badge and QR code, 12-question bilingual OSDI questionnaire, recharts trend chart with severity bands, and cross-visit comparison panel with delta indicators in patient profile Dry Eye tab**

## Performance

- **Duration:** 13 min
- **Started:** 2026-03-05T06:45:46Z
- **Completed:** 2026-03-05T06:58:50Z
- **Tasks:** 2
- **Files modified:** 16

## Accomplishments
- Created DryEyeForm mirroring RefractionForm pattern exactly (OD/OS grid-cols-[80px_1fr_1fr], debounced auto-save on blur 500ms, read-only when visit is signed)
- Built OsdiSection with color-coded severity badge (Normal=green, Mild=yellow, Moderate=orange, Severe=red), QR code generation via qrcode.react, and inline collapsible questionnaire
- Implemented OsdiQuestionnaire with all 12 OSDI questions in Vietnamese and English, N/A option for Q6-12, and live score + severity preview
- Created OsdiTrendChart using recharts with ResponsiveContainer, 4 ReferenceArea severity bands, and custom tooltip showing date/score/severity
- Built DryEyeComparisonPanel with dual visit selectors, side-by-side OD/OS metric table, and delta indicators with correct improvement directions
- Added "Dry Eye" tab to PatientProfilePage composing trend chart and comparison panel
- Extended VisitDetailDto with DryEyeAssessmentDto on both backend and frontend for data access
- Added RadioGroup shadcn component with wrapper for questionnaire radio options
- All i18n keys added in both languages with proper Vietnamese diacritics

## Task Commits

Each task was committed atomically:

1. **Task 1: DryEyeForm, OsdiSection, OsdiQuestionnaire + VisitDetailPage integration** - `a6e9091` (feat, captured by parallel plan 04-05)
2. **Task 2: OSDI trend chart + Dry Eye comparison in patient profile** - `277db98` (feat)

**Plan metadata:** (pending)

## Files Created/Modified
- `frontend/src/features/clinical/components/DryEyeForm.tsx` - OD/OS grid for dry eye metrics with auto-save on blur
- `frontend/src/features/clinical/components/DryEyeSection.tsx` - VisitSection wrapper with DryEyeForm + OsdiSection
- `frontend/src/features/clinical/components/OsdiSection.tsx` - OSDI score display, severity badge, QR code, questionnaire toggle
- `frontend/src/features/clinical/components/OsdiQuestionnaire.tsx` - 12 bilingual questions with live score preview
- `frontend/src/features/patient/components/OsdiTrendChart.tsx` - recharts LineChart with severity background bands
- `frontend/src/features/patient/components/DryEyeComparisonPanel.tsx` - Side-by-side visit comparison with delta indicators
- `frontend/src/features/patient/components/PatientDryEyeTab.tsx` - Tab container composing trend chart + comparison
- `frontend/src/shared/components/RadioGroup.tsx` - Re-export wrapper for shadcn RadioGroup
- `frontend/src/shared/components/ui/radio-group.tsx` - shadcn RadioGroup primitive
- `frontend/src/features/clinical/api/clinical-api.ts` - Added DryEyeAssessmentDto, hooks (useUpdateDryEye, useOsdiHistory, useDryEyeComparison, useGenerateOsdiLink)
- `frontend/src/features/clinical/components/VisitDetailPage.tsx` - Added DryEyeSection between Refraction and Notes
- `frontend/src/features/patient/components/PatientProfilePage.tsx` - Added "Dry Eye" tab with PatientDryEyeTab
- `frontend/public/locales/en/clinical.json` - All dry eye and OSDI labels in English
- `frontend/public/locales/vi/clinical.json` - All dry eye and OSDI labels in Vietnamese with diacritics
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/VisitDetailDto.cs` - Added DryEyeAssessments to VisitDetailDto
- `backend/src/Modules/Clinical/Clinical.Application/Features/GetVisitById.cs` - Map DryEyeAssessments to DTO

## Decisions Made
- Added DryEyeAssessmentDto to VisitDetailDto so the frontend can read dry eye data from the visit detail query (backend had no separate GET endpoint for dry eye data)
- Used RadioGroup from shadcn/ui for OSDI questionnaire radio options (component did not exist, added following wrapper pattern)
- Reused OSDI history data for comparison panel visit selectors (avoids separate visits-with-dry-eye endpoint)
- Chose inline collapsible section for OSDI questionnaire over modal/dialog to match existing VisitSection pattern and avoid modal fatigue
- Delta indicator directions: TBUT/Schirmer/TearMeniscus higher=better (green up), Meibomian/Staining lower=better (green down)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added DryEyeAssessmentDto to VisitDetailDto**
- **Found during:** Task 1 (DryEyeForm integration)
- **Issue:** VisitDetailDto did not include dry eye assessments, making it impossible for the frontend to display dry eye data from the visit detail query
- **Fix:** Added List<DryEyeAssessmentDto> DryEyeAssessments to VisitDetailDto and mapped it in GetVisitByIdHandler
- **Files modified:** VisitDetailDto.cs, GetVisitById.cs
- **Verification:** Backend builds with 0 errors
- **Committed in:** a6e9091 (Task 1 commit)

**2. [Rule 3 - Blocking] Added RadioGroup shadcn component**
- **Found during:** Task 1 (OsdiQuestionnaire implementation)
- **Issue:** RadioGroup component did not exist in shared components, needed for OSDI radio options
- **Fix:** Installed shadcn radio-group and created RadioGroup.tsx wrapper
- **Files modified:** RadioGroup.tsx, ui/radio-group.tsx
- **Verification:** Frontend builds with 0 errors
- **Committed in:** a6e9091 (Task 1 commit)

**3. [Rule 1 - Bug] Task 1 commit captured by parallel plan 04-05**
- **Found during:** Task 1 commit
- **Issue:** Plan 04-05 ran in parallel and committed all modified files including Task 1 files
- **Fix:** Task 1 work is captured in commit a6e9091 from the parallel execution
- **Impact:** No data loss, all files correctly committed

---

**Total deviations:** 3 auto-fixed (2 blocking, 1 parallel execution overlap)
**Impact on plan:** Blocking issues were essential for frontend data access and component availability. No scope creep.

## Issues Encountered
- Parallel plan execution (04-05) committed Task 1 files along with its own medical image files in commit a6e9091. This is because both plans modified shared files (clinical-api.ts, VisitDetailPage.tsx, i18n JSON files). Task 1 work is verified correct in the committed state.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All dry eye frontend components ready for visual verification
- DryEyeForm auto-save connects to PUT /api/clinical/{visitId}/dry-eye endpoint
- OsdiTrendChart connects to GET /api/clinical/osdi-history/{patientId} endpoint
- DryEyeComparisonPanel connects to GET /api/clinical/dry-eye-comparison endpoint
- QR code generation connects to POST /api/clinical/{visitId}/osdi-link endpoint
- Patient profile "Dry Eye" tab ready for longitudinal dry eye data review

## Self-Check: PASSED

- All 9 created files verified on disk
- Task commits a6e9091, 277db98 verified in git log
- Frontend builds with 0 errors
- Backend builds with 0 errors
- DryEyeForm uses grid-cols-[80px_1fr_1fr] pattern (matches RefractionForm)
- OSDI severity badge colors verified (green/yellow/orange/red)
- 12 OSDI questions have both Vietnamese and English text
- Delta indicators use correct improvement directions

---
*Phase: 04-dry-eye-template-medical-imaging*
*Completed: 2026-03-05*

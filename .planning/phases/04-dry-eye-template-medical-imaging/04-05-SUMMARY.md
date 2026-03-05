---
phase: 04-dry-eye-template-medical-imaging
plan: 05
subsystem: ui
tags: [medical-imaging, lightbox, yarl, qrcode, osdi, public-page, file-upload, react, i18n]

# Dependency graph
requires:
  - phase: 04-dry-eye-template-medical-imaging
    plan: 02
    provides: Dry eye handlers, OSDI handlers, UpdateDryEyeAssessment, GenerateOsdiLink
  - phase: 04-dry-eye-template-medical-imaging
    plan: 03
    provides: Medical image handlers (Upload, GetVisitImages, GetImageComparison, Delete), PublicOsdiEndpoints, ClinicalApiEndpoints with imaging routes
provides:
  - ImageUploader with drag-and-drop, type/eye selection, batch upload with progress indicator
  - ImageGallery with tabbed thumbnails by type, delete confirmation, lightbox integration
  - ImageLightbox wrapping yet-another-react-lightbox with Zoom, Video, Fullscreen plugins
  - MedicalImagesSection combining upload + gallery in VisitSection wrapper
  - ImageComparison full-screen dialog with two-panel side-by-side view and visit/type selectors
  - Public OSDI self-fill page at /osdi/$token with bilingual questions and live score
  - Bilingual i18n labels for imaging and OSDI UI (en + vi with proper diacritics)
affects: [04-06]

# Tech tracking
tech-stack:
  added: [yet-another-react-lightbox, qrcode.react]
  patterns:
    - "Image upload: native fetch + FormData (not openapi-fetch) matching patient photo pattern"
    - "SAS URL cache: staleTime 30min for query refresh before 1-hour SAS expiry"
    - "Public page: standalone route outside _authenticated layout, publicApi client without auth"
    - "Lightbox: YARL with Zoom (maxZoomPixelRatio 5), Video, Fullscreen plugins"
    - "Image gallery: CSS object-fit cover with fixed aspect-square thumbnails, no server-side thumbnails"

key-files:
  created:
    - frontend/src/features/clinical/components/ImageUploader.tsx
    - frontend/src/features/clinical/components/ImageGallery.tsx
    - frontend/src/features/clinical/components/ImageLightbox.tsx
    - frontend/src/features/clinical/components/MedicalImagesSection.tsx
    - frontend/src/features/clinical/components/ImageComparison.tsx
    - frontend/src/app/routes/osdi/$token.tsx
  modified:
    - frontend/src/features/clinical/api/clinical-api.ts
    - frontend/src/features/clinical/components/VisitDetailPage.tsx
    - frontend/public/locales/en/clinical.json
    - frontend/public/locales/vi/clinical.json
    - frontend/package.json

key-decisions:
  - "Image upload uses native fetch + FormData (not openapi-fetch) following established patient photo upload pattern"
  - "SAS URL query cache uses staleTime 30 minutes to refresh before 1-hour server-side SAS expiry"
  - "Image comparison uses Dialog (not Sheet) for full-screen overlay with two independent scroll panels"
  - "Public OSDI page uses separate publicApi client (no auth middleware) following booking page pattern"
  - "OSDI questions display Vietnamese primary with English secondary text"
  - "Questions 6-12 support N/A responses, questions 1-5 are mandatory"

patterns-established:
  - "Medical image upload: FormData with file + imageType + optional eyeTag form fields"
  - "Image gallery: Tabs component for type grouping + grid of thumbnails with object-fit cover"
  - "Public questionnaire page: standalone route + publicApi + error states for expired/invalid/network"

requirements-completed: [IMG-01, IMG-02, IMG-03, IMG-04]

# Metrics
duration: 12min
completed: 2026-03-05
---

# Phase 04 Plan 05: Medical Imaging UI & Public OSDI Self-Fill Page Summary

**Image upload/gallery/lightbox/comparison frontend with yet-another-react-lightbox, plus public OSDI questionnaire page with live scoring at /osdi/$token**

## Performance

- **Duration:** 12 min
- **Started:** 2026-03-05T06:45:40Z
- **Completed:** 2026-03-05T06:57:40Z
- **Tasks:** 2
- **Files modified:** 11

## Accomplishments
- Built complete medical imaging UI: drag-and-drop upload with type/eye selection, tabbed gallery with thumbnails, lightbox with zoom/video/fullscreen, and full-screen comparison overlay
- Created public OSDI self-fill page with 12 bilingual questions, live score calculation, severity color-coding, and mobile-responsive layout
- Installed yet-another-react-lightbox and qrcode.react as new frontend dependencies
- Added comprehensive bilingual i18n labels (Vietnamese with proper diacritics) for all imaging and OSDI UI elements

## Task Commits

Each task was committed atomically:

1. **Task 1: Image upload, gallery, lightbox + MedicalImagesSection** - `a6e9091` (feat)
2. **Task 2: Image comparison overlay + Public OSDI self-fill page** - `0ecc1da` (feat)

**Plan metadata:** (pending)

## Files Created/Modified
- `frontend/src/features/clinical/components/ImageUploader.tsx` - Drag-and-drop upload with image type/eye tag selectors, batch upload with progress, client-side file validation (50MB images, 200MB video)
- `frontend/src/features/clinical/components/ImageGallery.tsx` - Tabbed thumbnail gallery grouped by image type, delete with confirmation, lightbox on click
- `frontend/src/features/clinical/components/ImageLightbox.tsx` - YARL wrapper with Zoom (maxZoomPixelRatio 5), Video, Fullscreen plugins
- `frontend/src/features/clinical/components/MedicalImagesSection.tsx` - VisitSection wrapper combining ImageUploader + ImageGallery with image count badge
- `frontend/src/features/clinical/components/ImageComparison.tsx` - Full-screen Dialog with two-panel side-by-side image view, visit selectors, image type filter
- `frontend/src/app/routes/osdi/$token.tsx` - Public OSDI self-fill page with 12 bilingual questions, live score, severity badges, mobile-responsive
- `frontend/src/features/clinical/api/clinical-api.ts` - Added MedicalImageDto, ImageType/EyeTag enums, upload/gallery/comparison API functions and TanStack Query hooks
- `frontend/src/features/clinical/components/VisitDetailPage.tsx` - Integrated MedicalImagesSection after DiagnosisSection
- `frontend/public/locales/en/clinical.json` - Added images, comparison, and osdi i18n sections
- `frontend/public/locales/vi/clinical.json` - Added Vietnamese translations with proper diacritics
- `frontend/package.json` - Added yet-another-react-lightbox and qrcode.react dependencies

## Decisions Made
- Image upload uses native fetch + FormData (not openapi-fetch) following the established patient photo upload pattern from Phase 2
- SAS URL query cache uses staleTime 30 minutes to allow refresh before the 1-hour server-side SAS token expiry
- Image comparison uses shadcn Dialog (max-w-[95vw], max-h-[95vh]) as full-screen overlay with two independently scrollable panels
- Public OSDI page uses a separate publicApi client without auth middleware, following the booking page pattern
- OSDI questions display Vietnamese text as primary with English as secondary italic text
- Questions 1-5 (Ocular Symptoms) are mandatory; questions 6-12 (Vision-Related + Environmental Triggers) support N/A responses
- OSDI score calculation: (sum * 100) / (answeredCount * 4), matching the backend OsdiCalculator formula

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None - implementation followed established patterns without issues.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All medical imaging frontend UI (IMG-01 through IMG-04) is complete
- Upload, gallery, lightbox, and comparison are fully integrated into visit detail page
- Public OSDI self-fill page is accessible at /osdi/{token} without authentication
- Ready for Plan 06 (OSDI trend chart + dry eye comparison in patient profile) to consume these components

## Self-Check: PASSED

- All 6 created files verified on disk
- Task commits a6e9091, 0ecc1da verified in git log
- Frontend builds with 0 errors
- Image upload uses native fetch + FormData (verified in clinical-api.ts)
- Lightbox uses yet-another-react-lightbox with Zoom, Video, Fullscreen plugins (verified in ImageLightbox.tsx)
- Image comparison is a full-screen overlay Dialog (verified in ImageComparison.tsx)
- Public OSDI page is outside _authenticated layout at /osdi/$token (verified in route tree)
- SAS URL cache uses staleTime 30 minutes (verified in useVisitImages hook)

---
*Phase: 04-dry-eye-template-medical-imaging*
*Completed: 2026-03-05*

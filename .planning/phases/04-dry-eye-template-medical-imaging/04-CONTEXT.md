# Phase 4: Dry Eye Template & Medical Imaging - Context

**Gathered:** 2026-03-05
**Status:** Ready for planning

<domain>
## Phase Boundary

Doctors can perform structured Dry Eye assessments with OSDI scoring and compare clinical data and images across visits. This includes: OSDI questionnaire (12-question, per-visit), per-eye dry eye metrics (TBUT, Schirmer, Meibomian gland grading, Tear meniscus, Staining score), OSDI severity classification with color-coding, OSDI trend chart across visits, cross-visit metrics comparison, medical image/video upload (Fluorescein, Meibography, OCT, Specular microscopy, Topography, lacrimal duct video), image lightbox with zoom, and side-by-side image comparison across visits.

This phase does NOT include: prescriptions (Phase 5), treatment protocols (Phase 9), billing (Phase 7), pharmacy (Phase 6), or Myopia Control template (post-launch).

</domain>

<decisions>
## Implementation Decisions

### OSDI Questionnaire Flow
- Both modes available: doctor records answers during exam OR patient self-fills
- Patient self-fill via QR code / unique link — public page, no login required, score syncs back to visit
- All 12 OSDI questions displayed with full Vietnamese text (not abbreviated score-only inputs)
- System auto-calculates OSDI score and color-codes severity: Normal 0-12 (green), Mild 13-22 (yellow), Moderate 23-32 (orange), Severe 33-100 (red)

### Dry Eye Metrics Layout
- Single "Dry Eye Assessment" VisitSection with all metrics in one OD/OS side-by-side grid (mirrors Refraction layout)
- Fields per eye: TBUT (seconds), Schirmer (mm), Meibomian gland grading, Tear meniscus (mm), Staining score
- Meibomian gland grading: 0-3 scale (standard Arita grading — 0=no loss, 1=<33%, 2=33-66%, 3=>66%)
- Auto-save on blur (debounced 500ms) — consistent with Refraction pattern
- Read-only when visit is signed off

### Image Upload & Gallery
- Images categorized by type on upload: Fluorescein, Meibography, OCT, Specular microscopy, Topography, Video (lacrimal duct, etc.)
- Staff must select image type when uploading
- Images can be uploaded anytime — even after visit sign-off (append-only, not editing medical record)
- Reasonable file limits (Claude decides specific values based on typical clinical imaging sizes)
- Optional OD/OS/OU eye tag per image (not mandatory — some images like face photos don't apply to a specific eye)

### Claude's Discretion
- OSDI questionnaire UI presentation (inline section vs popup dialog vs step-by-step wizard)
- Staining score grading system (Oxford 0-5 vs NEI per-zone — pick what's most practical)
- Cross-visit comparison location (patient profile tab vs dedicated page vs visit detail overlay)
- Image comparison UX (full-screen overlay vs inline split view)
- OSDI trend chart default time range and line configuration (overall OSDI + optional per-eye metric overlays)
- Lightbox component choice (yet-another-react-lightbox or similar free/MIT library)
- Chart library: recharts (free, MIT) for OSDI trend chart
- File size/count limits for image uploads
- Image thumbnail generation strategy
- Loading states and error handling

</decisions>

<specifics>
## Specific Ideas

- OSDI is a patient-reported symptom score (not per-eye) — the 12 questions are about the patient's overall eye discomfort. Per-eye metrics (TBUT, Schirmer, etc.) are separate objective measurements by the doctor
- QR code / unique link for patient self-fill follows the same public-page pattern as self-booking (Phase 2) — no authentication required
- Image types enable same-type comparison across visits (IMG-04) — e.g., compare Meibography images from visit A vs visit B
- Meibomian gland grading 0-3 (Arita) is the standard used in Vietnamese ophthalmology clinics
- Images are additive/append-only even after sign-off — this is standard clinical practice since diagnostic results (OCT, Meibography) often arrive after the doctor signs the visit record

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- **VisitSection**: Collapsible card wrapper with `headerExtra` slot — use for Dry Eye Assessment section and Medical Images section
- **RefractionForm**: OD/OS side-by-side grid (`grid-cols-[80px_1fr_1fr]`) with debounced auto-save — mirror for Dry Eye metrics form
- **RefractionSection**: Tabbed multi-type data pattern (Manifest/Autorefraction/Cycloplegic) — could adapt for image type tabs
- **IAzureBlobService**: Full blob storage interface with Upload/Download/SAS URL/List — ready for medical image storage
- **Patient photo upload**: `FormData` + raw `fetch` pattern (not openapi-fetch) for multipart file uploads with `DisableAntiforgery()`
- **ITemplateDefinition**: Stub in Shared.Domain explicitly designed for Dry Eye template as first concrete implementation
- **Public self-booking page**: Pattern for unauthenticated public pages — reuse for OSDI patient self-fill page
- **AllergyAlert**: Banner component pattern — adapt for OSDI severity badge display

### Established Patterns
- **Visit aggregate**: Child entity pattern (Refraction, Diagnosis) with backing fields, `EnsureEditable()` guard, `Add___()` methods
- **Per-eye flat columns**: `OdTbut/OsTbut` naming convention (same as `OdSph/OsSph` in Refraction)
- **Wolverine handlers**: Static handler classes with FluentValidation, `Result<T>` return, per-feature files
- **Minimal API endpoints**: MapGroup with RequireAuthorization, `bus.InvokeAsync` pattern
- **React Hook Form + Zod**: zodResolver with `optionalNumber` transform for nullable decimals
- **TanStack Query**: queryKey factories, mutation with cache invalidation
- **openapi-fetch**: Typed API client (use raw fetch for file uploads only)
- **Public API pattern**: `/api/public/` routes without RequireAuthorization, with RequireRateLimiting

### Integration Points
- **Visit entity**: Add `_dryEyeAssessments` backing field collection + `AddDryEyeAssessment()` domain method
- **ClinicalDbContext**: Add `DbSet<DryEyeAssessment>` + `DbSet<MedicalImage>`, create migrations
- **VisitDetailPage**: Insert `<DryEyeSection>` and `<MedicalImagesSection>` as new `<VisitSection>` blocks
- **GetByIdWithDetailsAsync**: Extend with `.Include(v => v.DryEyeAssessments).Include(v => v.MedicalImages)`
- **ClinicalApiEndpoints**: Add endpoints for dry eye CRUD, image upload/list/delete, OSDI history, comparison data
- **Patient profile page**: Potential tab for Dry Eye history / OSDI trend chart
- **i18n**: Add keys to `en/clinical.json` and `vi/clinical.json`
- **Azure Blob container**: New "clinical-images" container for medical images

</code_context>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 04-dry-eye-template-medical-imaging*
*Context gathered: 2026-03-05*

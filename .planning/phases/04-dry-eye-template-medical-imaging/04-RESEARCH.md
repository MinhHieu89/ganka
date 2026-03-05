# Phase 4: Dry Eye Template & Medical Imaging - Research

**Researched:** 2026-03-05
**Domain:** Clinical ophthalmology data collection (Dry Eye assessment), medical imaging (upload/view/compare), patient-reported outcome measures (OSDI)
**Confidence:** HIGH

## Summary

Phase 4 adds two major feature groups to the existing Clinical module: (1) structured Dry Eye assessment with OSDI scoring, trend visualization, and cross-visit comparison, and (2) medical image/video upload, lightbox viewing, and side-by-side comparison. Both feature groups build directly on established patterns from Phase 3 (Visit entity, VisitSection component, debounced auto-save forms, ClinicalApiEndpoints, IAzureBlobService).

The backend work follows the proven Visit child entity pattern (backing fields, `EnsureEditable()` guard, direct DbSet.Add for EF Core tracking). Two new entities are needed: `DryEyeAssessment` (per-visit, stores per-eye metrics + OSDI score) and `MedicalImage` (per-visit, stores blob metadata). The OSDI public self-fill feature reuses the existing public API pattern from Phase 2 self-booking (rate-limited, unauthenticated endpoints). The frontend requires three new libraries: `recharts` for the OSDI trend chart, `yet-another-react-lightbox` for image viewing with zoom, and `qrcode.react` for the QR code on the OSDI self-fill link.

**Primary recommendation:** Follow the Refraction entity/form pattern exactly for DryEyeAssessment. Use the existing IAzureBlobService for image storage. Add recharts (already free/MIT) and yet-another-react-lightbox (MIT) as the only new dependencies. Images should NOT use client-side thumbnails; instead, serve SAS URLs with original images and let the browser handle responsive display via CSS.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Both modes available for OSDI: doctor records answers during exam OR patient self-fills
- Patient self-fill via QR code / unique link -- public page, no login required, score syncs back to visit
- All 12 OSDI questions displayed with full Vietnamese text (not abbreviated score-only inputs)
- System auto-calculates OSDI score and color-codes severity: Normal 0-12 (green), Mild 13-22 (yellow), Moderate 23-32 (orange), Severe 33-100 (red)
- Single "Dry Eye Assessment" VisitSection with all metrics in one OD/OS side-by-side grid (mirrors Refraction layout)
- Fields per eye: TBUT (seconds), Schirmer (mm), Meibomian gland grading, Tear meniscus (mm), Staining score
- Meibomian gland grading: 0-3 scale (standard Arita grading -- 0=no loss, 1=<33%, 2=33-66%, 3=>66%)
- Auto-save on blur (debounced 500ms) -- consistent with Refraction pattern
- Read-only when visit is signed off
- Images categorized by type on upload: Fluorescein, Meibography, OCT, Specular microscopy, Topography, Video
- Staff must select image type when uploading
- Images can be uploaded anytime -- even after visit sign-off (append-only, not editing medical record)
- Optional OD/OS/OU eye tag per image (not mandatory)
- OSDI is a patient-reported symptom score (not per-eye) -- 12 questions about overall eye discomfort; per-eye metrics are separate
- QR code / unique link follows public-page pattern from self-booking (no authentication)
- Image types enable same-type comparison across visits (IMG-04)
- Meibomian gland grading 0-3 (Arita) is standard in Vietnamese ophthalmology clinics
- Images are additive/append-only even after sign-off -- standard clinical practice

### Claude's Discretion
- OSDI questionnaire UI presentation (inline section vs popup dialog vs step-by-step wizard)
- Staining score grading system (Oxford 0-5 vs NEI per-zone -- pick what's most practical)
- Cross-visit comparison location (patient profile tab vs dedicated page vs visit detail overlay)
- Image comparison UX (full-screen overlay vs inline split view)
- OSDI trend chart default time range and line configuration
- Lightbox component choice (yet-another-react-lightbox or similar free/MIT library)
- Chart library: recharts (free, MIT) for OSDI trend chart
- File size/count limits for image uploads
- Image thumbnail generation strategy
- Loading states and error handling

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| DRY-01 | Doctor can record Dry Eye exam with structured fields: OSDI score, TBUT, Schirmer, Meibomian gland grading, Tear meniscus, Staining score -- all per eye | DryEyeAssessment entity following Refraction pattern; DryEyeForm mirroring RefractionForm with OD/OS grid; debounced auto-save on blur |
| DRY-02 | System calculates and displays OSDI severity classification with color coding | OSDI formula: (sum x 100) / (answered x 4); severity badge component with 4-tier color mapping; calculated server-side and client-side |
| DRY-03 | Doctor can view OSDI trend chart across visits for a patient | recharts LineChart with ResponsiveContainer; new API endpoint returning historical OSDI data; patient profile "Dry Eye" tab |
| DRY-04 | Doctor can compare TBUT, Schirmer, and other metrics between visits side-by-side | Comparison API endpoint returning two visits' dry eye data; side-by-side card layout with delta indicators |
| IMG-01 | Staff can upload medical images associated with visits | MedicalImage entity; IAzureBlobService upload to "clinical-images" container; IFormFile endpoint with DisableAntiforgery; FormData + native fetch on frontend |
| IMG-02 | Staff can upload video files associated with visits | Same upload flow as IMG-01 with extended MIME type support and 200MB limit for video |
| IMG-03 | Doctor can view images in a lightbox with zoom | yet-another-react-lightbox with Zoom plugin; SAS URL generation for secure image access |
| IMG-04 | Doctor can compare images side-by-side across two visits | Side-by-side full-screen overlay with two image slots; visit selector + image type filter |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| recharts | ^3.7.0 | OSDI trend line chart | Most popular React chart library (3.6M weekly downloads), MIT license, declarative API, built-in ResponsiveContainer |
| yet-another-react-lightbox | ^3.29.1 | Image lightbox with zoom | MIT license, React 19 compatible, built-in Zoom/Fullscreen/Video plugins, TypeScript types included |
| qrcode.react | ^4.1.0 | QR code for OSDI self-fill link | Most popular React QR code library, SVG rendering, lightweight |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| SixLabors.ImageSharp | ^3.x | Server-side thumbnail generation (optional) | Only if thumbnail generation is needed; defer to Phase 4.1 if not blocking |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| recharts | nivo / visx | recharts is simpler declarative API; nivo heavier; visx lower-level. User pre-approved recharts |
| yet-another-react-lightbox | react-medium-image-zoom | YARL has full gallery navigation, video support, and zoom built-in as plugins; react-medium-image-zoom is single-image only |
| qrcode.react | react-qr-code | Both MIT. qrcode.react is more mature with more weekly downloads |
| Side-by-side comparison | react-compare-slider | react-compare-slider is a slider overlay; medical image comparison is better as two images side-by-side (not overlaid) since image types differ per visit |

### Not Needed
| Problem | Why Not a Library |
|---------|-------------------|
| Image thumbnail CSS | Use CSS `object-fit: cover` with fixed dimensions on `<img>` tags; no JS thumbnail generation needed for display |
| OSDI calculation | Simple formula `(sum * 100) / (answered * 4)`; pure function, no library |

**Installation:**
```bash
# Frontend
cd frontend && npm install recharts yet-another-react-lightbox qrcode.react

# Backend -- no new NuGet packages needed
# IAzureBlobService already exists in Shared.Infrastructure
# IFormFile is built into ASP.NET Core
```

## Architecture Patterns

### Recommended Project Structure

**Backend additions:**
```
backend/src/Modules/Clinical/
├── Clinical.Domain/
│   ├── Entities/
│   │   ├── DryEyeAssessment.cs        # New entity (per-visit, per-eye metrics)
│   │   ├── MedicalImage.cs             # New entity (per-visit image metadata)
│   │   └── OsdiSubmission.cs           # New entity (OSDI questionnaire answers)
│   └── Enums/
│       ├── ImageType.cs                # Fluorescein, Meibography, OCT, etc.
│       ├── EyeTag.cs                   # OD, OS, OU (optional on images)
│       └── OsdiSeverity.cs             # Normal, Mild, Moderate, Severe
├── Clinical.Application/
│   ├── Features/
│   │   ├── UpdateDryEyeAssessment.cs   # Command + Handler (mirrors UpdateRefraction)
│   │   ├── SubmitOsdiQuestionnaire.cs  # Command + Handler (public endpoint)
│   │   ├── GetOsdiHistory.cs           # Query (trend chart data)
│   │   ├── GetDryEyeComparison.cs      # Query (two visits side-by-side)
│   │   ├── UploadMedicalImage.cs       # Command + Handler (blob upload)
│   │   ├── GetVisitImages.cs           # Query (list images for a visit)
│   │   ├── GetImageComparisonData.cs   # Query (images for comparison)
│   │   └── DeleteMedicalImage.cs       # Command + Handler
│   └── Interfaces/
│       └── IVisitRepository.cs         # Extended with new methods
├── Clinical.Contracts/
│   └── Dtos/
│       ├── DryEyeAssessmentDto.cs      # DTO for dry eye data
│       ├── OsdiHistoryDto.cs           # DTO for trend chart data
│       ├── MedicalImageDto.cs          # DTO for image metadata
│       └── DryEyeComparisonDto.cs      # DTO for comparison view
├── Clinical.Infrastructure/
│   ├── Configurations/
│   │   ├── DryEyeAssessmentConfiguration.cs
│   │   ├── MedicalImageConfiguration.cs
│   │   └── OsdiSubmissionConfiguration.cs
│   └── ClinicalDbContext.cs            # Add new DbSets
└── Clinical.Presentation/
    ├── ClinicalApiEndpoints.cs         # Extended with new endpoint groups
    └── PublicOsdiEndpoints.cs          # Public OSDI self-fill endpoints
```

**Frontend additions:**
```
frontend/src/
├── features/clinical/
│   ├── components/
│   │   ├── DryEyeSection.tsx           # VisitSection wrapper with DryEyeForm
│   │   ├── DryEyeForm.tsx              # OD/OS grid (mirrors RefractionForm)
│   │   ├── OsdiSection.tsx             # OSDI score display + severity badge
│   │   ├── OsdiQuestionnaire.tsx       # 12-question form (inline in visit)
│   │   ├── MedicalImagesSection.tsx    # VisitSection wrapper for images
│   │   ├── ImageUploader.tsx           # Upload with type/eye selection
│   │   ├── ImageGallery.tsx            # Grid of thumbnails per type
│   │   ├── ImageLightbox.tsx           # YARL lightbox wrapper
│   │   └── ImageComparison.tsx         # Side-by-side overlay
│   └── api/
│       └── clinical-api.ts             # Extended with new endpoints
├── features/patient/
│   └── components/
│       ├── PatientDryEyeTab.tsx        # OSDI trend chart + comparison
│       └── OsdiTrendChart.tsx          # recharts LineChart component
├── app/routes/
│   ├── _authenticated/
│   │   └── patients/
│   │       └── $patientId.tsx          # Add "Dry Eye" tab
│   └── osdi/
│       └── $token.tsx                  # Public OSDI self-fill page
└── public/locales/
    ├── en/clinical.json                # Extended with dry eye + image keys
    └── vi/clinical.json                # Vietnamese translations
```

### Pattern 1: DryEyeAssessment Entity (mirrors Refraction)
**What:** Child entity of Visit with per-eye flat columns
**When to use:** All per-visit dry eye metric storage
**Example:**
```csharp
// Source: follows Refraction.cs pattern exactly
public class DryEyeAssessment : Entity
{
    public Guid VisitId { get; private set; }

    // Per-eye dry eye metrics (OD/OS flat columns)
    public decimal? OdTbut { get; private set; }      // seconds
    public decimal? OsTbut { get; private set; }
    public decimal? OdSchirmer { get; private set; }   // mm
    public decimal? OsSchirmer { get; private set; }
    public int? OdMeibomianGrading { get; private set; }  // 0-3 Arita
    public int? OsMeibomianGrading { get; private set; }
    public decimal? OdTearMeniscus { get; private set; }   // mm
    public decimal? OsTearMeniscus { get; private set; }
    public int? OdStaining { get; private set; }        // Oxford 0-5
    public int? OsStaining { get; private set; }

    // OSDI score (patient-level, NOT per-eye)
    public decimal? OsdiScore { get; private set; }     // 0-100 calculated

    private DryEyeAssessment() { }

    public static DryEyeAssessment Create(Guid visitId) =>
        new() { VisitId = visitId };

    public void Update(/* all fields */) { /* set all + SetUpdatedAt() */ }
    public void SetOsdiScore(decimal score) { OsdiScore = score; SetUpdatedAt(); }
}
```

### Pattern 2: MedicalImage Entity (new pattern, not a Visit child via domain)
**What:** Image metadata entity linked to Visit but NOT subject to EnsureEditable (append-only even after sign-off)
**When to use:** All medical image/video storage
**Example:**
```csharp
// Source: new entity, but follows Entity base class
public class MedicalImage : Entity
{
    public Guid VisitId { get; private set; }
    public Guid UploadedById { get; private set; }
    public ImageType Type { get; private set; }         // Fluorescein, Meibography, etc.
    public EyeTag? EyeTag { get; private set; }         // OD, OS, OU (optional)
    public string OriginalFileName { get; private set; } = string.Empty;
    public string BlobName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string? Description { get; private set; }

    // NOTE: MedicalImage is NOT added through Visit aggregate.
    // It bypasses EnsureEditable because images are append-only even after sign-off.
    // This is intentional -- clinical images often arrive after visit is signed.
}
```

### Pattern 3: OsdiSubmission Entity (public self-fill)
**What:** Stores the full 12-question OSDI responses for a visit, submitted by patient or doctor
**When to use:** OSDI questionnaire answers and calculated score
**Example:**
```csharp
public class OsdiSubmission : Entity
{
    public Guid VisitId { get; private set; }
    public string SubmittedBy { get; private set; } = string.Empty;  // "patient" or userId
    public string AnswersJson { get; private set; } = "[]";  // JSON array of 12 answers (0-4 each)
    public int QuestionsAnswered { get; private set; }
    public decimal Score { get; private set; }  // Calculated: (sum * 100) / (answered * 4)

    // Token for public access (patient self-fill)
    public string? PublicToken { get; private set; }
    public DateTime? TokenExpiresAt { get; private set; }
}
```

### Pattern 4: Image Upload Endpoint (mirrors patient photo pattern)
**What:** IFormFile endpoint with DisableAntiforgery for multipart upload
**When to use:** All file upload endpoints
**Example:**
```csharp
// Source: PatientApiEndpoints.cs photo upload pattern
group.MapPost("/{visitId:guid}/images", async (
    Guid visitId,
    IFormFile file,
    [FromForm] int imageType,
    [FromForm] int? eyeTag,
    IMessageBus bus,
    CancellationToken ct) =>
{
    using var stream = file.OpenReadStream();
    var command = new UploadMedicalImageCommand(
        visitId, stream, file.FileName, file.ContentType, file.Length,
        (ImageType)imageType, eyeTag.HasValue ? (EyeTag)eyeTag.Value : null);
    var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
    return result.ToCreatedHttpResult($"/api/clinical/{visitId}/images");
}).DisableAntiforgery();
```

### Pattern 5: Public OSDI Endpoint (mirrors PublicBookingEndpoints)
**What:** Rate-limited, unauthenticated endpoint for patient OSDI self-fill
**When to use:** Patient-facing OSDI questionnaire submission
**Example:**
```csharp
// Source: PublicBookingEndpoints.cs pattern
public static class PublicOsdiEndpoints
{
    public static IEndpointRouteBuilder MapPublicOsdiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public/osdi")
            .RequireRateLimiting("public-booking");  // Reuse existing rate limit policy

        group.MapGet("/{token}", async (string token, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<OsdiQuestionnaireDto>>(
                new GetOsdiByTokenQuery(token), ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{token}", async (string token, SubmitOsdiCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = command with { Token = token };
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        return app;
    }
}
```

### Pattern 6: Frontend Image Upload (mirrors patient photo upload)
**What:** Native fetch with FormData for multipart file uploads
**When to use:** All file uploads (cannot use openapi-fetch for multipart)
**Example:**
```typescript
// Source: patient photo upload pattern from Phase 02
async function uploadMedicalImage(
  visitId: string,
  file: File,
  imageType: number,
  eyeTag?: number,
): Promise<{ id: string }> {
  const formData = new FormData()
  formData.append("file", file)
  formData.append("imageType", String(imageType))
  if (eyeTag !== undefined) formData.append("eyeTag", String(eyeTag))

  const token = useAuthStore.getState().accessToken
  const response = await fetch(
    `${API_URL}/api/clinical/${visitId}/images`,
    {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: formData,
      credentials: "include",
    },
  )
  if (!response.ok) throw new Error("Failed to upload image")
  return response.json()
}
```

### Anti-Patterns to Avoid
- **Adding images through Visit aggregate:** MedicalImage must NOT go through Visit.AddImage() because images are append-only even after sign-off. Add directly via repository, NOT through domain aggregate.
- **Storing image blobs in database:** Always use Azure Blob Storage via IAzureBlobService. Store only metadata (blob name, content type, size) in SQL.
- **Generating thumbnails on upload:** Defer server-side thumbnail generation. Use CSS `object-fit: cover` with fixed dimensions for gallery thumbnails. Add SixLabors.ImageSharp only if performance becomes an issue.
- **Making OSDI per-eye:** OSDI is a patient-reported outcome measure -- it is NOT per-eye. The 12 questions assess overall ocular surface disease. Per-eye metrics (TBUT, Schirmer, etc.) are separate objective measurements.
- **Using openapi-fetch for file uploads:** Must use native `fetch` with `FormData` for multipart uploads (established pattern from patient photo upload).

## Discretion Recommendations

### OSDI Questionnaire UI: Inline Collapsible Section
**Recommendation:** Use an inline collapsible section within the visit detail page, displaying all 12 questions in a card grid. This is consistent with the existing VisitSection pattern and avoids modal fatigue.
**Rationale:** Doctors already work within the visit detail page scrolling through sections. A popup/dialog would break the flow. A step-by-step wizard adds unnecessary clicks for 12 simple questions.

### Staining Score: Oxford 0-5
**Recommendation:** Use Oxford grading scale 0-5.
**Rationale:** The Oxford scheme is the most widely used grading system globally, simple integer scale (0=absent, 1=minimal, 2=mild, 3=moderate, 4=severe, 5=very severe), and aligns with the integer-based Meibomian gland grading (0-3). NEI per-zone scoring requires 5 separate zone scores per eye -- significantly more complex for minimal clinical benefit in a private clinic setting.

### Cross-Visit Comparison Location: Patient Profile Tab
**Recommendation:** Add a "Dry Eye" tab to the existing PatientProfilePage (alongside Overview, Allergies, Appointments). This tab contains the OSDI trend chart and a visit comparison panel.
**Rationale:** The patient profile already has a tabbed layout. Dry Eye history is patient-level context, not visit-level. This keeps the visit detail page focused on current-visit data entry while the patient profile provides longitudinal views.

### Image Comparison UX: Full-Screen Overlay
**Recommendation:** Use a full-screen overlay with two image panels side-by-side, each with independent zoom. A visit selector dropdown at the top of each panel lets the doctor pick which visit's image to show.
**Rationale:** Medical image comparison requires maximum screen real estate. An inline split view within the visit page would be too small for meaningful comparison. The full-screen overlay provides the space needed for clinical decision-making.

### OSDI Trend Chart Config
**Recommendation:** Default to showing all visits (no time range limit). Single line for OSDI score. Color-coded severity bands as background regions (green/yellow/orange/red). Tooltip showing date, score, and severity label.
**Rationale:** Dry eye patients typically have 4-12 visits per year. The entire history is manageable in a single chart. Severity bands provide instant visual context.

### Lightbox: yet-another-react-lightbox
**Recommendation:** Use yet-another-react-lightbox v3.29.1.
**Rationale:** MIT license, React 19 compatible, built-in Zoom plugin (required for IMG-03), Video plugin (required for IMG-02 viewing), Fullscreen plugin, TypeScript types included. Most actively maintained React lightbox library.

### File Size/Count Limits
**Recommendation:**
- Images: max 50 MB per file (OCT TIFF files can be 20-40 MB)
- Videos: max 200 MB per file (lacrimal duct procedure videos)
- Max 20 files per upload batch
- Max 100 images per visit (soft limit, warning at 50)
- Accepted image types: JPEG, PNG, TIFF, BMP, WebP
- Accepted video types: MP4, MOV, AVI

**Rationale:** Clinical imaging devices export large files. OCT exports especially can be 20-40 MB as TIFF. Video recordings of lacrimal duct procedures are typically 1-3 minutes, 50-150 MB. The 200 MB video limit accommodates most clinical recordings while preventing storage abuse.

### Thumbnail Strategy: CSS-Only (No Server-Side Generation)
**Recommendation:** Use SAS URLs pointing to original images. Display thumbnails using `<img>` with CSS `object-fit: cover` and fixed dimensions (e.g., 120x120 for gallery grid). The browser handles downscaling.
**Rationale:** Server-side thumbnail generation adds complexity (Azure Functions or background jobs) for minimal benefit in a small clinic system with ~8 staff. SAS URLs with 1-hour expiry provide secure access. If performance becomes an issue with many large images, add server-side thumbnails as a follow-up optimization.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Line chart for OSDI trends | Custom SVG/Canvas chart | recharts LineChart + ResponsiveContainer | Axis labels, tooltips, responsive sizing, date formatting are deceptively complex |
| Image lightbox with zoom | Custom modal with pinch-to-zoom | yet-another-react-lightbox + Zoom plugin | Touch gesture handling, keyboard navigation, preloading are hard to get right |
| QR code generation | Custom QR encoding | qrcode.react | QR encoding algorithm is non-trivial; error correction levels matter |
| OSDI score calculation | Manual percentage math | Pure utility function with null-handling | Simple formula but needs proper handling of skipped questions |
| Blob storage upload | Custom HTTP file handling | IAzureBlobService (existing) | Already handles container creation, content types, SAS generation |
| Image MIME type detection | Extension-only check | Extend existing GetContentType pattern | Must validate actual content type for security, not just extension |

**Key insight:** The most dangerous hand-roll in this phase would be building a custom lightbox or chart. Both have edge cases (touch gestures, zoom boundaries, axis tick calculation, responsive resize) that take weeks to polish. Use the libraries.

## Common Pitfalls

### Pitfall 1: MedicalImage Through Visit Aggregate
**What goes wrong:** Adding MedicalImage as a Visit child entity with EnsureEditable guard means images cannot be added after sign-off.
**Why it happens:** Following the Refraction pattern blindly. Refraction IS subject to sign-off immutability. Images are NOT.
**How to avoid:** MedicalImage has a VisitId foreign key but is NOT in the Visit._medicalImages backing field. It is added directly via repository, bypassing the aggregate. The Visit entity does not have an `AddImage()` method.
**Warning signs:** If you see `Visit.AddMedicalImage()` or `_medicalImages` backing field in the Visit entity, the pattern is wrong.

### Pitfall 2: OSDI Score as Per-Eye
**What goes wrong:** Storing OdOsdi / OsOsdi on the DryEyeAssessment. OSDI is patient-level, not per-eye.
**Why it happens:** All other metrics in the Dry Eye form are per-eye, so developers assume OSDI follows the same pattern.
**How to avoid:** OsdiScore is a single decimal on DryEyeAssessment (or a separate OsdiSubmission entity). The 12 OSDI questions ask about overall symptoms, not specific eyes.
**Warning signs:** Any OD/OS prefixed OSDI field in the entity.

### Pitfall 3: OSDI Formula Division By Zero
**What goes wrong:** `(sum * 100) / (answered * 4)` throws when no questions are answered.
**Why it happens:** Patients may skip all questions or the form is initialized empty.
**How to avoid:** Guard: `if (answered == 0) return null`. FluentValidation should require at least 1 answered question. Frontend should disable submit when 0 answered.
**Warning signs:** OSDI score of 0 when no questions answered (should be null, not 0).

### Pitfall 4: Azure Blob SAS URL Expiry in Gallery
**What goes wrong:** Gallery loads SAS URLs on page mount. After 1 hour, images fail to load without page refresh.
**Why it happens:** SAS URLs have finite expiry (typically 1 hour).
**How to avoid:** Generate SAS URLs on demand (per gallery render) with TanStack Query caching. Set staleTime to 30 minutes so URLs are refreshed before expiry. Or use a longer expiry (4 hours) with shorter cache.
**Warning signs:** Broken images appearing in the gallery after the user has been on the page for a while.

### Pitfall 5: File Upload Without Validation
**What goes wrong:** Malicious files uploaded (executables renamed to .jpg, oversized files, etc.).
**Why it happens:** Trusting client-side file type from the filename extension.
**How to avoid:** Server-side validation: check file magic bytes (not just extension), enforce size limits, validate content type. FluentValidation on the command. Frontend: `accept` attribute on input for UX, but always validate server-side.
**Warning signs:** Missing server-side content type validation.

### Pitfall 6: Cross-Visit Query N+1
**What goes wrong:** Loading OSDI history or comparison data results in N+1 queries -- one query per visit.
**Why it happens:** Fetching visits individually instead of batch query.
**How to avoid:** Single LINQ query: `dbContext.DryEyeAssessments.Where(d => d.Visit.PatientId == patientId).Include(d => d.Visit).OrderBy(d => d.Visit.VisitDate)`. Return a flat DTO projection, not full entities.
**Warning signs:** Multiple SQL queries in logs when loading the trend chart.

### Pitfall 7: Public OSDI Token Security
**What goes wrong:** Guessable tokens allow unauthorized access to patient visit data.
**Why it happens:** Using sequential IDs or simple random strings.
**How to avoid:** Generate cryptographically secure tokens (e.g., `Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))`). Set token expiry (24 hours). Return minimal data (questions + visit date only, no patient name or medical data).
**Warning signs:** Token is a GUID (guessable pattern) or has no expiry.

## Code Examples

### OSDI Score Calculation (TypeScript)
```typescript
// Verified formula from OSDI clinical standard
// Source: https://eyecalc.org/osdi/
interface OsdiResult {
  score: number | null
  severity: "normal" | "mild" | "moderate" | "severe"
  answeredCount: number
}

function calculateOsdi(answers: (number | null)[]): OsdiResult {
  const answered = answers.filter((a): a is number => a !== null && a !== undefined)
  if (answered.length === 0) return { score: null, severity: "normal", answeredCount: 0 }

  const sum = answered.reduce((acc, val) => acc + val, 0)
  const score = (sum * 100) / (answered.length * 4)

  let severity: OsdiResult["severity"]
  if (score <= 12) severity = "normal"
  else if (score <= 22) severity = "mild"
  else if (score <= 32) severity = "moderate"
  else severity = "severe"

  return { score: Math.round(score * 100) / 100, severity, answeredCount: answered.length }
}
```

### OSDI Severity Badge Component
```typescript
// Source: follows AllergyAlert banner pattern
const SEVERITY_CONFIG = {
  normal: { label: "osdi.normal", color: "bg-green-100 text-green-800 border-green-300" },
  mild: { label: "osdi.mild", color: "bg-yellow-100 text-yellow-800 border-yellow-300" },
  moderate: { label: "osdi.moderate", color: "bg-orange-100 text-orange-800 border-orange-300" },
  severe: { label: "osdi.severe", color: "bg-red-100 text-red-800 border-red-300" },
} as const
```

### OSDI Trend Chart (recharts)
```typescript
// Source: recharts official docs + OSDI severity bands
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, ReferenceArea } from "recharts"

interface OsdiDataPoint {
  visitDate: string
  score: number
}

function OsdiTrendChart({ data }: { data: OsdiDataPoint[] }) {
  return (
    <ResponsiveContainer width="100%" height={300}>
      <LineChart data={data}>
        {/* Severity background bands */}
        <ReferenceArea y1={0} y2={12} fill="#dcfce7" fillOpacity={0.5} />
        <ReferenceArea y1={12} y2={22} fill="#fef9c3" fillOpacity={0.5} />
        <ReferenceArea y1={22} y2={32} fill="#fed7aa" fillOpacity={0.5} />
        <ReferenceArea y1={32} y2={100} fill="#fecaca" fillOpacity={0.5} />
        <XAxis dataKey="visitDate" />
        <YAxis domain={[0, 100]} />
        <Tooltip />
        <Line type="monotone" dataKey="score" stroke="hsl(var(--primary))" strokeWidth={2} />
      </LineChart>
    </ResponsiveContainer>
  )
}
```

### Lightbox with Zoom (yet-another-react-lightbox)
```typescript
// Source: yet-another-react-lightbox official docs
import Lightbox from "yet-another-react-lightbox"
import Zoom from "yet-another-react-lightbox/plugins/zoom"
import Video from "yet-another-react-lightbox/plugins/video"
import Fullscreen from "yet-another-react-lightbox/plugins/fullscreen"
import "yet-another-react-lightbox/styles.css"

interface ImageLightboxProps {
  open: boolean
  onClose: () => void
  slides: Array<{ src: string; alt?: string } | { type: "video"; sources: Array<{ src: string; type: string }> }>
  index: number
}

function ImageLightbox({ open, onClose, slides, index }: ImageLightboxProps) {
  return (
    <Lightbox
      open={open}
      close={onClose}
      slides={slides}
      index={index}
      plugins={[Zoom, Video, Fullscreen]}
      zoom={{ maxZoomPixelRatio: 5 }}
    />
  )
}
```

### DryEyeForm OD/OS Grid (mirrors RefractionForm)
```typescript
// Source: follows RefractionForm.tsx grid-cols-[80px_1fr_1fr] pattern exactly
const DRY_EYE_FIELDS = [
  { key: "Tbut", label: "tbut", min: 0, max: 30, step: 1, unit: "s" },
  { key: "Schirmer", label: "schirmer", min: 0, max: 35, step: 1, unit: "mm" },
  { key: "MeibomianGrading", label: "meibomianGrading", min: 0, max: 3, step: 1 },
  { key: "TearMeniscus", label: "tearMeniscus", min: 0, max: 2, step: 0.1, unit: "mm" },
  { key: "Staining", label: "staining", min: 0, max: 5, step: 1 },
] as const

// Render in grid-cols-[80px_1fr_1fr] with OD/OS headers
// Auto-save on blur with 500ms debounce (same as RefractionForm)
```

## OSDI 12 Questions (Reference Data)

The OSDI questionnaire consists of 12 questions in 3 subscales. Each question is rated 0-4 (None of the time / Some of the time / Half of the time / Most of the time / All of the time).

**Subscale A: Ocular Symptoms (Q1-5)**
1. Eyes that are sensitive to light?
2. Eyes that feel gritty?
3. Painful or sore eyes?
4. Blurred vision?
5. Poor vision?

**Subscale B: Vision-Related Function (Q6-9)**
6. Reading?
7. Driving at night?
8. Working with a computer or bank machine (ATM)?
9. Watching TV?

**Subscale C: Environmental Triggers (Q10-12)**
10. Windy conditions?
11. Places or areas with low humidity (very dry)?
12. Areas that are air conditioned?

**Note:** Questions 6-12 also have "N/A" option. N/A responses are excluded from the denominator in the OSDI formula (this is why the formula divides by `answered * 4`, not `12 * 4`).

**Vietnamese translations are required.** The validated Vietnamese OSDI translation exists and should be used. Store both English and Vietnamese text for each question.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Store images as BLOBs in SQL | Store in Azure Blob Storage, metadata in SQL | Azure best practice | Performance, cost, scalability |
| DICOM integration for all clinical images | JPEG/PNG for v1, DICOM deferred | Project decision | Simpler implementation, sufficient for private clinic |
| Custom chart rendering (D3 direct) | Declarative chart libraries (recharts) | 2020+ React ecosystem | 10x faster development, maintained by community |
| Server-side image thumbnails | CSS object-fit with SAS URLs | 2023+ with modern browsers | No server infrastructure needed for thumbnails |

**Deprecated/outdated:**
- react-image-lightbox: deprecated, replaced by yet-another-react-lightbox
- recharts v2: recharts v3 has breaking changes (import paths changed); use v3.7.0+

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit + FluentAssertions + NSubstitute (backend) |
| Config file | Clinical.Unit.Tests.csproj (exists) |
| Quick run command | `dotnet test backend/tests/Clinical.Unit.Tests --filter "Category!=Integration" -x` |
| Full suite command | `dotnet test backend/tests/ --no-restore` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| DRY-01 | Create/update dry eye assessment with structured fields | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~UpdateDryEyeAssessment" -x` | No - Wave 0 |
| DRY-02 | OSDI score calculation and severity classification | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~OsdiCalculation" -x` | No - Wave 0 |
| DRY-03 | OSDI history query returns chronological data | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~GetOsdiHistory" -x` | No - Wave 0 |
| DRY-04 | Dry eye comparison returns two visits' data | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~DryEyeComparison" -x` | No - Wave 0 |
| IMG-01 | Upload medical image stores blob and metadata | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~UploadMedicalImage" -x` | No - Wave 0 |
| IMG-02 | Upload video with extended size limit | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~UploadMedicalImage" -x` | No - Wave 0 |
| IMG-03 | Get visit images returns SAS URLs | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~GetVisitImages" -x` | No - Wave 0 |
| IMG-04 | Image comparison returns matched images by type | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~ImageComparison" -x` | No - Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test backend/tests/Clinical.Unit.Tests -x`
- **Per wave merge:** `dotnet test backend/tests/ --no-restore`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `backend/tests/Clinical.Unit.Tests/Features/UpdateDryEyeAssessmentHandlerTests.cs` -- covers DRY-01
- [ ] `backend/tests/Clinical.Unit.Tests/Features/OsdiCalculationTests.cs` -- covers DRY-02
- [ ] `backend/tests/Clinical.Unit.Tests/Features/SubmitOsdiQuestionnaireHandlerTests.cs` -- covers DRY-02 (public endpoint)
- [ ] `backend/tests/Clinical.Unit.Tests/Features/GetOsdiHistoryHandlerTests.cs` -- covers DRY-03
- [ ] `backend/tests/Clinical.Unit.Tests/Features/GetDryEyeComparisonHandlerTests.cs` -- covers DRY-04
- [ ] `backend/tests/Clinical.Unit.Tests/Features/UploadMedicalImageHandlerTests.cs` -- covers IMG-01, IMG-02
- [ ] `backend/tests/Clinical.Unit.Tests/Features/GetVisitImagesHandlerTests.cs` -- covers IMG-03
- [ ] `backend/tests/Clinical.Unit.Tests/Features/GetImageComparisonDataHandlerTests.cs` -- covers IMG-04
- [ ] `backend/tests/Clinical.Unit.Tests/Features/DeleteMedicalImageHandlerTests.cs` -- covers IMG-01 (delete)
- [ ] Framework install: none needed -- xUnit + FluentAssertions + NSubstitute already in Clinical.Unit.Tests.csproj

## Open Questions

1. **Vietnamese OSDI translation source**
   - What we know: A validated Vietnamese OSDI translation exists in clinical literature
   - What's unclear: Whether the exact wording is copyrighted or freely usable
   - Recommendation: Use the standard Vietnamese OSDI translation from published clinical literature; the questionnaire itself (OSDI) is widely reproduced in clinical tools. Store the 12 questions in a JSON seed file for easy updating.

2. **Azure Storage emulator for local development**
   - What we know: AzureBlobService defaults to `UseDevelopmentStorage=true` when no connection string is set
   - What's unclear: Whether Azurite is installed/configured for local development
   - Recommendation: Ensure Azurite (Azure Storage emulator) is running locally for image upload testing. Document in development setup.

3. **Image comparison UX edge cases**
   - What we know: User wants side-by-side image comparison across two visits of the same type
   - What's unclear: What happens when a visit has multiple images of the same type for the same eye
   - Recommendation: Show a sub-gallery within each comparison panel; user can click through images of the same type for that visit

## Sources

### Primary (HIGH confidence)
- Existing codebase: Visit.cs, Refraction.cs, RefractionForm.tsx, RefractionSection.tsx, IAzureBlobService.cs, AzureBlobService.cs, PatientApiEndpoints.cs (photo upload), PublicBookingEndpoints.cs, ClinicalApiEndpoints.cs, VisitSection.tsx, VisitDetailPage.tsx, IVisitRepository.cs, ClinicalDbContext.cs, ITemplateDefinition.cs
- [recharts npm](https://www.npmjs.com/package/recharts) - v3.7.0, MIT license, 3.6M weekly downloads
- [yet-another-react-lightbox npm](https://www.npmjs.com/package/yet-another-react-lightbox) - v3.29.1, MIT license, React 19 compatible
- [qrcode.react npm](https://www.npmjs.com/package/qrcode.react) - v4.1.0, MIT license
- [OSDI scoring formula](https://eyecalc.org/osdi/) - Standard clinical scoring

### Secondary (MEDIUM confidence)
- [Oxford Grading System - AAO](https://www.aao.org/education/image/oxford-grading-system) - Oxford staining scale 0-5
- [OSDI questionnaire validation](https://jamanetwork.com/journals/jamaophthalmology/fullarticle/413145) - OSDI reliability and validity
- [OSDI Vietnamese translation](https://pmc.ncbi.nlm.nih.gov/articles/PMC11890261/) - OSDI in different languages

### Tertiary (LOW confidence)
- Clinical image file sizes: estimated from general OCT/meibography knowledge; actual sizes depend on specific devices used at Ganka28 clinic

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - recharts, YARL, and qrcode.react are well-established MIT libraries verified via npm
- Architecture: HIGH - all patterns directly mirror existing Phase 3 codebase (Refraction entity, RefractionForm, photo upload, public booking endpoints)
- Pitfalls: HIGH - identified from direct codebase analysis (EnsureEditable guard, OSDI formula, SAS expiry)
- OSDI clinical reference: MEDIUM - standard ophthalmology scoring verified via published literature
- File size limits: MEDIUM - based on general clinical imaging knowledge, not device-specific data from Ganka28

**Research date:** 2026-03-05
**Valid until:** 2026-04-05 (stable domain, no fast-moving dependencies)

# Pitfalls Research

**Domain:** Ophthalmology Clinic Management System (HIS + Pharmacy + Optical + Chronic Disease)
**Researched:** 2026-02-28
**Confidence:** MEDIUM-HIGH (domain-specific patterns verified across multiple sources; Vietnamese regulatory specifics have some gaps due to limited English-language documentation)

## Critical Pitfalls

### Pitfall 1: Treating Ophthalmology Data Like General Medical Data

**What goes wrong:**
The system stores ophthalmic exam data as free-text or generic key-value pairs instead of structured, typed fields. Refraction data (SPH, CYL, AXIS, ADD, PD, VA, IOP, Axial Length) gets stored as strings. OSDI scores, TBUT measurements, and Schirmer values lack proper numeric typing and validation ranges. Laterality (OD/OS/OU) is inconsistently captured. The result is data that cannot be trended, compared across visits, or exported for research -- destroying the clinic's core differentiator.

**Why it happens:**
Developers coming from general web development treat medical data as "just another form." They underestimate the complexity: ophthalmology requires hundreds more numerical values than typical medical specialties, complex optical mathematical formulas, and laterality on nearly every measurement. Generic EHR approaches fail because ophthalmology documentation is radically different from most specialties.

**How to avoid:**
- Design domain-specific value objects for every ophthalmic measurement: `RefractionReading(SPH, CYL, AXIS)`, `VisualAcuity(Notation, Value)`, `IntraocularPressure(Value, Method)`, `OSDIScore(Value)`, `TBUT(Seconds)`, `SchirmerTest(Mm, Duration)`
- Enforce laterality (Right/Left/Both) as a required discriminator on ALL eye-specific measurements at the domain level, not just the UI
- Use proper numeric types with clinically valid ranges (e.g., SPH: -30.00 to +30.00 in 0.25 steps; CYL: -10.00 to 0.00; IOP: 5-80 mmHg)
- Build the Dry Eye examination template as a first-class aggregate, not a generic form builder

**Warning signs:**
- Data model has generic `ExamField(Name, Value)` tables instead of typed columns
- Refraction stored as a single string like "-2.50/-1.25x180"
- No laterality enum/discriminator on eye measurements
- Cannot write a SQL query to "find all patients whose OSDI improved by >10 points"
- Cannot generate trend charts from stored data without parsing strings

**Phase to address:**
Phase 1 (Foundation) -- Domain modeling. This must be correct from Day 1. Retrofitting structured data onto a generic schema is a rewrite-level effort because every query, report, and comparison feature depends on it.

---

### Pitfall 2: ICD-10 Laterality and Specificity Failures

**What goes wrong:**
The system allows unspecified ICD-10 codes (using digit "9" for unspecified eye) or codes without required laterality. Diagnoses get recorded as H52.1 (myopia, unspecified) instead of H52.11 (right eye), H52.12 (left eye), or H52.13 (bilateral). When it is time to connect to So Y Te or submit data, claims are denied, data is rejected, and a massive retroactive data cleanup is required.

**Why it happens:**
ICD-10 coding for ophthalmology is uniquely complex. The 6th character encodes laterality (1=right, 2=left, 3=bilateral), and eyelid codes use 1=right upper, 2=right lower, 4=left upper, 5=left lower. Developers implement a simple code lookup without enforcing specificity rules. Doctors pick the first matching code from autocomplete without being prompted for laterality. Some codes require laterality while others (like presbyopia H52.4) do not -- the rules are inconsistent.

**How to avoid:**
- Load the AAO's ICD-10-CM for Ophthalmology reference to identify which codes require laterality
- When a selected code has laterality options, force the doctor to choose OD/OS/OU before accepting the code -- never allow the unspecified variant when laterality is known
- Pre-filter ICD-10 suggestions based on the eye being examined (if the doctor is documenting a right-eye finding, suggest right-eye codes first)
- Validate at the domain level: if an examination record specifies laterality, the linked ICD-10 code must match
- Build the ICD-10 lookup as a bounded context service with ophthalmology-specific business rules, not a generic code picker

**Warning signs:**
- ICD-10 picker shows all codes without filtering by laterality
- High percentage of diagnoses using "unspecified" codes
- No validation between exam laterality and diagnosis code laterality
- Data exports contain codes that would be rejected by insurance or regulatory systems

**Phase to address:**
Phase 1 (Foundation) -- ICD-10 from Day 1 per PROJECT.md. The lookup service with laterality enforcement must be built into the diagnosis workflow from the start to ensure data readiness for So Y Te connection (31/12/2026).

---

### Pitfall 3: Immutable Medical Records Done Wrong

**What goes wrong:**
The system either (a) makes records truly immutable and provides no way to correct errors, leading to dangerous medical inaccuracies; or (b) implements "immutability" via soft-delete flags that can be toggled, which is not actually immutable and fails regulatory audit. The most common failure: amendments overwrite original data instead of creating new amendment records, destroying the audit trail.

**Why it happens:**
"Immutable" in software usually means "append-only" but developers implement it as "no UPDATE queries." In medical records, the requirement is more nuanced: the original record must be preserved AND corrections must be possible through amendment records that reference the original. This is an event-sourcing-like pattern that is counterintuitive if you are used to CRUD.

**How to avoid:**
- Model visit records as append-only: `VisitRecord` is created once and never modified via UPDATE
- Corrections create `AmendmentRecord` entities that reference the original record, contain the corrected data, the reason for correction, the amending user, and a timestamp
- The "current truth" is computed by applying amendments in order on top of the original
- Field-level audit trail: every change to any medical data field must record who, when, what changed, old value, new value
- Use EF Core interceptors or domain events to automatically capture audit data -- do not rely on developers remembering to log changes
- Cryptographic signing (hash chain) of records is a strong-but-optional enhancement for tamper detection

**Warning signs:**
- `UPDATE` statements on medical record tables in the codebase
- Audit trail stored in a separate table that could be independently deleted
- No way for a doctor to "correct" a record in the UI (they will work around immutability by deleting and recreating)
- Amendment records lack a reference to the original field/value being corrected

**Phase to address:**
Phase 1 (Foundation) -- Core domain model. The Visit aggregate and amendment pattern must be established before any clinical data is entered. Retrofitting immutability onto mutable records is effectively impossible without data migration.

---

### Pitfall 4: Vietnamese Regulatory Compliance as an Afterthought

**What goes wrong:**
The system is built as a generic clinic management tool and the So Y Te data connection, electronic prescription requirements (Circular 26/2025/TT-BYT), and national e-prescription portal linkage are treated as "Phase 2" features. When the 31/12/2026 deadline approaches, the data model is discovered to be missing required fields (personal identification number/CCCD, specific prescription format requirements, digital signature support), and the entire prescription workflow needs rearchitecting.

**Why it happens:**
Vietnamese healthcare IT regulations are evolving rapidly. Circular 26/2025 requires electronic prescriptions with specific fields (patient CCCD/passport, INN generic drug names, precise quantity formats, digital signatures) and connection to the Ministry of Health's National e-Prescription Portal. The technical specifications for the portal API are not widely published in English, making it easy to defer. Additionally, only ~20% of private clinics have implemented national prescription linkage, creating a false sense that enforcement is lenient.

**How to avoid:**
- From Day 1, prescription data model must include ALL Circular 26/2025 required fields: patient CCCD, INN drug names, prescriber digital certificate reference, QR code identifier field
- Design the prescription entity to be exportable in whatever format So Y Te requires (even if the actual API is not yet integrated)
- Track the Ganka28 deadline explicitly: So Y Te connection before 31/12/2026 means the integration must be TESTED by ~October 2026
- Build a `RegulatoryExport` interface/abstraction in the Pharmacy module from the start, even if the implementation is a stub
- Contact So Y Te Hanoi or the clinic's licensing authority early (before development starts) to obtain current technical specifications for data connection

**Warning signs:**
- Prescription model lacks CCCD/citizen ID field
- Drug names stored only as Vietnamese brand names without INN/generic names
- No placeholder for digital signature in the prescription workflow
- No export/reporting capability for prescription data
- The regulatory deadline is not tracked in the project timeline

**Phase to address:**
Phase 1 (Foundation) -- Data model must include all regulatory fields. Phase 3 or 4 (Integration) -- Actual So Y Te API connection. But the data MUST be ready from Phase 1. The project deadline (31/12/2026) leaves limited runway for rework.

---

### Pitfall 5: Medical Image Management as File Upload

**What goes wrong:**
Medical images (Fluorescein angiography, Meibography, OCT scans) are stored as generic file uploads with minimal metadata. There is no structured association between images and specific exams, no laterality tagging on images, no temporal ordering for comparison views, and no efficient progressive loading for high-resolution images. The side-by-side comparison feature -- critical for chronic disease tracking -- becomes unusable because images cannot be efficiently queried, ordered, or rendered.

**Why it happens:**
Developers implement image upload as a generic "attach file to visit" feature. In ophthalmology, images are clinical data, not attachments. Each image has specific metadata: image type (Fluorescein, Meibography, OCT, etc.), laterality (OD/OS), acquisition date/time, device used, and clinical context. Without this structure, the comparison view (showing disease progression across visits) cannot be built.

**How to avoid:**
- Design `MedicalImage` as a first-class domain entity with typed metadata: `ImageType` enum (Fluorescein, Meibography, OCT, SlitLamp, Fundus, etc.), `Eye` (Right/Left), `AcquisitionDate`, `DeviceId`, `VisitId`, `PatientId`
- Store images in Azure Blob Storage with a structured path: `{patientId}/{imageType}/{date}/{filename}`
- Generate thumbnails on upload for list views; use SAS tokens with short expiration (15-30 minutes) for full-resolution access
- Implement progressive image loading: thumbnail first, then full resolution on demand
- Pre-build the comparison query: "Get all OCT images for patient X, right eye, ordered by date" must be a single indexed query
- Plan for image sizes: OCT volumes can be 50-200MB; Fluorescein/Fundus photos 5-20MB each; budget Azure Blob storage accordingly

**Warning signs:**
- Images stored in a generic `Attachments` table with no type/laterality columns
- No thumbnail generation -- all views load full-resolution images
- Comparison view requires loading all patient images and filtering client-side
- SAS tokens have long expiration times (hours/days) or no expiration
- No storage tier strategy (all images in Hot tier regardless of age)

**Phase to address:**
Phase 2 (Clinical Core) -- Image management module. But the `MedicalImage` entity and Azure Blob infrastructure must be designed in Phase 1 (Foundation) to ensure the domain model supports structured image metadata from the start.

---

### Pitfall 6: Chronic Disease Tracking Without Standardized Parameters

**What goes wrong:**
The Dry Eye management template stores OSDI, TBUT, Schirmer, and other measurements but does not enforce consistent measurement protocols across visits. Visit 1 records TBUT in seconds; visit 3 the technician records it differently. The OSDI questionnaire version changes but old scores are not flagged. Treatment effectiveness reports become unreliable because the underlying data is inconsistent, undermining the clinic's core value proposition.

**Why it happens:**
Published research highlights this exact problem: "a lack of a validated or uniformly agreed upon objective dry eye parameters to assess dry eye severity over time." Even within a single clinic, there can be "variations in the recording and interpretation of data at each visit." Software that allows free-form entry of clinical measurements enables this inconsistency.

**How to avoid:**
- Build the Dry Eye template as a structured aggregate: `DryEyeAssessment(OSDI, TBUT_OD, TBUT_OS, Schirmer_OD, Schirmer_OS, MeibomianGrading, LidMarginScore, ...)`
- OSDI must be a computed score from the 12-question questionnaire with version tracking -- do not allow manual score entry without the underlying responses
- Enforce units and measurement methods: TBUT must specify seconds (integer), Schirmer must specify mm and duration (5 min standard)
- For treatment protocols (IPL/LLLT), require a `DryEyeAssessment` at session 1 and session N (final) at minimum to enable before/after comparison
- Version the assessment template so that when parameters change, historical data retains its original context

**Warning signs:**
- OSDI stored as a single number without the underlying 12 questionnaire responses
- No unit enforcement on measurements (TBUT could be "8" or "8 seconds" or "8s")
- Treatment sessions do not require/link to assessment data
- Cannot generate a "OSDI trend over time" chart for a patient
- No template versioning -- changes to the form break historical data interpretation

**Phase to address:**
Phase 2 (Clinical Core) -- Dry Eye template implementation. The assessment aggregate design must be locked down before any clinical data is entered. This is the clinic's primary differentiator and must be built with data integrity as the top priority.

---

### Pitfall 7: Appointment Scheduling Race Conditions

**What goes wrong:**
Two staff members simultaneously book different patients into the same slot for the same doctor. The system shows the slot as available to both users because availability was checked at read time, and the booking was committed without a uniqueness constraint or optimistic concurrency check. With only 2 doctors and limited slots, even one double-booking disrupts the entire afternoon.

**Why it happens:**
The classic TOCTOU (Time-Of-Check-Time-Of-Use) problem. The system checks "is slot X available?" (returns yes), then creates the appointment. Between the check and the create, another user books the same slot. Without database-level enforcement, the application-level check is insufficient.

**How to avoid:**
- Unique constraint on `(DoctorId, SlotDate, SlotTime)` at the database level -- this is the only reliable prevention
- Use optimistic concurrency (EF Core `RowVersion`) on the schedule/slot entity so concurrent writes fail fast
- Return a clear, user-friendly error when a conflict occurs: "This slot was just booked by another user. Please select a different time."
- Consider a "slot reservation" pattern: when a user starts the booking form, place a 2-minute hold on the slot to reduce conflicts

**Warning signs:**
- No unique constraint on appointment slots in the database schema
- Availability check is a simple SELECT without any locking
- No concurrency handling in the booking endpoint
- No conflict error message exists in the UI

**Phase to address:**
Phase 2 (Clinical Core) -- Appointment scheduling module. The unique constraint must be in the initial migration; adding it later risks data cleanup.

---

## Technical Debt Patterns

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Generic `ExamField(Name, Value)` tables | Faster initial development, flexible schema | Cannot query, trend, or report on structured data; eventual rewrite | Never for ophthalmology-specific data. Use only for truly unstructured notes |
| Storing medical images in SQL Server FILESTREAM | Simpler deployment (one store) | Database bloat, backup complexity, cost at scale, harder to serve via CDN | Never -- Azure Blob is already the chosen approach |
| Single DbContext for all modules | Simpler EF migrations | Coupling between modules, migration conflicts, slow startup | Only acceptable in first 2 weeks of rapid prototyping; split immediately after |
| Skipping thumbnail generation for images | Faster upload pipeline | Every list view loads full-resolution images; becomes unusable with 50+ images per patient | MVP only if image volume is very low (<10 per patient); fix before launch |
| Manual audit logging (developer must remember) | No infrastructure overhead | Missed audit entries on critical medical fields; compliance failure | Never for medical record changes. Automated interceptors only |
| Storing Zalo OA tokens in appsettings.json | Quick integration setup | Token expires in 25 hours; manual refresh required; integration breaks silently | Never -- tokens must be stored in database with automated refresh |
| Hardcoding VIP discount rules | Faster implementation | Every pricing change requires code deployment | MVP only; make configurable before launch (project explicitly requires "configurable discounts") |

## Integration Gotchas

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| Zalo OA API | Storing access token statically; not handling 25-hour token expiration | Store refresh token in database. Run background job to refresh access token before expiration. Implement exponential backoff on failures. Log all token refresh attempts |
| Zalo ZNS Messages | No fallback when patient has not followed OA or blocked messages | Check follower status before sending. Maintain a delivery status log. Provide staff UI to see which patients did NOT receive notifications. Do NOT assume delivery |
| So Y Te Data Connection | Waiting until deadline to discover API requirements | Contact So Y Te Hanoi early for technical specifications. Build prescription data model to be export-ready from Day 1. Create a `RegulatoryExport` abstraction layer |
| National e-Prescription Portal | Prescription data missing required fields (CCCD, INN names) | Include all Circular 26/2025 required fields in prescription entity from Day 1, even if the portal connection is built later |
| MISA E-invoicing | Generating export data from normalized tables (slow, complex joins) | Build denormalized invoice export view/table specifically for MISA. Use CQRS read model for financial exports |
| Azure Blob Storage | Using account-level SAS tokens for image access | Use delegation-based SAS tokens scoped to specific blobs/containers. Set 15-30 minute expiration. Audit token generation |
| Wolverine FX Outbox | Messages accumulating in `wolverine_outgoing_envelopes` table and never being cleaned up | Configure `OutboxStaleTime` threshold. Monitor outbox table size. Set up alerting when messages are older than expected processing time |
| Wolverine FX + EF Core | Not adding Wolverine envelope mappings to DbContext | Add envelope storage mappings to enable EF Core command batching. Without this, Wolverine makes separate database calls instead of batching |

## Performance Traps

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| Loading all patient images for comparison view | Page takes 10+ seconds, browser memory spikes | Paginated image API, thumbnail-first loading, lazy load full-res only on click | >20 images per patient (reached within 3-6 months of chronic disease tracking) |
| ICD-10 code search on full table scan | Autocomplete laggy (>500ms) | Pre-indexed search table, trigram index for partial match, client-side caching of common codes | Immediate -- ICD-10 has ~70,000 codes |
| Appointment availability query scanning all appointments | Calendar view slow to load | Materialized slot availability table or indexed view. Query slots, not appointments | >500 appointments (reached within 3-6 months) |
| Audit trail table growing unbounded | Database size balloons, queries on audit table slow down | Partition audit table by month/year. Archive old audit data to cold storage. Index on (EntityId, Timestamp) | >100K audit records (reached within 6-12 months with field-level auditing) |
| Denormalized reporting read models not being updated | Reports show stale data, discrepancies with live data | Ensure domain events trigger read model updates via Wolverine. Monitor event processing lag. Build reconciliation checks | As soon as reporting module is used for business decisions |
| Vietnamese text search without proper collation | Search for "Nguyen" does not find "Nguyen" with diacritics; or vice versa | Use SQL Server Vietnamese collation (Vietnamese_CI_AS). Implement accent-insensitive search option. NFC-normalize all text input | Immediate -- every patient name search |

## Security Mistakes

| Mistake | Risk | Prevention |
|---------|------|------------|
| All staff roles can view all patient records | Violation of least-privilege; technicians can see financial data, cashiers can see clinical notes | Implement granular RBAC from Day 1: Doctors see clinical + prescriptions; Technicians see refraction + images; Cashiers see billing only; Optical staff see optical orders only. Map to the 6 roles defined in PROJECT.md |
| RBAC "role explosion" with per-user overrides | Administrative chaos; orphaned permissions when staff change roles | Keep roles to the defined 6 + Admin/Owner. Use permission groups within roles. Avoid per-user permission overrides. Regular permission audits |
| SAS tokens for medical images with long expiration or account-level scope | If a token leaks, attacker can access all patient images for hours/days | Use user-delegation SAS tokens. Scope to individual containers. 15-30 minute expiration maximum. Log all token generation. Rotate storage keys regularly |
| Audit trail accessible to application users for deletion | Defeats the purpose of audit trail if records can be tampered with | Separate audit database user with INSERT-only permissions. Application connection string for audit should not have DELETE/UPDATE on audit tables. Consider append-only table design in SQL Server |
| No break-glass access for emergencies | Legitimate urgent access is impossible, leading to workaround accounts or shared passwords | Implement "emergency access" flag that grants temporary elevated access with mandatory post-incident review and prominent audit logging |
| Zalo OA tokens stored in source code or config files | Token leak exposes patient notification channel; attacker could send messages to all patients | Store tokens in Azure Key Vault or encrypted database column. Never commit tokens to source control. Rotate tokens on suspected compromise |

## UX Pitfalls

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| Generic EHR interface requiring too many clicks for routine eye exams | Doctors stay after-hours inputting data; some revert to paper. Ophthalmologists have abandoned EHR systems over this exact issue | Design the Dry Eye exam flow as a single-screen "dashboard" with direct data entry for all measurements. Minimize clicks: no modals for each measurement, no separate pages for OD vs OS. Target: complete a follow-up Dry Eye exam entry in <3 minutes |
| Separate systems for imaging, exam, pharmacy, optical that do not communicate | Staff re-enters patient info across modules; errors from transcription; delays in workflow | Single patient context across all modules. When a doctor prescribes glasses, the optical order auto-populates with the refraction data from the exam. When pharmacy dispenses, it links back to the prescription |
| OSDI questionnaire as a separate form patients fill out on paper, then transcribed | Data entry delay, transcription errors, patients waiting | Build patient-facing OSDI questionnaire (tablet/phone). Auto-score. Auto-link to the visit record. Consider sending via Zalo OA pre-appointment |
| Bilingual UI that is only skin-deep | English-speaking patients or foreign staff see mixed Vietnamese/English because some terms (drug names, ICD descriptions) are only in one language | ICD-10 codes must have both Vietnamese and English descriptions. Drug names: INN (English) + Vietnamese brand name. UI labels: complete translation coverage. System messages: bilingual. Do not mix languages within a single screen |
| Image comparison view that requires manual image selection | Doctors waste time finding and selecting the right images to compare | Auto-suggest comparison pairs: same image type, same eye, most recent vs. first visit. One-click "show progression" for chronic disease patients |
| Prescription print format that does not match Circular 26/2025 requirements | Printed prescriptions are non-compliant; clinic risks regulatory issues | Build prescription print template to exact Circular 26/2025 specifications from Day 1. Include all required fields even if some are optional for the clinic's current workflow |

## "Looks Done But Isn't" Checklist

- [ ] **Patient Search:** Often missing accent-insensitive Vietnamese search -- verify "Nguyen" finds "Nguy-n" and vice versa. Also verify phone number search works with/without country code and formatting
- [ ] **ICD-10 Lookup:** Often missing laterality enforcement -- verify that selecting a code requiring laterality forces OD/OS/OU selection before saving
- [ ] **Immutable Records:** Often missing the amendment workflow -- verify that correcting a diagnosis creates an amendment record, not an UPDATE on the original. Verify the original value is still visible in the audit trail
- [ ] **Appointment Booking:** Often missing concurrent booking protection -- verify two users cannot book the same slot simultaneously (test with parallel requests)
- [ ] **Prescription Printing:** Often missing required regulatory fields -- verify CCCD, INN drug names, and all Circular 26/2025 fields appear on printed and electronic prescriptions
- [ ] **Image Comparison:** Often missing temporal ordering -- verify images are sorted by acquisition date, not upload date. Verify comparison works when images come from different devices
- [ ] **VIP Discounts:** Often missing exclusion rules -- verify VIP discount is NOT applied to prescription drugs or fixed-price items (per PROJECT.md requirements)
- [ ] **Pharmacy Expiry:** Often missing FIFO enforcement -- verify dispensing prioritizes nearest-expiry stock. Verify expiry alerts fire at the configured threshold (e.g., 3 months before)
- [ ] **Zalo Notifications:** Often missing delivery failure handling -- verify the system logs when a message fails (patient not on Zalo, blocked OA, etc.) and provides staff visibility into failures
- [ ] **Optical Order Tracking:** Often missing the full lifecycle -- verify status transitions (ordered -> processing -> ready -> delivered) are enforced (cannot skip states) and that each transition is timestamped
- [ ] **Audit Trail:** Often missing field-level granularity -- verify that changing a single field (e.g., IOP from 18 to 21) creates an audit entry showing the old and new values, not just "record was modified"
- [ ] **Multi-currency/formatting:** Often missing -- verify VND currency formatting (no decimal places, dot as thousands separator), Vietnamese date formats (dd/MM/yyyy), and phone number format (+84...)

## Recovery Strategies

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| Generic data model (no structured ophthalmic fields) | HIGH | Requires schema migration, data migration (parsing strings to structured fields), and rewriting all queries/reports. Plan 4-6 weeks of rework |
| Missing ICD-10 laterality | MEDIUM | Add laterality column, backfill from exam records where possible, flag ambiguous records for doctor review. Plan 2-3 weeks |
| Mutable medical records (using UPDATEs) | HIGH | Requires moving to append-only model, migrating existing data to initial "snapshots," and rebuilding all read queries to account for amendments. Plan 6-8 weeks |
| Regulatory fields missing from prescription model | MEDIUM | Add columns, but historical prescriptions cannot be retroactively updated with CCCD/INN data. Manual cleanup required. Plan 2-3 weeks for schema + 2-4 weeks for data cleanup |
| No image metadata structure | MEDIUM | Add metadata columns to image table. Existing images need manual metadata tagging (type, laterality, date) which is labor-intensive. Plan 2-3 weeks for code + ongoing manual tagging |
| Zalo token management failure | LOW | Implement token storage + refresh job. May need to re-authenticate OA. Plan 1-2 days |
| Appointment double-booking in production | LOW | Add unique constraint (may need to resolve existing conflicts first). Plan 1-2 days |
| RBAC too permissive | MEDIUM | Tighten permissions (may break workflows staff depend on). Requires role audit with clinic manager. Plan 1-2 weeks including testing |
| Audit trail gaps | HIGH | Cannot recover data that was not logged. Going forward, add automated interceptors. Historical gap is permanent. Prevention is the only strategy |

## Pitfall-to-Phase Mapping

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Generic data model for ophthalmology | Phase 1 (Foundation) | Can you write a query: "All patients with OSDI > 30 and TBUT < 5 in both eyes"? If yes, data model is correct |
| ICD-10 laterality failures | Phase 1 (Foundation) | Submit a diagnosis without laterality for a code that requires it -- system should reject |
| Immutable records done wrong | Phase 1 (Foundation) | Amend a diagnosis. Verify original is preserved. Verify audit trail shows both. Verify no UPDATE on medical tables |
| Regulatory compliance deferred | Phase 1 (Foundation, data model) + Phase 4 (Integration) | Export a prescription and verify all Circular 26/2025 fields are present. By October 2026, test So Y Te connection |
| Medical image as file upload | Phase 1 (data model) + Phase 2 (implementation) | Query: "All OCT images, right eye, for patient X, ordered by date." If this works as a single DB query, image metadata is correct |
| Inconsistent chronic disease tracking | Phase 2 (Clinical Core) | Create two Dry Eye assessments for same patient. Generate a comparison report. Verify units match and trend is computable |
| Appointment race conditions | Phase 2 (Clinical Core) | Run concurrent booking test (two parallel HTTP requests for same slot). Exactly one should succeed, one should fail with clear error |
| Zalo token management | Phase 3 (Integrations) | Simulate token expiration. Verify automatic refresh. Verify notification sending recovers without manual intervention |
| RBAC too permissive | Phase 1 (Foundation) | Log in as Technician. Verify cannot access financial reports. Log in as Cashier. Verify cannot view clinical notes |
| Vietnamese text handling | Phase 1 (Foundation) | Search for "Nguyen Thi Lan" using "nguyen thi lan" (no diacritics). Verify match. Verify database collation is Vietnamese_CI_AS |
| Wolverine outbox message accumulation | Phase 1 (Foundation) | After running for 24 hours, check `wolverine_outgoing_envelopes` table. Should be near-empty if messages are being processed and cleaned |
| VIP discount logic errors | Phase 3 (Finance) | Apply VIP discount to a mixed invoice (consultation + drugs + glasses). Verify drugs are excluded from discount. Verify discount percentage matches tier configuration |

## Sources

- [Top 5 Challenges in Ophthalmology Practices 2025](https://ehnote.com/blog/ophthalmology-practice-challenges-2025) -- Ophthalmology-specific EHR challenges (MEDIUM confidence)
- [5 Signs Your Ophthalmology EHR Is a Time Thief](https://www.sightview.com/articles/5-signs-your-ophthalmology-ehr-is-a-time-thief-and-how-to-reclaim-your-day) -- Workflow inefficiency patterns (MEDIUM confidence)
- [Long-Term Financial and Clinical Impact of EHR on Ophthalmology Practice (PMC)](https://pmc.ncbi.nlm.nih.gov/articles/PMC4354962/) -- Productivity impact data (HIGH confidence, peer-reviewed)
- [EHR Usability in Ophthalmology (PMC)](https://pmc.ncbi.nlm.nih.gov/articles/PMC7587158/) -- Usability evaluation findings (HIGH confidence, peer-reviewed)
- [ICD-10-CM for Ophthalmology - AAO](https://www.aao.org/practice-management/coding/icd-10-cm) -- Laterality coding rules (HIGH confidence, official)
- [ICD-10 Specificity for Ophthalmology - Nextech](https://www.nextech.com/blog/getting-specific-icd-10-for-ophthalmology) -- Coding specificity requirements (MEDIUM confidence)
- [Influence of Ophthalmological EHR on ICD-10 Coding (BMC)](https://bmcmedinformdecismak.biomedcentral.com/articles/10.1186/s12911-016-0340-1) -- EHR impact on coding quality (HIGH confidence, peer-reviewed)
- [Immutable Audit Trails Guide](https://www.hubifi.com/blog/immutable-audit-log-basics) -- Audit trail implementation patterns (MEDIUM confidence)
- [Understanding EMR Audit Trails - Frier Levitt](https://www.frierlevitt.com/articles/understanding-emr-audit-trails-importance-and-implications-for-medical-record-alteration/) -- Legal implications (MEDIUM confidence)
- [Circular 26/2025/TT-BYT - Vietnam Electronic Prescriptions](https://lts.com.vn/updated-news/all-hospitals-and-medical-facilities-required-to-adopt-electronic-prescriptions-by-october-1-2025-lts-law/) -- Vietnamese regulatory requirements (MEDIUM confidence, secondary source)
- [Vietnam.vn - Electronic Prescription Requirements](https://www.vietnam.vn/en/tu-ngay-1-10-tat-ca-cac-benh-vien-bat-buoc-phai-thuc-hien-viec-ke-don-thuoc-bang-hinh-thuc-dien-tu) -- Implementation timeline (MEDIUM confidence, government news)
- [HCMC Prescription Linkage Statistics](https://www.vietnam.vn/en/tphcm-1-667-co-so-kham-chua-benh-chua-thuc-hien-lien-thong-don-thuoc) -- Private clinic compliance rates (MEDIUM confidence)
- [Dry Eye Module Software (PMC)](https://pmc.ncbi.nlm.nih.gov/articles/PMC10276731/) -- Standardized dry eye documentation (HIGH confidence, peer-reviewed)
- [Web-based Longitudinal Dry Eye Assessment (PubMed)](https://pubmed.ncbi.nlm.nih.gov/29409963/) -- Longitudinal tracking challenges (HIGH confidence, peer-reviewed)
- [Wolverine SQL Server Integration](https://wolverinefx.net/guide/durability/sqlserver) -- Outbox configuration (HIGH confidence, official docs)
- [Wolverine Outbox Issues - GitHub #689](https://github.com/JasperFx/wolverine/issues/689) -- Known message persistence issues (HIGH confidence, primary source)
- [Wolverine EF Core Outbox](https://wolverinefx.net/guide/durability/efcore/outbox-and-inbox) -- EF Core integration gotchas (HIGH confidence, official docs)
- [TanStack Start Selective SSR](https://tanstack.com/start/latest/docs/framework/react/guide/selective-ssr) -- SSR/SPA hybrid limitations (HIGH confidence, official docs)
- [RBAC Challenges in Healthcare (PMC)](https://pmc.ncbi.nlm.nih.gov/articles/PMC5836325/) -- Access control pitfalls (HIGH confidence, peer-reviewed)
- [Common RBAC Implementation Challenges - Censinet](https://censinet.com/perspectives/common-challenges-role-based-access-control-implementation) -- Role explosion and over-permissioning (MEDIUM confidence)
- [Azure Blob Storage Security Recommendations](https://learn.microsoft.com/en-us/azure/storage/blobs/security-recommendations) -- SAS token best practices (HIGH confidence, official Microsoft docs)
- [Pharmacy Inventory Management - TruMed](https://trumedsystems.com/blog/keeping-track-of-medications-overcoming-challenges-with-best-practices/) -- Expiry tracking patterns (MEDIUM confidence)
- [EF Core Multi-Context Schema Isolation](https://mehmetozkaya.medium.com/using-multi-context-ef-core-for-schema-isolation-in-modular-monoliths-38eda712628d) -- Schema-per-module migration patterns (MEDIUM confidence)
- [Vietnamese Internationalization](https://www.globalizationpartners.com/resources/vietnamese-internationalization/) -- Unicode and sorting considerations (MEDIUM confidence)
- [Zalo OA API Documentation](https://developers.zalo.me/docs/api/official-account-api-230) -- API structure and requirements (HIGH confidence, official)

---
*Pitfalls research for: Ophthalmology Clinic Management System (Ganka28)*
*Researched: 2026-02-28*

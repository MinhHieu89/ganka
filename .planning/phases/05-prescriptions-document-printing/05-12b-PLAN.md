---
phase: 05-prescriptions-document-printing
plan: 12b
type: execute
wave: 6
depends_on: ["05-11"]
files_modified:
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Services/DocumentService.cs
  - backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
autonomous: true
requirements:
  - PRT-01
  - PRT-02
  - PRT-04
  - PRT-05
  - PRT-06
must_haves:
  truths:
    - "DocumentService fully implements all 5 document generation methods"
    - "Print API endpoints exist for all document types"
    - "Print endpoints return PDF file responses with correct content type"
  artifacts:
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Services/DocumentService.cs"
      provides: "Complete DocumentService with all PDF generators"
      contains: "GenerateOpticalPrescriptionAsync|GenerateReferralLetterAsync"
    - path: "backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs"
      provides: "Print endpoints for all document types"
      contains: "print/drug-rx|print/optical-rx"
  key_links:
    - from: "ClinicalApiEndpoints.cs"
      to: "DocumentService.cs"
      via: "IDocumentService injection in print endpoints"
      pattern: "IDocumentService"
    - from: "DocumentService.cs"
      to: "All 5 document classes"
      via: "Instantiate and call GeneratePdf()"
      pattern: "new (Optical|Referral|Consent|PharmacyLabel|DrugPrescription).*Document"
---

<objective>
Complete DocumentService implementation and add print API endpoints to ClinicalApiEndpoints.

Purpose: Wires the document generation to the API layer. Completes all remaining DocumentService methods (removing NotImplementedException stubs) and adds HTTP endpoints that return PDF file responses. This plan can run in parallel with Plan 12a since it depends on the IDocument interface, not the concrete classes (it instantiates them but both compile independently).

Output: Complete DocumentService, print endpoints in ClinicalApiEndpoints
</objective>

<execution_context>
@C:/Users/minhh/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/minhh/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/phases/05-prescriptions-document-printing/05-CONTEXT.md
@.planning/phases/05-prescriptions-document-printing/05-11-SUMMARY.md
@.planning/phases/05-prescriptions-document-printing/05-12a-SUMMARY.md

@backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
@backend/src/Modules/Clinical/Clinical.Infrastructure/Services/DocumentService.cs

<interfaces>
From 05-11:
```csharp
public interface IDocumentService
{
    Task<byte[]> GenerateDrugPrescriptionAsync(Guid visitId, CancellationToken ct);
    Task<byte[]> GenerateOpticalPrescriptionAsync(Guid visitId, CancellationToken ct);
    Task<byte[]> GenerateReferralLetterAsync(Guid visitId, string referralReason, string referralTo, CancellationToken ct);
    Task<byte[]> GenerateConsentFormAsync(Guid visitId, string procedureType, CancellationToken ct);
    Task<byte[]> GeneratePharmacyLabelAsync(Guid prescriptionItemId, CancellationToken ct);
}
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Complete DocumentService and add print endpoints</name>
  <files>
    backend/src/Modules/Clinical/Clinical.Infrastructure/Services/DocumentService.cs,
    backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
  </files>
  <action>
**DocumentService.cs** -- complete remaining methods:
- GenerateOpticalPrescriptionAsync: load visit with optical prescription + patient data, build data record, generate PDF
- GenerateReferralLetterAsync: load visit + patient data, build data record with referralReason/referralTo params, generate PDF
- GenerateConsentFormAsync: load visit + patient data, build data record with procedureType param, generate PDF
- GeneratePharmacyLabelAsync: load prescription item by ID (via ClinicalDbContext direct query), build label data, generate PDF
- Remove all NotImplementedException stubs

**ClinicalApiEndpoints.cs** -- add print endpoint group:
```csharp
// In MapClinicalApiEndpoints or in MapPrescriptionEndpoints, add print routes:
group.MapGet("/{visitId:guid}/print/drug-rx", async (Guid visitId, IDocumentService docs, CancellationToken ct) =>
{
    var pdf = await docs.GenerateDrugPrescriptionAsync(visitId, ct);
    return Results.File(pdf, "application/pdf", $"drug-rx-{visitId}.pdf");
});

group.MapGet("/{visitId:guid}/print/optical-rx", async (Guid visitId, IDocumentService docs, CancellationToken ct) => ...);
group.MapGet("/{visitId:guid}/print/referral-letter", async (Guid visitId, [FromQuery] string reason, [FromQuery] string to, IDocumentService docs, CancellationToken ct) => ...);
group.MapGet("/{visitId:guid}/print/consent-form", async (Guid visitId, [FromQuery] string procedureType, IDocumentService docs, CancellationToken ct) => ...);
group.MapGet("/prescription-items/{itemId:guid}/print/label", async (Guid itemId, IDocumentService docs, CancellationToken ct) => ...);
```

All return `Results.File(pdfBytes, "application/pdf", filename)`.
  </action>
  <verify>
    <automated>dotnet build backend/src/Bootstrapper/Bootstrapper.csproj</automated>
  </verify>
  <done>DocumentService fully implemented -- no stubs remaining. Print endpoints added for all 5 document types returning PDF file responses.</done>
</task>

</tasks>

<verification>
- `dotnet build backend/src/Bootstrapper/Bootstrapper.csproj` passes
- DocumentService has no NotImplementedException stubs remaining
- Print endpoints added for all document types
</verification>

<success_criteria>
DocumentService complete with all 5 document generation methods. Print API endpoints wired and returning PDF files with correct content types.
</success_criteria>

<output>
After completion, create `.planning/phases/05-prescriptions-document-printing/05-12b-SUMMARY.md`
</output>

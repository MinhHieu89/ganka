using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Patient.Contracts.Dtos;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Clinical.Presentation;

/// <summary>
/// Clinical API endpoints for visit lifecycle, refraction, diagnosis, ICD-10,
/// dry eye assessment, and medical imaging management.
/// All endpoints require authorization and are grouped under /api/clinical.
/// </summary>
public static class ClinicalApiEndpoints
{
    public static IEndpointRouteBuilder MapClinicalApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clinical").RequireAuthorization();

        MapVisitLifecycleEndpoints(group);
        MapVisitDataEndpoints(group);
        MapIcd10Endpoints(group);
        MapDryEyeEndpoints(group);
        MapMedicalImageEndpoints(group);
        MapPrescriptionEndpoints(group);
        MapPrintEndpoints(group);

        return app;
    }

    private static void MapVisitLifecycleEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateVisitCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/clinical");
        });

        group.MapGet("/{visitId:guid}", async (Guid visitId, IMessageBus bus, CancellationToken ct) =>
        {
            var dto = await bus.InvokeAsync<VisitDetailDto?>(new GetVisitByIdQuery(visitId), ct);
            return dto is not null ? Results.Ok(dto) : Results.NotFound();
        });

        group.MapGet("/active", async (IMessageBus bus, CancellationToken ct) =>
        {
            var visits = await bus.InvokeAsync<List<ActiveVisitDto>>(new GetActiveVisitsQuery(), ct);
            return Results.Ok(visits);
        });

        group.MapPut("/{visitId:guid}/sign-off", async (Guid visitId, SignOffVisitCommand? command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new SignOffVisitCommand(visitId, command?.FieldChangesJson);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{visitId:guid}/cancel", async (Guid visitId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new CancelVisitCommand(visitId), ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{visitId:guid}/amend", async (Guid visitId, AmendVisitCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new AmendVisitCommand(visitId, command.Reason, command.FieldChangesJson);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapPut("/{visitId:guid}/stage", async (Guid visitId, AdvanceWorkflowStageCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new AdvanceWorkflowStageCommand(visitId, command.NewStage);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });
    }

    private static void MapVisitDataEndpoints(RouteGroupBuilder group)
    {
        group.MapPut("/{visitId:guid}/notes", async (Guid visitId, UpdateVisitNotesCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateVisitNotesCommand(visitId, command.Notes);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapPut("/{visitId:guid}/refraction", async (Guid visitId, UpdateRefractionCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateRefractionCommand(
                visitId, command.RefractionType,
                command.OdSph, command.OdCyl, command.OdAxis, command.OdAdd, command.OdPd,
                command.OsSph, command.OsCyl, command.OsAxis, command.OsAdd, command.OsPd,
                command.UcvaOd, command.UcvaOs, command.BcvaOd, command.BcvaOs,
                command.IopOd, command.IopOs, command.IopMethod,
                command.AxialLengthOd, command.AxialLengthOs);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{visitId:guid}/diagnoses", async (Guid visitId, AddVisitDiagnosisCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new AddVisitDiagnosisCommand(
                visitId, command.Icd10Code, command.DescriptionEn, command.DescriptionVi,
                command.Laterality, command.Role, command.SortOrder);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapDelete("/{visitId:guid}/diagnoses/{diagnosisId:guid}", async (Guid visitId, Guid diagnosisId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new RemoveVisitDiagnosisCommand(visitId, diagnosisId), ct);
            return result.ToHttpResult();
        });

        group.MapPut("/{visitId:guid}/diagnoses/{diagnosisId:guid}/set-primary",
            async (Guid visitId, Guid diagnosisId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(
                new SetPrimaryDiagnosisCommand(visitId, diagnosisId), ct);
            return result.ToHttpResult();
        });
    }

    private static void MapIcd10Endpoints(RouteGroupBuilder group)
    {
        group.MapGet("/icd10/search", async (string term, IMessageBus bus, HttpContext httpContext, CancellationToken ct) =>
        {
            var doctorId = httpContext.User.FindFirst("sub")?.Value;
            Guid? parsedDoctorId = Guid.TryParse(doctorId, out var id) ? id : null;
            var results = await bus.InvokeAsync<List<Icd10SearchResultDto>>(
                new SearchIcd10CodesQuery(term, parsedDoctorId), ct);
            return Results.Ok(results);
        });

        group.MapPost("/icd10/favorites/toggle", async (ToggleIcd10FavoriteCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(command, ct);
            return result.ToHttpResult();
        });

        group.MapGet("/icd10/favorites", async (Guid doctorId, IMessageBus bus, CancellationToken ct) =>
        {
            var results = await bus.InvokeAsync<List<Icd10SearchResultDto>>(
                new GetDoctorFavoritesQuery(doctorId), ct);
            return Results.Ok(results);
        });
    }

    private static void MapDryEyeEndpoints(RouteGroupBuilder group)
    {
        group.MapPut("/{visitId:guid}/dry-eye", async (Guid visitId, UpdateDryEyeAssessmentCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateDryEyeAssessmentCommand(
                visitId,
                command.OdTbut, command.OsTbut,
                command.OdSchirmer, command.OsSchirmer,
                command.OdMeibomianGrading, command.OsMeibomianGrading,
                command.OdTearMeniscus, command.OsTearMeniscus,
                command.OdStaining, command.OsStaining);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapGet("/osdi-history/{patientId:guid}", async (Guid patientId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<OsdiHistoryResponse>(new GetOsdiHistoryQuery(patientId), ct);
            return Results.Ok(result);
        });

        group.MapGet("/dry-eye-comparison", async (Guid patientId, Guid visitId1, Guid visitId2, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<DryEyeComparisonDto>>(
                new GetDryEyeComparisonQuery(patientId, visitId1, visitId2), ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{visitId:guid}/osdi-link", async (Guid visitId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<OsdiLinkResponse>>(
                new GenerateOsdiLinkCommand(visitId), ct);
            return result.ToHttpResult();
        });

        group.MapGet("/patients/{patientId:guid}/dry-eye/metric-history", async (Guid patientId, string? timeRange, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<DryEyeMetricHistoryResponse>(
                new GetDryEyeMetricHistoryQuery(patientId, timeRange ?? "all"), ct);
            return Results.Ok(result);
        });
    }

    private static void MapMedicalImageEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/{visitId:guid}/images", async (Guid visitId, IFormFile file, [AsParameters] ImageUploadParams uploadParams, IMessageBus bus, CancellationToken ct) =>
        {
            using var stream = file.OpenReadStream();
            var command = new UploadMedicalImageCommand(
                visitId, stream, file.FileName, file.ContentType, file.Length,
                uploadParams.ImageType, uploadParams.EyeTag);
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult($"/api/clinical/{visitId}/images");
        }).DisableAntiforgery();

        group.MapGet("/{visitId:guid}/images", async (Guid visitId, IMessageBus bus, CancellationToken ct) =>
        {
            var images = await bus.InvokeAsync<List<MedicalImageDto>>(
                new GetVisitImagesQuery(visitId), ct);
            return Results.Ok(images);
        });

        group.MapDelete("/images/{imageId:guid}", async (Guid imageId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new DeleteMedicalImageCommand(imageId), ct);
            return result.ToHttpResult();
        });

        group.MapGet("/image-comparison", async (Guid patientId, Guid visitId1, Guid visitId2, int imageType, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<ImageComparisonResponse>>(
                new GetImageComparisonQuery(patientId, visitId1, visitId2, imageType), ct);
            return result.ToHttpResult();
        });
    }
    private static void MapPrescriptionEndpoints(RouteGroupBuilder group)
    {
        // Drug prescription endpoints
        group.MapPost("/{visitId:guid}/drug-prescriptions", async (Guid visitId, AddDrugPrescriptionCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new AddDrugPrescriptionCommand(visitId, command.Notes, command.Items);
            var result = await bus.InvokeAsync<Result<Guid>>(enriched, ct);
            return result.ToCreatedHttpResult($"/api/clinical/{visitId}/drug-prescriptions");
        });

        group.MapPut("/{visitId:guid}/drug-prescriptions/{prescriptionId:guid}", async (Guid visitId, Guid prescriptionId, UpdateDrugPrescriptionCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateDrugPrescriptionCommand(visitId, prescriptionId, command.Notes);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapDelete("/{visitId:guid}/drug-prescriptions/{prescriptionId:guid}", async (Guid visitId, Guid prescriptionId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new RemoveDrugPrescriptionCommand(visitId, prescriptionId), ct);
            return result.ToHttpResult();
        });

        // Allergy check
        group.MapGet("/{visitId:guid}/check-drug-allergy", async ([AsParameters] CheckDrugAllergyParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var query = new CheckDrugAllergyQuery(p.PatientId, p.DrugName ?? "", p.GenericName);
            var matches = await bus.InvokeAsync<List<AllergyDto>>(query, ct);
            return Results.Ok(matches);
        });

        // Optical prescription endpoints
        group.MapPost("/{visitId:guid}/optical-prescription", async (Guid visitId, AddOpticalPrescriptionCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new AddOpticalPrescriptionCommand(
                visitId,
                command.OdSph, command.OdCyl, command.OdAxis, command.OdAdd,
                command.OsSph, command.OsCyl, command.OsAxis, command.OsAdd,
                command.FarPd, command.NearPd,
                command.NearOdSph, command.NearOdCyl, command.NearOdAxis,
                command.NearOsSph, command.NearOsCyl, command.NearOsAxis,
                command.LensType, command.Notes);
            var result = await bus.InvokeAsync<Result<Guid>>(enriched, ct);
            return result.ToCreatedHttpResult($"/api/clinical/{visitId}/optical-prescription");
        });

        group.MapPut("/{visitId:guid}/optical-prescription/{prescriptionId:guid}", async (Guid visitId, Guid prescriptionId, UpdateOpticalPrescriptionCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateOpticalPrescriptionCommand(
                visitId, prescriptionId,
                command.OdSph, command.OdCyl, command.OdAxis, command.OdAdd,
                command.OsSph, command.OsCyl, command.OsAxis, command.OsAdd,
                command.FarPd, command.NearPd,
                command.NearOdSph, command.NearOdCyl, command.NearOdAxis,
                command.NearOsSph, command.NearOsCyl, command.NearOsAxis,
                command.LensType, command.Notes);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });
    }

    private static void MapPrintEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/{visitId:guid}/print/drug-rx", async (Guid visitId, IDocumentService docs, CancellationToken ct) =>
        {
            var pdf = await docs.GenerateDrugPrescriptionAsync(visitId, ct);
            return Results.File(pdf, "application/pdf", $"drug-rx-{visitId}.pdf");
        });

        group.MapGet("/{visitId:guid}/print/optical-rx", async (Guid visitId, IDocumentService docs, CancellationToken ct) =>
        {
            var pdf = await docs.GenerateOpticalPrescriptionAsync(visitId, ct);
            return Results.File(pdf, "application/pdf", $"optical-rx-{visitId}.pdf");
        });

        group.MapGet("/{visitId:guid}/print/referral-letter", async (Guid visitId, string reason, string to, IDocumentService docs, CancellationToken ct) =>
        {
            var pdf = await docs.GenerateReferralLetterAsync(visitId, reason, to, ct);
            return Results.File(pdf, "application/pdf", $"referral-{visitId}.pdf");
        });

        group.MapGet("/{visitId:guid}/print/consent-form", async (Guid visitId, string procedureType, IDocumentService docs, CancellationToken ct) =>
        {
            var pdf = await docs.GenerateConsentFormAsync(visitId, procedureType, ct);
            return Results.File(pdf, "application/pdf", $"consent-{visitId}.pdf");
        });

        group.MapGet("/prescription-items/{itemId:guid}/print/label", async (Guid itemId, IDocumentService docs, CancellationToken ct) =>
        {
            var pdf = await docs.GeneratePharmacyLabelAsync(itemId, ct);
            return Results.File(pdf, "application/pdf", $"label-{itemId}.pdf");
        });

        group.MapGet("/prescriptions/{prescriptionId:guid}/labels/batch", async (Guid prescriptionId, IDocumentService docs, CancellationToken ct) =>
        {
            try
            {
                var pdf = await docs.GenerateBatchPharmacyLabelsAsync(prescriptionId, ct);
                return Results.File(pdf, "application/pdf", $"batch-labels-{prescriptionId}.pdf");
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });
    }
}

/// <summary>
/// Form data binding for image upload parameters.
/// </summary>
public class ImageUploadParams
{
    [Microsoft.AspNetCore.Mvc.FromForm(Name = "imageType")]
    public int ImageType { get; set; }

    [Microsoft.AspNetCore.Mvc.FromForm(Name = "eyeTag")]
    public int? EyeTag { get; set; }
}

/// <summary>
/// Query string binding for drug allergy check endpoint.
/// </summary>
public class CheckDrugAllergyParams
{
    public Guid PatientId { get; set; }
    public string? DrugName { get; set; }
    public string? GenericName { get; set; }
}

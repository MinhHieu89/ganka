using Clinical.Application.Interfaces;
using Clinical.Domain.Enums;
using Clinical.Infrastructure.Documents;
using Clinical.Infrastructure.Documents.Shared;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Shared.Application.Interfaces;

namespace Clinical.Infrastructure.Services;

/// <summary>
/// Service for generating clinical document PDFs.
/// Loads visit/patient data from the database and generates PDF bytes using QuestPDF.
/// Cross-module patient data is queried via raw SQL to avoid project reference coupling.
/// Clinic header data is pulled from IClinicSettingsService (configurable, not hardcoded).
/// </summary>
public sealed class DocumentService : IDocumentService
{
    private readonly ClinicalDbContext _clinicalDb;
    private readonly IClinicSettingsService _clinicSettingsService;

    public DocumentService(ClinicalDbContext clinicalDb, IClinicSettingsService clinicSettingsService)
    {
        _clinicalDb = clinicalDb;
        _clinicSettingsService = clinicSettingsService;

        // Ensure fonts are registered (idempotent, thread-safe)
        DocumentFontManager.RegisterFonts();
    }

    public async Task<byte[]> GenerateDrugPrescriptionAsync(Guid visitId, CancellationToken ct)
    {
        var visit = await _clinicalDb.Visits
            .AsNoTracking()
            .Include(v => v.Diagnoses)
            .FirstOrDefaultAsync(v => v.Id == visitId, ct)
            ?? throw new InvalidOperationException($"Visit {visitId} not found.");

        var prescription = await _clinicalDb.DrugPrescriptions
            .AsNoTracking()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.VisitId == visitId, ct)
            ?? throw new InvalidOperationException($"No drug prescription found for visit {visitId}.");

        var patientInfo = await GetPatientInfoAsync(visit.PatientId, ct);
        var headerData = await GetClinicHeaderDataAsync(ct);

        var diagnoses = visit.Diagnoses
            .OrderBy(d => d.SortOrder)
            .Select(d => !string.IsNullOrWhiteSpace(d.DescriptionVi) ? d.DescriptionVi : d.DescriptionEn)
            .ToList();

        var items = prescription.Items
            .OrderBy(i => i.SortOrder)
            .Select(i => new DrugPrescriptionItemData(
                i.SortOrder,
                i.DrugName,
                i.GenericName,
                i.Strength,
                i.Dosage,
                i.DosageOverride,
                i.Quantity,
                i.Unit))
            .ToList();

        var data = new DrugPrescriptionData(
            visit.PatientName,
            patientInfo?.PatientCode,
            patientInfo?.DateOfBirth,
            FormatGender(patientInfo?.Gender),
            patientInfo?.Address,
            patientInfo?.IdentityNumber,
            diagnoses,
            items,
            prescription.Notes,
            visit.DoctorName,
            prescription.PrescribedAt,
            prescription.PrescriptionCode);

        var document = new DrugPrescriptionDocument(data, headerData);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateOpticalPrescriptionAsync(Guid visitId, CancellationToken ct)
    {
        var visit = await _clinicalDb.Visits
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == visitId, ct)
            ?? throw new InvalidOperationException($"Visit {visitId} not found.");

        var opticalRx = await _clinicalDb.OpticalPrescriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.VisitId == visitId, ct)
            ?? throw new InvalidOperationException($"No optical prescription found for visit {visitId}.");

        var patientInfo = await GetPatientInfoAsync(visit.PatientId, ct);
        var headerData = await GetClinicHeaderDataAsync(ct);

        var lensTypeDisplay = opticalRx.LensType switch
        {
            LensType.SingleVision => "Tr\u00f2ng \u0111\u01a1n ti\u00eau (Single Vision)",
            LensType.Bifocal => "Tr\u00f2ng hai ti\u00eau (Bifocal)",
            LensType.Progressive => "Tr\u00f2ng l\u0169y ti\u1ebfn (Progressive)",
            LensType.Reading => "Tr\u00f2ng \u0111\u1ecdc s\u00e1ch (Reading)",
            _ => opticalRx.LensType.ToString()
        };

        var data = new OpticalPrescriptionData(
            visit.PatientName,
            patientInfo?.PatientCode,
            patientInfo?.DateOfBirth,
            FormatGender(patientInfo?.Gender),
            opticalRx.OdSph, opticalRx.OdCyl, opticalRx.OdAxis, opticalRx.OdAdd,
            opticalRx.OsSph, opticalRx.OsCyl, opticalRx.OsAxis, opticalRx.OsAdd,
            opticalRx.NearOdSph, opticalRx.NearOdCyl, opticalRx.NearOdAxis,
            opticalRx.NearOsSph, opticalRx.NearOsCyl, opticalRx.NearOsAxis,
            opticalRx.FarPd,
            opticalRx.NearPd,
            lensTypeDisplay,
            opticalRx.Notes,
            visit.DoctorName,
            opticalRx.PrescribedAt);

        var document = new OpticalPrescriptionDocument(data, headerData);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateReferralLetterAsync(Guid visitId, string referralReason, string referralTo, CancellationToken ct)
    {
        var visit = await _clinicalDb.Visits
            .AsNoTracking()
            .Include(v => v.Diagnoses)
            .FirstOrDefaultAsync(v => v.Id == visitId, ct)
            ?? throw new InvalidOperationException($"Visit {visitId} not found.");

        var patientInfo = await GetPatientInfoAsync(visit.PatientId, ct);
        var headerData = await GetClinicHeaderDataAsync(ct);

        var diagnoses = visit.Diagnoses
            .OrderBy(d => d.SortOrder)
            .Select(d => !string.IsNullOrWhiteSpace(d.DescriptionVi) ? d.DescriptionVi : d.DescriptionEn)
            .ToList();

        var data = new ReferralLetterData(
            visit.PatientName,
            patientInfo?.PatientCode,
            patientInfo?.DateOfBirth,
            FormatGender(patientInfo?.Gender),
            patientInfo?.Address,
            patientInfo?.IdentityNumber,
            diagnoses,
            visit.ExaminationNotes,
            referralReason,
            referralTo,
            visit.DoctorName,
            DateTime.UtcNow);

        var document = new ReferralLetterDocument(data, headerData);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateConsentFormAsync(Guid visitId, string procedureType, CancellationToken ct)
    {
        var visit = await _clinicalDb.Visits
            .AsNoTracking()
            .Include(v => v.Diagnoses)
            .FirstOrDefaultAsync(v => v.Id == visitId, ct)
            ?? throw new InvalidOperationException($"Visit {visitId} not found.");

        var patientInfo = await GetPatientInfoAsync(visit.PatientId, ct);
        var headerData = await GetClinicHeaderDataAsync(ct);

        var diagnoses = visit.Diagnoses
            .OrderBy(d => d.SortOrder)
            .Select(d => !string.IsNullOrWhiteSpace(d.DescriptionVi) ? d.DescriptionVi : d.DescriptionEn)
            .ToList();

        var data = new ConsentFormData(
            visit.PatientName,
            patientInfo?.PatientCode,
            patientInfo?.DateOfBirth,
            FormatGender(patientInfo?.Gender),
            patientInfo?.Address,
            patientInfo?.IdentityNumber,
            procedureType,
            diagnoses,
            visit.DoctorName,
            DateTime.UtcNow);

        var document = new ConsentFormDocument(data, headerData);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateBatchPharmacyLabelsAsync(Guid prescriptionId, CancellationToken ct)
    {
        // Run prescription and header queries concurrently
        var prescriptionTask = _clinicalDb.DrugPrescriptions
            .AsNoTracking()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == prescriptionId, ct);

        var headerTask = GetClinicHeaderDataAsync(ct);

        await Task.WhenAll(prescriptionTask, headerTask);

        var prescription = await prescriptionTask
            ?? throw new InvalidOperationException($"Drug prescription {prescriptionId} not found.");

        var visit = await _clinicalDb.Visits
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == prescription.VisitId, ct)
            ?? throw new InvalidOperationException($"Visit {prescription.VisitId} not found.");

        var headerData = await headerTask;
        var clinicName = headerData.ClinicNameVi ?? headerData.ClinicName;

        var labels = prescription.Items
            .OrderBy(i => i.SortOrder)
            .Select(item => new PharmacyLabelData(
                clinicName,
                visit.PatientName,
                item.DrugName,
                item.Strength,
                item.Dosage,
                item.DosageOverride,
                item.Quantity,
                item.Unit,
                prescription.PrescribedAt))
            .ToList();

        var document = new BatchPharmacyLabelDocument(labels);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GeneratePharmacyLabelAsync(Guid prescriptionItemId, CancellationToken ct)
    {
        var item = await _clinicalDb.PrescriptionItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == prescriptionItemId, ct)
            ?? throw new InvalidOperationException($"Prescription item {prescriptionItemId} not found.");

        var prescription = await _clinicalDb.DrugPrescriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == item.DrugPrescriptionId, ct)
            ?? throw new InvalidOperationException($"Drug prescription {item.DrugPrescriptionId} not found.");

        var visit = await _clinicalDb.Visits
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == prescription.VisitId, ct)
            ?? throw new InvalidOperationException($"Visit {prescription.VisitId} not found.");

        var headerData = await GetClinicHeaderDataAsync(ct);

        var data = new PharmacyLabelData(
            headerData.ClinicNameVi ?? headerData.ClinicName,
            visit.PatientName,
            item.DrugName,
            item.Strength,
            item.Dosage,
            item.DosageOverride,
            item.Quantity,
            item.Unit,
            prescription.PrescribedAt);

        var document = new PharmacyLabelDocument(data);
        return document.GeneratePdf();
    }

    /// <summary>
    /// Queries patient info from the patient schema using raw SQL.
    /// Avoids cross-module project references while accessing shared database.
    /// </summary>
    private async Task<PatientBasicInfo?> GetPatientInfoAsync(Guid patientId, CancellationToken ct)
    {
        var results = await _clinicalDb.Database
            .SqlQuery<PatientBasicInfo>(
                $"SELECT [DateOfBirth], [Gender], [Address], [Cccd] AS [IdentityNumber], [PatientCode] FROM [patient].[Patients] WHERE [Id] = {patientId}")
            .ToListAsync(ct);

        return results.FirstOrDefault();
    }

    /// <summary>
    /// Gets clinic header data from IClinicSettingsService.
    /// Returns sensible defaults if no clinic settings are configured yet.
    /// </summary>
    private async Task<ClinicHeaderData> GetClinicHeaderDataAsync(CancellationToken ct)
    {
        var settings = await _clinicSettingsService.GetCurrentAsync(ct);

        if (settings is null)
        {
            return new ClinicHeaderData(
                ClinicName: "GANKA Eye Clinic",
                ClinicNameVi: "PH\u00d2NG KH\u00c1M M\u1eaeT GANKA",
                Address: null,
                Phone: null,
                Fax: null,
                LicenseNumber: null,
                Tagline: null,
                LogoBytes: null);
        }

        return new ClinicHeaderData(
            ClinicName: settings.ClinicName,
            ClinicNameVi: settings.ClinicNameVi,
            Address: settings.Address,
            Phone: settings.Phone,
            Fax: settings.Fax,
            LicenseNumber: settings.LicenseNumber,
            Tagline: settings.Tagline,
            LogoBytes: null); // Logo bytes loaded separately if needed
    }

    /// <summary>
    /// Converts gender int value to Vietnamese display string.
    /// </summary>
    private static string? FormatGender(int? gender) => gender switch
    {
        0 => "Nam",
        1 => "N\u1eef",
        2 => "Kh\u00e1c",
        _ => null
    };

    /// <summary>
    /// Minimal patient data record for cross-schema SQL queries.
    /// Maps to patient.Patients table columns.
    /// </summary>
    private sealed record PatientBasicInfo(
        DateTime? DateOfBirth,
        int? Gender,
        string? Address,
        string? IdentityNumber,
        string? PatientCode);
}

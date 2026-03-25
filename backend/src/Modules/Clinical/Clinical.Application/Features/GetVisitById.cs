using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for getting a visit by ID with full details.
/// Returns VisitDetailDto with refractions, diagnoses, and amendments.
/// </summary>
public static class GetVisitByIdHandler
{
    public static async Task<VisitDetailDto?> Handle(
        GetVisitByIdQuery query,
        IVisitRepository visitRepository,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdWithDetailsAsync(query.VisitId, ct);
        if (visit is null)
            return null;

        return new VisitDetailDto(
            visit.Id,
            visit.PatientId,
            visit.PatientName,
            visit.DoctorId,
            visit.DoctorName,
            (int)visit.CurrentStage,
            (int)visit.Status,
            visit.VisitDate,
            visit.ExaminationNotes,
            visit.Refractions.Select(r => new RefractionDto(
                r.Id,
                (int)r.Type,
                r.OdSph, r.OdCyl, r.OdAxis, r.OdAdd, r.OdPd,
                r.OsSph, r.OsCyl, r.OsAxis, r.OsAdd, r.OsPd,
                r.UcvaOd, r.UcvaOs, r.BcvaOd, r.BcvaOs,
                r.IopOd, r.IopOs, r.IopMethod.HasValue ? (int)r.IopMethod.Value : null,
                r.AxialLengthOd, r.AxialLengthOs
            )).ToList(),
            visit.Diagnoses.Select(d => new VisitDiagnosisDto(
                d.Id,
                d.Icd10Code,
                d.DescriptionEn,
                d.DescriptionVi,
                (int)d.Laterality,
                (int)d.Role,
                d.SortOrder
            )).ToList(),
            visit.Amendments.Select(a => new VisitAmendmentDto(
                a.Id,
                a.AmendedByName,
                a.Reason,
                a.FieldChangesJson,
                a.AmendedAt
            )).ToList(),
            visit.DryEyeAssessments.Select(d => new DryEyeAssessmentDto(
                d.Id,
                d.VisitId,
                d.OdTbut, d.OsTbut,
                d.OdSchirmer, d.OsSchirmer,
                d.OdMeibomianGrading, d.OsMeibomianGrading,
                d.OdTearMeniscus, d.OsTearMeniscus,
                d.OdStaining, d.OsStaining,
                d.OsdiScore,
                d.OsdiSeverity.HasValue ? (int)d.OsdiSeverity.Value : null
            )).ToList(),
            visit.DrugPrescriptions.Select(dp => new DrugPrescriptionDto(
                dp.Id,
                dp.VisitId,
                dp.Notes,
                dp.PrescriptionCode,
                dp.PrescribedAt,
                dp.Items.Select(i => new PrescriptionItemDto(
                    i.Id,
                    i.DrugCatalogItemId,
                    i.DrugName,
                    i.GenericName,
                    i.Strength,
                    i.Form,
                    i.Route,
                    i.Dosage,
                    i.DosageOverride,
                    i.Quantity,
                    i.Unit,
                    i.Frequency,
                    i.DurationDays,
                    i.IsOffCatalog,
                    i.HasAllergyWarning,
                    i.SortOrder
                )).ToList()
            )).ToList(),
            visit.OpticalPrescriptions.Select(op => new OpticalPrescriptionDto(
                op.Id,
                op.VisitId,
                op.OdSph, op.OdCyl, op.OdAxis, op.OdAdd,
                op.OsSph, op.OsCyl, op.OsAxis, op.OsAdd,
                op.FarPd, op.NearPd,
                op.NearOdSph, op.NearOdCyl, op.NearOdAxis,
                op.NearOsSph, op.NearOsCyl, op.NearOsAxis,
                (int)op.LensType,
                op.Notes,
                op.PrescribedAt
            )).ToList(),
            visit.SignedAt,
            visit.SignedById,
            visit.AppointmentId,
            visit.ImagingRequested,
            visit.RefractionSkipped);
    }
}

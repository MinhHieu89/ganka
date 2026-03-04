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
            visit.SignedAt,
            visit.SignedById,
            visit.AppointmentId);
    }
}

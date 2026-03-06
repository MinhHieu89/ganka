using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Clinical.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IVisitRepository"/>.
/// Pure data access -- no business logic, no SaveChanges.
/// </summary>
public sealed class VisitRepository : IVisitRepository
{
    private readonly ClinicalDbContext _dbContext;

    public VisitRepository(ClinicalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Visit?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Visits.FindAsync([id], ct);
    }

    public async Task<Visit?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Visits
            .Include(v => v.Refractions)
            .Include(v => v.Diagnoses)
            .Include(v => v.DryEyeAssessments)
            .Include(v => v.Amendments)
            .Include(v => v.DrugPrescriptions)
                .ThenInclude(dp => dp.Items)
            .Include(v => v.OpticalPrescriptions)
            .FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task<List<Visit>> GetActiveVisitsAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);

        return await _dbContext.Visits
            .AsNoTracking()
            .Where(v => !v.IsDeleted &&
                (v.Status == VisitStatus.Draft ||
                 v.Status == VisitStatus.Amended ||
                 (v.Status == VisitStatus.Signed && v.VisitDate >= cutoff)))
            .OrderBy(v => v.VisitDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Visit visit, CancellationToken ct = default)
    {
        await _dbContext.Visits.AddAsync(visit, ct);
    }

    public async Task<bool> HasActiveVisitForPatientAsync(Guid patientId, CancellationToken ct = default)
    {
        return await _dbContext.Visits
            .AnyAsync(v => v.PatientId == patientId &&
                !v.IsDeleted &&
                (v.Status == VisitStatus.Draft || v.Status == VisitStatus.Amended), ct);
    }

    public void AddRefraction(Refraction refraction)
    {
        _dbContext.Refractions.Add(refraction);
    }

    public void AddDiagnosis(VisitDiagnosis diagnosis)
    {
        _dbContext.VisitDiagnoses.Add(diagnosis);
    }

    public void AddAmendment(VisitAmendment amendment)
    {
        _dbContext.VisitAmendments.Add(amendment);
    }

    public void AddDryEyeAssessment(DryEyeAssessment assessment)
    {
        _dbContext.DryEyeAssessments.Add(assessment);
    }

    public void AddDrugPrescription(DrugPrescription prescription)
    {
        _dbContext.DrugPrescriptions.Add(prescription);
    }

    public void AddPrescriptionItem(PrescriptionItem item)
    {
        _dbContext.PrescriptionItems.Add(item);
    }

    public void AddOpticalPrescription(OpticalPrescription prescription)
    {
        _dbContext.OpticalPrescriptions.Add(prescription);
    }

    public void RemoveOpticalPrescriptions(IEnumerable<OpticalPrescription> prescriptions)
    {
        _dbContext.OpticalPrescriptions.RemoveRange(prescriptions);
    }

    public async Task<List<DryEyeAssessment>> GetDryEyeAssessmentsByPatientAsync(Guid patientId, CancellationToken ct = default)
    {
        return await _dbContext.DryEyeAssessments
            .AsNoTracking()
            .Join(
                _dbContext.Visits,
                d => d.VisitId,
                v => v.Id,
                (d, v) => new { Assessment = d, Visit = v })
            .Where(x => x.Visit.PatientId == patientId && !x.Visit.IsDeleted)
            .OrderBy(x => x.Visit.VisitDate)
            .Select(x => x.Assessment)
            .ToListAsync(ct);
    }

    public async Task<DryEyeAssessment?> GetDryEyeAssessmentByVisitAsync(Guid visitId, CancellationToken ct = default)
    {
        return await _dbContext.DryEyeAssessments
            .FirstOrDefaultAsync(d => d.VisitId == visitId, ct);
    }

    public async Task<List<(DrugPrescription Prescription, Visit Visit)>> GetPrescriptionsWithVisitsAsync(
        Guid? patientId,
        CancellationToken ct = default)
    {
        var query = _dbContext.DrugPrescriptions
            .AsNoTracking()
            .Include(dp => dp.Items)
            .Join(
                _dbContext.Visits.AsNoTracking(),
                dp => dp.VisitId,
                v => v.Id,
                (dp, v) => new { Prescription = dp, Visit = v })
            .Where(x => !x.Visit.IsDeleted);

        if (patientId.HasValue)
            query = query.Where(x => x.Visit.PatientId == patientId.Value);

        var results = await query
            .OrderByDescending(x => x.Prescription.PrescribedAt)
            .ToListAsync(ct);

        return results.Select(x => (x.Prescription, x.Visit)).ToList();
    }
}

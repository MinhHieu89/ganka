using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IDispensingRepository"/>.
/// Provides eager loading for DispensingRecord aggregate including Lines and BatchDeductions.
/// </summary>
public sealed class DispensingRepository(PharmacyDbContext context) : IDispensingRepository
{
    public async Task<DispensingRecord?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.DispensingRecords
            .Include(r => r.Lines)
                .ThenInclude(l => l.BatchDeductions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<DispensingRecord?> GetByPrescriptionIdAsync(Guid prescriptionId, CancellationToken ct)
    {
        return await context.DispensingRecords
            .Include(r => r.Lines)
                .ThenInclude(l => l.BatchDeductions)
            .SingleOrDefaultAsync(r => r.PrescriptionId == prescriptionId, ct);
    }

    public async Task<(List<DispensingRecordDto> Items, int TotalCount)> GetHistoryAsync(
        int page,
        int pageSize,
        Guid? patientId,
        CancellationToken ct)
    {
        var query = context.DispensingRecords
            .AsNoTracking()
            .AsQueryable();

        if (patientId.HasValue)
        {
            query = query.Where(r => r.PatientId == patientId.Value);
        }

        query = query.OrderByDescending(r => r.DispensedAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(r => r.Lines)
                .ThenInclude(l => l.BatchDeductions)
            .Select(r => new DispensingRecordDto(
                r.Id,
                r.PrescriptionId,
                r.VisitId,
                r.PatientId,
                r.PatientName,
                r.DispensedAt,
                r.OverrideReason,
                r.Lines.Select(l => new DispensingLineDto(
                    l.Id,
                    l.DrugCatalogItemId,
                    l.DrugName,
                    l.Quantity,
                    (int)l.Status,
                    l.BatchDeductions.Select(bd => new BatchDeductionDto(
                        bd.Id,
                        bd.DrugBatchId,
                        bd.BatchNumber,
                        bd.Quantity)).ToList())).ToList()))
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public void Add(DispensingRecord record)
    {
        context.DispensingRecords.Add(record);
    }
}

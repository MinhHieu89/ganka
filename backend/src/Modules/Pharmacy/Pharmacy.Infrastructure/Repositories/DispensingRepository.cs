using Clinical.Contracts.Dtos;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;
using Wolverine;

namespace Pharmacy.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IDispensingRepository"/>.
/// Provides eager loading for DispensingRecord aggregate including Lines and BatchDeductions.
/// GetPendingPrescriptionsAsync delegates to Clinical module via IMessageBus cross-module query.
/// </summary>
public sealed class DispensingRepository(PharmacyDbContext context, IMessageBus bus) : IDispensingRepository
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

    public async Task<List<PendingPrescriptionDto>> GetPendingPrescriptionsAsync(
        Guid? patientId,
        CancellationToken ct)
    {
        // Cross-module: retrieve all prescriptions from Clinical module via IMessageBus
        var clinicalPrescriptions = await bus.InvokeAsync<List<ClinicalPendingPrescriptionDto>>(
            new GetPendingPrescriptionsQuery(patientId), ct);

        if (clinicalPrescriptions is null || clinicalPrescriptions.Count == 0)
            return [];

        // Get IDs of prescriptions already dispensed in this module
        var prescriptionIds = clinicalPrescriptions.Select(p => p.PrescriptionId).ToList();
        var dispensedIds = await context.DispensingRecords
            .AsNoTracking()
            .Where(r => prescriptionIds.Contains(r.PrescriptionId))
            .Select(r => r.PrescriptionId)
            .ToListAsync(ct);

        var dispensedSet = new HashSet<Guid>(dispensedIds);

        // Filter out already-dispensed prescriptions and map to Pharmacy.Contracts DTO
        return clinicalPrescriptions
            .Where(p => !dispensedSet.Contains(p.PrescriptionId))
            .Select(p => new PendingPrescriptionDto(
                PrescriptionId: p.PrescriptionId,
                VisitId: p.VisitId,
                PatientId: p.PatientId,
                PatientName: p.PatientName,
                PrescribedAt: p.PrescribedAt,
                IsExpired: p.IsExpired,
                DaysRemaining: p.DaysRemaining,
                Items: p.Items.Select(item => new PendingPrescriptionItemDto(
                    PrescriptionItemId: item.PrescriptionItemId,
                    DrugCatalogItemId: item.DrugCatalogItemId,
                    DrugName: item.DrugName,
                    Quantity: item.Quantity,
                    Unit: item.Unit,
                    Dosage: item.Dosage,
                    IsOffCatalog: item.IsOffCatalog)).ToList()))
            .ToList();
    }
}

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

        var records = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(r => r.Lines)
                .ThenInclude(l => l.BatchDeductions)
            .ToListAsync(ct);

        // Batch-load units for all drug IDs in one query
        var drugIds = records
            .SelectMany(r => r.Lines)
            .Select(l => l.DrugCatalogItemId)
            .Distinct()
            .ToList();

        var unitMap = drugIds.Count > 0
            ? await context.DrugCatalogItems
                .AsNoTracking()
                .Where(d => drugIds.Contains(d.Id))
                .Select(d => new { d.Id, d.Unit })
                .ToDictionaryAsync(d => d.Id, d => d.Unit, ct)
            : new Dictionary<Guid, string>();

        var items = records.Select(r => new DispensingRecordDto(
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
                unitMap.GetValueOrDefault(l.DrugCatalogItemId, ""),
                l.Quantity,
                (int)l.Status,
                l.BatchDeductions.Select(bd => new BatchDeductionDto(
                    bd.Id,
                    bd.DrugBatchId,
                    bd.BatchNumber,
                    bd.Quantity)).ToList())).ToList())).ToList();

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
                PrescriptionCode: p.PrescriptionCode,
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

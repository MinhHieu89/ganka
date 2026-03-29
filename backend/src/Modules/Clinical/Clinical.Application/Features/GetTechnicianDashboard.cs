using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Enums;
using Patient.Application.Interfaces;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for the technician dashboard query.
/// Queries TechnicianOrders for today, joins with Visit for display fields,
/// derives status per D-08 and visit type per D-10.
/// </summary>
public static class GetTechnicianDashboardHandler
{
    public static async Task<TechnicianDashboardDto> Handle(
        GetTechnicianDashboardQuery query,
        ITechnicianOrderQueryService queryService,
        IPatientRepository patientRepository,
        CancellationToken ct)
    {
        var searchTerm = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        var rawData = await queryService.GetTodayOrdersWithVisitsAsync(searchTerm, ct);

        // Get visit counts per patient for visit type detection
        var patientIds = rawData.Select(x => x.PatientId).Distinct().ToList();
        var visitCountsByPatient = await queryService.GetVisitCountsByPatientIdsAsync(patientIds, ct);

        // Get patient details for PatientCode and BirthYear
        var patients = await patientRepository.GetByIdsAsync(patientIds, ct);
        var patientLookup = patients.ToDictionary(p => p.Id);

        var rows = rawData.Select(x =>
        {
            var status = DeriveStatus(x.TechnicianId, x.TechnicianName, x.CompletedAt,
                x.IsRedFlag, query.CurrentTechnicianId);
            var visitType = DeriveVisitType(x.OrderType, x.PatientId, visitCountsByPatient);
            var waitMinutes = (int)(DateTime.UtcNow - x.OrderedAt).TotalMinutes;
            patientLookup.TryGetValue(x.PatientId, out var pat);

            return new TechnicianDashboardRowDto(
                OrderId: x.OrderId,
                VisitId: x.VisitId,
                PatientId: x.PatientId,
                PatientName: x.PatientName,
                PatientCode: pat?.PatientCode,
                BirthYear: pat?.DateOfBirth?.Year,
                CheckinTime: x.VisitDate,
                WaitMinutes: waitMinutes,
                Reason: x.Reason,
                VisitType: visitType,
                Status: status,
                TechnicianName: x.TechnicianName,
                RedFlagReason: x.RedFlagReason,
                IsRedFlag: x.IsRedFlag);
        }).ToList();

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            rows = rows.Where(r => r.Status == query.Status).ToList();
        }

        var totalCount = rows.Count;

        // Sort: in_progress first (pinned), then by checkin time ASC (FIFO)
        rows = rows
            .OrderBy(r => r.Status == "in_progress" ? 0 : 1)
            .ThenBy(r => r.CheckinTime)
            .ToList();

        // Pagination
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, Math.Min(query.PageSize, 100));
        var paged = rows.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new TechnicianDashboardDto(paged, totalCount);
    }

    /// <summary>
    /// Derives technician display status per D-08.
    /// </summary>
    internal static string DeriveStatus(
        Guid? technicianId, string? technicianName, DateTime? completedAt,
        bool isRedFlag, Guid? currentTechnicianId)
    {
        if (isRedFlag)
            return "red_flag";
        if (completedAt.HasValue)
            return "completed";
        if (technicianId.HasValue && technicianId == currentTechnicianId)
            return "in_progress";
        if (!technicianId.HasValue)
            return "waiting";
        // Accepted by another technician -- show as waiting to current user
        return "waiting";
    }

    /// <summary>
    /// Derives visit type per D-10.
    /// </summary>
    private static string DeriveVisitType(
        TechnicianOrderType orderType,
        Guid patientId,
        Dictionary<Guid, int> visitCountsByPatient)
    {
        if (orderType == TechnicianOrderType.AdditionalExam)
            return "additional";

        var visitCount = visitCountsByPatient.GetValueOrDefault(patientId, 1);
        return visitCount > 1 ? "follow_up" : "new";
    }
}

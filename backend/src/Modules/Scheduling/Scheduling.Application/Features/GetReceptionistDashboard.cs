using Clinical.Application.Interfaces;
using Clinical.Domain.Enums;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Scheduling.Contracts.Queries;
using Scheduling.Domain.Enums;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Wolverine handler for the receptionist dashboard query.
/// Joins today's appointments + visits and maps to 4 receptionist statuses.
/// </summary>
public static class GetReceptionistDashboardHandler
{
    public static async Task<Result<ReceptionistDashboardDto>> Handle(
        GetReceptionistDashboardQuery query,
        IAppointmentRepository appointmentRepository,
        IVisitRepository visitRepository,
        Patient.Application.Interfaces.IPatientRepository patientRepository,
        CancellationToken ct)
    {
        // Load today's appointments and visits (filtered at DB level when search is provided)
        var searchTerm = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        var appointments = await appointmentRepository.GetTodayAppointmentsAsync(searchTerm, ct);
        var visits = await visitRepository.GetTodayVisitsAsync(searchTerm, ct);

        // Load patient data for birth year and patient code
        var patientIds = appointments
            .Where(a => a.PatientId.HasValue)
            .Select(a => a.PatientId!.Value)
            .Union(visits.Select(v => v.PatientId))
            .Distinct()
            .ToList();
        var patients = await patientRepository.GetByIdsAsync(patientIds, ct);
        var patientLookup = patients.ToDictionary(p => p.Id);

        // Build lookup: appointmentId -> visit
        var visitByAppointment = visits
            .Where(v => v.AppointmentId.HasValue)
            .ToDictionary(v => v.AppointmentId!.Value);

        var rows = new List<ReceptionistDashboardRowDto>();

        // Process appointments (may have linked visits)
        foreach (var apt in appointments)
        {
            var hasVisit = visitByAppointment.TryGetValue(apt.Id, out var visit);
            var status = MapStatus(apt, visit);

            if (status == null) continue;

            var pat = apt.PatientId.HasValue && patientLookup.TryGetValue(apt.PatientId.Value, out var p) ? p : null;

            rows.Add(new ReceptionistDashboardRowDto(
                Id: hasVisit ? visit!.Id : apt.Id,
                AppointmentId: apt.Id,
                VisitId: hasVisit ? visit!.Id : null,
                PatientId: apt.PatientId,
                PatientName: apt.PatientName,
                PatientCode: pat?.PatientCode,
                BirthYear: pat?.DateOfBirth?.Year,
                AppointmentTime: apt.StartTime,
                Source: "appointment",
                Reason: visit?.Reason ?? apt.GuestReason ?? apt.Notes,
                Status: status,
                CheckedInAt: apt.CheckedInAt,
                IsGuestBooking: apt.PatientId == null,
                GuestPhone: apt.GuestPhone));
        }

        // Process walk-in visits (no appointment)
        var walkInVisits = visits.Where(v => v.AppointmentId == null || !appointments.Any(a => a.Id == v.AppointmentId));
        foreach (var visit in walkInVisits)
        {
            var status = MapVisitStatus(visit);
            if (status == null) continue;

            var wiPat = patientLookup.TryGetValue(visit.PatientId, out var wp) ? wp : null;

            rows.Add(new ReceptionistDashboardRowDto(
                Id: visit.Id,
                AppointmentId: null,
                VisitId: visit.Id,
                PatientId: visit.PatientId,
                PatientName: visit.PatientName,
                PatientCode: wiPat?.PatientCode,
                BirthYear: wiPat?.DateOfBirth?.Year,
                AppointmentTime: null,
                Source: "walkin",
                Reason: visit.Reason,
                Status: status,
                CheckedInAt: visit.VisitDate,
                IsGuestBooking: false,
                GuestPhone: null));
        }

        // Apply filters
        if (!string.IsNullOrEmpty(query.StatusFilter))
            rows = rows.Where(r => r.Status == query.StatusFilter).ToList();

        // Sort: appointments by time ascending, walk-ins at bottom by created time
        rows = rows
            .OrderBy(r => r.AppointmentTime == null ? 1 : 0)
            .ThenBy(r => r.AppointmentTime ?? DateTime.MaxValue)
            .ToList();

        var totalCount = rows.Count;
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, Math.Min(query.PageSize, 100));
        var paged = rows.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result<ReceptionistDashboardDto>.Success(
            new ReceptionistDashboardDto(paged, totalCount, page, pageSize));
    }

    private static string? MapStatus(Domain.Entities.Appointment apt, Clinical.Domain.Entities.Visit? visit)
    {
        if (apt.Status == AppointmentStatus.Cancelled || apt.Status == AppointmentStatus.NoShow)
        {
            if (visit != null)
                return MapVisitStatus(visit);
            return apt.Status == AppointmentStatus.Cancelled ? "cancelled" : null;
        }

        if (visit == null)
            return "not_arrived";

        return MapVisitStatus(visit);
    }

    private static string? MapVisitStatus(Clinical.Domain.Entities.Visit visit)
    {
        if (visit.Status == VisitStatus.Cancelled)
            return "cancelled";

        if (visit.Status == VisitStatus.Signed || visit.CurrentStage == WorkflowStage.Done)
            return "completed";

        if (visit.CurrentStage >= WorkflowStage.PreExam)
            return "examining";

        if (visit.CurrentStage == WorkflowStage.Reception)
            return "waiting";

        return "waiting";
    }
}

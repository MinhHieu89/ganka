using Clinical.Domain.Enums;

namespace Clinical.Application.Interfaces;

/// <summary>
/// Read-side query service for technician order data.
/// Bypasses the repository pattern for CQRS read-side queries
/// that need complex joins across entities.
/// </summary>
public interface ITechnicianOrderQueryService
{
    /// <summary>
    /// Gets today's technician orders joined with visit data.
    /// Optionally filtered by patient name search.
    /// </summary>
    Task<List<TechnicianOrderWithVisitData>> GetTodayOrdersWithVisitsAsync(
        string? searchTerm, CancellationToken ct = default);

    /// <summary>
    /// Gets today's technician order summaries for KPI calculation.
    /// </summary>
    Task<List<TechnicianOrderSummary>> GetTodayOrderSummariesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets visit counts per patient for visit type derivation.
    /// </summary>
    Task<Dictionary<Guid, int>> GetVisitCountsByPatientIdsAsync(
        List<Guid> patientIds, CancellationToken ct = default);
}

/// <summary>
/// Projection of TechnicianOrder joined with Visit for dashboard display.
/// </summary>
public record TechnicianOrderWithVisitData(
    Guid OrderId,
    Guid VisitId,
    Guid PatientId,
    string PatientName,
    DateTime VisitDate,
    string? Reason,
    TechnicianOrderType OrderType,
    Guid? TechnicianId,
    string? TechnicianName,
    DateTime? CompletedAt,
    bool IsRedFlag,
    string? RedFlagReason,
    DateTime OrderedAt);

/// <summary>
/// Lightweight projection of TechnicianOrder for KPI aggregation.
/// </summary>
public record TechnicianOrderSummary(
    Guid? TechnicianId,
    DateTime? CompletedAt,
    bool IsRedFlag);

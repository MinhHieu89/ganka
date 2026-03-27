using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Command to create a walk-in visit for an existing patient (no appointment).
/// Full handler implementation provided by plan 14-02.
/// </summary>
public record CreateWalkInVisitCommand(Guid PatientId, Guid DoctorId, string? Reason);

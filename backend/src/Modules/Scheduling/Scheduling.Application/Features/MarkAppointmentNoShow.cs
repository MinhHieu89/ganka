using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Command to mark an appointment as no-show.
/// Full handler implementation provided by plan 14-02.
/// </summary>
public record MarkAppointmentNoShowCommand(Guid AppointmentId, Guid UserId, string? Notes);

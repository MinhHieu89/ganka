using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Command to check in an appointment. Creates a Visit in Reception stage.
/// Full handler implementation provided by plan 14-02.
/// </summary>
public record CheckInAppointmentCommand(Guid AppointmentId, Guid UserId);

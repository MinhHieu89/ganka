using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Command to cancel a visit with a mandatory reason (receptionist action).
/// Full handler implementation provided by plan 14-02.
/// </summary>
public record CancelVisitWithReasonCommand(Guid VisitId, string Reason, Guid CancelledBy);

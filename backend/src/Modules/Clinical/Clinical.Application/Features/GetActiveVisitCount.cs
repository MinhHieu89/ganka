using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler returning the count of active visits (Draft/Amended or recently Signed).
/// Used by the Patient module dashboard via IMessageBus cross-module query.
/// </summary>
public static class GetActiveVisitCountHandler
{
    public static async Task<int> Handle(
        GetActiveVisitCountQuery query,
        IVisitRepository visitRepository,
        CancellationToken ct)
    {
        var activeVisits = await visitRepository.GetActiveVisitsAsync(ct);
        return activeVisits.Count;
    }
}

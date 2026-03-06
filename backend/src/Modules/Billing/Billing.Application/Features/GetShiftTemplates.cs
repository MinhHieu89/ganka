using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Query to retrieve all active shift templates for the current user's branch.
/// No parameters needed -- uses ICurrentUser for BranchId.
/// </summary>
public sealed record GetShiftTemplatesQuery();

/// <summary>
/// Wolverine static handler for retrieving active shift templates.
/// Queries ICashierShiftRepository for active templates by branch.
/// </summary>
public static class GetShiftTemplatesHandler
{
    public static async Task<Result<List<ShiftTemplateDto>>> Handle(
        GetShiftTemplatesQuery query,
        ICashierShiftRepository shiftRepository,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var branchId = new BranchId(currentUser.BranchId);

        var templates = await shiftRepository.GetActiveShiftTemplatesAsync(branchId, ct);

        var dtos = templates.Select(t => new ShiftTemplateDto(
            Id: t.Id,
            Name: t.Name,
            NameVi: t.NameVi,
            DefaultStartTime: t.DefaultStartTime.ToString("HH:mm"),
            DefaultEndTime: t.DefaultEndTime.ToString("HH:mm"),
            IsActive: t.IsActive)).ToList();

        return dtos;
    }
}

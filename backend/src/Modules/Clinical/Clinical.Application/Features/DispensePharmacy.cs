using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for dispensing pharmacy drugs for a visit.
/// Creates a PharmacyDispensing record with line items.
/// Completes the drug track and advances to Done if all tracks complete.
/// </summary>
public static class DispensePharmacyHandler
{
    public static async Task<Result> Handle(
        DispensePharmacyCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        if (visit.DrugTrackStatus != TrackStatus.Pending)
            return Result.Failure(Error.Validation("Drug track is not pending for this visit."));

        try
        {
            var items = command.DispensedItems
                .Select(i => (i.DrugName, i.Quantity, i.Instruction))
                .ToList();

            var dispensing = PharmacyDispensing.Create(
                visit.Id,
                currentUser.UserId,
                currentUser.Email,
                items,
                command.Note);

            visit.AddPharmacyDispensing(dispensing);
            visit.CompleteDrugTrack();

            if (visit.IsComplete)
            {
                visit.AdvanceStage(WorkflowStage.Done);
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

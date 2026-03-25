using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for confirming visit payment at Cashier.
/// Creates a VisitPayment record. Checks if drugs/glasses were prescribed
/// and activates post-payment tracks accordingly. If neither track,
/// advances directly to Done.
/// </summary>
public static class ConfirmVisitPaymentHandler
{
    public static async Task<Result> Handle(
        ConfirmVisitPaymentCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdWithDetailsAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        if (visit.CurrentStage != WorkflowStage.Cashier)
            return Result.Failure(Error.Validation("Payment can only be confirmed at Cashier stage."));

        try
        {
            var paymentMethod = (PaymentMethod)command.PaymentMethod;
            var changeGiven = command.AmountReceived - command.Amount;

            var payment = VisitPayment.Create(
                visit.Id,
                PaymentType.Visit,
                command.Amount,
                paymentMethod,
                command.AmountReceived,
                changeGiven > 0 ? changeGiven : 0,
                currentUser.UserId,
                currentUser.Email);

            visit.AddVisitPayment(payment);

            var hasDrugs = visit.DrugPrescriptions.Any();
            var hasGlasses = visit.OpticalPrescriptions.Any();

            visit.ActivatePostPaymentTracks(hasDrugs, hasGlasses);

            if (!hasDrugs && !hasGlasses)
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

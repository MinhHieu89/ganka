using FluentValidation;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Scheduling.Domain.Enums;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Validator for <see cref="CancelAppointmentCommand"/>.
/// </summary>
public class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage("Appointment ID is required.");
        RuleFor(x => x.CancellationReason)
            .Must(r => Enum.IsDefined(typeof(CancellationReason), r))
            .WithMessage("Invalid cancellation reason.");
    }
}

/// <summary>
/// Wolverine handler for cancelling an appointment.
/// </summary>
public static class CancelAppointmentHandler
{
    public static async Task<Result> Handle(
        CancelAppointmentCommand command,
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork,
        IValidator<CancelAppointmentCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(Error.Validation(errors));
        }

        var appointment = await appointmentRepository.GetByIdAsync(command.AppointmentId, ct);
        if (appointment is null)
            return Result.Failure(Error.NotFound("Appointment", command.AppointmentId));

        try
        {
            appointment.Cancel((CancellationReason)command.CancellationReason, command.CancellationNote);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

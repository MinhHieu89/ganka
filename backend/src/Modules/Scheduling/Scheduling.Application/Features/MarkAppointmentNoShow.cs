using FluentValidation;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Validator for <see cref="MarkAppointmentNoShowCommand"/>.
/// </summary>
public class MarkAppointmentNoShowCommandValidator : AbstractValidator<MarkAppointmentNoShowCommand>
{
    public MarkAppointmentNoShowCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage("Appointment ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
    }
}

/// <summary>
/// Wolverine handler for marking an appointment as no-show.
/// </summary>
public static class MarkAppointmentNoShowHandler
{
    public static async Task<Result> Handle(
        MarkAppointmentNoShowCommand command,
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork,
        IValidator<MarkAppointmentNoShowCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure(Error.ValidationWithDetails(errors));
        }

        var appointment = await appointmentRepository.GetByIdAsync(command.AppointmentId, ct);
        if (appointment is null)
            return Result.Failure(Error.NotFound("Appointment", command.AppointmentId));

        try
        {
            appointment.MarkNoShow(command.UserId, command.Notes);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

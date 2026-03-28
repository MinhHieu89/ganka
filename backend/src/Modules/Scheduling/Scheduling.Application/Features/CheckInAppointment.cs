using FluentValidation;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Validator for <see cref="CheckInAppointmentCommand"/>.
/// </summary>
public class CheckInAppointmentCommandValidator : AbstractValidator<CheckInAppointmentCommand>
{
    public CheckInAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage("Appointment ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
    }
}

/// <summary>
/// Wolverine handler for checking in an appointment.
/// Marks appointment as checked-in; Visit creation is handled by Clinical module
/// via the AppointmentCheckedInIntegrationEvent domain event cascade.
/// </summary>
public static class CheckInAppointmentHandler
{
    public static async Task<Result<Guid>> Handle(
        CheckInAppointmentCommand command,
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork,
        IValidator<CheckInAppointmentCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<Guid>.Failure(Error.ValidationWithDetails(errors));
        }

        var appointment = await appointmentRepository.GetByIdAsync(command.AppointmentId, ct);
        if (appointment is null)
            return Result<Guid>.Failure(Error.NotFound("Appointment", command.AppointmentId));

        try
        {
            appointment.CheckIn();
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(command.AppointmentId);
    }
}

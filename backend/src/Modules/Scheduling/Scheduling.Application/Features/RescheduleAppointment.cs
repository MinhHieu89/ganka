using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Validator for <see cref="RescheduleAppointmentCommand"/>.
/// </summary>
public class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage("Appointment ID is required.");
        RuleFor(x => x.NewStartTime).GreaterThan(DateTime.UtcNow).WithMessage("New start time must be in the future.");
    }
}

/// <summary>
/// Wolverine handler for rescheduling an appointment.
/// </summary>
public static class RescheduleAppointmentHandler
{
    public static async Task<Result> Handle(
        RescheduleAppointmentCommand command,
        IAppointmentRepository appointmentRepository,
        IClinicScheduleRepository clinicScheduleRepository,
        IUnitOfWork unitOfWork,
        IValidator<RescheduleAppointmentCommand> validator,
        ILogger<RescheduleAppointmentCommand> logger,
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

        // Load appointment type for duration
        var appointmentType = await appointmentRepository.GetAppointmentTypeAsync(appointment.AppointmentTypeId, ct);
        if (appointmentType is null)
            return Result.Failure(Error.Validation("Appointment type not found."));

        var newEndTime = command.NewStartTime.AddMinutes(appointmentType.DefaultDurationMinutes);

        // Validate clinic schedule for new time
        var schedule = await clinicScheduleRepository.GetForDayAsync(command.NewStartTime.DayOfWeek, ct);
        if (schedule is null || !schedule.IsWithinHours(command.NewStartTime.TimeOfDay, newEndTime.TimeOfDay))
            return Result.Failure(Error.Validation("New appointment time is outside clinic operating hours."));

        // Check overlap (exclude current appointment)
        var hasOverlap = await appointmentRepository.HasOverlappingAsync(
            appointment.DoctorId, command.NewStartTime, newEndTime, command.AppointmentId, ct);
        if (hasOverlap)
            return Result.Failure(Error.Conflict("This time slot is already taken for the selected doctor."));

        try
        {
            appointment.Reschedule(command.NewStartTime, newEndTime);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            logger.LogWarning("Double-booking detected by DB constraint during reschedule for appointment {AppointmentId}",
                command.AppointmentId);
            return Result.Failure(Error.Conflict("This time slot was just taken. Please select a different time."));
        }

        return Result.Success();
    }
}

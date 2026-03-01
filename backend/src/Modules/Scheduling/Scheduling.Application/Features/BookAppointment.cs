using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Scheduling.Domain.Entities;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Validator for <see cref="BookAppointmentCommand"/>.
/// </summary>
public class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("Patient is required.");
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage("Doctor is required.");
        RuleFor(x => x.StartTime).GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");
        RuleFor(x => x.AppointmentTypeId).NotEmpty().WithMessage("Appointment type is required.");
        RuleFor(x => x.PatientName).NotEmpty().WithMessage("Patient name is required.");
        RuleFor(x => x.DoctorName).NotEmpty().WithMessage("Doctor name is required.");
    }
}

/// <summary>
/// Wolverine handler for booking a new appointment.
/// Validates clinic hours, checks for overlaps, and creates the appointment.
/// </summary>
public static class BookAppointmentHandler
{
    public static async Task<Result<Guid>> Handle(
        BookAppointmentCommand command,
        IAppointmentRepository appointmentRepository,
        IClinicScheduleRepository clinicScheduleRepository,
        IUnitOfWork unitOfWork,
        IValidator<BookAppointmentCommand> validator,
        ILogger<BookAppointmentCommand> logger,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result<Guid>.Failure(Error.Validation(errors));
        }

        // Load appointment type for duration
        var appointmentType = await appointmentRepository.GetAppointmentTypeAsync(command.AppointmentTypeId, ct);
        if (appointmentType is null)
            return Result<Guid>.Failure(Error.Validation("Invalid appointment type."));

        var endTime = command.StartTime.AddMinutes(appointmentType.DefaultDurationMinutes);

        // Validate clinic schedule
        var schedule = await clinicScheduleRepository.GetForDayAsync(command.StartTime.DayOfWeek, ct);
        if (schedule is null || !schedule.IsWithinHours(command.StartTime.TimeOfDay, endTime.TimeOfDay))
            return Result<Guid>.Failure(Error.Validation("Appointment time is outside clinic operating hours."));

        // Check overlap
        var hasOverlap = await appointmentRepository.HasOverlappingAsync(
            command.DoctorId, command.StartTime, endTime, ct: ct);
        if (hasOverlap)
            return Result<Guid>.Failure(Error.Conflict("This time slot is already taken for the selected doctor."));

        // Use default branch
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var appointment = Appointment.Create(
            command.PatientId,
            command.PatientName,
            command.DoctorId,
            command.DoctorName,
            command.StartTime,
            endTime,
            command.AppointmentTypeId,
            branchId,
            command.Notes);

        appointmentRepository.Add(appointment);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            logger.LogWarning("Double-booking detected by DB constraint for doctor {DoctorId} at {StartTime}",
                command.DoctorId, command.StartTime);
            return Result<Guid>.Failure(Error.Conflict("This time slot was just taken. Please select a different time."));
        }

        return appointment.Id;
    }
}

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Validator for <see cref="BookGuestAppointmentCommand"/>.
/// </summary>
public class BookGuestAppointmentCommandValidator : AbstractValidator<BookGuestAppointmentCommand>
{
    public BookGuestAppointmentCommandValidator()
    {
        RuleFor(x => x.GuestName).NotEmpty().WithMessage("Guest name is required.");
        RuleFor(x => x.GuestPhone)
            .NotEmpty().WithMessage("Guest phone is required.")
            .Matches(@"^0\d{9,10}$").WithMessage("Phone must be a valid Vietnamese phone number.");
        RuleFor(x => x.StartTime).GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");
    }
}

/// <summary>
/// Wolverine handler for booking a guest appointment (D-11).
/// Creates appointment with GuestName/GuestPhone, no PatientId.
/// If DoctorId provided, checks for doctor overlap (D-12).
/// </summary>
public static class BookGuestAppointmentHandler
{
    public static async Task<Result<Guid>> Handle(
        BookGuestAppointmentCommand command,
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork,
        IValidator<BookGuestAppointmentCommand> validator,
        ILogger<BookGuestAppointmentCommand> logger,
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

        var endTime = command.StartTime.AddMinutes(30); // D-10: hardcoded 30-min slots

        // D-12: If DoctorId provided, check for overlap. If null ("BS nao trong"), skip.
        if (command.DoctorId.HasValue)
        {
            var hasOverlap = await appointmentRepository.HasOverlappingAsync(
                command.DoctorId.Value, command.StartTime, endTime, ct: ct);
            if (hasOverlap)
                return Result<Guid>.Failure(Error.Conflict("This time slot is already taken for the selected doctor."));
        }

        // Use default branch and a default appointment type ID for guest bookings
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var defaultAppointmentTypeId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var source = (AppointmentSource)command.Source;
        var doctorId = command.DoctorId ?? Guid.Empty;
        var doctorName = command.DoctorName ?? string.Empty;

        var appointment = Appointment.CreateGuest(
            command.GuestName,
            command.GuestPhone,
            command.GuestReason,
            doctorId,
            doctorName,
            command.StartTime,
            endTime,
            defaultAppointmentTypeId,
            branchId,
            source);

        appointmentRepository.Add(appointment);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            logger.LogWarning("Double-booking detected for guest appointment at {StartTime}", command.StartTime);
            return Result<Guid>.Failure(Error.Conflict("This time slot was just taken. Please select a different time."));
        }

        return appointment.Id;
    }
}

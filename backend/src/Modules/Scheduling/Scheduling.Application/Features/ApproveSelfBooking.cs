using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Scheduling.Domain.Entities;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Validator for <see cref="ApproveSelfBookingCommand"/>.
/// </summary>
public class ApproveSelfBookingCommandValidator : AbstractValidator<ApproveSelfBookingCommand>
{
    public ApproveSelfBookingCommandValidator()
    {
        RuleFor(x => x.SelfBookingRequestId).NotEmpty().WithMessage("Self-booking request ID is required.");
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage("Doctor is required.");
        RuleFor(x => x.DoctorName).NotEmpty().WithMessage("Doctor name is required.");
        RuleFor(x => x.PatientName).NotEmpty().WithMessage("Patient name is required.");
        RuleFor(x => x.StartTime).GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");
    }
}

/// <summary>
/// Wolverine handler for approving a self-booking request.
/// Creates an actual Appointment when approved.
/// </summary>
public static class ApproveSelfBookingHandler
{
    public static async Task<Result<Guid>> Handle(
        ApproveSelfBookingCommand command,
        ISelfBookingRepository selfBookingRepository,
        IAppointmentRepository appointmentRepository,
        IClinicScheduleRepository clinicScheduleRepository,
        IUnitOfWork unitOfWork,
        IValidator<ApproveSelfBookingCommand> validator,
        ILogger<ApproveSelfBookingCommand> logger,
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

        var request = await selfBookingRepository.GetByIdAsync(command.SelfBookingRequestId, ct);
        if (request is null)
            return Result<Guid>.Failure(Error.NotFound("SelfBookingRequest", command.SelfBookingRequestId));

        if (request.Status != Domain.Enums.BookingStatus.Pending)
            return Result<Guid>.Failure(Error.Validation("Only pending booking requests can be approved."));

        // Load appointment type for duration
        var appointmentType = await appointmentRepository.GetAppointmentTypeAsync(request.AppointmentTypeId, ct);
        if (appointmentType is null)
            return Result<Guid>.Failure(Error.Validation("Appointment type not found."));

        var endTime = command.StartTime.AddMinutes(appointmentType.DefaultDurationMinutes);

        // Convert UTC to Vietnam local time for schedule validation
        var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(command.StartTime, DateTimeKind.Utc), vietnamTz);
        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(endTime, DateTimeKind.Utc), vietnamTz);

        // Validate clinic schedule using Vietnam local time
        var schedule = await clinicScheduleRepository.GetForDayAsync(localStart.DayOfWeek, ct);
        if (schedule is null || !schedule.IsWithinHours(localStart.TimeOfDay, localEnd.TimeOfDay))
            return Result<Guid>.Failure(Error.Validation("Appointment time is outside clinic operating hours."));

        // Check overlap
        var hasOverlap = await appointmentRepository.HasOverlappingAsync(
            command.DoctorId, command.StartTime, endTime, ct: ct);
        if (hasOverlap)
            return Result<Guid>.Failure(Error.Conflict("This time slot is already taken for the selected doctor."));

        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var appointment = Appointment.Create(
            Guid.Empty, // PatientId not yet linked; will be matched later
            command.PatientName,
            command.DoctorId,
            command.DoctorName,
            command.StartTime,
            endTime,
            request.AppointmentTypeId,
            branchId);

        appointmentRepository.Add(appointment);
        request.Approve(appointment.Id);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            logger.LogWarning("Double-booking detected by DB constraint during self-booking approval for request {RequestId}",
                command.SelfBookingRequestId);
            return Result<Guid>.Failure(Error.Conflict("This time slot was just taken. Please select a different time."));
        }

        return appointment.Id;
    }
}

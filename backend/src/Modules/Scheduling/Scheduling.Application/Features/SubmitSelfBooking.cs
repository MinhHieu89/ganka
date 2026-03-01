using FluentValidation;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Scheduling.Domain.Entities;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Validator for <see cref="SubmitSelfBookingCommand"/>.
/// </summary>
public class SubmitSelfBookingCommandValidator : AbstractValidator<SubmitSelfBookingCommand>
{
    public SubmitSelfBookingCommandValidator()
    {
        RuleFor(x => x.PatientName)
            .NotEmpty().WithMessage("Patient name is required.")
            .MaximumLength(200).WithMessage("Patient name must not exceed 200 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^(0|\+84)\d{9,10}$").WithMessage("Invalid Vietnamese phone number format.");

        RuleFor(x => x.PreferredDate)
            .GreaterThan(DateTime.UtcNow.Date).WithMessage("Preferred date must be in the future.");

        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty().WithMessage("Appointment type is required.");
    }
}

/// <summary>
/// Wolverine handler for submitting a self-booking request (public, no auth).
/// </summary>
public static class SubmitSelfBookingHandler
{
    public static async Task<Result<string>> Handle(
        SubmitSelfBookingCommand command,
        ISelfBookingRepository selfBookingRepository,
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork,
        IValidator<SubmitSelfBookingCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result<string>.Failure(Error.Validation(errors));
        }

        // Validate appointment type exists
        var appointmentType = await appointmentRepository.GetAppointmentTypeAsync(command.AppointmentTypeId, ct);
        if (appointmentType is null)
            return Result<string>.Failure(Error.Validation("Invalid appointment type."));

        // Rate limit: max 2 pending bookings per phone
        var pendingCount = await selfBookingRepository.CountPendingByPhoneAsync(command.Phone, ct);
        if (pendingCount >= 2)
            return Result<string>.Failure(Error.Validation("Maximum pending bookings reached. Please wait for your existing requests to be processed."));

        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var request = SelfBookingRequest.Create(
            command.PatientName,
            command.Phone,
            command.Email,
            command.PreferredDoctorId,
            command.PreferredDate,
            command.PreferredTimeSlot,
            command.AppointmentTypeId,
            command.Notes,
            branchId);

        selfBookingRepository.Add(request);
        await unitOfWork.SaveChangesAsync(ct);

        return request.ReferenceNumber;
    }
}

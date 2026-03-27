using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
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
/// Marks appointment as checked-in and creates a Visit at Reception stage.
/// </summary>
public static class CheckInAppointmentHandler
{
    public static async Task<Result<Guid>> Handle(
        CheckInAppointmentCommand command,
        IAppointmentRepository appointmentRepository,
        Clinical.Application.Interfaces.IVisitRepository visitRepository,
        Patient.Application.Interfaces.IPatientRepository patientRepository,
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

        // Load patient data for visit creation
        var hasAllergies = false;
        var patientName = appointment.PatientName;
        var patientId = appointment.PatientId ?? Guid.Empty;

        if (appointment.PatientId.HasValue)
        {
            var patient = await patientRepository.GetByIdAsync(appointment.PatientId.Value, ct);
            if (patient is not null)
            {
                hasAllergies = patient.Allergies.Count > 0;
                patientName = patient.FullName;
            }
        }

        // Use default branch
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var visit = Visit.Create(
            patientId,
            patientName,
            appointment.DoctorId,
            appointment.DoctorName,
            branchId,
            hasAllergies,
            appointment.Id,
            source: VisitSource.Appointment);

        await visitRepository.AddAsync(visit, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return visit.Id;
    }
}

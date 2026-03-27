using Scheduling.Contracts.Dtos;

namespace Scheduling.Application.Features;

/// <summary>
/// Query to get available 30-min time slots for a given date and optional doctor.
/// Full handler implementation provided by plan 14-02.
/// </summary>
public record GetAvailableSlotsQuery(DateTime Date, Guid? DoctorId);

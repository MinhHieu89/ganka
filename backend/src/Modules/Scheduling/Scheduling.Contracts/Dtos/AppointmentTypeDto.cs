namespace Scheduling.Contracts.Dtos;

/// <summary>
/// DTO for appointment type reference data.
/// </summary>
public record AppointmentTypeDto(
    Guid Id,
    string Name,
    string NameVi,
    int DefaultDurationMinutes,
    string Color);

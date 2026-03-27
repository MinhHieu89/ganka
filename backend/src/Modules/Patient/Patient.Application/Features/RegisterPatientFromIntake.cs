using Shared.Domain;

namespace Patient.Application.Features;

/// <summary>
/// Command to register a new patient from the receptionist intake form.
/// Includes all standard + intake-specific fields.
/// Full handler implementation provided by plan 14-02.
/// </summary>
public record RegisterPatientFromIntakeCommand(
    string FullName,
    string Phone,
    DateTime DateOfBirth,
    int Gender,
    string? Address,
    string? Cccd,
    string? Email,
    string? Occupation,
    string? OcularHistory,
    string? SystemicHistory,
    string? CurrentMedications,
    decimal? ScreenTimeHours,
    int? WorkEnvironment,
    int? ContactLensUsage,
    string? LifestyleNotes);

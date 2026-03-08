namespace Treatment.Domain.ValueObjects;

/// <summary>
/// Structured clinical parameters for an IPL (Intense Pulsed Light) treatment session.
/// Captures the specific device settings and treatment zones used during the procedure.
/// Serialized as JSON when stored on protocol templates (defaults) and session records (actuals).
/// </summary>
/// <param name="EnergyJoules">Energy fluence in J/cm2. Typical range: 10.0 - 16.0 J/cm2.</param>
/// <param name="PulseCount">Number of light pulses delivered per treatment zone. Typical range: 3 - 5.</param>
/// <param name="SpotSize">Diameter of the treatment handpiece aperture (e.g., "8mm", "10mm").</param>
/// <param name="TreatmentZones">Anatomical areas treated (e.g., "Upper lid", "Lower lid", "Periorbital").</param>
public sealed record IplParameters(
    decimal EnergyJoules,
    int PulseCount,
    string SpotSize,
    string[] TreatmentZones);

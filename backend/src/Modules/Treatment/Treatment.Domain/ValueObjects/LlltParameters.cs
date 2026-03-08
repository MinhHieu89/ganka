namespace Treatment.Domain.ValueObjects;

/// <summary>
/// Structured clinical parameters for an LLLT (Low-Level Light Therapy) treatment session.
/// Captures the photobiomodulation device settings used to reduce inflammation
/// and stimulate cellular repair in meibomian gland dysfunction.
/// Serialized as JSON when stored on protocol templates (defaults) and session records (actuals).
/// </summary>
/// <param name="WavelengthNm">Light wavelength in nanometers (e.g., 810 nm for near-infrared).</param>
/// <param name="PowerMw">Output power in milliwatts (e.g., 100 mW).</param>
/// <param name="DurationMinutes">Treatment exposure duration in minutes (e.g., 15 minutes).</param>
/// <param name="TreatmentArea">Anatomical area targeted (e.g., "Bilateral eyelids").</param>
public sealed record LlltParameters(
    decimal WavelengthNm,
    decimal PowerMw,
    decimal DurationMinutes,
    string TreatmentArea);

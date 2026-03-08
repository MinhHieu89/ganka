namespace Treatment.Domain.ValueObjects;

/// <summary>
/// Structured clinical parameters for a Lid Care treatment session.
/// Captures the eyelid hygiene procedure steps, products used, and total duration
/// for blepharitis and lid margin disease management.
/// Serialized as JSON when stored on protocol templates (defaults) and session records (actuals).
/// </summary>
/// <param name="ProcedureSteps">Ordered checklist of procedure steps (e.g., "Warm compress", "Lid margin cleaning", "Expression of meibomian glands").</param>
/// <param name="ProductsUsed">Products applied during the procedure (e.g., "BlephEx pad", "Tea tree oil wipes").</param>
/// <param name="DurationMinutes">Total procedure duration in minutes.</param>
public sealed record LidCareParameters(
    string[] ProcedureSteps,
    string[] ProductsUsed,
    decimal DurationMinutes);

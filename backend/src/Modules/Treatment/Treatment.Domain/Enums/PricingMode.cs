namespace Treatment.Domain.Enums;

/// <summary>
/// Pricing model for a treatment offering.
/// Determines whether the patient pays per individual session or for a bundled package of sessions.
/// </summary>
public enum PricingMode
{
    /// <summary>Per Session — patient pays for each treatment session individually</summary>
    PerSession = 0,

    /// <summary>Per Package — patient pays a fixed price for a bundle of sessions (e.g., 4-session package)</summary>
    PerPackage = 1
}

namespace Treatment.Domain.Enums;

/// <summary>
/// Type of treatment offered by the clinic.
/// Each type corresponds to a distinct clinical protocol with specific equipment and session parameters.
/// </summary>
public enum TreatmentType
{
    /// <summary>IPL (Intense Pulsed Light) — light-based therapy for dry eye and meibomian gland dysfunction</summary>
    IPL = 0,

    /// <summary>LLLT (Low-Level Light Therapy) — photobiomodulation therapy to reduce inflammation and stimulate cellular repair</summary>
    LLLT = 1,

    /// <summary>LidCare — eyelid hygiene and maintenance treatment for blepharitis and lid margin disease</summary>
    LidCare = 2
}

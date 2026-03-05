namespace Pharmacy.Domain.Enums;

/// <summary>
/// Route of drug administration.
/// Includes ophthalmic-specific routes (Subconjunctival, Intravitreal, Periocular).
/// </summary>
public enum DrugRoute
{
    /// <summary>Topical application (eye drops, ointment)</summary>
    Topical = 0,

    /// <summary>Oral administration (tablets, capsules)</summary>
    Oral = 1,

    /// <summary>Intramuscular injection (IM)</summary>
    Intramuscular = 2,

    /// <summary>Intravenous injection (IV)</summary>
    Intravenous = 3,

    /// <summary>Subconjunctival injection</summary>
    Subconjunctival = 4,

    /// <summary>Intravitreal injection</summary>
    Intravitreal = 5,

    /// <summary>Periocular injection</summary>
    Periocular = 6
}

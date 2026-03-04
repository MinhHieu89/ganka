namespace Clinical.Domain.Enums;

/// <summary>
/// Eye laterality for ICD-10 diagnosis.
/// No Unspecified value -- .9 codes are blocked per ophthalmology best practice.
/// </summary>
public enum Laterality
{
    /// <summary>Right eye (Oculus Dexter)</summary>
    OD = 0,
    /// <summary>Left eye (Oculus Sinister)</summary>
    OS = 1,
    /// <summary>Both eyes (Oculus Uterque)</summary>
    OU = 2
}

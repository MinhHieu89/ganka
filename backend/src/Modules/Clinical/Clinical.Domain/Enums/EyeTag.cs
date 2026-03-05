namespace Clinical.Domain.Enums;

/// <summary>
/// Tag indicating which eye(s) an image or measurement pertains to.
/// Optional for images that are not eye-specific (e.g., face photos).
/// </summary>
public enum EyeTag
{
    OD = 0,
    OS = 1,
    OU = 2
}

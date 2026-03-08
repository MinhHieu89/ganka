namespace Optical.Domain.Enums;

/// <summary>
/// Target gender category for an eyeglass frame.
/// Used to filter and present frames by intended audience in the catalog.
/// </summary>
public enum FrameGender
{
    /// <summary>Male (Nam) — frames designed for men</summary>
    Male = 0,

    /// <summary>Female (Nữ) — frames designed for women</summary>
    Female = 1,

    /// <summary>Unisex (Unisex) — frames suitable for all genders</summary>
    Unisex = 2
}

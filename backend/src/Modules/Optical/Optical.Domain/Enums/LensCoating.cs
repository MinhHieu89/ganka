namespace Optical.Domain.Enums;

/// <summary>
/// Coating options applied to a lens. A lens may have multiple coatings simultaneously.
/// Uses [Flags] to allow bitwise combination of coating values.
/// </summary>
[Flags]
public enum LensCoating
{
    /// <summary>No coating applied</summary>
    None = 0,

    /// <summary>Anti-reflective coating — reduces glare and reflections (Chống phản chiếu)</summary>
    AntiReflective = 1,

    /// <summary>Blue light cut coating — filters harmful blue light from screens (Chống ánh sáng xanh)</summary>
    BlueCut = 2,

    /// <summary>Photochromic coating — darkens in sunlight, clears indoors (Đổi màu)</summary>
    Photochromic = 4,

    /// <summary>Scratch-resistant coating — hardened surface for durability (Chống trầy xước)</summary>
    ScratchResistant = 8,

    /// <summary>UV protection coating — blocks ultraviolet radiation (Chống tia UV)</summary>
    UVProtection = 16
}

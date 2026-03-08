namespace Optical.Domain.Enums;

/// <summary>
/// Material composition of a lens catalog item.
/// Determines optical quality, weight, and impact resistance characteristics.
/// </summary>
public enum LensMaterial
{
    /// <summary>CR-39 plastic — standard lightweight optical plastic (Nhựa CR-39)</summary>
    CR39 = 0,

    /// <summary>Polycarbonate — impact-resistant, lightweight material (Nhựa polycarbonate)</summary>
    Polycarbonate = 1,

    /// <summary>High-index plastic — thinner lenses for strong prescriptions (Kính chỉ số cao)</summary>
    HiIndex = 2,

    /// <summary>Trivex — lightweight, impact-resistant with better optics than polycarbonate (Nhựa Trivex)</summary>
    Trivex = 3
}

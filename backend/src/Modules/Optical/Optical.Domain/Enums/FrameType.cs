namespace Optical.Domain.Enums;

/// <summary>
/// Structural type of an eyeglass frame indicating how the lens is held.
/// Affects lens edging requirements and the aesthetic of the finished glasses.
/// </summary>
public enum FrameType
{
    /// <summary>Full Rim (Gọng đầy đủ) — lens is fully enclosed by the frame; most durable</summary>
    FullRim = 0,

    /// <summary>Semi-Rimless (Nửa gọng) — only the upper half of the lens is enclosed; lighter appearance</summary>
    SemiRimless = 1,

    /// <summary>Rimless (Không gọng) — lens held only by nose bridge and temples; minimal, lightweight style</summary>
    Rimless = 2
}

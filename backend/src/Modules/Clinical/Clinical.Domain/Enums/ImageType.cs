namespace Clinical.Domain.Enums;

/// <summary>
/// Type of medical image captured during a clinical visit.
/// Used to categorize images for same-type cross-visit comparison.
/// </summary>
public enum ImageType
{
    Fluorescein = 0,
    Meibography = 1,
    OCT = 2,
    SpecularMicroscopy = 3,
    Topography = 4,
    Video = 5
}

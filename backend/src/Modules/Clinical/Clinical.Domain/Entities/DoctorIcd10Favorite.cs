using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Per-doctor ICD-10 favorite junction table.
/// Each doctor has their own set of pinned ICD-10 codes for quick access.
/// This is NOT the global Icd10Code.IsFavorite field -- that is ignored.
/// </summary>
public class DoctorIcd10Favorite : Entity
{
    public Guid DoctorId { get; private set; }
    public string Icd10Code { get; private set; } = string.Empty;

    private DoctorIcd10Favorite() { }

    /// <summary>
    /// Factory method for creating a per-doctor ICD-10 favorite.
    /// </summary>
    public static DoctorIcd10Favorite Create(Guid doctorId, string icd10Code)
    {
        return new DoctorIcd10Favorite
        {
            DoctorId = doctorId,
            Icd10Code = icd10Code
        };
    }
}

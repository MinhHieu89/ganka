using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Dry eye assessment data for a visit. Stores per-eye measurements (TBUT, Schirmer,
/// Meibomian grading, Tear meniscus, Staining) and patient-level OSDI score.
/// Follows Refraction pattern with per-eye flat columns (OdTbut/OsTbut).
/// This is a Visit child entity subject to EnsureEditable guard.
/// </summary>
public class DryEyeAssessment : Entity
{
    public Guid VisitId { get; private set; }

    // Tear Break-Up Time per eye (seconds)
    public decimal? OdTbut { get; private set; }
    public decimal? OsTbut { get; private set; }

    // Schirmer test per eye (mm)
    public decimal? OdSchirmer { get; private set; }
    public decimal? OsSchirmer { get; private set; }

    // Meibomian gland grading per eye (0-3 Arita scale)
    public int? OdMeibomianGrading { get; private set; }
    public int? OsMeibomianGrading { get; private set; }

    // Tear meniscus height per eye (mm)
    public decimal? OdTearMeniscus { get; private set; }
    public decimal? OsTearMeniscus { get; private set; }

    // Staining score per eye (Oxford 0-5)
    public int? OdStaining { get; private set; }
    public int? OsStaining { get; private set; }

    // OSDI score is patient-level, NOT per-eye (patient-reported symptom score)
    public decimal? OsdiScore { get; private set; }
    public OsdiSeverity? OsdiSeverity { get; private set; }

    private DryEyeAssessment() { }

    /// <summary>
    /// Factory method for creating an empty dry eye assessment for a visit.
    /// </summary>
    public static DryEyeAssessment Create(Guid visitId)
    {
        return new DryEyeAssessment
        {
            VisitId = visitId
        };
    }

    /// <summary>
    /// Updates all per-eye dry eye measurement fields at once.
    /// </summary>
    public void Update(
        decimal? odTbut, decimal? osTbut,
        decimal? odSchirmer, decimal? osSchirmer,
        int? odMeibomianGrading, int? osMeibomianGrading,
        decimal? odTearMeniscus, decimal? osTearMeniscus,
        int? odStaining, int? osStaining)
    {
        OdTbut = odTbut;
        OsTbut = osTbut;
        OdSchirmer = odSchirmer;
        OsSchirmer = osSchirmer;
        OdMeibomianGrading = odMeibomianGrading;
        OsMeibomianGrading = osMeibomianGrading;
        OdTearMeniscus = odTearMeniscus;
        OsTearMeniscus = osTearMeniscus;
        OdStaining = odStaining;
        OsStaining = osStaining;

        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the OSDI score and its calculated severity classification.
    /// Called separately from Update because OSDI can come from patient self-fill.
    /// </summary>
    public void SetOsdiScore(decimal score, OsdiSeverity severity)
    {
        OsdiScore = score;
        OsdiSeverity = severity;

        SetUpdatedAt();
    }
}

using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Optical prescription (glasses Rx) as a Visit child entity.
/// Stores OD/OS refraction parameters for distance and near vision,
/// far/near PD, lens type recommendation, and doctor notes.
/// Only one optical prescription per visit (enforced by Visit.SetOpticalPrescription).
/// </summary>
public class OpticalPrescription : Entity
{
    public Guid VisitId { get; private set; }

    // OD (right eye) distance Rx
    public decimal? OdSph { get; private set; }
    public decimal? OdCyl { get; private set; }
    public int? OdAxis { get; private set; }
    public decimal? OdAdd { get; private set; }

    // OS (left eye) distance Rx
    public decimal? OsSph { get; private set; }
    public decimal? OsCyl { get; private set; }
    public int? OsAxis { get; private set; }
    public decimal? OsAdd { get; private set; }

    // Interpupillary distance
    public decimal? FarPd { get; private set; }
    public decimal? NearPd { get; private set; }

    // Near Rx override fields (for bifocal/progressive where near differs from distance+ADD)
    public decimal? NearOdSph { get; private set; }
    public decimal? NearOdCyl { get; private set; }
    public int? NearOdAxis { get; private set; }
    public decimal? NearOsSph { get; private set; }
    public decimal? NearOsCyl { get; private set; }
    public int? NearOsAxis { get; private set; }

    // Metadata
    public LensType LensType { get; private set; }
    public string? Notes { get; private set; }
    public DateTime PrescribedAt { get; private set; }

    private OpticalPrescription() { }

    /// <summary>
    /// Creates a new optical prescription for a visit.
    /// </summary>
    public static OpticalPrescription Create(
        Guid visitId,
        LensType lensType,
        decimal? odSph, decimal? odCyl, int? odAxis, decimal? odAdd,
        decimal? osSph, decimal? osCyl, int? osAxis, decimal? osAdd,
        decimal? farPd, decimal? nearPd,
        decimal? nearOdSph, decimal? nearOdCyl, int? nearOdAxis,
        decimal? nearOsSph, decimal? nearOsCyl, int? nearOsAxis,
        string? notes)
    {
        return new OpticalPrescription
        {
            VisitId = visitId,
            LensType = lensType,
            OdSph = odSph,
            OdCyl = odCyl,
            OdAxis = odAxis,
            OdAdd = odAdd,
            OsSph = osSph,
            OsCyl = osCyl,
            OsAxis = osAxis,
            OsAdd = osAdd,
            FarPd = farPd,
            NearPd = nearPd,
            NearOdSph = nearOdSph,
            NearOdCyl = nearOdCyl,
            NearOdAxis = nearOdAxis,
            NearOsSph = nearOsSph,
            NearOsCyl = nearOsCyl,
            NearOsAxis = nearOsAxis,
            Notes = notes,
            PrescribedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates all optical prescription values.
    /// </summary>
    public void Update(
        LensType lensType,
        decimal? odSph, decimal? odCyl, int? odAxis, decimal? odAdd,
        decimal? osSph, decimal? osCyl, int? osAxis, decimal? osAdd,
        decimal? farPd, decimal? nearPd,
        decimal? nearOdSph, decimal? nearOdCyl, int? nearOdAxis,
        decimal? nearOsSph, decimal? nearOsCyl, int? nearOsAxis,
        string? notes)
    {
        LensType = lensType;
        OdSph = odSph;
        OdCyl = odCyl;
        OdAxis = odAxis;
        OdAdd = odAdd;
        OsSph = osSph;
        OsCyl = osCyl;
        OsAxis = osAxis;
        OsAdd = osAdd;
        FarPd = farPd;
        NearPd = nearPd;
        NearOdSph = nearOdSph;
        NearOdCyl = nearOdCyl;
        NearOdAxis = nearOdAxis;
        NearOsSph = nearOsSph;
        NearOsCyl = nearOsCyl;
        NearOsAxis = nearOsAxis;
        Notes = notes;
        SetUpdatedAt();
    }
}

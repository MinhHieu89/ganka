using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Refraction data for a visit. Stores per-eye measurements for SPH, CYL, AXIS, ADD, PD,
/// visual acuity (UCVA/BCVA), IOP with method, and axial length.
/// One refraction record per type (Manifest, Autorefraction, Cycloplegic) per visit.
/// </summary>
public class Refraction : Entity
{
    public Guid VisitId { get; private set; }
    public RefractionType Type { get; private set; }

    // Right eye (OD) refraction values
    public decimal? OdSph { get; private set; }
    public decimal? OdCyl { get; private set; }
    public decimal? OdAxis { get; private set; }
    public decimal? OdAdd { get; private set; }
    public decimal? OdPd { get; private set; }

    // Left eye (OS) refraction values
    public decimal? OsSph { get; private set; }
    public decimal? OsCyl { get; private set; }
    public decimal? OsAxis { get; private set; }
    public decimal? OsAdd { get; private set; }
    public decimal? OsPd { get; private set; }

    // Visual acuity per eye
    public decimal? UcvaOd { get; private set; }
    public decimal? UcvaOs { get; private set; }
    public decimal? BcvaOd { get; private set; }
    public decimal? BcvaOs { get; private set; }

    // Intraocular pressure per eye
    public decimal? IopOd { get; private set; }
    public decimal? IopOs { get; private set; }
    public IopMethod? IopMethod { get; private set; }

    // Axial length per eye
    public decimal? AxialLengthOd { get; private set; }
    public decimal? AxialLengthOs { get; private set; }

    private Refraction() { }

    /// <summary>
    /// Factory method for creating an empty refraction of a given type.
    /// </summary>
    public static Refraction Create(Guid visitId, RefractionType type)
    {
        return new Refraction
        {
            VisitId = visitId,
            Type = type
        };
    }

    /// <summary>
    /// Updates all refraction values at once.
    /// </summary>
    public void Update(
        decimal? odSph, decimal? odCyl, decimal? odAxis, decimal? odAdd, decimal? odPd,
        decimal? osSph, decimal? osCyl, decimal? osAxis, decimal? osAdd, decimal? osPd,
        decimal? ucvaOd, decimal? ucvaOs, decimal? bcvaOd, decimal? bcvaOs,
        decimal? iopOd, decimal? iopOs, IopMethod? iopMethod,
        decimal? axialLengthOd, decimal? axialLengthOs)
    {
        OdSph = odSph;
        OdCyl = odCyl;
        OdAxis = odAxis;
        OdAdd = odAdd;
        OdPd = odPd;

        OsSph = osSph;
        OsCyl = osCyl;
        OsAxis = osAxis;
        OsAdd = osAdd;
        OsPd = osPd;

        UcvaOd = ucvaOd;
        UcvaOs = ucvaOs;
        BcvaOd = bcvaOd;
        BcvaOs = bcvaOs;

        IopOd = iopOd;
        IopOs = iopOs;
        IopMethod = iopMethod;

        AxialLengthOd = axialLengthOd;
        AxialLengthOs = axialLengthOs;

        SetUpdatedAt();
    }
}
